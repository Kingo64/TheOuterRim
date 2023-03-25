using ThunderRoad;
using System.Collections;
using System.Collections.Generic;

namespace TOR {
    // DELETE THIS WHEN OFFICIAL CREATURES ACTUALLY RANDOMISE FROM POOL
    public class LevelModuleRandomiseEthnicity : LevelModule {
        public string[] creatures = new string[]{
            "TORMale",
            "TORFemale",
            "Stormtrooper",
        };
        HashSet<int> creatureHash;

        public override IEnumerator OnLoadCoroutine() {
            creatureHash = Utils.HashArray(creatures);
            EventManager.onCreatureSpawn += OnCreatureSpawned;
            yield break;
        }

        public override void OnUnload() {
            EventManager.onCreatureSpawn -= OnCreatureSpawned;
        }

        void OnCreatureSpawned(Creature creature) {
            if (!creature.isPlayer && creatureHash.Contains(creature.data.hashId)) {
                creature.SetRandomEthnicGroup();
            }
        }
    }
}
