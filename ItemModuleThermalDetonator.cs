using ThunderRoad;

namespace TOR {
    public class ItemModuleThermalDetonator : ItemModule {
        public float radius = 6f;
        public float damage = 1000f;
        public float impuse = 5f;
        public float detonateTime = 3f;

        public string gripPrimaryAction = "";
        public string gripPrimaryActionHold = "";
        public string gripSecondaryAction = "arm";
        public string gripSecondaryActionHold = "toggleSlider";

        public string explosionSound;
        public AudioContainer explosionSoundAsset;
        public string explosionSound2;
        public AudioContainer explosionSoundAsset2;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemThermalDetonator>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }

        public override System.Collections.IEnumerator LoadAddressableAssetsCoroutine(ItemData data) {
            if (!string.IsNullOrEmpty(explosionSound)) yield return Catalog.LoadAssetCoroutine(explosionSound, delegate (AudioContainer x) { explosionSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(explosionSound2)) yield return Catalog.LoadAssetCoroutine(explosionSound2, delegate (AudioContainer x) { explosionSoundAsset2 = x; }, GetType().Name);
            yield return base.LoadAddressableAssetsCoroutine(data);
            yield break;
        }

        public override void ReleaseAddressableAssets() {
            base.ReleaseAddressableAssets();
            Utils.ReleaseAsset(explosionSoundAsset);
            Utils.ReleaseAsset(explosionSoundAsset2);
        }
    }
}
