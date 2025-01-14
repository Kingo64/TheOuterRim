using ThunderRoad;

namespace TOR {
    public class ItemModuleDiscoverable : ItemModule {
        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemDiscoverable>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }
    }
}
