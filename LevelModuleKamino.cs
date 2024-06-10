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

        public override IEnumerator OnLoadCoroutine() {
            rainTrans = level.customReferences.Find(x => x.name == "Rain").transforms[0];

            if (Level.current.options != null) {
                // Toggle Map Options seem to not support initialising with True values so we need to use the inverse
                if (Level.current.options.TryGetValue("rainEnabled", out string val)) rainEnabled = float.Parse(val) == 1;
                if (Level.current.options.TryGetValue("rainPhysics", out val)) rainPhysics = float.Parse(val) == 1;
                if (Level.current.options.TryGetValue("rainDensity", out val)) rainDensity = float.Parse(val) * 0.2f;
            }

            if (rainEnabled) {
                rainController = rainTrans.gameObject.AddComponent<RainController>();
                var emitters = level.customReferences.Find(x => x.name == "RainEmitters").transforms.Select(x => x.GetComponent<ParticleSystem>());
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
            level.customReferences.Find(x => x.name == "Sky").transforms[0].gameObject.AddComponent<SkyController>();
            Player.onSpawn += OnPlayerSpawned;
            yield break;
        }

        void OnPlayerSpawned(Player player) {
            if (rainEnabled && rainController != null) {
                rainController.target = player.transform;
            }
        }

        public override void OnUnload() {
            Player.onSpawn -= OnPlayerSpawned;
        }

    }

    public class RainController : ThunderBehaviour {
        public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

        public Transform target;

        protected override void ManagedUpdate() {
            if (target) transform.position = target.position;
        }
    }
}
