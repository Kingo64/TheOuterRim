﻿using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class LevelModuleFelucia : LevelModule {
        public float grassDensity = 0.6f;
        public float grassDistance = 80f;
        public float pollenDensity = 0.5f;
        Terrain terrain;
        Transform pollenTrans;

        public override IEnumerator OnLoadCoroutine() {
            Player.onSpawn += OnPlayerSpawned;

            if (Level.current.options != null) {
                if (Level.current.options.TryGetValue("grassDensity", out double val)) grassDensity = (float)val * 0.2f;
                if (Level.current.options.TryGetValue("grassDistance", out val)) grassDistance = (float)val * 20f;
                if (Level.current.options.TryGetValue("pollenDensity", out val)) pollenDensity = (float)val * 0.2f;
            }

            pollenTrans = level.customReferences.Find(x => x.name == "Pollen").transforms[0];
            var springs = level.customReferences.Find(x => x.name == "Springs").transforms[0];
            foreach (Transform child in springs) {
                child.gameObject.AddComponent<SpringController>();
            }
            var sky = level.customReferences.Find(x => x.name == "Sky").transforms[0].gameObject.AddComponent<SkyController>();
            sky.speed = 0.25f;
            terrain = level.customReferences.Find(x => x.name == "Terrain").transforms[0].GetComponent<Terrain>();
            terrain.detailObjectDensity = Mathf.Clamp(grassDensity, 0, 1);
            terrain.detailObjectDistance = Mathf.Abs(grassDistance);
            yield break;
        }

        void OnPlayerSpawned(Player player) {
            if (pollenTrans != null && pollenDensity > 0) {
                var pollenClone = Object.Instantiate(pollenTrans, player.transform);
                pollenClone.gameObject.SetActive(true);
                var emission = pollenClone.GetComponent<ParticleSystem>().emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve(emission.rateOverTime.constant * Mathf.Abs(pollenDensity));
            }
        }

        public override void OnUnload() {
            Player.onSpawn -= OnPlayerSpawned;
        }
    }

    public class SpringController : MonoBehaviour {
        ParticleSystem particle;
        AudioSource[] sounds;
        GameObject zone;
        float waitTime;
        readonly float radius = 0.5f;

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
