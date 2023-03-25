using ThunderRoad;

namespace TOR {
    public class ItemModuleBottle : ItemModule {
        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemBottle>(item.gameObject);
        }
    }
}
