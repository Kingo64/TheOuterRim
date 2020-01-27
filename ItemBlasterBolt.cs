using UnityEngine;
using BS;
using System.Linq;

namespace TOR {
    public class ItemBlasterBolt : MonoBehaviour {
        protected Item item;
        protected ItemModuleBlasterBolt module;

        bool markForDeletion;
        bool destroyNextTick;
        float despawnTime;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBlasterBolt>();
            despawnTime = module.despawnTime;
            item.OnCollisionEvent += CollisionHandler;

            if (module.colliderScale > 0 && !Mathf.Approximately(module.colliderScale, 1.0f)) {
                var colliders = item.GetComponentsInChildren<Collider>();
                for (int i = 0, l = colliders.Count(); i < l; i++) {
                    var colTrans = colliders[i].transform;
                    var colScale = colTrans.localScale;
                    colTrans.localScale = new Vector3(colScale.x * module.colliderScale, colScale.y * module.colliderScale, colScale.z * module.colliderScale);
                }
            }
        }

        void CollisionHandler(ref CollisionStruct collisionInstance) {
            Collider collider = collisionInstance.targetCollider;
            if (!module.deflectionMaterials.Any(material => collider.material.name == material + " (Instance)")) {
                GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                markForDeletion = true;
            }
        }

        protected void Update() {
            if (destroyNextTick) {
                item.Despawn();
            }
            destroyNextTick = markForDeletion;

            despawnTime -= Time.deltaTime;
            if (despawnTime <= 0) markForDeletion = true;
        }
    }
}