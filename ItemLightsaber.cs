using ThunderRoad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;

namespace TOR {
    public class ItemLightsaber : MonoBehaviour {
        protected Item item;
        public ItemModuleLightsaber module;
        protected Rigidbody body;

        public LightsaberBlade[] blades;
        internal bool isActive;
        bool isHelicoptering;
        bool isHolding;
        public bool isSnapped;
        bool isOpen;

        Interactor leftInteractor;
        Interactor rightInteractor;
        SpellTelekinesis telekinesis;

        Animator[] animators = { };
        string[] kyberCrystals = { };
        float[] bladeLengths = { };
        bool thrown;
        bool returning;
        bool tapReturning;
        bool tapToReturn;
        bool useExpensiveCollisions;
        float ignoreCrystalTime;
        public float ignoreCoupleTime;
        float primaryControlHoldTime;
        float secondaryControlHoldTime;
        ItemModuleAI.WeaponClass originalWeaponClass;
        PlayerHand playerHand;
        Coroutine unpenetrateCoroutine;

        Transform itemTrans;
        Collider couplerCollider;
        Transform couplerTrans;
        FixedJoint couplerJoint;
        Item coupledItem;
        string coupledItemOriginalSlot;
        ItemLightsaber coupledLightsaber;

        Rigidbody playerBody;

        char listSeperator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLightsaber>();
            body = item.GetComponent<Rigidbody>();
            itemTrans = item.transform;

            if (!string.IsNullOrEmpty(module.animatorId)) animators = item.definition.GetCustomReference(module.animatorId).GetComponentsInChildren<Animator>();

            if (module.hasCoupler) {
                couplerCollider = item.definition.GetCustomReference("CouplerCollider").GetComponent<Collider>();
                couplerTrans = item.definition.GetCustomReference("CouplerHolder");

                item.definition.TryGetSavedValue("coupledLightsaberProps", out string coupledLightsaberProps);
                if (!string.IsNullOrEmpty(coupledLightsaberProps)) {
                    var decodedBytes = Convert.FromBase64String(coupledLightsaberProps);
                    var savedLightsaber = JsonUtility.FromJson<SavedLightsaber>(Encoding.UTF8.GetString(decodedBytes));
                    SpawnCoupledItem(savedLightsaber.itemId);

                    var coupledKyberCrystals = savedLightsaber.kyberCrystals.Split(listSeperator) ?? null;
                    for (int i = 0, l = coupledKyberCrystals.Count(); i < l; i++) {
                        coupledLightsaber.blades[i].AddCrystal(coupledKyberCrystals[i]);
                    }

                    var lengths = savedLightsaber.bladeLengths.Split(listSeperator);
                    for (int i = 0, l = lengths.Count(); i < l; i++) {
                        coupledLightsaber.blades[i].SetBladeLength(float.Parse(lengths[i]) / coupledLightsaber.blades[i].saberBodyTrans.parent.localScale.z * 0.1f);
                    }

                    coupledLightsaber.StoreSaberState();
                }
            }

            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnTelekinesisGrabEvent += OnTeleGrabEvent;
            item.OnTelekinesisReleaseEvent += OnTeleUngrabEvent;
            item.OnHeldActionEvent += OnHeldAction;
            item.OnSnapEvent += OnSnapEvent;
            item.OnUnSnapEvent += OnUnSnapEvent;

