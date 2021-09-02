using ThunderRoad;
using UnityEngine;
using System.Collections;

namespace TOR {
    // DELETE THIS WHEN THE OFFICIAL EVENT LISTENER IS FIXED
    public class LevelModuleFixCreatureEvent : LevelModule {
        public override IEnumerator OnLoadCoroutine(Level level) {
            EventManager.onCreatureSpawn += OnCreatureSpawned;
            yield break;
        }

        public override void OnUnload(Level level) {
            EventManager.onCreatureSpawn -= OnCreatureSpawned;
        }

        void OnCreatureSpawned(Creature creature) {
            creature.gameObject.AddComponent<CreatureZoneController>();
        }
    }

    public class CreatureZoneController : MonoBehaviour {
        GameObject trigger;

        void Awake() {
            trigger = new GameObject("CreatureZoneController", typeof(CreatureZoneTrigger)) {
                layer = 27
            };
            trigger.transform.parent = transform;
            trigger.transform.position = transform.position;
            trigger.AddComponent<SphereCollider>().isTrigger = true;
        }
    }

    public class CreatureZoneTrigger : MonoBehaviour {
        void OnTriggerEnter(Collider other) {
            if (other.gameObject.layer == GameManager.zoneLayer) {
                Zone component = other.GetComponent<Zone>();
                component.creatureEnterEvent.Invoke(this);
            }
        }

        void OnTriggerExit(Collider other) {
            if (other.gameObject.layer == GameManager.zoneLayer) {
                other.GetComponent<Zone>().creatureExitEvent.Invoke(this);
            }
        }
    }
}
