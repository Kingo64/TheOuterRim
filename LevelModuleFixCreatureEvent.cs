using ThunderRoad;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace TOR {
    public class LevelModuleFixCreatureEvent : LevelModule {
        GameObject creatureObserver;
        public float checkInterval = 0.5f;

        public override void OnLevelLoaded(LevelDefinition levelDefinition) {
            creatureObserver = new GameObject();
            creatureObserver.AddComponent<CreatureObserver>().loopDelay = new WaitForSeconds(checkInterval);
            initialized = true;
        }

        public override void OnLevelUnloaded(LevelDefinition levelDefinition) {
            creatureObserver = null;
            initialized = false;
        }

    }

    // DELETE THIS WHEN THE EVENT LISTENER IS FIXED IN U8.4
    public class CreatureObserver : MonoBehaviour {
        Coroutine observer;
        List<Creature> creatures;
        public WaitForSeconds loopDelay;

        void Awake() {
            creatures = new List<Creature>();
            observer = StartCoroutine(Observe());
        }

        void Destroy() {
            StopAllCoroutines();
        }

        IEnumerator Observe() {
            while (true) {
                yield return loopDelay;
                var toAdd = Creature.list.Except(creatures);
                if (toAdd.Any()) {
                    var player = Player.local?.body;
                    if (player != null) {
                        foreach (Creature creature in toAdd) {
                            if (creature != Creature.player && !creature.body.gameObject.GetComponent<CreatureZoneController>()) {
                                creature.body.gameObject.AddComponent<CreatureZoneController>();
                            }
                        }
                        creatures = Creature.list.ToList();
                    }
                }
            }
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
            // trigger.AddComponent<Rigidbody>().isKinematic = true;
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
