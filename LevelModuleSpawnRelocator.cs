using ThunderRoad;
using System.Collections;

namespace TOR {
    public class LevelModuleSpawnRelocator : LevelModule {
        public string startLocation;

        public override IEnumerator OnLoadCoroutine() {
            if (Level.current.options != null) {
                if (Level.current.options.TryGetValue("startLocation", out string val)) startLocation = val;
            }

            if (!string.IsNullOrEmpty(startLocation)) {
                var startLocations = level.customReferences.Find(x => x.name == "StartLocations");
                var location = startLocation == "Random" ? startLocations.transforms.RandomChoice() : startLocations.transforms.Find(x => x.name == startLocation);
                if (location) {
                    level.playerSpawnerId = location.GetComponent<PlayerSpawner>().id;
                }
            }
            yield break;
        }
    }
}
