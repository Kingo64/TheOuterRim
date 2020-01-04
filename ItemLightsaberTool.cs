using UnityEngine;
using BS;

namespace TOR {
    public class ItemLightsaberTool : MonoBehaviour {
        protected Item item;
        protected ItemModuleLightsaberTool module;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLightsaberTool>();

            item.OnCollisionEvent += CollisionHandler;
        }

        void CollisionHandler(ref CollisionStruct collisionInstance) {
            try {
                if (collisionInstance.sourceColliderGroup.name == "CollisionLightsaberTool" && collisionInstance.targetColliderGroup.name == "CollisionHilt") {
                    collisionInstance.targetColliderGroup.transform.root.SendMessage("TryEjectCrystal", module.allowDisarm);
                }
            }
            catch { }
        }
    }
}