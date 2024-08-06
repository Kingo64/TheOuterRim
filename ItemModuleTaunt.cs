using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemModuleTaunt : ItemModule
    {
        public string gripID;
        public string tauntID;
        public string tauntSound;
        public AudioContainer tauntAsset;
        public string tauntDropSound;
        public AudioContainer tauntDropAsset;
        public float aiTauntChance = 0.5f;

        // controls
        public string gripPrimaryAction = "";
        public string gripSecondaryAction = "playTaunt";

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemTaunt>(item.gameObject);
        }

        public override System.Collections.IEnumerator LoadAddressableAssetsCoroutine(ItemData data) {
            if (!string.IsNullOrEmpty(tauntSound)) yield return Catalog.LoadAssetCoroutine(tauntSound, delegate (AudioContainer x) { tauntAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(tauntDropSound)) yield return Catalog.LoadAssetCoroutine(tauntDropSound, delegate (AudioContainer x) { tauntDropAsset = x; }, GetType().Name);
            yield return base.LoadAddressableAssetsCoroutine(data);
            yield break;
        }

        public override void ReleaseAddressableAssets() {
            base.ReleaseAddressableAssets();
            Utils.ReleaseAsset(tauntAsset);
            Utils.ReleaseAsset(tauntDropAsset);
        }
    }
}
