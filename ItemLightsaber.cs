using UnityEngine;
using BS;
using System;
using System.Linq;

namespace TOR {
    // The item module will add a unity component to the item object. See unity monobehaviour for more information: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    // This component will apply a force on the player rigidbody to the direction of an item transform when the trigger is pressed (see custom reference in the item definition component of the item prefab)
    public class ItemLightsaber : MonoBehaviour {
        protected Item item;
        protected ItemModuleLightsaber module;
        protected Rigidbody body;

        LightsaberBlade[] blades;
        bool isActive;
        bool isHolding;
        bool isTeleGrabbed;

        bool thrown;
        float ignoreCrystalTime;
        float primaryControlHoldTime;
        float secondaryControlHoldTime;
        ItemModuleAI.WeaponClass originalWeaponClass;
        PlayerHand playerHand;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLightsaber>();
            body = item.GetComponent<Rigidbody>();

            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnTeleGrabEvent += OnTeleGrabEvent;
            item.OnTeleUnGrabEvent += OnTeleUngrabEvent;
            item.OnHeldActionEvent += OnHeldAction;
            item.OnCollisionEvent += CollisionHandler;

            blades = module.lightsaberBlades.Select(Instantiate).ToArray();

            foreach (var blade in blades) {
                blade.Initialise(item);
                blade.extendDelta = -(blade.maxLength / module.ignitionDuration);
            }
            if (module.startActive) TurnOn(true);
            else TurnOff(false);

            originalWeaponClass = item.data.moduleAI.weaponClass;
        }

        public void ExecuteAction(string action, Interactor interactor = null) {
            if (action == "nextPhase") NextPhase(interactor);
            else if (action == "toggleIgnition") ToggleLightsaber(interactor);
            else if (action == "toggleSingle") ToggleSingle(interactor);
            else if (action == "turnOn") TurnOn();
            else if (action == "turnOff") TurnOff();
        }

        void NextPhase(Interactor interactor = null) {
            foreach (var blade in blades) {
                blade.NextPhase();
            }
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
        }

        void ToggleLightsaber(Interactor interactor = null) {
            if (isActive) TurnOff();
            else TurnOn();
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
        }

