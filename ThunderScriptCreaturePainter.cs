using ThunderRoad;
using UnityEngine;
using System.Collections.Generic;

namespace TOR {
    public class ThunderScriptCreaturePainter : ThunderScript {
        public string[] creatures = new string[]{
            "TORMale",
            "TORFemale",
            "YounglingMale",
            "YounglingFemale",
        };
        HashSet<int> creatureHashes;
        Texture2D moesGreen;

        public override void ScriptLoaded(ModManager.ModData modData) {
            base.ScriptLoaded(modData);
            creatureHashes = Utils.HashArray(creatures);
            EventManager.onCreatureSpawn += OnCreatureSpawn;

            if (!moesGreen) {
                moesGreen = new Texture2D(1, 1);
                moesGreen.SetPixel(0, 0, Color.green);
                moesGreen.Reinitialize(1, 1);
                moesGreen.Apply();
            }
        }

        public override void ScriptDisable() {
            base.ScriptDisable();
            EventManager.onCreatureSpawn -= OnCreatureSpawn;
        }

        public override void ScriptUnload() {
            base.ScriptUnload();
            creatureHashes = null;
            Object.Destroy(moesGreen);
            moesGreen = null;
        }

        void OnCreatureSpawn(Creature creature) {
            if (creatureHashes.Contains(creature.data.hashId)) {
                if (creature.manikinParts) {
                    if (Random.Range(0f, 1f) < 0.9f) creature.SetColor(new Color(Random.Range(0.7f, 1), Random.Range(0.7f, 1), Random.Range(0.7f, 1)), Creature.ColorModifier.Skin);
                    if (Random.Range(0f, 1f) < 0.3f) creature.SetColor(new Color(Random.Range(0.35f, 0.7f), Random.Range(0.35f, 0.7f), Random.Range(0.35f, 0.7f)), Creature.ColorModifier.Hair);
                    creature.manikinProperties.UpdateProperties();
                }
            };
        }
    }
}
