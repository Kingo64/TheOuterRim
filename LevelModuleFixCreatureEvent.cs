using ThunderRoad;
using UnityEngine;
using System.Collections;

namespace TOR {
    // DELETE THIS WHEN THE OFFICIAL EVENT LISTENER IS FIXED
    public class LevelModuleFixCreatureEvent : LevelModule {
        public override IEnumerator OnLoadCoroutine() {
            EventManager.onCreatureSpawn += OnCreatureSpawned;
            EventManager.onCreatureDespawn += OnCreatureDespawn;
            yield break;
        }

        public override void OnUnload() {
            EventManager.onCreatureSpawn -= OnCreatureSpawned;
            EventManager.onCreatureDespawn -= OnCreatureDespawn;
        }

        void OnCreatureSpawned(Creature creature) {
            creature.gameObject.TryGetOrAddComponent(out CreatureZoneController _);
        }

        void OnCreatureDespawn(Creature creature, EventTime eventTime) {
            if (eventTime == EventTime.OnStart) {
                foreach (var c in creature.gameObject.GetComponents<CreatureZoneController>()) {
                    Object.Destroy(c);
                }
            }
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

        protected void OnDestroy() {
            if (trigger) Destroy(trigger);
            trigger= null;
        }
    }

    public class CreatureZoneTrigger : ThunderBehaviour {
        protected void OnTriggerEnter(Collider other) {
            if (other.gameObject.layer == Common.zoneLayer) {
                Zone component = other.GetComponent<Zone>();
                if (component) component.creatureEnterEvent.Invoke(this);
            }
        }

        protected void OnTriggerExit(Collider other) {
            if (other.gameObject.layer == Common.zoneLayer) {
                Zone component = other.GetComponent<Zone>();
                if (component) component.creatureExitEvent.Invoke(this);
            }
        }
    }
}
