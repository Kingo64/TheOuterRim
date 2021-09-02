using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemModuleLightsaber : ItemModule
    {
        public string animatorId;
        public bool canThrow;
        public int fastCollisionMode = (int)CollisionDetectionMode.ContinuousDynamic;
        public float fastCollisionSpeed;
        public float[] helicopterThrust = { 0f, 100f };
        public string kyberCrystalID = "KyberCrystalBlue";
        public float ignitionDuration = 0.1f;
        public bool startActive;
        public float throwSpeed;
        public LightsaberBlade[] lightsaberBlades;

        // coupling
        public bool hasCoupler;
        public string[] couplingWhitelist;

        // controls
        public string primaryGripPrimaryAction = "";
        public string primaryGripPrimaryActionHold = "";
        public string primaryGripSecondaryAction = "toggleIgnition";
        public string primaryGripSecondaryActionHold = "";
        public float controlHoldTime;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            LoadGlobalSettings();
            Utils.AddModule<ItemLightsaber>(item.gameObject);
        }

        private void LoadGlobalSettings() {
            if (canThrow) canThrow = GlobalSettings.SaberThrowable;
            fastCollisionSpeed = GlobalSettings.SaberExpensiveCollisionsMinVelocity;
            throwSpeed = GlobalSettings.SaberThrowMinVelocity;
            controlHoldTime = GlobalSettings.ControlsHoldDuration;
        }
    }
}
