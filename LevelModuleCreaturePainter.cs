using ThunderRoad;
using UnityEngine;
using System.Collections;

namespace TOR {
    public class LevelModuleCreaturePainter : LevelModule {
        public float checkInterval = 1f;

        public override IEnumerator OnLoadCoroutine(Level level) {
            EventManager.onCreatureSpawn += OnCreatureSpawn;
            yield break;
        }

        public override void OnUnload(Level level) {
            base.OnUnload(level);
            EventManager.onCreatureSpawn -= OnCreatureSpawn;
        }

        bool IsSkin(string name) {
            name = name.ToLower();
            return name.Contains("head") || name.Contains("humanmale_hands") || name.Contains("humanfemale_hands") || name.Contains("body");
        }

        bool IsHair(string name) {
            name = name.ToLower();
            return name.Contains("brow") || name.Contains("hair");
        }

        void OnCreatureSpawn(Creature creature) {
            if (Constants.CREATURE_IDS.ContainsKey(creature.data.hashId)) {
                if (creature.manikinParts) {
                    creature.manikinParts.PartsCompletedEvent += delegate () {
                        var creatureId = Constants.CREATURE_IDS[creature.data.hashId];
                        if (creatureId == "ForceSensitiveMale" || creatureId == "ForceSensitiveFemale") {
                            var skinColour = new Color(Random.Range(0.7f, 1), Random.Range(0.7f, 1), Random.Range(0.7f, 1));
                            var hairColour = (Random.Range(0, 1) > 0.5f) ? new Color(Random.Range(0.35f, 0.7f), Random.Range(0.35f, 0.7f), Random.Range(0.35f, 0.7f)) : Color.clear;

                            foreach (var rendererData in creature.renderers) {
                                foreach (Material material in rendererData.renderer.materials) {
                                    if (IsSkin(material.name)) {
                                        material.SetColor("_BaseColor", skinColour);
                                    }
                                    if (IsHair(material.name) && hairColour != Color.clear) {
                                        material.SetColor("_BaseColor", hairColour);
                                    }
                                }
                            }
                        } else if (creatureId == "Gamorrean") {
                            var skinColour = new Color(0.55f, 0.6f, 0.3f);
                            foreach (var rendererData in creature.renderers) {
                                foreach (Material material in rendererData.renderer.materials) {
                                    if (IsSkin(material.name)) {
                                        material.SetColor("_BaseColor", skinColour);
                                    }
                                }
                            }
                        } else if (creatureId == "TuskenRaider") {
                            var colour = new Color(0.55f, 0.6f, 0.4f);
                            foreach (var rendererData in creature.renderers) {
                                foreach (Material material in rendererData.renderer.materials) {
                                    material.SetColor("_BaseColor", colour);
                                }
                            }
                        } else if (creatureId == "CloneTrooper" || creatureId == "Stormtrooper") {
                            var colour = creatureId == "Stormtrooper" ? new Color(0.728f, 0.708f, 0.662f) : new Color(0.8f, 0.8f, 0.8f);
                            foreach (var rendererData in creature.renderers) {
                                foreach (Material material in rendererData.renderer.materials) {
                                    if ((!material.name.Contains("Eye") && !material.name.Contains("Mouth") && !IsSkin(material.name) && !IsHair(material.name)) || material.name.ToLower().Contains("hand") || material.name.ToLower().Contains("body")) {
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
                    };
                }
            }
        }
    }
}
