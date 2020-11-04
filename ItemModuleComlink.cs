using System.Collections.Generic;
using ThunderRoad;

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

        public override void OnItemDataRefresh() {
            base.OnItemDataRefresh();
            if (!string.IsNullOrEmpty(useSound)) useSoundAsset = CatalogData.GetPrefab<AudioContainer>("", useSound);
        }

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemComlink>();
        }
    }
}
