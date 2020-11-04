using ThunderRoad;

namespace TOR {
    public class ItemModuleBlasterPowerCell : ItemModule {
        public string projectileID;
        public string audioSoundPath;
        public AudioContainer audioAsset;

        public override void OnItemDataRefresh() {
            base.OnItemDataRefresh();
            if (!string.IsNullOrEmpty(audioSoundPath)) audioAsset = CatalogData.GetPrefab<AudioContainer>("", audioSoundPath);
        }

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemBlasterPowerCell>();
        }
    }
}
