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

        public override System.Collections.IEnumerator LoadAddressableAssetsCoroutine(ItemData data) {
            if (!string.IsNullOrEmpty(idleSoundLeft)) yield return Catalog.LoadAssetCoroutine(idleSoundLeft, delegate (AudioContainer x) { idleSoundLeftAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(idleSoundRight)) yield return Catalog.LoadAssetCoroutine(idleSoundRight, delegate (AudioContainer x) { idleSoundRightAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(startSound)) yield return Catalog.LoadAssetCoroutine(startSound, delegate (AudioContainer x) { startSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(stopSound)) yield return Catalog.LoadAssetCoroutine(stopSound, delegate (AudioContainer x) { stopSoundAsset = x; }, GetType().Name);
            yield return base.LoadAddressableAssetsCoroutine(data);
            yield break;
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
