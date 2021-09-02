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

        public override void OnItemDataRefresh(ItemData data) {
            base.OnItemDataRefresh(data);
            if (!string.IsNullOrEmpty(explosionSound)) Catalog.LoadAssetAsync<AudioContainer>(explosionSound, ac => explosionSoundAsset = ac, null);
            if (!string.IsNullOrEmpty(explosionSound2)) Catalog.LoadAssetAsync<AudioContainer>(explosionSound2, ac => explosionSoundAsset2 = ac, null);
        }

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemThermalDetonator>(item.gameObject);
        }
    }
}
