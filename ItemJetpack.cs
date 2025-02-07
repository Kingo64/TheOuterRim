using UnityEngine;
using ThunderRoad;
using System.Collections;
using ThunderRoad.Skill.SpellPower;

namespace TOR {
    public class ItemJetpack : ThunderBehaviour {
        public override ManagedLoops EnabledManagedLoops => ManagedLoops.FixedUpdate | ManagedLoops.Update;

        internal Item item;
        internal ItemModuleJetpack module;

        Creature creature;
        PlayerControl playerControl;
        WaterHandler waterHandler;
        Side locomotionSide;
        InputXR.Controller locomotionController;
        InputXR.Controller steeringController;
        InputSteamVR steamController;
        Locomotion locomotion;
        Rigidbody creatureRb;
        bool isFlying;
        bool equipped;
        float currentThrust;
        float groundIgnoreTime;

        Coroutine destabiliseGrabbedCoroutine;

        AudioSource idleSoundLeft;
        AudioSource idleSoundRight;
        AudioSource startSound;
        AudioSource stopSound;

        NoiseManager.Noise idleNoiseLeft;
        NoiseManager.Noise idleNoiseRight;

        ParticleSystem thrusterLeft;
        ParticleSystem thrusterRight;
        ParticleSystem.VelocityOverLifetimeModule sparksVelocityLeft;
        ParticleSystem.VelocityOverLifetimeModule sparksVelocityRight;

        float originalAirSpeed;

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleJetpack>();

            playerControl = PlayerControl.local;

            idleSoundLeft = item.GetCustomReference("idleSoundLeft").GetComponent<AudioSource>();
            idleSoundRight = item.GetCustomReference("idleSoundRight").GetComponent<AudioSource>();
            startSound = item.GetCustomReference("startSound").GetComponent<AudioSource>();
            stopSound = item.GetCustomReference("stopSound").GetComponent<AudioSource>();
            thrusterLeft = item.GetCustomReference("thrusterLeft").GetComponent<ParticleSystem>();
            thrusterRight = item.GetCustomReference("thrusterRight").GetComponent<ParticleSystem>();

            sparksVelocityLeft = thrusterLeft.transform.Find("Sparks").GetComponent<ParticleSystem>().velocityOverLifetime;
            sparksVelocityRight = thrusterRight.transform.Find("Sparks").GetComponent<ParticleSystem>().velocityOverLifetime;

            SetControllers();

            item.OnSnapEvent += OnSnapEvent;
            item.OnUnSnapEvent += OnUnSnapEvent;
        }

        protected void OnDestroy() {
            UnassignItem();
        }

        public void OnSnapEvent(Holder holder) {
            creature = holder?.creature;
            creature.OnKillEvent += delegate { UnassignItem(); };
            equipped = creature == Player.local.creature;
            locomotion = equipped ? Player.local.locomotion : creature.locomotion;
            originalAirSpeed = locomotion.horizontalAirSpeed;

            creatureRb = locomotion.physicBody.rigidBody;

            waterHandler = creature.waterHandler;
            waterHandler.OnWaterEnter += TurnOff;

            SpellPowerSlowTime.OnTimeScaleChangeEvent += OnTimeScaleChangeEvent; ;
        }

        private void OnTimeScaleChangeEvent(SpellPowerSlowTime spell, float scale) {
            if (isFlying && creature == Player.currentCreature) {
                locomotion.horizontalAirSpeed = module.airSpeed;
                creatureRb.drag = module.drag;
                creatureRb.useGravity = false;
            }
        }

        public void OnUnSnapEvent(Holder holder) {
            UnassignItem();
        }

        public void UnassignItem() {
            TurnOff();
            equipped = false;
            creature = null;
            if (waterHandler != null) {
                waterHandler.OnWaterEnter -= TurnOff;
                waterHandler = null;
            }
            locomotion = null;
            creatureRb = null;
        }

        public void SetControllers() {
            try {
                locomotionSide = playerControl.locomotionController;
                if (PlayerControl.loader == PlayerControl.Loader.Oculus) {
                    locomotionController = locomotionSide == Side.Left ? ((InputXR)PlayerControl.input).leftController : ((InputXR)PlayerControl.input).rightController;
                    steeringController = locomotionSide == Side.Left ? ((InputXR)PlayerControl.input).rightController : ((InputXR)PlayerControl.input).leftController;
                } else {
                    steamController = (InputSteamVR)PlayerControl.input;
                }
            } catch {
                Utils.LogError("Couldn't setup Jetpack as VR controllers not detected");
            }
        }

        void ApplyThrust(float multiplier) {
            creatureRb.AddForce(Vector3.up * module.thrust * multiplier, ForceMode.Acceleration);
        }