            RetrieveSaberState();
            blades = module.lightsaberBlades.Select(Instantiate).ToArray();

            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].Initialise(item, module.ignitionDuration, kyberCrystals.ElementAtOrDefault(i) ?? null, bladeLengths.ElementAtOrDefault(i));
            }

            if (module.startActive) TurnOn(true);
            else TurnOff(false);

            tapToReturn = !GameManager.options.GetController().holdGripForHandles;
            originalWeaponClass = item.data.moduleAI.weaponClass;

            playerBody = Player.local.locomotion.rb;
            StoreSaberState();
        }

        void Destory() {
            if (TORGlobalSettings.lightsaberColliders.ContainsKey(GetInstanceID())) {
                TORGlobalSettings.lightsaberColliders.Remove(GetInstanceID());
            }
        }

        void LoadGlobalSettings() {
            useExpensiveCollisions = TORGlobalSettings.SaberExpensiveCollisions;
        }

        public void ExecuteAction(string action, Interactor interactor = null) {
            if (action == "decouple") Decouple();
            else if (action == "nextPhase") NextPhase(interactor);
            else if (action == "toggleAnimation") ToggleAnimation(interactor);
            else if (action == "toggleHelicopter") ToggleHelicopter(interactor);
            else if (action == "toggleIgnition") ToggleLightsaber(interactor);
            else if (action == "toggleSingle") ToggleSingle(interactor);
            else if (action == "turnOn") TurnOn();
            else if (action == "turnOff") TurnOff();
        }

        void DecreaseBladeLength(dynamic args) {
            if (args.allowDisarm && (item.leftNpcHand || item.rightNpcHand)) return;
            for (int i = 0, l = blades.Count(); i < l; i++) {
                if (blades[i].maxLength - args.lengthChange > 0) {
                    blades[i].SetBladeLength(blades[i].maxLength - args.lengthChange);
                }
            }
            StoreSaberState();
        }

        void IncreaseBladeLength(dynamic args) {
            if (args.allowDisarm && (item.leftNpcHand || item.rightNpcHand)) return;
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].SetBladeLength(blades[i].maxLength + args.lengthChange);
            }
            StoreSaberState();
        }

        void ResetBladeLength(dynamic args) {
            if (args.allowDisarm && (item.leftNpcHand || item.rightNpcHand)) return;
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].SetBladeLength(module.lightsaberBlades[i].bladeLength / blades[i].saberBodyTrans.parent.localScale.z * 0.1f);
            }
            StoreSaberState();
        }

        void NextPhase(Interactor interactor = null) {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].NextPhase();
            }
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
        }

        public void Couple(Item itemToCouple) {
            if (itemToCouple && (itemToCouple.definition.itemId == item.definition.itemId || module.couplingWhitelist.Contains(itemToCouple.definition.itemId))) {
                coupledItem = itemToCouple;
                coupledItemOriginalSlot = coupledItem.data.slot;
                coupledItem.data.slot = "Cork";
                coupledItem.transform.MoveAlign(coupledItem.definition.holderPoint, couplerTrans, couplerTrans);
                couplerJoint = itemTrans.gameObject.AddComponent<FixedJoint>();
                couplerJoint.connectedBody = coupledItem.rb;
                coupledLightsaber = coupledItem.GetComponent<ItemLightsaber>();
                SetCouplerCollider(false);
                StoreSaberState();
            }
        }

        public void Decouple() {
            if (couplerJoint) Destroy(couplerJoint);
            coupledItem.transform.SetParent(null);
            coupledItem.data.slot = coupledItemOriginalSlot;
            coupledItem = null;
            SetCouplerCollider(true);
            coupledLightsaber = null;
            StoreSaberState();
        }

        public void SetCouplerCollider(bool enabled) {
            couplerCollider.enabled = enabled;
            ignoreCoupleTime = 0.5f;
            if (coupledLightsaber) {
                coupledLightsaber.SetCouplerCollider(enabled);
                coupledLightsaber.ignoreCoupleTime = 1f;
            }
        }

        void SpawnCoupledItem(string itemId) {
            Couple(Catalog.GetData<ItemPhysic>(itemId, true).Spawn(true, null));
        }

        void ToggleAnimation(Interactor interactor = null) {
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
            isOpen = !isOpen;
            for (int i = 0, l = animators.Count(); i < l; i++) {
                animators[i].SetTrigger(isOpen ? "open" : "close");
                animators[i].ResetTrigger(isOpen ? "close" : "open");
            }
        }

        void ToggleHelicopter(Interactor interactor = null) {
            ToggleAnimation(interactor);
            isHelicoptering = !isHelicoptering;
            if (isHelicoptering) unpenetrateCoroutine = StartCoroutine(UnpenetrateCoroutine());
            else StopCoroutine(unpenetrateCoroutine);
        }

        void ToggleLightsaber(Interactor interactor = null) {
            if (isActive) TurnOff();
            else TurnOn();
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
        }

        // Turn on only first blade - used for saber staff
        void ToggleSingle(Interactor interactor = null) {
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
            var singleAlreadyActive = blades[0].isActive;
            if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) {
                isActive = true;

                var firstEnabled = false;
                for (int i = 0, l = blades.Count(); i < l; i++) {
                    if (!firstEnabled || singleAlreadyActive && !blades[i].isActive) blades[i].TurnOn(!blades[i].isActive);
                    else blades[i].TurnOff(blades[i].isActive);
                    firstEnabled = true;
                }
                ResetCollisions();
            }
        }

        void TurnOn(bool playSound = true) {
            if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) {
                isActive = true;

                for (int i = 0, l = blades.Count(); i < l; i++) {
                    blades[i].TurnOn(playSound && !blades[i].isActive);
                }
                ResetCollisions();

                if (TORGlobalSettings.lightsaberColliders != null && !TORGlobalSettings.lightsaberColliders.ContainsKey(GetInstanceID())) {
                    TORGlobalSettings.lightsaberColliders.Add(GetInstanceID(), blades.Select(blade => blade.collisionBlade).ToArray());
                }
            }
        }

        void TurnOff(bool playSound = true) {
            isActive = false;

            Unpenetrate();
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].TurnOff(playSound && blades[i].isActive);
            }
            ResetCollisions();

            if (TORGlobalSettings.lightsaberColliders != null && TORGlobalSettings.lightsaberColliders.ContainsKey(GetInstanceID())) {
                TORGlobalSettings.lightsaberColliders.Remove(GetInstanceID());
            }
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            playerHand = interactor.playerHand;

            // Turn on lightsaber automatically if NPC hand
            if (playerHand != Player.local.handRight && playerHand != Player.local.handLeft) {
                TurnOn();
                var creature = interactor.bodyHand.body.creature;
                if (!creature.gameObject.GetComponent<LightsaberNPCAnimator>()) {
                    creature.gameObject.AddComponent<LightsaberNPCAnimator>().SetCreature(creature);
                }
            }
            isHolding = true;
            ResetCollisions();
            thrown = false;
            returning = false;
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool throwing) {
            // Turn off lightsaber automatically if NPC hand
            if (playerHand != Player.local.handRight && playerHand != Player.local.handLeft) {
                TurnOff();
                var otherHandle = interactor.otherInteractor?.grabbedHandle;
                if (!otherHandle || (otherHandle.item && !otherHandle.item.gameObject.GetComponent<ItemLightsaber>())) {
                    Destroy(interactor.bodyHand.body.creature.gameObject.GetComponent<LightsaberNPCAnimator>());
                }
            }
            isHolding = false;
            ResetCollisions();

            // throw the lightsaber
            if (!thrown && body.velocity.magnitude > module.throwSpeed && module.canThrow) {
                playerHand = interactor.playerHand;
                thrown = true;
            }

            leftInteractor = null;
            rightInteractor = null;
        }

        public void OnSnapEvent(ObjectHolder holder) {
            isSnapped = true;
            if (playerHand) playerHand.bodyHand.interactor.TryRelease();
        }

        public void OnUnSnapEvent(ObjectHolder holder) {
            isSnapped = false;
        }

        public void OnTeleGrabEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = teleGrabber;
            ResetCollisions();

            if (isHelicoptering && !thrown) {
                ToggleHelicopter();
            }
        }

        public void OnTeleUngrabEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = null;
            ResetCollisions();
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
            // If priamry hold action available
            if (!string.IsNullOrEmpty(module.primaryGripPrimaryActionHold)) {
                // start primary control timer
                if (action == Interactable.Action.UseStart) {
                    primaryControlHoldTime = module.controlHoldTime;
                    if (interactor.side == Side.Right) {
                        rightInteractor = interactor;
                    } else {
                        leftInteractor = interactor;
                    }
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

            if (action == Interactable.Action.UseStart) {
                if (interactor.side == Side.Right) {
                    rightInteractor = interactor;
                } else {
                    leftInteractor = interactor;
                }
            } else if (action == Interactable.Action.UseStop || action == Interactable.Action.Ungrab) {
                if (interactor.side == Side.Right) {
                    rightInteractor = null;
                } else {
                    leftInteractor = null;
                }
            }
        }

        void TryAddCrystal(ItemKyberCrystal crystal) {
            if (crystal && ignoreCrystalTime <= 0) {
                for (int i = 0, l = blades.Count(); i < l; i++) {
                    if (string.IsNullOrEmpty(blades[i].kyberCrystal)) {
                        blades[i].AddCrystal(crystal);
                        StoreSaberState();
                        break;
                    }
                }
                // Allow AI to use lightsaber again
                if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) item.data.moduleAI.weaponClass = originalWeaponClass;
            }
        }

        void TryEjectCrystal(bool allowDisarm) {
            if (allowDisarm && (item.leftNpcHand || item.rightNpcHand)) return;
            for (int i = 0, l = blades.Count(); i < l; i++) {
                if (!string.IsNullOrEmpty(blades[i].kyberCrystal)) {
                    TurnOff(isActive);
                    item.data.moduleAI.weaponClass = 0; // Tell NPCs not to use lightsaber
                    blades[i].RemoveCrystal();
                    StoreSaberState();
                    ignoreCrystalTime = 0.5f;
                    break;
                }
            }
        }

        void OnTriggerEnter(Collider other) {
            if (other.name == "CouplerCollider" && module.hasCoupler && coupledItem == null && ignoreCoupleTime <= 0 && isSnapped == false) {
                var itemToCouple = other.attachedRigidbody.GetComponentInParent<Item>();
                var lightsaberToCouple = other.attachedRigidbody.GetComponentInParent<ItemLightsaber>();
                if (lightsaberToCouple && !lightsaberToCouple.isSnapped && lightsaberToCouple.coupledItem == null && lightsaberToCouple.item.data.slot != "Cork") {
                    Couple(itemToCouple);
                }
            }
        }

        void ResetCollisions() {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                if (blades[i].collisionBlade) blades[i].collisionBlade.enabled = blades[i].isActive;
            }
            body.ResetCenterOfMass();
        }

        void Unpenetrate() {
            // Unpenetrate all currently penetrated objects - fixes glitchy physics
            foreach (var handler in item.definition.collisionHandlers) {
                for (int i = 0, l = handler.collisions.Count(); i < l; i++) {
                    if (handler.collisions[i].damageStruct.penetration == DamageStruct.Penetration.Hit ||
                        handler.collisions[i].damageStruct.penetration == DamageStruct.Penetration.Pressure) {
                        handler.collisions[i].damageStruct.damager.UnPenetrateAll();
                        handler.collisions[i].active = false;
                    }
                }
            }
        }

        WaitForSeconds unpenetrateLoopDelay = new WaitForSeconds(0.1f);
        IEnumerator UnpenetrateCoroutine() {
            while (true) {
                yield return unpenetrateLoopDelay;
                Unpenetrate();
            }
        }

        protected void Update() {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].UpdateSize();

                // Turn off blade completely if at minimum length and currently active
                if (blades[i].currentLength <= blades[i].minLength && (blades[i].saberBody.enabled)) {
                    blades[i].idleSoundSource.Stop();
                    ResetCollisions();
                    blades[i].SetComponentState(false);
                }
            }

            if (playerHand && thrown && (PlayerControl.GetHand(playerHand.side).gripPressed || tapReturning) && !item.isGripped && telekinesis == null) {
                // forget hand if hand is currently holding something
                if (!playerHand.bodyHand || playerHand.bodyHand.interactor.grabbedHandle || playerHand.bodyHand.caster.telekinesis?.catchedHandle) playerHand = null;
                else {
                    if (!returning) {
                        var handToPlay = (playerHand.side == Side.Left) ? TORGlobalSettings.HandAudioLeft : TORGlobalSettings.HandAudioRight;
                        handToPlay.PlayOneShot(TORGlobalSettings.SaberRecallSound.PickAudioClip());
                        if (TORGlobalSettings.SaberActivateOnRecall) TurnOn();
                        Unpenetrate();
                        returning = true;
                    }
                    ReturnSaber();
                }
            }

            if (isHolding && telekinesis == null) {
                thrown = false;
                returning = false;
                if (useExpensiveCollisions) body.collisionDetectionMode = (body.velocity.magnitude > module.fastCollisionSpeed) ? (CollisionDetectionMode)module.fastCollisionMode : CollisionDetectionMode.Discrete;
            } else if (telekinesis != null && telekinesis.spinMode && !isActive) {
                TurnOn();
                telekinesis.SetSpinMode(false);
            }

            if (ignoreCrystalTime > 0) ignoreCrystalTime -= Time.deltaTime;
            if (ignoreCoupleTime > 0) ignoreCoupleTime -= Time.deltaTime;

            if (primaryControlHoldTime > 0) {
                primaryControlHoldTime -= Time.deltaTime;
                if (primaryControlHoldTime <= 0) ExecuteAction(module.primaryGripPrimaryActionHold);
            }
            if (secondaryControlHoldTime > 0) {
                secondaryControlHoldTime -= Time.deltaTime;
                if (secondaryControlHoldTime <= 0) ExecuteAction(module.primaryGripSecondaryActionHold);
            }
        }

        protected void FixedUpdate() {
            if (isHelicoptering && isActive) {
                float thrustLeft = 0;
                float thrustRight = 0;
                if (leftInteractor) thrustLeft = PlayerControl.GetHand(leftInteractor.side).useAxis;
                if (rightInteractor) thrustRight = PlayerControl.GetHand(rightInteractor.side).useAxis;
                float maxThrust = Mathf.Max(thrustLeft, thrustRight);
                playerBody.AddForce(itemTrans.right * Mathf.Lerp(module.helicopterThrust[0], module.helicopterThrust[1], maxThrust), ForceMode.Force);

                for (int i = 0, l = animators.Count(); i < l; i++) {
                    animators[i].speed = 1 + maxThrust;
                }
            }
        }

        void ReturnSaber() {
            var hand = PlayerControl.GetHand(playerHand.side);
            float grabDistance = 0.3f;
            float returnSpeed = 10f;
            if (!tapToReturn) {
                var gripAxis = (hand.middleCurl + hand.ringCurl + hand.littleCurl) / 3f;
                returnSpeed *= gripAxis;
            }
            tapReturning = tapToReturn;

            if (Vector3.SqrMagnitude(itemTrans.position - playerHand.transform.position) < grabDistance * grabDistance) {
                playerHand.bodyHand.interactor.TryRelease();
                playerHand.bodyHand.interactor.Grab(item.mainHandleRight);
                tapReturning = false;
            } else {
                body.velocity = (playerHand.transform.position - body.position) * returnSpeed;
            }
        }

        void RetrieveSaberState() {
            item.definition.TryGetSavedValue("kyberCrystals", out string tempCrystals);
            if (!string.IsNullOrEmpty(tempCrystals)) {
                kyberCrystals = tempCrystals.Split(listSeperator);
            }
            item.definition.TryGetSavedValue("bladeLengths", out string tempLengths);
            if (!string.IsNullOrEmpty(tempLengths)) {
                var lengths = tempLengths.Split(listSeperator);
                bladeLengths = new float[lengths.Count()];
                for (int i = 0, l = module.lightsaberBlades.Count(); i < l; i++) {
                    bladeLengths[i] = float.Parse(lengths[i]);
                }
            }
        }

        void StoreSaberState() {
            if (blades != null) {
                item.definition.SetSavedValue("kyberCrystals", string.Join(listSeperator.ToString(), blades.Select(blade => blade.kyberCrystal)));
                item.definition.SetSavedValue("bladeLengths", string.Join(listSeperator.ToString(), blades.Select(blade => (blade.maxLength * blade.saberBodyTrans.parent.localScale.z * 10f).ToString())));
            }
            if (coupledItem) {
                coupledItem.definition.TryGetSavedValue("kyberCrystals", out string tempCrystals);
                coupledItem.definition.TryGetSavedValue("bladeLengths", out string tempLengths);
                var savedLightsaber = new SavedLightsaber {
                    itemId = coupledItem.definition.itemId,
                    kyberCrystals = tempCrystals,
                    bladeLengths = tempLengths
                };
                var bytesToEncode = Encoding.UTF8.GetBytes(JsonUtility.ToJson(savedLightsaber));
                item.definition.SetSavedValue("coupledLightsaberProps", Convert.ToBase64String(bytesToEncode));
            } else {
                item.definition.SetSavedValue("coupledLightsaberProps", null);
            }
        }
    }

    [Serializable]
    public class SavedLightsaber {
        public string itemId;
        public string kyberCrystals;
        public string bladeLengths;
    }

    [Serializable]
    public class LightsaberBlade : ScriptableObject {
        // sets blade length in metres - defaults to default mesh size if blank
        public float bladeLength;
        public float[] phaseLengths;
        public float audioPitch = 1f;
        public float glowIntensity = 1f;

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
        public LightsaberTrail trail;
        public Transform crystalEject;
        public MeshRenderer saberBody;
        public MeshRenderer saberGlow;
        public MeshRenderer trailMeshRenderer;
        public Light saberGlowLight;
        public Light saberTipGlow;
        public ParticleSystem saberParticles;
        public ParticleSystem unstableParticles;
        public Whoosh whooshBlade;
        public Collider collisionBlade;
        public AudioContainer idleSound;
        public AudioContainer startSound;
        public AudioContainer stopSound;
        public AudioSource idleSoundSource;
        public AudioSource startSoundSource;
        public AudioSource stopSoundSource;

        // internal properties
        public Item parent;
        public float currentLength;
        public int currentPhase;
        public float minLength;
        public float maxLength;
        public float extendDelta;
        public float ignitionDuration;
        public bool isActive;
        public bool isUnstable;
        public MaterialPropertyBlock propBlock;
        float originalWhooshMaxVel;
        float originalWhooshMinVel;

        public Transform saberBodyTrans;

        public void Initialise(Item parent, float ignitionDuration, string kyberCrystalOverride = null, float bladeLengthOverride = 0) {
            this.parent = parent;
            this.ignitionDuration = ignitionDuration;
            if (!string.IsNullOrEmpty(collisionRef)) {
                collisionBlade = parent.definition.GetCustomReference(collisionRef).GetComponent<Collider>();
                collisionBlade.enabled = true;
                collisionBlade.enabled = isActive;
            }
            if (!string.IsNullOrEmpty(crystalEjectRef)) crystalEject = parent.definition.GetCustomReference(crystalEjectRef);
            if (!string.IsNullOrEmpty(startSoundsRef)) startSoundSource = parent.definition.GetCustomReference(startSoundsRef).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(stopSoundsRef)) stopSoundSource = parent.definition.GetCustomReference(stopSoundsRef).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(saberBodyRef)) {
                var tempSaberBody = parent.definition.GetCustomReference(saberBodyRef);
                saberBody = tempSaberBody.GetComponent<MeshRenderer>();
                saberBodyTrans = tempSaberBody.transform;
                propBlock = new MaterialPropertyBlock();
                idleSoundSource = tempSaberBody.GetComponent<AudioSource>();
                var tempUnstable = saberBodyTrans.Find("UnstableParticles");
                if (tempUnstable) unstableParticles = tempUnstable.GetComponent<ParticleSystem>();
                if (TORGlobalSettings.SaberTrailEnabled) {
                    var trailTrans = saberBodyTrans.Find("Trail");
                    trail = trailTrans.gameObject.AddComponent<LightsaberTrail>();
                    trailMeshRenderer = trailTrans.gameObject.GetComponent<MeshRenderer>();
                }
            }
            if (!string.IsNullOrEmpty(saberTipGlowRef)) saberTipGlow = parent.definition.GetCustomReference(saberTipGlowRef).GetComponent<Light>();
            if (!string.IsNullOrEmpty(saberGlowRef)) saberGlow = parent.definition.GetCustomReference(saberGlowRef).GetComponent<MeshRenderer>();
            if (!string.IsNullOrEmpty(saberGlowRef)) saberGlowLight = parent.definition.GetCustomReference(saberGlowRef).GetComponent<Light>();
            if (!string.IsNullOrEmpty(saberParticlesRef)) saberParticles = parent.definition.GetCustomReference(saberParticlesRef).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(whooshRef)) {
                whooshBlade = parent.definition.GetCustomReference(whooshRef).GetComponent<Whoosh>();
                originalWhooshMaxVel = whooshBlade.maxVelocity;
                originalWhooshMinVel = whooshBlade.minVelocity;
            }
            float initialBladeLength = (bladeLengthOverride > 0) ? bladeLengthOverride : bladeLength;
            SetBladeLength((initialBladeLength > 0f) ? (initialBladeLength / saberBodyTrans.parent.localScale.z * 0.1f) : saberBodyTrans.localScale.z);
            extendDelta = -extendDelta;
            kyberCrystal = kyberCrystalOverride ?? kyberCrystal;
            AddCrystal(kyberCrystal);

            SetComponentState(false);

            // Always set this to false now, it is only used for item previews
            if (saberGlow) saberGlow.enabled = false;

            // setup audio sources
            // Utils.ApplyStandardMixer(new AudioSource[] { idleSound });
            // Utils.ApplyStandardMixer(startSounds);
            // Utils.ApplyStandardMixer(stopSounds);
        }

        public void SetComponentState(bool state) {
            if (whooshBlade) {
                whooshBlade.maxVelocity = state ? originalWhooshMaxVel : float.MaxValue;
                whooshBlade.minVelocity = state ? originalWhooshMinVel : float.MaxValue;
            }
            if (saberBody) saberBody.enabled = state;
            if (saberGlowLight) saberGlowLight.enabled = state;
            if (saberTipGlow) saberTipGlow.enabled = state;
            if (saberParticles) {
                if (state) saberParticles.Play();
                else {
                    saberParticles.Stop();
                    saberParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); // destroy leftover beam particles
                }
            }
            if (unstableParticles) {
                if (state && isUnstable) unstableParticles.Play();
                else {
                    unstableParticles.Stop();
                    unstableParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); // destroy leftover beam particles
                }
            }
            if (trail) {
                trail.enabled = state;
                trailMeshRenderer.enabled = state;
            }
        }

        public void NextPhase() {
            currentPhase = (currentPhase >= phaseLengths.Length - 1) ? -1 : currentPhase;
            SetBladeLength(phaseLengths[++currentPhase] * 0.1f);
        }

        public void SetBladeLength(float length) {
            maxLength = length;
            if (trail) trail.height = length * saberBodyTrans.parent.localScale.z;
            CalculateUnstableParticleSize();
            extendDelta = maxLength / ignitionDuration;
        }

        public void CalculateUnstableParticleSize() {
            if (unstableParticles) {
                var main = unstableParticles.main;
                main.startLifetimeMultiplier = 33.333f * maxLength * saberBodyTrans.parent.localScale.z;
                var shape = unstableParticles.GetComponentInChildren<ParticleSystem>().shape;
                shape.scale = new Vector3(shape.scale.x, maxLength, shape.scale.y);
            }
        }

        public void AddCrystal(string kyberCrystalId) {
            if (!string.IsNullOrEmpty(kyberCrystalId)) {
                var kyberCrystalData = Catalog.GetData<ItemPhysic>(kyberCrystalId, true);
                if (kyberCrystalData == null) return;
                var kyberCrystalObject = kyberCrystalData.Spawn(true);
                AddCrystal(kyberCrystalObject.GetComponent<ItemKyberCrystal>());
            }
        }

        public void AddCrystal(ItemKyberCrystal kyberCrystalObject) {
            saberBody.GetPropertyBlock(propBlock);
            propBlock.SetColor("_GlowColor", kyberCrystalObject.bladeColour);
            propBlock.SetColor("_Color", kyberCrystalObject.coreColour);
            propBlock.SetFloat("_InnerGlow", kyberCrystalObject.module.innerGlow);
            propBlock.SetFloat("_OuterGlow", kyberCrystalObject.module.outerGlow);
            propBlock.SetFloat("_CoreRadius", kyberCrystalObject.module.coreRadius);
            propBlock.SetFloat("_CoreStrength", kyberCrystalObject.module.coreStrength);
            propBlock.SetFloat("_Flicker", kyberCrystalObject.module.flicker);
            propBlock.SetFloat("_FlickerSpeed", kyberCrystalObject.module.flickerSpeed);
            propBlock.SetFloatArray("_FlickerScale", kyberCrystalObject.module.flickerScale);
            saberBody.SetPropertyBlock(propBlock);

            if (trailMeshRenderer) {
                trailMeshRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_GlowColor", kyberCrystalObject.bladeColour);
                propBlock.SetColor("_Color", kyberCrystalObject.coreColour);
                propBlock.SetFloat("_InnerGlow", kyberCrystalObject.module.innerGlow);
                propBlock.SetFloat("_OuterGlow", kyberCrystalObject.module.outerGlow * (kyberCrystalObject.module.innerGlow <= 0.01 ? 0.5f : 1f));
                propBlock.SetFloat("_CoreRadius", kyberCrystalObject.module.innerGlow <= 0.01 ? 0 : kyberCrystalObject.module.coreRadius);
                propBlock.SetFloat("_CoreStrength", kyberCrystalObject.module.innerGlow <= 0.01 ? 0 : kyberCrystalObject.module.coreStrength);
                propBlock.SetFloat("_Flicker", kyberCrystalObject.module.flicker);
                propBlock.SetFloat("_FlickerSpeed", kyberCrystalObject.module.flickerSpeed);
                propBlock.SetFloatArray("_FlickerScale", kyberCrystalObject.module.flickerScale);
                trailMeshRenderer.SetPropertyBlock(propBlock);
            }

            saberGlow.material.SetColor("_DiffuseColor", kyberCrystalObject.bladeColour);

            if (unstableParticles) {
                var main = unstableParticles.main;
                main.startColor = kyberCrystalObject.bladeColour;
                CalculateUnstableParticleSize();
            }

            if (saberGlowLight) {
                saberGlowLight.color = kyberCrystalObject.glowColour;
                saberGlowLight.intensity = kyberCrystalObject.module.glowIntensity * glowIntensity * 0.1f;
                saberGlowLight.range = kyberCrystalObject.module.glowRange;
            }

            if (saberTipGlow) {
                saberTipGlow.color = kyberCrystalObject.glowColour;
                saberTipGlow.intensity = kyberCrystalObject.module.glowIntensity * glowIntensity;
                saberTipGlow.range = kyberCrystalObject.module.glowRange;
            }

            isUnstable = kyberCrystalObject.module.isUnstable;

            if (idleSoundSource) {
                idleSound = kyberCrystalObject.module.idleSoundAsset;
                idleSoundSource.volume = kyberCrystalObject.module.idleSoundVolume;
                idleSoundSource.pitch = kyberCrystalObject.module.idleSoundPitch * audioPitch;
            }
            if (startSoundSource) {
                startSound = kyberCrystalObject.module.startSoundAsset;
                startSoundSource.volume = kyberCrystalObject.module.startSoundVolume;
                startSoundSource.pitch = kyberCrystalObject.module.startSoundPitch * audioPitch;
            }

            if (stopSoundSource) {
                stopSound = kyberCrystalObject.module.stopSoundAsset;
                stopSoundSource.volume = kyberCrystalObject.module.stopSoundVolume;
                stopSoundSource.pitch = kyberCrystalObject.module.stopSoundPitch * audioPitch;
            }

            if (!string.IsNullOrEmpty(whooshRef)) {
                var fxData = kyberCrystalObject.module.whoosh;
                fxData.globalMaxPitch *= audioPitch;
                fxData.globalMinPitch *= audioPitch;
                whooshBlade.Load(fxData, whooshBlade.trigger, whooshBlade.minVelocity, whooshBlade.maxVelocity, whooshBlade.stopOnSnap);
            }

            kyberCrystal = kyberCrystalObject.item.definition.itemId;
            var tempItem = kyberCrystalObject.GetComponent<Item>();
            if (tempItem.mainHandler) tempItem.mainHandler.TryRelease();
            tempItem.Despawn();
        }

        public void RemoveCrystal() {
            if (!string.IsNullOrEmpty(kyberCrystal)) {
                var kyberCrystalData = Catalog.GetData<ItemPhysic>(kyberCrystal, true);
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
                if (playSound && startSound && startSoundSource) {
                    startSoundSource.PlayOneShot(startSound.PickAudioClip());
                }
                if (idleSound && idleSoundSource) {
                    idleSoundSource.clip = idleSound.PickAudioClip();
                    idleSoundSource.Play();
                }
                SetComponentState(true);
            }
        }

        public void TurnOff(bool playSound = true) {
            if (!string.IsNullOrEmpty(kyberCrystal)) {
                isActive = false;
                if (playSound && stopSound && stopSoundSource) {
                    stopSoundSource.PlayOneShot(stopSound.PickAudioClip());
                }
            }
        }

        public void UpdateBladeDirection() {
            extendDelta = (!isActive | currentLength > maxLength) ? -Mathf.Abs(extendDelta) : Mathf.Abs(extendDelta);
        }

        public void UpdateSize() {
            // if blade is currently longer than expected (going from long to short blade phase)
            if (isActive && currentLength > maxLength) {
                UpdateBladeDirection();
                currentLength = Mathf.Clamp(currentLength + (extendDelta * Time.deltaTime), maxLength, currentLength);
                saberBodyTrans.localScale = new Vector3(saberBodyTrans.localScale.x, saberBodyTrans.localScale.y, currentLength);
                if (trail) trail.height = currentLength * 10 * saberBodyTrans.parent.localScale.z;
                return;
            }

            // if blade still extending or retracting
            if ((isActive && (currentLength < maxLength || currentLength > maxLength)) || (!isActive && currentLength > minLength)) {
                UpdateBladeDirection();
                currentLength = Mathf.Clamp(currentLength + (extendDelta * Time.deltaTime), minLength, maxLength);
                saberBodyTrans.localScale = new Vector3(saberBodyTrans.localScale.x, saberBodyTrans.localScale.y, currentLength);
                if (trail) trail.height = currentLength * 10 * saberBodyTrans.parent.localScale.z;
                return;
            }
        }

        /* 
         * The following Lightsaber Trail code has been adapted from "PocketRPG weapon trail"
         * Source: https://www.assetstore.unity3d.com/en/#!/content/2458
         */
        public class TronTrailSection {
            public Vector3 point;
            public Vector3 upDir;
            public float time;
            public TronTrailSection() { }
            public TronTrailSection(Vector3 p, float t) {
                point = p;
                time = t;
            }
        }

        public class LightsaberTrail : MonoBehaviour {
            public float height = 0.9f;
            public float time = TORGlobalSettings.SaberTrailDuration;
            public float desiredTime = TORGlobalSettings.SaberTrailDuration;
            public float minVelocity = TORGlobalSettings.SaberTrailMinVelocity;
            public float timeTransitionSpeed = 5f;
            public Color startColor = Color.white;
            public Color endColor = new Color(1, 1, 1, 0);

            Vector3 position;
            Vector3 lastRotation;
            float now;
            TronTrailSection currentSection;
            Matrix4x4 localSpaceTransform;

            Transform trans;
            Mesh mesh;
            Vector3[] vertices;
            Color[] colors;
            Vector2[] uv;

            public List<TronTrailSection> sections = new List<TronTrailSection>();

            void Awake() {
                MeshFilter meshF = GetComponent<MeshFilter>();
                mesh = meshF.mesh;
                trans = transform;
            }

            void FixedUpdate() {
                Iterate(Time.time);
                UpdateTrail(Time.time, Time.deltaTime);
            }

            public void Iterate(float itterateTime) { // ** call everytime you sample animation **
                position = trans.position;
                now = itterateTime;

                var velocity = (lastRotation - trans.rotation.eulerAngles).sqrMagnitude;
                lastRotation = trans.rotation.eulerAngles;

                if (sections.Count == 0 || velocity > minVelocity) {
                    TronTrailSection section = new TronTrailSection();
                    section.point = position;
                    section.upDir = trans.TransformDirection(Vector3.up);

                    section.time = now;
                    sections.Insert(0, section);
                }
            }

            public void UpdateTrail(float currentTime, float deltaTime) {
                mesh.Clear();

                while (sections.Count > 0 && currentTime > sections[sections.Count - 1].time + time) {
                    sections.RemoveAt(sections.Count - 1);
                }

                if (sections.Count < 2) return;

                vertices = new Vector3[sections.Count * 2];
                colors = new Color[sections.Count * 2];
                uv = new Vector2[sections.Count * 2];

                currentSection = sections[0];
                localSpaceTransform = trans.worldToLocalMatrix;

                for (var i = 0; i < sections.Count; i++) {
                    currentSection = sections[i];
                    float u = 0.0f;
                    if (i != 0) u = Mathf.Clamp01((currentTime - currentSection.time) / time);

                    Vector3 upDir = currentSection.upDir;

                    vertices[i * 2 + 0] = localSpaceTransform.MultiplyPoint(currentSection.point);
                    vertices[i * 2 + 1] = localSpaceTransform.MultiplyPoint(currentSection.point + upDir * height);

                    uv[i * 2 + 0] = new Vector2(u, 0);
                    uv[i * 2 + 1] = new Vector2(u, 1);

                    Color interpolatedColor = Color.Lerp(startColor, endColor, (u - 0.3f) * 10f);
                    colors[i * 2 + 0] = interpolatedColor;
                    colors[i * 2 + 1] = interpolatedColor;
                }

                int[] triangles = new int[(sections.Count - 1) * 2 * 3];
                for (int i = 0; i < triangles.Length / 6; i++) {
                    triangles[i * 6 + 0] = i * 2;
                    triangles[i * 6 + 1] = i * 2 + 1;
                    triangles[i * 6 + 2] = i * 2 + 2;

                    triangles[i * 6 + 3] = i * 2 + 2;
                    triangles[i * 6 + 4] = i * 2 + 1;
                    triangles[i * 6 + 5] = i * 2 + 3;
                }

                mesh.vertices = vertices;
                mesh.colors = colors;
                mesh.uv = uv;
                mesh.triangles = triangles;

                if (time > desiredTime) {
                    time -= deltaTime * timeTransitionSpeed;
                    if (time <= desiredTime) time = desiredTime;
                } else if (time < desiredTime) {
                    time += deltaTime * timeTransitionSpeed;
                    if (time >= desiredTime) time = desiredTime;
                }
            }
        }
    }

    internal class LightsaberNPCAnimator : MonoBehaviour {
        public Creature creature;
        BrainHuman brain;
        bool originalRecoilOnParry;

        public void SetCreature(Creature newCreature) {
            creature = newCreature;
            if (creature.brainId == "ForceSensitive") {
                if (creature.animator) {
                    creature.animator.speed *= TORGlobalSettings.SaberNPCAttackSpeed;
                }
                brain = (BrainHuman)creature.brain;
                originalRecoilOnParry = brain.meleeRecoilOnParry;
                brain.meleeRecoilOnParry = TORGlobalSettings.SaberNPCRecoilOnParry;
            }
            creature.health.OnKillEvent += OnKillEvent;
        }

        void OnKillEvent(ref CollisionStruct collisionStruct) {
            creature.health.OnKillEvent -= OnKillEvent;
            Destroy(this);
        }

        protected void OnDestroy() {
            if (creature && brain != null) {
                if (creature.animator) {
                    creature.animator.speed /= TORGlobalSettings.SaberNPCAttackSpeed;
                }
                brain.meleeRecoilOnParry = originalRecoilOnParry;
            }
        }
    }
}