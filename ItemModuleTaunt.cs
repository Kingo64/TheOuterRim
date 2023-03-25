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

        public override void OnItemDataRefresh(ItemData data) {
            base.OnItemDataRefresh(data);
            if (!string.IsNullOrEmpty(tauntSound)) Catalog.LoadAssetAsync<AudioContainer>(tauntSound, x => tauntAsset = x, null);
            if (!string.IsNullOrEmpty(tauntDropSound)) Catalog.LoadAssetAsync<AudioContainer>(tauntDropSound, x => tauntDropAsset = x, null);
        }

        public override void ReleaseAddressableAssets() {
            base.ReleaseAddressableAssets();
            Utils.ReleaseAsset(tauntAsset);
            Utils.ReleaseAsset(tauntDropAsset);
        }
    }
}
