using ThunderRoad;

namespace TOR {
    public class ItemModuleStorage : ItemModule {
        public string[] holders;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemStorage>(item.gameObject);
        }
    }
}