        IEnumerator DestabliseGrabbedCoroutine() {
            Creature GetCreature(Side side) {
                return Player.local?.GetHand(side)?.ragdollHand?.grabbedHandle?.gameObject?.GetComponentInParent<Creature>();
            }

            while (true) {
                yield return Utils.waitSeconds_01;
                if (!equipped && destabiliseGrabbedCoroutine != null) StopCoroutine(destabiliseGrabbedCoroutine);
                var creature = GetCreature(Side.Right) ?? GetCreature(Side.Left);
                if (creature?.ragdoll && creature.ragdoll.state == Ragdoll.State.Standing) {
                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                }
            }
        }

        public void TurnOn() {
            idleNoiseLeft = Utils.PlaySoundLoop(idleSoundLeft, module.idleSoundLeftAsset, item, Utils.NoiseLevel.LOUD);
            idleNoiseRight = Utils.PlaySoundLoop(idleSoundRight, module.idleSoundRightAsset, item, Utils.NoiseLevel.LOUD);
            Utils.PlaySound(startSound, module.startSoundAsset, item);
            stopSound.Stop();

            thrusterLeft.Play();
            thrusterRight.Play();

            isFlying = true;
            groundIgnoreTime = 0.1f;

            locomotion.horizontalAirSpeed = module.airSpeed;
            creatureRb.drag = module.drag;
            creatureRb.useGravity = false;

            if (equipped) destabiliseGrabbedCoroutine = StartCoroutine(DestabliseGrabbedCoroutine());
        }

        public void TurnOff() {
            if (idleSoundLeft && idleSoundRight) {
                if (idleSoundLeft.isPlaying || idleSoundRight.isPlaying) Utils.PlaySound(stopSound, module.stopSoundAsset, item);
                Utils.StopSoundLoop(idleSoundLeft, ref idleNoiseLeft);
                Utils.StopSoundLoop(idleSoundRight, ref idleNoiseRight);
            }

            thrusterLeft.Stop();
            thrusterRight.Stop();

            isFlying = false;

            try {
                locomotion.horizontalAirSpeed = originalAirSpeed;
                creatureRb.drag = locomotion.isGrounded ? locomotion.groundDrag : locomotion.flyDrag;
                creatureRb.useGravity = true;
            }
            catch { }

            if (destabiliseGrabbedCoroutine != null) StopCoroutine(destabiliseGrabbedCoroutine);
        }

        protected override void ManagedFixedUpdate() {
            if (equipped && isFlying && currentThrust != 0f) {
                ApplyThrust(currentThrust);
                currentThrust = 0;
            }
        }

        protected override void ManagedUpdate() {
            if (equipped) {
                if (locomotionController == null && steamController == null) return;
                if (locomotionController != null && playerControl.locomotionController != locomotionSide) SetControllers();
                if (isFlying && (locomotion.isGrounded && groundIgnoreTime <= 0 || waterHandler.inWater)) {
                    TurnOff();
                } else if (!waterHandler.inWater) {
                    if (!Pointer.GetActive() || !Pointer.GetActive().isPointingUI) {
                        var yMult = steeringController != null ? steeringController.thumbstick.GetValue().y : steamController.turnAction.axis.y;
                        if (Mathf.Abs(yMult) > playerControl.axisTurnDeadZone) {
                            if (!isFlying && yMult > module.startDeadzone && !GlobalSettings.JetpackJumpButtonOnly) {
                                TurnOn();
                            }
                            if (isFlying) currentThrust = yMult;
                        }
                    }
                    if (steeringController != null ? steeringController.thumbstickClick.GetDown() : steamController.jumpAction.stateDown) {
                        if (isFlying) TurnOff();
                        else if (!locomotion.isGrounded) TurnOn();
                    }
                }
                if (isFlying) {
                    var xMult = locomotionController != null ? GetVectorIntensity(locomotionController.thumbstick.GetValue()) : GetVectorIntensity(steamController.moveAction.axis);
                    var yMult = steeringController != null ? steeringController.thumbstick.GetValue().y : steamController.turnAction.axis.y;
                    var throttleAmount = (xMult + ((yMult + 1) / 2)) / 2;
                    var volume = Mathf.Lerp(0.2f, 0.45f, throttleAmount);
                    var pitch = Mathf.Lerp(0.8f, 1.5f, throttleAmount);
                    var sparkVelocity = Mathf.Lerp(0.4f, 1.0f, throttleAmount);
                    idleSoundLeft.volume = volume;
                    idleSoundRight.volume = volume;
                    idleSoundLeft.pitch = pitch;
                    idleSoundRight.pitch = pitch;
                    sparksVelocityLeft.speedModifierMultiplier = sparkVelocity;
                    sparksVelocityRight.speedModifierMultiplier = sparkVelocity;
                    locomotion.groundAngle = 0;
                }
            }
            if (groundIgnoreTime > 0) groundIgnoreTime -= Time.deltaTime;
        }

        static float GetVectorIntensity(Vector2 vector) {
            return Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }
    }
}