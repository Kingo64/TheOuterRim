using ThunderRoad;
using System.Collections;
using UnityEngine;

namespace TOR {
    public class LevelModuleForceDistanceShadowmask : LevelModule {
        private ShadowmaskMode prevShadowmaskMode;

        public override IEnumerator OnLoadCoroutine() {
            prevShadowmaskMode = QualitySettings.shadowmaskMode;
            QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
            yield break;
        }

        public override void OnUnload() {
            QualitySettings.shadowmaskMode = prevShadowmaskMode;
        }
    }
}
