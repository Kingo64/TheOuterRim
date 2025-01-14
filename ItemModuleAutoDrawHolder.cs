using ThunderRoad;

namespace TOR {
    public class ItemModuleAutoDrawHolder : ItemModule {
        public bool aiOnly = true;
        public string[] holders;
        public string[] drawToHolders = { "HipsLeft", "HipsRight", "BackLeft" };

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemAutoDrawHolder>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }
    }
}
