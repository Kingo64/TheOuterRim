using ThunderRoad;

namespace TOR {
    public class ItemModuleBlasterPowerCell : ItemModule {
        public string projectileID;
        public string audioSoundPath;
        public AudioContainer audioAsset;

        public override void OnItemDataRefresh(ItemPhysic data) {
            base.OnItemDataRefresh(data);
            if (!string.IsNullOrEmpty(audioSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(audioSoundPath, ac => audioAsset = ac, null);
        }

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemBlasterPowerCell>();
        }
    }
}
