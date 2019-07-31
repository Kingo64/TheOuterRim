﻿using UnityEngine;
using BS;
using System.Linq;

namespace TOR {
    public class ItemBlasterBolt : MonoBehaviour {
        protected Item item;
        protected ItemModuleBlasterBolt module;

        bool markForDeletion;
        bool destroyNextTick;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBlasterBolt>();
            item.OnCollisionEvent += CollisionHandler;
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
        }
    }
}