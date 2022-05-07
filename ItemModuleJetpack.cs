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

        public override void OnItemDataRefresh(ItemData data) {
            base.OnItemDataRefresh(data);
            if (!string.IsNullOrEmpty(idleSoundLeft)) Catalog.LoadAssetAsync<AudioContainer>(idleSoundLeft, ac => idleSoundLeftAsset = ac, null);
            if (!string.IsNullOrEmpty(idleSoundRight)) Catalog.LoadAssetAsync<AudioContainer>(idleSoundRight, ac => idleSoundRightAsset = ac, null);
            if (!string.IsNullOrEmpty(startSound)) Catalog.LoadAssetAsync<AudioContainer>(startSound, ac => startSoundAsset = ac, null);
            if (!string.IsNullOrEmpty(stopSound)) Catalog.LoadAssetAsync<AudioContainer>(stopSound, ac => stopSoundAsset = ac, null);
        }

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemJetpack>(item.gameObject);
        }
    }
}
