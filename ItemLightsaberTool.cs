using UnityEngine;
using BS;

namespace TOR {
    public class ItemLightsaberTool : MonoBehaviour {
        protected Item item;
        protected ItemModuleLightsaberTool module;

        public int currentMode;
        public string[] modes = {"TryEjectCrystal", "IncreaseBladeLength", "DecreaseBladeLength", "ResetBladeLength"};
        Material[] materials;
        MeshRenderer renderer;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLightsaberTool>();

            item.OnCollisionEvent += CollisionHandler;
            item.OnHeldActionEvent += OnHeldAction;

            renderer = item.definition.GetCustomReference("Mesh").GetComponent<MeshRenderer>();
            materials = item.definition.GetCustomReference("Modes").GetComponent<MeshRenderer>().materials;
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
                }
            }
            catch { }
        }

        public void ExecuteAction(string action) {
            if (action == "cycleMode") {
                CycleMode();
            }
        }

        public void CycleMode() {
            currentMode = (currentMode >= modes.Length - 1) ? 0 : currentMode + 1;
            renderer.material = materials[currentMode];
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
            if (action == Interactable.Action.UseStart) {
                ExecuteAction(module.gripPrimaryAction);
            } else if (action == Interactable.Action.AlternateUseStart) {
                ExecuteAction(module.gripSecondaryAction);
            }
        }
    }
}