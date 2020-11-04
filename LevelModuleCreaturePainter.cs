using ThunderRoad;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

namespace TOR {
    public class LevelModuleCreaturePainter : LevelModule {
        GameObject creatureObserver;
        public float checkInterval = 1f;

        public override void OnLevelLoaded(LevelDefinition levelDefinition) {
            SceneManager.sceneLoaded += (scene, mode) => SetupObserver();
            SceneManager.sceneUnloaded += (scene) => Object.Destroy(creatureObserver);
            SetupObserver();
            initialized = true;
        }

        public override void OnLevelUnloaded(LevelDefinition levelDefinition) {
            initialized = false;
        }

        void SetupObserver() {
            creatureObserver = new GameObject();
            creatureObserver.AddComponent<CreaturePainter>().loopDelay = new WaitForSeconds(checkInterval);
        }
    }

    public class CreaturePainter : MonoBehaviour {
        Coroutine observer;
        HashSet<int> creatures = new HashSet<int>();
        public WaitForSeconds loopDelay;

        void Awake() {
            observer = StartCoroutine(Observe());
        }

        void Destroy() {
            StopAllCoroutines();
        }

        IEnumerator Observe() {
            while (true) {
                yield return loopDelay;
                foreach (var creature in Creature.list) {
                    if (!creatures.Contains(creature.GetInstanceID())) {
                        creatures.Add(creature.GetInstanceID());
                        if (creature.data.hashId == 121048391 || creature.data.hashId == -2046070811) {
                            creature.OnRagdollAttachEvent += (ragdoll) => SetupMaterials(ragdoll.creature);
                            SetupMaterials(creature);
                        }
                    }
                }
            }
        }

        void SetupMaterials(Creature creature) {
            var colour = creature.data.hashId == 121048391 ? new Color(0.728f, 0.708f, 0.662f) : new Color(0.8f, 0.8f, 0.8f); 
            foreach (Material material in creature.bodyMeshRenderer.materials) {
                if (!material.name.Contains("Eye") && !material.name.Contains("Brow") && !material.name.Contains("Hair") && !material.name.Contains("Skin")) {
                    material.SetTexture("_BaseMap", null);
                    material.SetTexture("_BumpMap", null);
                    material.SetTexture("_MainTex", null);
                    material.SetTexture("_MetallicGlossMap", null);
                    material.SetTexture("_SpecGlossMap", null);
                    material.SetFloat("_Metallic", 0);
                    material.SetFloat("_Smoothness", 0.4f);
                    material.SetColor("_BaseColor", colour);
                    material.DisableKeyword("_METALLICSPECGLOSSMAP");
                }
            }
        }
    }
}
