﻿using BS;

namespace TOR {
    // This create an item module that can be referenced in the item JSON
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
        public int scopeDepth;
        public int[] scopeResolution = { 512, 512 };
        public float[] scopeZoom = { 10f, 6f, 18f };

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemBinoculars>();
        }
    }
}