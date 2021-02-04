using ThunderRoad;
using UnityEngine;
using System.Collections;

namespace TOR {
    public class LevelModuleFixLightmaps : LevelModule {

        public override IEnumerator OnLoadCoroutine(Level level) {
            LightmapSettings.lightmapsMode = LightmapsMode.CombinedDirectional;
            yield break;
        }
    }
}
