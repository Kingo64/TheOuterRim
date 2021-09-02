using ThunderRoad;
using UnityEngine;
using System.Collections;
using RainyReignGames.RevealMask;

namespace TOR {
    public class LevelModuleCreaturePainter : LevelModule {

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

        public void PaintRenderer(Creature creature, Material[] materials) {
            var creatureId = Constants.CREATURE_IDS[creature.data.hashId];
            if (creatureId == "CloneTrooper" || creatureId == "Stormtrooper") {
                var colour = creatureId == "Stormtrooper" ? new Color(0.728f, 0.708f, 0.662f) : new Color(0.8f, 0.8f, 0.8f);
                foreach (Material material in materials) {
                    if ((!material.name.Contains("Eye") && !material.name.Contains("Mouth") && !IsSkin(material.name) && !IsHair(material.name)) || material.name.ToLower().Contains("hand") || material.name.ToLower().Contains("body")) {
                        material.SetTexture("_BaseMap", null);
                        material.SetTexture("_BumpMap", null);
                        material.SetTexture("_MainTex", null);
                        material.SetTexture("_MetallicGlossMap", null);
                        material.SetTexture("_SpecGlossMap", null);
                        material.SetFloat("_Metallic", 0);
                        material.SetFloat("_Smoothness", 0.2f);
                        material.SetColor("_BaseColor", colour);
                    }
                }
            }
        }

        void OnCreatureSpawn(Creature creature) {
            if (Constants.CREATURE_IDS.ContainsKey(creature.data.hashId)) {
                if (creature.manikinParts) {

                    var creatureId = Constants.CREATURE_IDS[creature.data.hashId];
                    if (creatureId == "ForceSensitiveMale" || creatureId == "ForceSensitiveFemale") {
                        if (Random.Range(0f, 1f) < 0.9f) creature.SetColor(new Color(Random.Range(0.7f, 1), Random.Range(0.7f, 1), Random.Range(0.7f, 1)), Creature.ColorModifier.Skin);
                        if (Random.Range(0f, 1f) < 0.3f) creature.SetColor(new Color(Random.Range(0.35f, 0.7f), Random.Range(0.35f, 0.7f), Random.Range(0.35f, 0.7f)), Creature.ColorModifier.Hair);
                    } else {
                        creature.manikinParts.PartsCompletedEvent += delegate () {
                            creature.StartCoroutine(ActivateMaterials(creature));
                        };
                    }
                }
            };
        }

        IEnumerator ActivateMaterials(Creature creature) {
            yield return new WaitForSeconds(0.01f);

            foreach (var rendererData in creature.renderers) {
                if (rendererData.revealDecal != null && rendererData.revealDecal.revealMaterialController != null) {
                    rendererData.revealDecal.revealMaterialController.OnActivated += delegate (object sender, RevealMaterialController.ActivatedEventArgs e) {
                        PaintRenderer(creature, e.activatedMaterials);
                    };
                    rendererData.revealDecal.revealMaterialController.ActivateRevealMaterials();
                }
            }
            yield return null;
        }
    }
}
