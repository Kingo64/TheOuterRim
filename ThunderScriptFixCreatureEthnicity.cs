using ThunderRoad;
using System.Collections.Generic;

namespace TOR {
    // DELETE THIS WHEN OFFICIAL CREATURES ACTUALLY RANDOMISE FROM POOL
    public class LevelModuleRandomiseEthnicity : ThunderScript {
        public string[] creatures = new string[]{
            "TORMale",
            "TORFemale",
            "YounglingMale",
            "YounglingFemale",
            "Stormtrooper",
        };
        HashSet<int> creatureHash;

        public override void ScriptLoaded(ModManager.ModData modData) { 
            base.ScriptLoaded(modData);
            creatureHash = Utils.HashArray(creatures);
            EventManager.onCreatureSpawn += OnCreatureSpawned;
        }

        public override void ScriptDisable() {
            base.ScriptDisable();
            EventManager.onCreatureSpawn -= OnCreatureSpawned;
        }

        void OnCreatureSpawned(Creature creature) {
            if (!creature.isPlayer && creatureHash.Contains(creature.data.hashId)) {
                creature.SetRandomEthnicGroup();
            }
        }
    }
}
