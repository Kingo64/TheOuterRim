using ThunderRoad;

namespace TOR {
    public class ItemModuleStorage : ItemModule {
        public string[] holders;
        public bool hideStoredItems;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemStorage>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }
    }
}
