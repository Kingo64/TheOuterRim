using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemModuleComlink : ItemModule {
        // controls
        public string gripPrimaryAction = "nextFaction";
        public string gripPrimaryActionHold = "";
        public string gripSecondaryAction = "nextTarget";
        public string gripSecondaryActionHold = "summonTarget";

        public string useSound;
        public AudioContainer useSoundAsset;

        public List<ItemComlink.FactionData> factions = new List<ItemComlink.FactionData>();
        public List<ItemComlink.ReinforcementData> reinforcements = new List<ItemComlink.ReinforcementData>();

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemComlink>(item.gameObject);
        }

        public override System.Collections.IEnumerator LoadAddressableAssetsCoroutine(ItemData data) {
            if (!string.IsNullOrEmpty(useSound)) yield return Catalog.LoadAssetCoroutine(useSound, delegate (AudioContainer x) { useSoundAsset = x; }, GetType().Name);
            yield return base.LoadAddressableAssetsCoroutine(data);
            yield break;
        }

        public override void ReleaseAddressableAssets() {
            base.ReleaseAddressableAssets();
            Utils.ReleaseAsset(useSoundAsset);
        }
    }
}
