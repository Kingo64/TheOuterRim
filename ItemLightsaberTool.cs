using UnityEngine;
using ThunderRoad;

namespace TOR {
    public class ItemLightsaberTool : MonoBehaviour {
        protected Item item;
        protected ItemModuleLightsaberTool module;

        public int currentMode;
        public string[] modes = {"TryEjectCrystal", "IncreaseBladeLength", "DecreaseBladeLength", "ResetBladeLength"};
        Material[] materials;
        MeshRenderer renderer;
        bool holdingLeft;
        bool holdingRight;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLightsaberTool>();

            for (int i = 0, l = item.collisionHandlers.Count; i < l; i++) {
                item.collisionHandlers[i].OnCollisionStartEvent += CollisionHandler;
            }

            item.OnHeldActionEvent += OnHeldAction;
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;

            renderer = item.GetCustomReference("Mesh").GetComponent<MeshRenderer>();
            materials = item.GetCustomReference("Modes").GetComponent<MeshRenderer>().materials;
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            holdingRight |= interactor.playerHand == Player.local.handRight;
            holdingLeft |= interactor.playerHand == Player.local.handLeft;
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            holdingRight &= interactor.playerHand != Player.local.handRight;
            holdingLeft &= interactor.playerHand == Player.local.handLeft;
        }

        void CollisionHandler(CollisionInstance collisionInstance) {
            try {
                if (collisionInstance.sourceColliderGroup.name == "CollisionLightsaberTool" && collisionInstance.targetColliderGroup.name == "CollisionHilt") {
                    if (currentMode == 0) {
                        collisionInstance.targetColliderGroup.transform.root.SendMessage(modes[currentMode], module.allowDisarm);
                    } else {
                        collisionInstance.targetColliderGroup.transform.root.SendMessage(modes[currentMode], new ItemLightsaber.AdjustBladeLength() {
                            allowDisarm = module.allowDisarm,
                            lengthChange = module.lengthChange * 0.1f
                        });
                    }
                    Utils.PlayHaptic(holdingLeft, holdingRight, Utils.HapticIntensity.Major);
                }
            }
            catch { }
        }

        public void ExecuteAction(string action, RagdollHand interactor = null) {
            if (action == "cycleMode") {
                CycleMode(interactor);
            }
        }

        public void CycleMode(RagdollHand interactor = null) {
            currentMode = (currentMode >= modes.Length - 1) ? 0 : currentMode + 1;
            renderer.material = materials[currentMode];
            if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action) {
            if (action == Interactable.Action.UseStart) {
                ExecuteAction(module.gripPrimaryAction, interactor);
            } else if (action == Interactable.Action.AlternateUseStart) {
                ExecuteAction(module.gripSecondaryAction, interactor);
            }
        }
    }
}