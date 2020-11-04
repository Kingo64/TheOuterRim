﻿using UnityEngine;
using UnityEngine.AI;
using ThunderRoad;
using System.Linq;
using System.Collections.Generic;

namespace TOR {
    public class ItemThermalDetonator : MonoBehaviour {
        protected Item item;
        protected ItemModuleThermalDetonator module;

        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
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

        float primaryControlHoldTime;
        float secondaryControlHoldTime;
        bool isOpen;
        bool isArmed;

        SpellTelekinesis telekinesis;
        float detonateTime;
        float beepTime;
        bool[] lastBeep = { false, false, false };
        System.Random rand = new System.Random();

        static Dictionary<HumanBodyBones, float> validBones = new Dictionary<HumanBodyBones, float> {
            { HumanBodyBones.Neck, 0.9f },
            { HumanBodyBones.LeftHand, 0.6f },
            { HumanBodyBones.RightHand, 0.6f },
            { HumanBodyBones.LeftLowerArm, 0.7f },
            { HumanBodyBones.RightLowerArm, 0.7f },
            { HumanBodyBones.LeftUpperArm, 0.8f },
            { HumanBodyBones.RightUpperArm, 0.8f },
            { HumanBodyBones.LeftFoot, 0.6f },
            { HumanBodyBones.RightFoot, 0.6f },
            { HumanBodyBones.LeftLowerLeg, 0.8f },
            { HumanBodyBones.RightLowerLeg, 0.8f }
        };

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleThermalDetonator>();

            item.OnHeldActionEvent += OnHeldAction;
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnTelekinesisGrabEvent += OnTelekinesisGrabEvent;
            item.OnTelekinesisReleaseEvent += OnTelekinesisReleaseEvent;

            animator = item.definition.GetCustomReference("Animator").GetComponent<Animator>();
            armedSound = item.definition.GetCustomReference("ArmedSound").GetComponent<AudioSource>();
            idleSound = item.definition.GetCustomReference("IdleSound").GetComponent<AudioSource>();
            beepSound1 = item.definition.GetCustomReference("BeepSound1").GetComponent<AudioSource>();
            beepSound2 = item.definition.GetCustomReference("BeepSound2").GetComponent<AudioSource>();
            beepSound3 = item.definition.GetCustomReference("BeepSound3").GetComponent<AudioSource>();
            explosionSound = item.definition.GetCustomReference("ExplosionSound").GetComponent<AudioSource>();
            explosionSound2 = item.definition.GetCustomReference("ExplosionSound2").GetComponent<AudioSource>();
            particles = item.definition.GetCustomReference("Explosion").GetComponent<ParticleSystem>();
            renderer = item.definition.GetCustomReference("Mesh").GetComponent<MeshRenderer>();

            obstacle = GetComponent<NavMeshObstacle>();
            obstacle.radius = module.radius;
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            if (isArmed) detonateTime = 0;
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool throwing) {
            if (isArmed) detonateTime = module.detonateTime;
        }

        public void OnTelekinesisReleaseEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = null;
        }

        public void OnTelekinesisGrabEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = teleGrabber;
        }

        public void ExecuteAction(string action, Interactor interactor = null) {
            if (action == "arm") {
                Arm(interactor);
            } else if (action == "toggleSlider") {
                ToggleSlider(interactor);
            }
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
            // If priamry hold action available
            if (!string.IsNullOrEmpty(module.gripPrimaryActionHold)) {
                // start primary control timer
                if (action == Interactable.Action.UseStart) {
                    primaryControlHoldTime = TORGlobalSettings.ControlsHoldDuration;
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
                    secondaryControlHoldTime = TORGlobalSettings.ControlsHoldDuration;
                } else if (action == Interactable.Action.AlternateUseStop) {
                    // if not held for long run standard action
                    if (secondaryControlHoldTime > 0 && secondaryControlHoldTime > (secondaryControlHoldTime / 2)) {
                        ExecuteAction(module.gripSecondaryAction, interactor);
                    }
                    secondaryControlHoldTime = 0;
                }
            } else if (action == Interactable.Action.AlternateUseStart) ExecuteAction(module.gripSecondaryAction, interactor);
        }

        public void Arm(Interactor interactor = null) {
            if (isOpen) {
                isArmed = !isArmed;
                if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
                if (isArmed) {
                    armedSound.Play();
                    idleSound.Stop();
                    beepTime = 1f;
                    obstacle.enabled = true;
                } else {
                    armedSound.Stop();
                    idleSound.Play();
                    beepTime = 0;
                    SetLights(new bool[] { false, false, false });
                    obstacle.enabled = false;
                }
            }
        }

        public void Detonate() {
            var pos = transform.position;
            var colliders = Physics.OverlapSphere(pos, module.radius);
            var damage = new CollisionStruct(new DamageStruct(DamageType.Energy, module.damage), null, null);
            var creatures = new FastList<Creature>();
            var layerMask = ~((1 << 10) | (1 << 13) | (1 << 26) | (1 << 27) | (1 << 31));
            var creatureMask = ~((1 << 13) | (1 << 26) | (1 << 27) | (1 << 31));

            armedSound.Stop();
            beepTime = 0;

            foreach (var hit in colliders) {
                var distance = Vector3.Distance(hit.transform.position, pos);
                var multiplier = (module.radius - distance) / module.radius;
                var rb = hit.GetComponent<Rigidbody>() ?? hit.GetComponentInParent<Rigidbody>();
                if (rb && (distance < 0.3 || !Physics.Linecast(pos, hit.transform.position, layerMask, QueryTriggerInteraction.Ignore))) {
                    rb.AddExplosionForce(module.impuse * multiplier, pos, module.radius, 1.0f);
                    var creature = hit.transform.GetComponentInParent<Creature>();
                    if (creature) {
                        if (creature != Player.local.body.creature) {
                            var rp = hit.GetComponent<RagdollPart>() ?? hit.GetComponentInParent<RagdollPart>();
                            if (rp && rp.partData != null && validBones.ContainsKey(rp.partData.bone) && validBones[rp.partData.bone] < multiplier) {
                                try {
                                    creature.ragdoll.Slice(rp.partData.bone);
                                }
                                catch { }
                            }
                        }
                        if (!creatures.Contains(creature) && !Physics.Linecast(pos, hit.transform.position, creatureMask, QueryTriggerInteraction.Ignore)) {
                            creatures.Add(creature);
                            damage.damageStruct.damage = module.damage * multiplier;
                            creature.health.Damage(ref damage);
                        }
                    }
                }
            }

            Utils.PlaySound(explosionSound, module.explosionSoundAsset);
            Utils.PlaySound(explosionSound2, module.explosionSoundAsset2);

            Utils.PlayParticleEffect(particles, true);
            renderer.enabled = false;
            item.enabled = false;
            item.Despawn(1.5f);
        }

        public void ToggleSlider(Interactor interactor = null) {
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
            isOpen = !isOpen;
            animator.SetTrigger(isOpen ? "open" : "close");
            animator.ResetTrigger(isOpen ? "close" : "open");
            if (isOpen) idleSound.Play();
            else {
                idleSound.Stop();
                armedSound.Stop();
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
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("Light1", beep[0] ? 1f : 0f);
            propBlock.SetFloat("Light2", beep[1] ? 1f : 0f);
            propBlock.SetFloat("Light3", beep[2] ? 1f : 0f);
            renderer.SetPropertyBlock(propBlock);

            if (beep[0]) beepSound1.Play();
            if (beep[1]) beepSound2.Play();
            if (beep[2]) beepSound3.Play();
        }

        void Update() {
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