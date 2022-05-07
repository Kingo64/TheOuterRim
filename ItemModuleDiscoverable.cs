using ThunderRoad;

namespace TOR {
    public class ItemModuleDiscoverable : ItemModule {
        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemDiscoverable>(item.gameObject);
        }
    }
}
