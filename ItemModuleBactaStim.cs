using ThunderRoad;

namespace TOR {
    public class ItemModuleBactaStim : ItemModule {
        public float chargeRate = 1f;
        public float healAmount = 10f;
        public float healDuration = 10f;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemBactaStim>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }
    }
}
