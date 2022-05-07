using UnityEngine;
using ThunderRoad;
using System.Collections;

namespace TOR {
    public class ItemJetpack : MonoBehaviour {
        internal Item item;
        internal ItemModuleJetpack module;

        PlayerControl playerControl;
        InputXR.Controller controllerLeft;
        InputXR.Controller controllerRight;
        InputSteamVR steamController;
        Locomotion locomotion;
        Rigidbody playerRb;
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
            locomotion = Player.local.locomotion;
            playerRb = locomotion.rb;

            idleSoundLeft = item.GetCustomReference("idleSoundLeft").GetComponent<AudioSource>();
            idleSoundRight = item.GetCustomReference("idleSoundRight").GetComponent<AudioSource>();
            startSound = item.GetCustomReference("startSound").GetComponent<AudioSource>();
            stopSound = item.GetCustomReference("stopSound").GetComponent<AudioSource>();
            thrusterLeft = item.GetCustomReference("thrusterLeft").GetComponent<ParticleSystem>();
            thrusterRight = item.GetCustomReference("thrusterRight").GetComponent<ParticleSystem>();

            sparksVelocityLeft = thrusterLeft.transform.Find("Sparks").GetComponent<ParticleSystem>().velocityOverLifetime;
            sparksVelocityRight = thrusterRight.transform.Find("Sparks").GetComponent<ParticleSystem>().velocityOverLifetime;

            if (PlayerControl.loader == PlayerControl.Loader.Oculus) {
                controllerLeft = ((InputXR)PlayerControl.input).leftController;
                controllerRight = ((InputXR)PlayerControl.input).rightController;
            } else {
                steamController = (InputSteamVR)PlayerControl.input;
            }

            var temp = thrusterLeft.transform.Find("Sparks").GetComponent<ParticleSystem>().emission;
            temp.enabled = false;
            temp = thrusterRight.transform.Find("Sparks").GetComponent<ParticleSystem>().emission;
            temp.enabled = false;

            item.OnSnapEvent += OnSnapEvent;
            item.OnUnSnapEvent += OnUnSnapEvent;
        }

        public void OnSnapEvent(Holder holder) {
            equipped = holder?.creature == Player.local.creature;
            if (equipped) {
                locomotion = Player.local.locomotion;
                playerRb = locomotion.rb;
            }
        }

        public void OnUnSnapEvent(Holder holder) {
            TurnOff();
            equipped = false;
            locomotion = null;
            playerRb = null;
        }

        void ApplyThrust(float multiplier) {
            playerRb.AddForce(Vector3.up * module.thrust * multiplier, ForceMode.Acceleration);
        }

        readonly WaitForSeconds destabliseGrabbedDelay = new WaitForSeconds(0.1f);
        IEnumerator DestabliseGrabbedCoroutine() {
            Creature GetCreature(Side side) {
                return Player.local?.GetHand(side)?.ragdollHand?.grabbedHandle?.gameObject?.GetComponentInParent<Creature>();
            }

            while (true) {
                yield return destabliseGrabbedDelay;
                if (!equipped && destabiliseGrabbedCoroutine != null) StopCoroutine(destabiliseGrabbedCoroutine);
                var creature = GetCreature(Side.Right) ?? GetCreature(Side.Left);
                if (creature?.ragdoll && creature.ragdoll.state == Ragdoll.State.Standing) {
                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                }
            }
        }

        void TurnOn() {
            idleNoiseLeft = Utils.PlaySoundLoop(idleSoundLeft, module.idleSoundLeftAsset, item);
            idleNoiseRight = Utils.PlaySoundLoop(idleSoundRight, module.idleSoundRightAsset, item);
            Utils.PlaySound(startSound, module.startSoundAsset, item);
            stopSound.Stop();

            thrusterLeft.Play();
            thrusterRight.Play();

            isFlying = true;
            groundIgnoreTime = 0.1f;

            originalAirSpeed = locomotion.airSpeed;

            locomotion.airSpeed = module.airSpeed;
            playerRb.drag = module.drag;
            playerRb.useGravity = false;

            if (equipped) destabiliseGrabbedCoroutine = StartCoroutine(DestabliseGrabbedCoroutine());
        }

        void TurnOff() {
            if (idleSoundLeft.isPlaying || idleSoundRight.isPlaying) Utils.PlaySound(stopSound, module.stopSoundAsset, item);
            Utils.StopSoundLoop(idleSoundLeft, ref idleNoiseLeft);
            Utils.StopSoundLoop(idleSoundRight, ref idleNoiseRight);

            thrusterLeft.Stop();
            thrusterRight.Stop();

            isFlying = false;

            try {
                locomotion.airSpeed = originalAirSpeed;
                playerRb.drag = locomotion.isGrounded ? locomotion.groundDrag : locomotion.flyDrag;
                playerRb.useGravity = true;
            }
            catch { }

            if (destabiliseGrabbedCoroutine != null) StopCoroutine(destabiliseGrabbedCoroutine);
        }

        void FixedUpdate() {
            if (equipped && isFlying && currentThrust != 0f) {
                ApplyThrust(currentThrust);
                currentThrust = 0;
            }
        }

        void Update() {
            if (equipped) {
                if (isFlying && locomotion.isGrounded && groundIgnoreTime <= 0) {
                    TurnOff();
                } else {
                    if (!Pointer.GetActive() || !Pointer.GetActive().isPointingUI) {
                        var yMult = controllerRight != null ? controllerRight.thumbstick.GetValue().y : steamController.turnAction.axis.y;
                        if (Mathf.Abs(yMult) > playerControl.axisTurnDeadZone) {
                            if (!isFlying && yMult > module.startDeadzone) {
                                TurnOn();
                            }
                            if (isFlying) currentThrust = yMult;
                        }
                    }
                    if (controllerRight != null ? controllerRight.thumbstickClick.GetDown() : steamController.jumpAction.stateDown) {
                        if (isFlying) TurnOff();
                        else if (!locomotion.isGrounded) TurnOn();
                    }
                }
                if (isFlying) {
                    var xMult = controllerLeft != null ? GetVectorIntensity(controllerLeft.thumbstick.GetValue()) : GetVectorIntensity(steamController.moveAction.axis);
                    var yMult = controllerRight != null ? controllerRight.thumbstick.GetValue().y : steamController.turnAction.axis.y;
                    var throttleAmount = (xMult + ((yMult + 1) / 2)) / 2;
                    var volume = Mathf.Lerp(0.2f, 0.45f, throttleAmount);
                    var pitch = Mathf.Lerp(0.8f, 1.5f, throttleAmount);
                    var sparkVelocity = Mathf.Lerp(0.4f, 1.0f, throttleAmount);
                    idleSoundLeft.volume = volume;
                    idleSoundRight.volume = volume;
                    if (idleNoiseLeft != null) idleNoiseLeft.UpdateVolume(volume);
                    if (idleNoiseRight != null) idleNoiseRight.UpdateVolume(volume);
                    idleSoundLeft.pitch = pitch;
                    idleSoundRight.pitch = pitch;
                    sparksVelocityLeft.speedModifierMultiplier = sparkVelocity;
                    sparksVelocityRight.speedModifierMultiplier = sparkVelocity;
                }
            }
            if (groundIgnoreTime > 0) groundIgnoreTime -= Time.deltaTime;
        }

        static float GetVectorIntensity(Vector2 vector) {
            return Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }
    }
}