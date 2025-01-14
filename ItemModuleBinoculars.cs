using ThunderRoad;

namespace TOR {
    public class ItemModuleBinoculars : ItemModule {
        // controls
        public string leftGripPrimaryAction = "";
        public string leftGripSecondaryAction = "cycleScope";
        public string rightGripPrimaryAction = "";
        public string rightGripSecondaryAction = "cycleScope";

        // standard refs
        public string leftScopeID = "LeftScope";
        public string leftScopeCameraID = "LeftScopeCamera";
        public string rightScopeID = "RightScope";
        public string rightScopeCameraID = "RightScopeCamera";
        public string zoomSoundsID = "ZoomSounds";
        
        // scope
        public int scopeDepth = 24;
        public int[] scopeResolution;
        public float[] scopeZoom = { 10f, 6f, 18f };

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemBinoculars>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }
    }
}
