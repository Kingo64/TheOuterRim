using ThunderRoad;
using System.Collections;

namespace TOR {
    public class LevelModuleForceShadowDistance : LevelModule {
        public float shadowDistance = 100f;
        private float userDistance;

        public override IEnumerator OnLoadCoroutine() {
            if (GameManager.options.shadowDistance < shadowDistance) {
                userDistance = GameManager.options.shadowDistance;
                GameManager.options.shadowDistance = shadowDistance;
            }
            yield break;
        }

        public override void OnUnload() {
            if (userDistance != 0 && GameManager.options.shadowDistance == shadowDistance)
                GameManager.options.shadowDistance = userDistance;
        }
    }
}
