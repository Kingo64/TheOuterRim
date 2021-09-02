using ThunderRoad;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace TOR {
    public class LevelModuleKamino : LevelModule {
        public bool rainEnabled = true;
        public float rainDensity = 1f;
        public bool rainPhysics = true;
        Transform rainTrans;
        RainController rainController;

        public override IEnumerator OnLoadCoroutine(Level levelDefinition) {
            rainTrans = levelDefinition.customReferences.Find(x => x.name == "Rain").transforms[0];

            if (rainEnabled) {
                rainController = rainTrans.gameObject.AddComponent<RainController>();
                var emitters = levelDefinition.customReferences.Find(x => x.name == "RainEmitters").transforms.Select(x => x.GetComponent<ParticleSystem>());
                foreach (var emitter in emitters) {
                    var emission = emitter.emission;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(emission.rateOverTime.constantMin * Mathf.Abs(rainDensity), emission.rateOverTime.constantMax * Mathf.Abs(rainDensity));

                    if (!rainPhysics) {
                        var col = emitter.collision;
                        col.enabled = false;
                    }
                }
            } else {
                var objs = Object.FindObjectsOfType(typeof(GameObject)).Select(g => g as GameObject).Where(g => g.name.Contains("Rain Surface"));
                foreach (var obj in objs) obj.SetActive(false);
                rainTrans.gameObject.SetActive(false);
            }
            levelDefinition.customReferences.Find(x => x.name == "Sky").transforms[0].gameObject.AddComponent<SkyController>();
            EventManager.onPlayerSpawn += OnPlayerSpawned;
            yield break;
        }

        void OnPlayerSpawned(Player player) {
            if (rainEnabled && rainController != null) {
                rainController.target = player.transform;
            }
        }

        public override void OnUnload(Level level) {
            EventManager.onPlayerSpawn -= OnPlayerSpawned;
        }

    }

    public class RainController : MonoBehaviour {
        public Transform target;

        void Update() {
            if (target) transform.position = target.position;
        }
    }
}
