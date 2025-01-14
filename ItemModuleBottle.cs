using ThunderRoad;

namespace TOR {
    public class ItemModuleBottle : ItemModule {
        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemBottle>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }
    }
}
