using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemTaunt : MonoBehaviour {
        protected Item item;
        protected ItemModuleTaunt module;

        protected Handle grip;
        protected AudioSource tauntSource;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleTaunt>();

            // setup item events
            item.OnHeldActionEvent += OnHeldAction;
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;

            if (!string.IsNullOrEmpty(module.gripID)) grip = item.definition.GetCustomReference(module.gripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.tauntID)) tauntSource = item.definition.GetCustomReference(module.tauntID).GetComponent<AudioSource>();

            if (grip == null) grip = item.mainHandleRight;
        }

        public void ExecuteAction(string action) {
            if (action == "playTaunt") {
                PlayTaunt(module.tauntAsset);
            } else if (action == "playTaunt2") {
                PlayTaunt(module.tauntDropAsset);
            }
        }

        public void PlayTaunt(AudioContainer audioContainer) {
            if (tauntSource != null && audioContainer != null) {
                tauntSource.clip = audioContainer.PickAudioClip();
                tauntSource.Play();
            }
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            if (interactor.playerHand != Player.local.handRight && interactor.playerHand != Player.local.handLeft && Random.value < module.aiTauntChance)
                PlayTaunt(module.tauntAsset);
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool thrown) {
            if (interactor.playerHand != Player.local.handRight && interactor.playerHand != Player.local.handLeft && Random.value < module.aiTauntChance)
                PlayTaunt(module.tauntDropAsset);
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
            if (handle == grip) {
                if (action == Interactable.Action.UseStart) {
                    ExecuteAction(module.gripPrimaryAction);
                } else if (action == Interactable.Action.AlternateUseStart) {
                    ExecuteAction(module.gripSecondaryAction);
                }
            }
        }
    }
}