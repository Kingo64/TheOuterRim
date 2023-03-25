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

        public override void OnItemDataRefresh(ItemData data) {
            base.OnItemDataRefresh(data);
            if (!string.IsNullOrEmpty(useSound)) Catalog.LoadAssetAsync<AudioContainer>(useSound, x => useSoundAsset = x, null);
        }

        public override void ReleaseAddressableAssets() {
            base.ReleaseAddressableAssets();
            Utils.ReleaseAsset(useSoundAsset);
        }
    }
}