        // Turn on only first blade - used for saber staff
        void ToggleSingle(Interactor interactor = null) {
            var singleAlreadyActive = blades[0].isActive;
            if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) {
                isActive = true;
                ResetCollisions();

                var firstEnabled = false;
                foreach (var blade in blades) {
                    if (!firstEnabled || singleAlreadyActive && !blade.isActive) blade.TurnOn(!blade.isActive);
                    else blade.TurnOff(blade.isActive);
                    firstEnabled = true;
                }
            }
        }

        void TurnOn(bool playSound = true) {
            if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) {
                isActive = true;
                ResetCollisions();

                foreach (var blade in blades) {
                    blade.TurnOn(playSound && !blade.isActive);
                }
            }
        }

        void TurnOff(bool playSound = true) {
            isActive = false;
            ResetCollisions();

            // Unpenetrate all currently penetrated objects - fixes glitchy physics
            Array.ForEach(item.collisions, collision => {
                if (collision.damageStruct.penetration == DamageStruct.Penetration.Hit) {
                    collision.damageStruct.damager.UnPenetrateAll();
                    collision.active = false;
                }
            });

            foreach (var blade in blades) {
                blade.TurnOff(playSound && blade.isActive);
            }
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            playerHand = interactor.playerHand;

            // Turn on lightsaber automatically if NPC hand
            if (interactor.playerHand != Player.local.handRight && interactor.playerHand != Player.local.handLeft)
                TurnOn();
            isHolding = true;
            ResetCollisions();
            thrown = false;
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool throwing) {
            // Turn off lightsaber automatically if NPC hand
            if (interactor.playerHand != Player.local.handRight && interactor.playerHand != Player.local.handLeft)
                TurnOff();
            isHolding = false;
            ResetCollisions();

            // throw the lightsaber
            if (!thrown && body.velocity.magnitude > module.throwSpeed && module.canThrow) {
                playerHand = interactor.playerHand;
                thrown = true;
            }
        }

        public void OnTeleGrabEvent(Handle handle, Telekinesis teleGrabber) {
            isTeleGrabbed = true;
            ResetCollisions();
        }

        public void OnTeleUngrabEvent(Handle handle, Telekinesis teleGrabber) {
            isTeleGrabbed = false;
            ResetCollisions();
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
            if (isHolding) {
                // If priamry hold action available
                if (!string.IsNullOrEmpty(module.primaryGripPrimaryActionHold)) {
                    // start primary control timer
                    if (action == Interactable.Action.UseStart) {
                        primaryControlHoldTime = module.controlHoldTime;
                    } else if (action == Interactable.Action.UseStop) {
                        // if not held for long run standard action
                        if (primaryControlHoldTime > 0 && primaryControlHoldTime > (primaryControlHoldTime / 2)) {
                            ExecuteAction(module.primaryGripPrimaryAction, interactor);
                        }
                        primaryControlHoldTime = 0;
                    }
                } else {
                    if (action == Interactable.Action.UseStart) ExecuteAction(module.primaryGripPrimaryAction, interactor);
                }

                // If secondary hold action available
                if (!string.IsNullOrEmpty(module.primaryGripSecondaryActionHold)) {
                    // start secondary control timer
                    if (action == Interactable.Action.AlternateUseStart) {
                        secondaryControlHoldTime = module.controlHoldTime;
                    } else if (action == Interactable.Action.AlternateUseStop) {
                        // if not held for long run standard action
                        if (secondaryControlHoldTime > 0 && secondaryControlHoldTime > (secondaryControlHoldTime / 2)) {
                            ExecuteAction(module.primaryGripSecondaryAction, interactor);
                        }
                        secondaryControlHoldTime = 0;
                    }
                } else {
                    if (action == Interactable.Action.AlternateUseStart) ExecuteAction(module.primaryGripSecondaryAction, interactor);
                }
            }
        }

        void CollisionHandler(ref CollisionStruct collisionInstance) {
            try {
                if (collisionInstance.sourceColliderGroup.name == "CollisionHilt") {
                    if (collisionInstance.targetColliderGroup.name == "CollisionLightsaberTool") {
                        if (item.leftNpcHand || item.rightNpcHand) return;
                        foreach (var blade in blades) {
                            if (!string.IsNullOrEmpty(blade.kyberCrystal)) {
                                TurnOff(isActive);
                                item.data.moduleAI.weaponClass = 0; // Tell NPCs not to use lightsaber
                                blade.RemoveCrystal();
                                ignoreCrystalTime = 0.5f;
                                break;
                            }
                        }
                    } else if (collisionInstance.targetColliderGroup.name == "KyberCrystalCollision" && ignoreCrystalTime <= 0) {
                        foreach (var blade in blades) {
                            if (string.IsNullOrEmpty(blade.kyberCrystal)) {
                                var kyberCrystal = collisionInstance.targetCollider.attachedRigidbody.GetComponentInParent<ItemKyberCrystal>();
                                blade.AddCrystal(kyberCrystal);
                                break;
                            }
                        }
                        // Allow AI to use lightsaber again
                        if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) item.data.moduleAI.weaponClass = originalWeaponClass;
                    }
                }
            } catch {}
        }

        void ResetCollisions() {
            foreach (var blade in blades) {
                blade.collisionBlade.enabled = isActive;
            }
            body.ResetCenterOfMass();
        }

        protected void Update() {
            foreach (var blade in blades) {
                blade.UpdateSize();

                // Turn off blade completely if at minimum length and currently active
                if (blade.currentLength <= blade.minLength && (blade.saberBody.enabled)) {
                    blade.idleSound.Stop();
                    ResetCollisions();
                    blade.SetComponentState(false);
                }
            }

            if (playerHand && thrown && PlayerControl.GetHand(playerHand.side).gripPressed && !item.isGripped && !isTeleGrabbed) {
                // forget hand if hand is currently holding something
                if (playerHand.bodyHand.interactor.grabbedHandle) playerHand = null;
                else ReturnSaber();
            }

            if (isHolding && !isTeleGrabbed) {
                thrown = false;
                body.collisionDetectionMode = (body.velocity.magnitude > module.fastCollisionSpeed) ? (CollisionDetectionMode)module.fastCollisionMode : CollisionDetectionMode.Discrete;
            }

            if (ignoreCrystalTime > 0) ignoreCrystalTime -= Time.deltaTime;

            if (primaryControlHoldTime > 0) {
                primaryControlHoldTime -= Time.deltaTime;
                if (primaryControlHoldTime <= 0) ExecuteAction(module.primaryGripPrimaryActionHold);
            }
            if (secondaryControlHoldTime > 0) {
                secondaryControlHoldTime -= Time.deltaTime;
                if (secondaryControlHoldTime <= 0) ExecuteAction(module.primaryGripSecondaryActionHold);
            }
        }

        void ReturnSaber() {
            var hand = PlayerControl.GetHand(playerHand.side);
            var gripAxis = (hand.middleCurl + hand.ringCurl + hand.littleCurl) / 3f;

            float grabDistance = 0.3f;
            float returnSpeed = 10f * gripAxis;
            if (Vector3.Distance(item.transform.position, playerHand.transform.position) < grabDistance) {
                playerHand.bodyHand.interactor.TryRelease();
                playerHand.bodyHand.interactor.Grab(item.mainHandleRight);
            } else {
                body.velocity = (playerHand.transform.position - body.position) * returnSpeed;
            }
        }
    }

    [Serializable]
    public class LightsaberBlade : ScriptableObject {
        // sets blade length in metres - defaults to default mesh size if blank
        public float bladeLength;
        public float[] phaseLengths;

        // custom reference strings
        public string kyberCrystal;
        public string crystalEjectRef;
        public string collisionRef;
        public string saberBodyRef;
        public string saberTipGlowRef;
        public string saberGlowRef;
        public string saberParticlesRef;
        public string startSoundsRef;
        public string stopSoundsRef;
        public string whooshRef;

        // primary objects for Lightsaber to interact with
        public Transform crystalEject;
        public AudioSource idleSound;
        public MeshRenderer saberBody;
        public MeshRenderer saberGlow;
        public Light saberGlowLight;
        public Light saberTipGlow;
        public ParticleSystem saberParticles;
        public ParticleSystem unstableParticles;
        public Whoosh whooshBlade;
        public Collider collisionBlade;
        protected AudioSource[] startSounds;
        protected AudioSource[] stopSounds;

        // internal properties
        public Item parent;
        public float currentLength;
        public int currentPhase;
        public float minLength;
        public float maxLength;
        public float extendDelta;
        public bool isActive;
        public bool isUnstable;

        public void Initialise(Item parent) {
            this.parent = parent;
            if (!string.IsNullOrEmpty(collisionRef)) {
                collisionBlade = parent.definition.GetCustomReference(collisionRef).GetComponent<Collider>();
                collisionBlade.enabled = true;
                collisionBlade.enabled = isActive;
            }
            if (!string.IsNullOrEmpty(crystalEjectRef)) crystalEject = parent.definition.GetCustomReference(crystalEjectRef);
            if (!string.IsNullOrEmpty(startSoundsRef)) startSounds = parent.definition.GetCustomReference(startSoundsRef).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(stopSoundsRef)) stopSounds = parent.definition.GetCustomReference(stopSoundsRef).GetComponents<AudioSource>();

            if (!string.IsNullOrEmpty(saberBodyRef)) {
                var tempSaberBody = parent.definition.GetCustomReference(saberBodyRef);
                saberBody = tempSaberBody.GetComponent<MeshRenderer>();
                idleSound = tempSaberBody.GetComponent<AudioSource>();
                var tempUnstable = tempSaberBody.transform.Find("UnstableParticles");
                if (tempUnstable != null) unstableParticles = tempUnstable.GetComponent<ParticleSystem>();
            }
            if (!string.IsNullOrEmpty(saberTipGlowRef)) saberTipGlow = parent.definition.GetCustomReference(saberTipGlowRef).GetComponent<Light>();
            if (!string.IsNullOrEmpty(saberGlowRef)) saberGlow = parent.definition.GetCustomReference(saberGlowRef).GetComponent<MeshRenderer>();
            if (!string.IsNullOrEmpty(saberGlowRef)) saberGlowLight = parent.definition.GetCustomReference(saberGlowRef).GetComponent<Light>();
            if (!string.IsNullOrEmpty(saberParticlesRef)) saberParticles = parent.definition.GetCustomReference(saberParticlesRef).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(whooshRef)) whooshBlade = parent.definition.GetCustomReference(whooshRef).GetComponent<Whoosh>();

            maxLength = (bladeLength > 0f) ? bladeLength / 10 : saberBody.transform.localScale.z;

            if (!string.IsNullOrEmpty(kyberCrystal)) {
                var kyberCrystalData = Catalog.current.GetData<ItemData>(kyberCrystal, true);
                if (kyberCrystalData == null) return;
                var kyberCrystalObject = kyberCrystalData.Spawn(true);
                if (!kyberCrystalObject.gameObject.activeInHierarchy) kyberCrystalObject.gameObject.SetActive(true);
                AddCrystal(kyberCrystalObject.GetComponent<ItemKyberCrystal>());
            }

            SetComponentState(false);

            // Always set this to false now, it is only used for item previews
            if (saberGlow != null) saberGlow.enabled = false;

            // setup audio sources
            // Utils.ApplyStandardMixer(new AudioSource[] { idleSound });
            // Utils.ApplyStandardMixer(startSounds);
            // Utils.ApplyStandardMixer(stopSounds);
        }

        public void SetComponentState(bool state) {
            if (whooshBlade != null) whooshBlade.enabled = state;
            if (saberBody != null) saberBody.enabled = state;
            if (saberGlowLight != null) saberGlowLight.enabled = state;
            if (saberTipGlow != null) saberTipGlow.enabled = state;
            if (saberParticles != null) {
                if (state) saberParticles.Play();
                else {
                    saberParticles.Stop();
                    saberParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); // destroy leftover beam particles
                }
            }
            if (unstableParticles != null) {
                if (state && isUnstable) unstableParticles.Play();
                else {
                    unstableParticles.Stop();
                    unstableParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); // destroy leftover beam particles
                }
            }
        }

        public void NextPhase() {
            currentPhase = (currentPhase >= phaseLengths.Length - 1) ? -1 : currentPhase;
            // var previousLength = maxLength;
            maxLength = phaseLengths[++currentPhase] / 10;
            // extendDelta = (previousLength < maxLength) ? Mathf.Abs(extendDelta) : -Mathf.Abs(extendDelta);
        }

        public void AddCrystal(ItemKyberCrystal kyberCrystalObject) {
            saberBody.material.SetColor("_GlowColor", kyberCrystalObject.bladeColour);
            saberGlow.material.SetColor("_DiffuseColor", kyberCrystalObject.bladeColour);
            if (unstableParticles) {
                var main = unstableParticles.main;
                main.startColor = kyberCrystalObject.bladeColour;
                main.startLifetimeMultiplier = 33.333f * maxLength;
                var shape = unstableParticles.GetComponentInChildren<ParticleSystem>().shape;
                shape.scale = new Vector3(shape.scale.x, maxLength, shape.scale.y);
            }

            saberGlowLight.color = kyberCrystalObject.glowColour;
            saberGlowLight.intensity = kyberCrystalObject.glowIntensity * 0.1f;
            saberGlowLight.range = kyberCrystalObject.glowRange;

            saberTipGlow.color = kyberCrystalObject.glowColour;
            saberTipGlow.intensity = kyberCrystalObject.glowIntensity;
            saberTipGlow.range = kyberCrystalObject.glowRange;

            isUnstable = kyberCrystalObject.isUnstable;

            if (!string.IsNullOrEmpty(saberBodyRef)) {
                var current = parent.definition.GetCustomReference(saberBodyRef);
                idleSound = Instantiate(kyberCrystalObject.idleSound, current.position, current.rotation, current.parent);
            }
            if (!string.IsNullOrEmpty(startSoundsRef)) {
                var current = parent.definition.GetCustomReference(startSoundsRef);
                startSounds = Instantiate(kyberCrystalObject.startSounds, current.position, current.rotation, current.parent).GetComponents<AudioSource>();
            }
            if (!string.IsNullOrEmpty(stopSoundsRef)) {
                var current = parent.definition.GetCustomReference(stopSoundsRef);
                stopSounds = Instantiate(kyberCrystalObject.stopSounds, current.position, current.rotation, current.parent).GetComponents<AudioSource>();
            }

            kyberCrystal = kyberCrystalObject.name.Replace("(Clone)", "");
            kyberCrystalObject.GetComponent<Item>().Despawn();
        }

        public void RemoveCrystal() {
            if (!string.IsNullOrEmpty(kyberCrystal)) {
                var kyberCrystalData = Catalog.current.GetData<ItemData>(kyberCrystal, true);
                if (kyberCrystalData == null) return;
                var kyberCrystalObject = kyberCrystalData.Spawn(true);
                if (!kyberCrystalObject.gameObject.activeInHierarchy) kyberCrystalObject.gameObject.SetActive(true);

                kyberCrystalObject.transform.position = crystalEject.position;
                kyberCrystalObject.transform.rotation = crystalEject.rotation;

                kyberCrystal = "";
            }
        }

        public void TurnOn(bool playSound = true) {
            if (!string.IsNullOrEmpty(kyberCrystal)) {
                isActive = true;
                if (playSound) Utils.PlayRandomSound(startSounds);
                idleSound.Play();
                SetComponentState(true);
            }
        }

        public void TurnOff(bool playSound = true) {
            if (!string.IsNullOrEmpty(kyberCrystal)) {
                isActive = false;
                if (playSound) Utils.PlayRandomSound(stopSounds);
            }
        }

        public void UpdateBladeDirection() {
            extendDelta = (!isActive || currentLength > maxLength) ? -Mathf.Abs(extendDelta) : Mathf.Abs(extendDelta);
        }

        public void UpdateSize() {
            // if blade is currently longer than expected (going from long to short blade phase)
            if (isActive && currentLength > maxLength) {
                UpdateBladeDirection();
                currentLength = Mathf.Clamp(currentLength + (extendDelta * Time.deltaTime), maxLength, currentLength);
                saberBody.transform.localScale = new Vector3(saberBody.transform.localScale.x, saberBody.transform.localScale.y, currentLength);
                return;
            }
            
            // if blade still extending or retracting
            if ((isActive && (currentLength < maxLength || currentLength > maxLength)) || (!isActive && currentLength > minLength)) {
                UpdateBladeDirection();
                currentLength = Mathf.Clamp(currentLength + (extendDelta * Time.deltaTime), minLength, maxLength);
                saberBody.transform.localScale = new Vector3(saberBody.transform.localScale.x, saberBody.transform.localScale.y, currentLength);
                return;
            }
        }
    }
}