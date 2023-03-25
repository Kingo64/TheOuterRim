using ThunderRoad;
using UnityEngine;
using System.Collections;
using RainyReignGames.RevealMask;
using System.Collections.Generic;

namespace TOR {
    public class LevelModuleCreaturePainter : LevelModule {
        public string[] creatures = new string[]{
            "TORMale",
            "TORFemale",
            "CloneTrooper",
            "Stormtrooper",
        };
        HashSet<int> creatureHashes;
        Texture2D moesGreen;

        public override IEnumerator OnLoadCoroutine() {
            creatureHashes = Utils.HashArray(creatures);
            EventManager.onCreatureSpawn += OnCreatureSpawn;

            if (!moesGreen) {
                moesGreen = new Texture2D(1, 1);
                moesGreen.SetPixel(0, 0, Color.green);
                moesGreen.Resize(1, 1);
                moesGreen.Apply();
            }

            yield break;
        }

        public override void OnUnload() {
            base.OnUnload();
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
            if (creatureHashes.Contains(creature.data.hashId)) {
                if (creature.manikinParts) {
                    var creatureId = creature.data.id;
                    if (creatureId == "TORMale" || creatureId == "TORFemale") {
                        if (Random.Range(0f, 1f) < 0.9f) creature.SetColor(new Color(Random.Range(0.7f, 1), Random.Range(0.7f, 1), Random.Range(0.7f, 1)), Creature.ColorModifier.Skin);
                        if (Random.Range(0f, 1f) < 0.3f) creature.SetColor(new Color(Random.Range(0.35f, 0.7f), Random.Range(0.35f, 0.7f), Random.Range(0.35f, 0.7f)), Creature.ColorModifier.Hair);
                        creature.manikinProperties.UpdateProperties();
                    } else {
                        creature.manikinParts.UpdateParts_Completed += delegate (Chabuk.ManikinMono.ManikinPart[] partsAdded) {
                            creature.StartCoroutine(ActivateMaterials(creature));
                        };
                    }
                }
            };
        }

        public void PaintRenderer(string creatureId, Material[] materials) {
            if (creatureId == "CloneTrooper" || creatureId == "Stormtrooper") {
                var colour = creatureId == "Stormtrooper" ? new Color(0.728f, 0.708f, 0.662f) : new Color(0.8f, 0.8f, 0.8f);
                foreach (Material material in materials) {
                    if ((!material.name.Contains("Eye") && !material.name.Contains("Mouth") && !IsSkin(material.name) && !IsHair(material.name)) || material.name.ToLower().Contains("hand") || material.name.ToLower().Contains("body")) {
                        material.SetTexture("_BaseMap", null);
                        material.SetTexture("_BumpMap", null);
                        material.SetTexture("_MainTex", null);
                        material.SetTexture("_MetallicGlossMap", moesGreen);
                        material.SetTexture("_SpecGlossMap", null);
                        material.SetFloat("_Metallic", 0);
                        material.SetFloat("_Smoothness", 0.2f);
                        material.SetColor("_BaseColor", colour);
                    }
                }
            }
        }

        IEnumerator ActivateMaterials(Creature creature) {
            var creatureId = creature.data.id;
            yield return Utils.waitSeconds_001;

            foreach (var rendererData in creature.renderers) {
                if (rendererData.revealDecal != null && rendererData.revealDecal.revealMaterialController != null) {
                    rendererData.revealDecal.revealMaterialController.OnActivated += delegate (object sender, RevealMaterialController.ActivatedEventArgs e) {
                        PaintRenderer(creatureId, e.activatedMaterials);
                    };
                    rendererData.revealDecal.revealMaterialController.ActivateRevealMaterials();
                }
            }
            yield return null;
        }
    }
}
