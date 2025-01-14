using ThunderRoad;

namespace TOR {
    public class ItemModuleImbueToggle : ItemModule {
        public string gripID;
        public string spell = "Lightning";

        public string primaryAction;
        public string primaryActionHold;
        public string secondaryAction;
        public string secondaryActionHold;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemImbueToggle>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }
    }
}