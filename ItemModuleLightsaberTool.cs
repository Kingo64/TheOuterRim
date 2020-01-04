using BS;

namespace TOR {
    public class ItemModuleLightsaberTool : ItemModule {
        public bool allowDisarm;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemLightsaberTool>();
        }
    }
}
