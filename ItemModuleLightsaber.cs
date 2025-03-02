﻿using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemModuleLightsaber : ItemModule
    {
        public string animatorId;
        public bool animateOnIgnition;
        public int fastCollisionMode = (int)CollisionDetectionMode.ContinuousDynamic;
        public float[] helicopterThrust = { 0f, 100f };
        public float ignitionDuration = 0.1f;
        public bool startActive;
        public LightsaberBlade[] lightsaberBlades;

        // coupling
        public bool hasCoupler;
        public string[] couplingWhitelist;

        // controls
        public string primaryGripPrimaryAction = "";
        public string primaryGripPrimaryActionHold = "";
        public string primaryGripSecondaryAction = "toggleIgnition";
        public string primaryGripSecondaryActionHold = "";

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemLightsaber>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }
    }
}
