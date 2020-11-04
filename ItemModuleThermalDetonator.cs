using ThunderRoad;

namespace TOR {
    public class ItemModuleThermalDetonator : ItemModule {
        public float radius = 6f;
        public float damage = 1000f;
        public float impuse = 5f;
        public float detonateTime = 3f;

        public string gripPrimaryAction = "";
        public string gripPrimaryActionHold = "";
        public string gripSecondaryAction = "arm";
        public string gripSecondaryActionHold = "toggleSlider";

        public string explosionSound;
        public AudioContainer explosionSoundAsset;
        public string explosionSound2;
        public AudioContainer explosionSoundAsset2;

        public override void OnItemDataRefresh() {
            base.OnItemDataRefresh();
            if (!string.IsNullOrEmpty(explosionSound)) explosionSoundAsset = CatalogData.GetPrefab<AudioContainer>("", explosionSound);
            if (!string.IsNullOrEmpty(explosionSound2)) explosionSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", explosionSound2);
        }

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemThermalDetonator>();
        }
    }
}
