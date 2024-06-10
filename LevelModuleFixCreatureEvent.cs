using ThunderRoad;
using UnityEngine;
using System.Collections;

namespace TOR {
    // DELETE THIS WHEN THE OFFICIAL EVENT LISTENER IS FIXED
    public class LevelModuleFixCreatureEvent : LevelModule {
        public override IEnumerator OnLoadCoroutine() {
            EventManager.onCreatureSpawn += OnCreatureSpawned;
            yield break;
        }

        public override void OnUnload() {
            EventManager.onCreatureSpawn -= OnCreatureSpawned;
        }

        void OnCreatureSpawned(Creature creature) {
            creature.gameObject.AddComponent<CreatureZoneController>();
        }
    }

    public class CreatureZoneController : ThunderBehaviour {
        GameObject trigger;

        protected void Awake() {
            trigger = new GameObject("CreatureZoneController", typeof(CreatureZoneTrigger)) {
                layer = 27
            };
            trigger.transform.parent = transform;
            trigger.transform.position = transform.position;
            trigger.AddComponent<SphereCollider>().isTrigger = true;
        }
    }

    public class CreatureZoneTrigger : ThunderBehaviour {
        protected void OnTriggerEnter(Collider other) {
            if (other.gameObject.layer == Common.zoneLayer) {
                Zone component = other.GetComponent<Zone>();
                component.creatureEnterEvent.Invoke(this);
            }
        }

        protected void OnTriggerExit(Collider other) {
            if (other.gameObject.layer == Common.zoneLayer) {
                other.GetComponent<Zone>().creatureExitEvent.Invoke(this);
            }
        }
    }
}
