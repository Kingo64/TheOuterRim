using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class LevelModuleFelucia : LevelModule {
        public float grassDensity = 1f;
        public float grassDistance = 80f;
        public bool pollenEnabled = true;
        public float pollenDensity = 1f;
        Terrain terrain;
        Transform pollenTrans;

        public override void OnLevelLoaded(LevelDefinition levelDefinition) {
            EventManager.onPlayerSpawned += OnPlayerSpawned;
            pollenTrans = levelDefinition.customReferences.Find(x => x.name == "Pollen").transforms[0];
            var springs = levelDefinition.customReferences.Find(x => x.name == "Springs").transforms[0];
            foreach (Transform child in springs) {
                child.gameObject.AddComponent<SpringController>();
            }
            var sky = levelDefinition.customReferences.Find(x => x.name == "Sky").transforms[0].gameObject.AddComponent<SkyController>();
            sky.speed = 0.25f;
            terrain = levelDefinition.customReferences.Find(x => x.name == "Terrain").transforms[0].GetComponent<Terrain>();
            terrain.detailObjectDensity = Mathf.Clamp(grassDensity, 0, 1);
            terrain.detailObjectDistance = Mathf.Abs(grassDistance);
            initialized = true;
        }

        void OnPlayerSpawned(Player player) {
            if (pollenTrans != null && pollenEnabled) {
                var pollenClone = Object.Instantiate(pollenTrans, player.transform);
                pollenClone.gameObject.SetActive(true);
                var emission = pollenClone.GetComponent<ParticleSystem>().emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(emission.rateOverTime.constant * Mathf.Abs(pollenDensity));
            }
        }

        public override void OnLevelUnloaded(LevelDefinition levelDefinition) {
            EventManager.onPlayerSpawned -= OnPlayerSpawned;
            initialized = false;
        }
    }

    public class SpringController : MonoBehaviour {
        ParticleSystem particle;
        AudioSource[] sounds;
        GameObject zone;
        float waitTime;
        float radius = 0.5f;

        void Awake() {
            var steam = transform.Find("Steam");
            particle = steam.GetComponent<ParticleSystem>();
            sounds = steam.GetComponents<AudioSource>();
            zone = transform.Find("Zone").gameObject;
            zone.SetActive(false);
            waitTime = Random.Range(5f, 15f);
        }

        void Update() {
            if (waitTime > 0) {
                waitTime -= Time.deltaTime;
                if (waitTime <= 0) {
                    Blast();
                    waitTime = Random.Range(5f, 15f);
                }
            }
        }

        void Blast() {
            StartCoroutine(DisableLate());
            particle.Play();
            Utils.PlayRandomSound(sounds);
            zone.SetActive(true);

            var colliders = Physics.OverlapSphere(transform.position, radius);
            foreach (var hit in colliders) {
                var rb = hit.GetComponent<Rigidbody>() ?? hit.GetComponentInParent<Rigidbody>();
                if (rb != null) {
                    rb.AddExplosionForce(100, transform.position, radius, 100.0f, ForceMode.Impulse);
                }
            }
        }

        IEnumerator DisableLate() {
            yield return new WaitForSeconds(1f);
            zone.SetActive(false);
        }
    }
}
