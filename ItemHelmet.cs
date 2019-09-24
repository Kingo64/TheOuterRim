using UnityEngine;
using BS;
using System.Linq;

namespace TOR {
    public class ItemHelmet : MonoBehaviour {
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

        int modelState;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleHelmet>();

            item.OnHeldActionEvent += OnHeldAction;

            if (!string.IsNullOrEmpty(module.lightSoundID)) lightSound = item.definition.GetCustomReference(module.lightSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.playSoundID)) playSound = item.definition.GetCustomReference(module.playSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.toggleSoundID)) toggleSound = item.definition.GetCustomReference(module.toggleSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.light1ID)) light1 = item.definition.GetCustomReference(module.light1ID).GetComponent<Light>();
            if (!string.IsNullOrEmpty(module.light2ID)) light2 = item.definition.GetCustomReference(module.light2ID).GetComponent<Light>();
            if (!string.IsNullOrEmpty(module.light1ID)) light1Sprite = item.definition.GetCustomReference(module.light1ID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.light2ID)) light2Sprite = item.definition.GetCustomReference(module.light2ID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.primaryModelID)) primaryModel = item.definition.GetCustomReference(module.primaryModelID).GetComponentsInChildren<MeshRenderer>();
            if (!string.IsNullOrEmpty(module.secondaryModelID)) secondaryModel = item.definition.GetCustomReference(module.secondaryModelID).GetComponentsInChildren<MeshRenderer>();
        }

        public void ExecuteAction(string action) {
            if (action == "playSound") {
                playSound.Play();
            } else if (action == "cycleModels") {
                CycleModels();
            } else if (action == "toggleSound") {
                ToggleSound();
            } else if (action == "toggleLight") {
                ToggleLight();
            }
        }

        public void CycleModels() {
            modelState++;
            if (modelState > 2) modelState = 0;
            primaryModel.ToList().ForEach(m => m.enabled = (modelState == 0 || modelState == 1));
            secondaryModel.ToList().ForEach(m => m.enabled = (modelState == 0 || modelState == 2));
        }

        public void ToggleSound() {
            if (toggleSound.isPlaying) toggleSound.Stop();
            else toggleSound.Play();
        }

        public void ToggleLight() {
            light1.enabled = !light1.enabled;
            light2.enabled = !light2.enabled;
            if (lightSound) lightSound.Play();
            if (light1.enabled) light1Sprite.Play();
            else light1Sprite.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (light2.enabled) light2Sprite.Play();
            else light2Sprite.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
            if (action == Interactable.Action.UseStart) {
                if (interactor.side == Side.Right) ExecuteAction(module.rightGripPrimaryAction);
                else ExecuteAction(module.leftGripPrimaryAction);
            } else if (action == Interactable.Action.AlternateUseStart) {
                if (interactor.side == Side.Right) ExecuteAction(module.rightGripSecondaryAction);
                else ExecuteAction(module.leftGripSecondaryAction);
            }
        }
    }
}