using UnityEngine;
using UnityEngine.AI;
using ThunderRoad;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TOR {
    public class ItemThermalDetonator : MonoBehaviour {
        protected Item item;
        protected ItemModuleThermalDetonator module;
        Renderer renderer;
        Animator animator;
        ParticleSystem particles;
        AudioSource armedSound;
        AudioSource idleSound;
        AudioSource beepSound1;
        AudioSource beepSound2;
        AudioSource beepSound3;
        AudioSource explosionSound;
        AudioSource explosionSound2;
        NavMeshObstacle obstacle;

        NoiseManager.Noise armedNoise;
        NoiseManager.Noise idleNoise;

        float primaryControlHoldTime;
        float secondaryControlHoldTime;
        public bool isOpen;
        public bool isArmed;
        public bool armedByPlayer;

        SpellTelekinesis telekinesis;
        float detonateTime;
        float beepTime;
        bool[] lastBeep = { false, false, false };
        readonly System.Random rand = new System.Random();

        MaterialPropertyBlock _propBlock;
        public MaterialPropertyBlock PropBlock {
            get {
                _propBlock = _propBlock ?? new MaterialPropertyBlock();
                return _propBlock;
            }
        }

        static readonly Dictionary<RagdollPart.Type, float> validParts = new Dictionary<RagdollPart.Type, float> {
            { RagdollPart.Type.Head, 0.9f },
            { RagdollPart.Type.LeftHand, 0.6f },
            { RagdollPart.Type.RightHand, 0.6f },
            { RagdollPart.Type.LeftArm, 0.7f },
            { RagdollPart.Type.RightArm, 0.8f },
            { RagdollPart.Type.LeftFoot, 0.6f },
            { RagdollPart.Type.RightFoot, 0.6f },
            { RagdollPart.Type.LeftLeg, 0.8f },
            { RagdollPart.Type.RightLeg, 0.8f }
        };

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleThermalDetonator>();

            item.OnHeldActionEvent += OnHeldAction;
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnTelekinesisGrabEvent += OnTelekinesisGrabEvent;
            item.OnTelekinesisReleaseEvent += OnTelekinesisReleaseEvent;

            animator = item.GetCustomReference("Animator").GetComponent<Animator>();
            armedSound = item.GetCustomReference("ArmedSound").GetComponent<AudioSource>();
            idleSound = item.GetCustomReference("IdleSound").GetComponent<AudioSource>();
            beepSound1 = item.GetCustomReference("BeepSound1").GetComponent<AudioSource>();
            beepSound2 = item.GetCustomReference("BeepSound2").GetComponent<AudioSource>();
            beepSound3 = item.GetCustomReference("BeepSound3").GetComponent<AudioSource>();
            explosionSound = item.GetCustomReference("ExplosionSound").GetComponent<AudioSource>();
            explosionSound2 = item.GetCustomReference("ExplosionSound2").GetComponent<AudioSource>();
            particles = item.GetCustomReference("Explosion").GetComponent<ParticleSystem>();
            renderer = item.GetCustomReference("Mesh").GetComponent<MeshRenderer>();

            obstacle = GetComponent<NavMeshObstacle>();
            obstacle.radius = module.radius;

            item.OnCullEvent += OnCullEvent;
        }

        private void OnCullEvent(bool culled) {
            if (culled && isArmed) gameObject.SetActive(true);
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            if (!interactor.playerHand) {
                if (!isOpen) ToggleSlider(interactor);
                if (!isArmed) Arm(interactor);
            }
            if (isArmed && ((armedByPlayer && interactor.playerHand) || (!armedByPlayer && !interactor.playerHand))) detonateTime = 0;
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            if (!interactor.playerHand && isOpen) {
                if (throwing) {
                    if (!isArmed) Arm(interactor);
                } else {
                    if (interactor.creature && !interactor.creature.isKilled) ToggleSlider(interactor);
                }
            }
            if (isArmed) {
                detonateTime = module.detonateTime;
                if (item.currentRoom) {
                    item.currentRoom.UnRegisterItem(item);
                    item.currentRoom = null;
                }
            }
        }

        public void OnTelekinesisReleaseEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = null;
        }

        public void OnTelekinesisGrabEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = teleGrabber;
        }

        public void ExecuteAction(string action, RagdollHand interactor = null) {
            if (action == "arm") {
                Arm(interactor);
            } else if (action == "toggleSlider") {
                ToggleSlider(interactor);
            }
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action) {
            // If primary hold action available
            if (!string.IsNullOrEmpty(module.gripPrimaryActionHold)) {
                // start primary control timer
                if (action == Interactable.Action.UseStart) {
                    primaryControlHoldTime = GlobalSettings.ControlsHoldDuration;
                } else if (action == Interactable.Action.UseStop) {
                    // if not held for long run standard action
                    if (primaryControlHoldTime > 0 && primaryControlHoldTime > (primaryControlHoldTime / 2)) {
                        ExecuteAction(module.gripPrimaryAction, interactor);
                    }
                    primaryControlHoldTime = 0;
                }
            } else if (action == Interactable.Action.UseStart) ExecuteAction(module.gripPrimaryAction, interactor);

            // If secondary hold action available
            if (!string.IsNullOrEmpty(module.gripSecondaryActionHold)) {
                // start secondary control timer
                if (action == Interactable.Action.AlternateUseStart) {
                    secondaryControlHoldTime = GlobalSettings.ControlsHoldDuration;
                } else if (action == Interactable.Action.AlternateUseStop) {
                    // if not held for long run standard action
                    if (secondaryControlHoldTime > 0 && secondaryControlHoldTime > (secondaryControlHoldTime / 2)) {
                        ExecuteAction(module.gripSecondaryAction, interactor);
                    }
                    secondaryControlHoldTime = 0;
                }
            } else if (action == Interactable.Action.AlternateUseStart) ExecuteAction(module.gripSecondaryAction, interactor);
        }

        public void Arm(RagdollHand interactor = null) {
            if (isOpen) {
                isArmed = !isArmed;
                if (interactor && interactor.playerHand) {
                    PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
                    armedByPlayer = true;
                } else armedByPlayer = false;
                if (isArmed) {
                    armedNoise = Utils.PlaySoundLoop(armedSound, null, item);
                    Utils.StopSoundLoop(idleSound, ref idleNoise);
                    beepTime = 1f;
                    obstacle.enabled = true;
                } else {
                    Utils.StopSoundLoop(armedSound, ref armedNoise);
                    idleNoise = Utils.PlaySoundLoop(idleSound, null, item);
                    beepTime = 0;
                    SetLights(new bool[] { false, false, false });
                    obstacle.enabled = false;
                }
            }
        }

        public void Detonate() {
            var pos = transform.position;
            var colliders = Physics.OverlapSphere(pos, module.radius);
            var creatures = new List<Creature>();
            var layerMask = ~((1 << 10) | (1 << 13) | (1 << 26) | (1 << 27) | (1 << 31));
            var creatureMask = ~((1 << 13) | (1 << 26) | (1 << 27) | (1 << 31));

            Utils.StopSoundLoop(armedSound, ref armedNoise);
            beepTime = 0;

            renderer.enabled = false;
            item.rb.isKinematic = true;

            foreach (var hit in colliders) {
                var distance = Vector3.Distance(hit.transform.position, pos);
                var multiplier = (module.radius - distance) / module.radius;
                var rb = hit.GetComponent<Rigidbody>() ?? hit.GetComponentInParent<Rigidbody>();
                if (rb && (distance < 0.3 || !Physics.Linecast(pos, hit.transform.position, layerMask, QueryTriggerInteraction.Ignore))) {
                    rb.AddExplosionForce(module.impuse * multiplier, pos, module.radius, 1.0f);
                    var creature = hit.transform.GetComponentInParent<Creature>();
                    if (creature) {
                        if (creature != Player.currentCreature) {
                            var rp = hit.GetComponent<RagdollPart>() ?? hit.GetComponentInParent<RagdollPart>();
                            if (rp && rp.sliceAllowed && validParts.ContainsKey(rp.type) && validParts[rp.type] < multiplier) {
                                try {
                                    rp.TrySlice();
                                }
                                catch { }
                            }
                        }
                        if (!creatures.Contains(creature) && !Physics.Linecast(pos, hit.transform.position, creatureMask, QueryTriggerInteraction.Ignore)) {
                            creatures.Add(creature);
                            var damage = new CollisionInstance(new DamageStruct(DamageType.Energy, module.damage), null, null);
                            damage.damageStruct.damage = module.damage * multiplier;
                            try {
                                creature.Damage(damage);
                            }
                            catch (NullReferenceException) {
                                // BrainModuleHitReaction seems to randomly fail when damaging NPCs
                            }
                        }
                    }
                }
            }

            var handler = item?.lastHandler?.creature;
            Utils.PlayParticleEffect(particles, true);

            Utils.PlaySound(explosionSound, module.explosionSoundAsset, handler);
            Utils.PlaySound(explosionSound2, module.explosionSoundAsset2, handler);

            item.Despawn(3f);
        }

        public void ToggleSlider(RagdollHand interactor = null) {
            if (interactor && interactor.playerHand) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
            isOpen = !isOpen;
            animator.SetTrigger(isOpen ? "open" : "close");
            animator.ResetTrigger(isOpen ? "close" : "open");
            if (isOpen) idleNoise = Utils.PlaySoundLoop(idleSound, null, item);
            else {
                Utils.StopSoundLoop(idleSound, ref idleNoise);
                Utils.StopSoundLoop(armedSound, ref armedNoise);
                detonateTime = 0;
                beepTime = 0;
                isArmed = false;
                SetLights(new bool[] { false, false, false});
            }
        }

        bool[] RandomBeep() {
            bool RandomBool() {
                return rand.Next() > (int.MaxValue / 2);
            }
            return new bool[] { RandomBool(), RandomBool(), RandomBool() };
        }

        void SetLights(bool[] beep) {
            renderer.GetPropertyBlock(PropBlock);
            PropBlock.SetFloat("Light1", beep[0] ? 1f : 0f);
            PropBlock.SetFloat("Light2", beep[1] ? 1f : 0f);
            PropBlock.SetFloat("Light3", beep[2] ? 1f : 0f);
            renderer.SetPropertyBlock(PropBlock);

            var handler = item?.lastHandler?.creature;
            if (beep[0]) Utils.PlaySound(beepSound1, null, handler);
            if (beep[1]) Utils.PlaySound(beepSound2, null, handler);
            if (beep[2]) Utils.PlaySound(beepSound3, null, handler);
            lastBeep = beep;
        }

        protected void Update() {
            if (primaryControlHoldTime > 0) {
                primaryControlHoldTime -= Time.deltaTime;
                if (primaryControlHoldTime <= 0) ExecuteAction(module.gripPrimaryActionHold);
            }
            if (secondaryControlHoldTime > 0) {
                secondaryControlHoldTime -= Time.deltaTime;
                if (secondaryControlHoldTime <= 0) ExecuteAction(module.gripSecondaryActionHold);
            }

            if (telekinesis != null && telekinesis.spinMode) {
                Arm();
                if (isArmed) detonateTime = module.detonateTime;
                telekinesis.SetSpinMode(false);
            }

            if (beepTime > 0) {
                beepTime -= Time.deltaTime;
                if (beepTime <= 0 && isArmed) {
                    beepTime = 1f;
                    bool[] beep = RandomBeep();
                    while (beep.SequenceEqual(lastBeep) || beep.All(x => x == false)) {
                        beep = RandomBeep();
                    }
                    SetLights(beep);
                }
            }

            if (detonateTime > 0) {
                detonateTime -= Time.deltaTime;
                if (detonateTime <= 0 && isArmed) {
                    Detonate();
                }
            }
        }
    }
}