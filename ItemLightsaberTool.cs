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

            for (int i = 0, l = item.definition.collisionHandlers.Count; i < l; i++) {
                item.definition.collisionHandlers[i].OnCollisionStartEvent += CollisionHandler;
            }

            item.OnHeldActionEvent += OnHeldAction;
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;

            renderer = item.definition.GetCustomReference("Mesh").GetComponent<MeshRenderer>();
            materials = item.definition.GetCustomReference("Modes").GetComponent<MeshRenderer>().materials;
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            holdingRight |= interactor.playerHand == Player.local.handRight;
            holdingLeft |= interactor.playerHand == Player.local.handLeft;
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool throwing) {
            holdingRight &= interactor.playerHand != Player.local.handRight;
            holdingLeft &= interactor.playerHand == Player.local.handLeft;
        }

        void CollisionHandler(ref CollisionStruct collisionInstance) {
            try {
                if (collisionInstance.sourceColliderGroup.name == "CollisionLightsaberTool" && collisionInstance.targetColliderGroup.name == "CollisionHilt") {
                    if (currentMode == 0) {
                        collisionInstance.targetColliderGroup.transform.root.SendMessage(modes[currentMode], module.allowDisarm);
                    } else {
                        float lengthChange = module.lengthChange * 0.1f;
                        collisionInstance.targetColliderGroup.transform.root.SendMessage(modes[currentMode], new { module.allowDisarm, lengthChange });
                    }
                    Utils.PlayHaptic(holdingLeft, holdingRight, Utils.HapticIntensity.Major);
                }
            }
            catch { }
        }

        public void ExecuteAction(string action, Interactor interactor = null) {
            if (action == "cycleMode") {
                CycleMode(interactor);
            }
        }

        public void CycleMode(Interactor interactor = null) {
            currentMode = (currentMode >= modes.Length - 1) ? 0 : currentMode + 1;
            renderer.material = materials[currentMode];
            if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
            if (action == Interactable.Action.UseStart) {
                ExecuteAction(module.gripPrimaryAction, interactor);
            } else if (action == Interactable.Action.AlternateUseStart) {
                ExecuteAction(module.gripSecondaryAction, interactor);
            }
        }
    }
}