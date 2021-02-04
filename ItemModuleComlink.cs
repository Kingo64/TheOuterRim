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

        public override void OnItemDataRefresh(ItemPhysic data) {
            base.OnItemDataRefresh(data);
            if (!string.IsNullOrEmpty(useSound)) Catalog.LoadAssetAsync<AudioContainer>(useSound, ac => useSoundAsset = ac, null);
        }

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemComlink>();
        }
    }
}
