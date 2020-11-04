using ThunderRoad;

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

        public override void OnItemDataRefresh() {
            base.OnItemDataRefresh();
            if (!string.IsNullOrEmpty(tauntSound)) tauntAsset = CatalogData.GetPrefab<AudioContainer>("", tauntSound);
            if (!string.IsNullOrEmpty(tauntDropSound)) tauntDropAsset = CatalogData.GetPrefab<AudioContainer>("", tauntDropSound);
        }

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemTaunt>();
        }
    }
}
