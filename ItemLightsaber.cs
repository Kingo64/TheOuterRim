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

        bool thrown;
        PlayerHand playerHand;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLightsaber>();
            body = item.GetComponent<Rigidbody>();

            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnTeleUnGrabEvent += OnTeleUngrabEvent;
            item.OnHeldActionEvent += OnHeldAction;

            blades = module.lightsaberBlades.Select(Instantiate).ToArray();

            foreach (var blade in blades) {
                blade.Initialise(item);
                blade.extendDelta = -(blade.maxLength / module.ignitionDuration);
            }
            TurnOff(false);
        }

        public void ExecuteAction(string action, Interactor interactor = null) {
            if (action == "toggleIgnition") ToggleLightsaber(interactor);
            else if (action == "turnOn") TurnOn();
            else if (action == "turnOff") TurnOff();
        }

        void ToggleLightsaber(Interactor interactor = null) {
            if (isActive) TurnOff();
            else TurnOn();
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
        }

        void TurnOn(bool playSound = true) {
            isActive = true;
            ResetCollisions();

            foreach (var blade in blades) {
                blade.TurnOn(playSound);
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
                blade.TurnOff(playSound);
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
                thrown = true;
            }
        }

        public void OnTeleUngrabEvent(Handle handle, Telekinesis teleGrabber) {
            ResetCollisions();
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
            if (action == Interactable.Action.UseStart && isHolding) {
                ExecuteAction(module.primaryGripPrimaryAction, interactor);
            } else if (action == Interactable.Action.AlternateUseStart && isHolding) {
                ExecuteAction(module.primaryGripSecondaryAction, interactor);
            }
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

            if (playerHand && thrown && PlayerControl.GetHand(playerHand.side).gripPressed && !item.isGripped && !item.isTeleGrabbed) {
                // forget hand if hand is currently holding something
                if (playerHand.bodyHand.interactor.grabbedHandle) playerHand = null;
                else ReturnSaber();
                
            }

            if (isHolding && !item.isTeleGrabbed) {
                thrown = false;
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

        // custom reference strings
        public string collisionRef;
        public string saberBodyRef;
        public string saberTipGlowRef;
        public string saberGlowRef;
        public string saberParticlesRef;
        public string startSoundsRef;
        public string stopSoundsRef;
        public string whooshRef;
        
        // primary objects for Lightsaber to interact with
        public AudioSource idleSound;
        public MeshRenderer saberBody;
        public MeshRenderer saberGlow;
        public Light saberGlowLight;
        public Light saberTipGlow;
        public ParticleSystem saberParticles;
        public Whoosh whooshBlade;
        public Collider collisionBlade;
        protected AudioSource[] startSounds;
        protected AudioSource[] stopSounds;

        // internal properties
        public Item parent;
        public float currentLength;
        public float minLength;
        public float maxLength;
        public float extendDelta;
        public bool isActive;

        public void Initialise(Item parent) {
            this.parent = parent;
            if (!string.IsNullOrEmpty(collisionRef)) {
                collisionBlade = parent.definition.GetCustomReference(collisionRef).GetComponent<Collider>();
                collisionBlade.enabled = true;
                collisionBlade.enabled = isActive;
            }
            if (!string.IsNullOrEmpty(startSoundsRef)) startSounds = parent.definition.GetCustomReference(startSoundsRef).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(stopSoundsRef)) stopSounds = parent.definition.GetCustomReference(stopSoundsRef).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(saberBodyRef)) saberBody = parent.definition.GetCustomReference(saberBodyRef).GetComponent<MeshRenderer>();
            if (!string.IsNullOrEmpty(saberBodyRef)) idleSound = parent.definition.GetCustomReference(saberBodyRef).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(saberTipGlowRef)) saberTipGlow = parent.definition.GetCustomReference(saberTipGlowRef).GetComponent<Light>();
            if (!string.IsNullOrEmpty(saberGlowRef)) saberGlow = parent.definition.GetCustomReference(saberGlowRef).GetComponent<MeshRenderer>();
            if (!string.IsNullOrEmpty(saberGlowRef)) saberGlowLight = parent.definition.GetCustomReference(saberGlowRef).GetComponent<Light>();
            if (!string.IsNullOrEmpty(saberParticlesRef)) saberParticles = parent.definition.GetCustomReference(saberParticlesRef).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(whooshRef)) whooshBlade = parent.definition.GetCustomReference(whooshRef).GetComponent<Whoosh>();

            SetComponentState(false);
            maxLength = (bladeLength > 0f) ? bladeLength / 10 : saberBody.transform.localScale.z;

            // setup audio sources
            Utils.ApplyStandardMixer(new AudioSource[] { idleSound });
            Utils.ApplyStandardMixer(startSounds);
            Utils.ApplyStandardMixer(stopSounds);
        }

        public void SetComponentState(bool state) {
            if (whooshBlade != null) whooshBlade.enabled = state;
            if (saberBody != null) saberBody.enabled = state;
            if (saberGlow != null) saberGlow.enabled = state;
            if (saberGlowLight != null) saberGlowLight.enabled = state;
            if (saberTipGlow != null) saberTipGlow.enabled = state;
            if (saberParticles !=null) {
                if (state) saberParticles.Play();
                else {
                    saberParticles.Stop();
                    saberParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); // destroy leftover beam particles
                }
            }
        }

        public void TurnOn(bool playSound = true) {
            isActive = true;
            if (playSound) Utils.PlayRandomSound(startSounds);
            idleSound.Play();
            SetComponentState(true);
            extendDelta = Mathf.Abs(extendDelta);
        }

        public void TurnOff(bool playSound = true) {
            isActive = false;
            if (playSound) Utils.PlayRandomSound(stopSounds);
            extendDelta = -Mathf.Abs(extendDelta);
        }

        public void UpdateSize() {
            // if blade still extending or retracting
            if ((isActive && currentLength < maxLength) || (!isActive && currentLength > minLength)) {
                currentLength = Mathf.Clamp(currentLength + (extendDelta * Time.deltaTime), minLength, maxLength);
                saberBody.transform.localScale = new Vector3(saberBody.transform.localScale.x, saberBody.transform.localScale.y, currentLength);
            }
        }
    }
}