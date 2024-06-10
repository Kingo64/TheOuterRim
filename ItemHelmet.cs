using UnityEngine;
using ThunderRoad;
using System.Linq;

namespace TOR {
    public class ItemHelmet : ThunderBehaviour {
        protected Item item;
        protected ItemModuleHelmet module;

        public AudioSource lightSound;
        public AudioSource playSound;
        public AudioSource toggleSound;
        public Light light1;
        public Light light2;
        public ParticleSystem light1Sprite;
        public ParticleSystem light2Sprite;
        public MeshRenderer[] primaryModel;
        public MeshRenderer[] secondaryModel;

        NoiseManager.Noise toggleNoise;

        int modelState;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleHelmet>();

            item.OnHeldActionEvent += OnHeldAction;

            if (!string.IsNullOrEmpty(module.lightSoundID)) lightSound = item.GetCustomReference(module.lightSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.playSoundID)) playSound = item.GetCustomReference(module.playSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.toggleSoundID)) toggleSound = item.GetCustomReference(module.toggleSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.light1ID)) light1 = item.GetCustomReference(module.light1ID).GetComponent<Light>();
            if (!string.IsNullOrEmpty(module.light2ID)) light2 = item.GetCustomReference(module.light2ID).GetComponent<Light>();
            if (!string.IsNullOrEmpty(module.light1ID)) light1Sprite = item.GetCustomReference(module.light1ID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.light2ID)) light2Sprite = item.GetCustomReference(module.light2ID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.primaryModelID)) primaryModel = item.GetCustomReference(module.primaryModelID).GetComponentsInChildren<MeshRenderer>();
            if (!string.IsNullOrEmpty(module.secondaryModelID)) secondaryModel = item.GetCustomReference(module.secondaryModelID).GetComponentsInChildren<MeshRenderer>();
        }

        public void ExecuteAction(string action, RagdollHand interactor = null) {
            if (action == "playSound") {
                Utils.PlaySound(playSound, null, item);
                Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
            } else if (action == "cycleModels") {
                CycleModels(interactor);
            } else if (action == "toggleSound") {
                ToggleSound(interactor);
            } else if (action == "toggleLight") {
                ToggleLight(interactor);
            }
        }

        public void CycleModels(RagdollHand interactor = null) {
            modelState++;
            if (modelState > 2) modelState = 0;
            primaryModel.ToList().ForEach(m => m.enabled = (modelState == 0 || modelState == 1));
            secondaryModel.ToList().ForEach(m => m.enabled = (modelState == 0 || modelState == 2));
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
        }

        public void ToggleSound(RagdollHand interactor = null) {
            if (toggleSound.isPlaying) Utils.StopSoundLoop(toggleSound, ref toggleNoise);
            else toggleNoise = Utils.PlaySoundLoop(toggleSound, null, item);
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
        }

        public void ToggleLight(RagdollHand interactor = null) {
            if (lightSound) Utils.PlaySound(lightSound, null, item);
            if (light1) {
                light1.enabled = !light1.enabled;
                if (light1.enabled) light1Sprite.Play();
                else light1Sprite.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (light2) {
                light2.enabled = !light2.enabled;
                if (light2.enabled) light2Sprite.Play();
                else light2Sprite.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action) {
            if (action == Interactable.Action.UseStart) {
                if (interactor.side == Side.Right) ExecuteAction(module.rightGripPrimaryAction, interactor);
                else ExecuteAction(module.leftGripPrimaryAction, interactor);
            } else if (action == Interactable.Action.AlternateUseStart) {
                if (interactor.side == Side.Right) ExecuteAction(module.rightGripSecondaryAction, interactor);
                else ExecuteAction(module.leftGripSecondaryAction, interactor);
            }
        }
    }
}