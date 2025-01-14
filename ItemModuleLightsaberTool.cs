using ThunderRoad;

namespace TOR {
    public class ItemModuleLightsaberTool : ItemModule {
        // controls
        public string gripPrimaryAction = "";
        public string gripSecondaryAction = "cycleMode";

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemLightsaberTool>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }
    }
}
