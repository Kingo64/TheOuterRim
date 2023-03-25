using ThunderRoad;

namespace TOR {
    public class ItemModuleJetpack : ItemModule {
        public float airSpeed = 29f;
        public float drag = 0.7f;
        public float thrust = 40f;
        public float startDeadzone = 0.9f;

        public AudioContainer idleSoundLeftAsset; public string idleSoundLeft;
        public AudioContainer idleSoundRightAsset; public string idleSoundRight;
        public AudioContainer startSoundAsset; public string startSound;
        public AudioContainer stopSoundAsset; public string stopSound;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemJetpack>(item.gameObject);
        }

        public override void OnItemDataRefresh(ItemData data) {
            base.OnItemDataRefresh(data);
            if (!string.IsNullOrEmpty(idleSoundLeft)) Catalog.LoadAssetAsync<AudioContainer>(idleSoundLeft, x => idleSoundLeftAsset = x, null);
            if (!string.IsNullOrEmpty(idleSoundRight)) Catalog.LoadAssetAsync<AudioContainer>(idleSoundRight, x => idleSoundRightAsset = x, null);
            if (!string.IsNullOrEmpty(startSound)) Catalog.LoadAssetAsync<AudioContainer>(startSound, x => startSoundAsset = x, null);
            if (!string.IsNullOrEmpty(stopSound)) Catalog.LoadAssetAsync<AudioContainer>(stopSound, x => stopSoundAsset = x, null);
        }

        public override void ReleaseAddressableAssets() {
            base.ReleaseAddressableAssets();
            Utils.ReleaseAsset(idleSoundLeftAsset);
            Utils.ReleaseAsset(idleSoundRightAsset);
            Utils.ReleaseAsset(startSoundAsset);
            Utils.ReleaseAsset(stopSoundAsset);
        }
    }
}
