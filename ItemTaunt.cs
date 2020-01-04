using BS;
using UnityEngine;

namespace TOR {
    public class ItemTaunt : MonoBehaviour {
        protected Item item;
        protected ItemModuleTaunt module;

        protected Handle grip;
        protected AudioSource[] tauntSounds;
        protected AudioSource[] tauntSounds2;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleTaunt>();

            // setup item events
            item.OnHeldActionEvent += OnHeldAction;
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;

            if (!string.IsNullOrEmpty(module.gripID)) grip = item.definition.GetCustomReference(module.gripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.tauntID)) tauntSounds = item.definition.GetCustomReference(module.tauntID).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.taunt2ID)) tauntSounds2 = item.definition.GetCustomReference(module.taunt2ID).GetComponents<AudioSource>();
        }

        public void ExecuteAction(string action) {
            if (action == "playTaunt") {
                PlayTaunt();
            } else if (action == "playTaunt2") {
                PlayTaunt2();
            }
        }

        public void PlayTaunt() {
            if (tauntSounds != null) Utils.PlayRandomSound(tauntSounds);
        }

        public void PlayTaunt2() {
            if (tauntSounds2 != null) Utils.PlayRandomSound(tauntSounds);
            else PlayTaunt();
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            if (interactor.playerHand != Player.local.handRight && interactor.playerHand != Player.local.handLeft && Random.value < module.aiTauntChance)
                PlayTaunt();
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool thrown) {
            if (interactor.playerHand != Player.local.handRight && interactor.playerHand != Player.local.handLeft && Random.value < module.aiTauntChance)
                PlayTaunt2();
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