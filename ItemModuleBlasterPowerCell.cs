﻿using ThunderRoad;

namespace TOR {
    public class ItemModuleBlasterPowerCell : ItemModule {
        public string projectileID;
        public string audioSoundPath;
        public AudioContainer audioAsset;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemBlasterPowerCell>(item.gameObject);
        }

        public override System.Collections.IEnumerator LoadAddressableAssetsCoroutine(ItemData data) {
            if (!string.IsNullOrEmpty(audioSoundPath)) yield return Catalog.LoadAssetCoroutine(audioSoundPath, delegate (AudioContainer x) { audioAsset = x; }, GetType().Name);
            yield return base.LoadAddressableAssetsCoroutine(data);
            yield break;
        }

        public override void ReleaseAddressableAssets() {
            base.ReleaseAddressableAssets();
            Utils.ReleaseAsset(audioAsset);
        }
    }
}
