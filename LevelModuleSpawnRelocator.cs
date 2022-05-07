using ThunderRoad;
using System.Collections;

namespace TOR {
    public class LevelModuleSpawnRelocator : LevelModule {
        public string startLocation;

        public override IEnumerator OnLoadCoroutine() {
            if (!string.IsNullOrEmpty(startLocation)) {
                var startLocations = level.customReferences.Find(x => x.name == "StartLocations");
                var location = startLocations.transforms.Find(x => x.name == startLocation);
                if (location) level.playerStart = location;
            }
            yield break;
        }
    }
}
