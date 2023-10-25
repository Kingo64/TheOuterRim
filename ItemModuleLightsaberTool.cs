using ThunderRoad;

namespace TOR {
    public class ItemModuleLightsaberTool : ItemModule {
        // controls
        public string gripPrimaryAction = "";
        public string gripSecondaryAction = "cycleMode";

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemLightsaberTool>(item.gameObject);
        }
    }
}
