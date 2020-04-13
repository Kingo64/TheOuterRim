using BS;

namespace TOR {
    public class ItemModuleLightsaberTool : ItemModule {
        public bool allowDisarm;
        public float lengthChange = 0.05f;

        // controls
        public string gripPrimaryAction = "";
        public string gripSecondaryAction = "cycleMode";

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemLightsaberTool>();
        }
    }
}
