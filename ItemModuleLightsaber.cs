﻿using BS;
using UnityEngine;

namespace TOR {
    // This create an item module that can be referenced in the item JSON
    public class ItemModuleLightsaber : ItemModule
    {
        public bool canThrow = true;
        public int fastCollisionMode = (int)CollisionDetectionMode.ContinuousDynamic;
        public float fastCollisionSpeed = 5f;
        public string kyberCrystalID = "KyberCrystalBlue";
        public float ignitionDuration = 0.1f;
        public bool startActive = false;
        public float throwSpeed = 7f;
        public LightsaberBlade[] lightsaberBlades = { new LightsaberBlade() };

        // controls
        public string primaryGripPrimaryAction = "";
        public string primaryGripPrimaryActionHold = "";
        public string primaryGripSecondaryAction = "toggleIgnition";
        public string primaryGripSecondaryActionHold = "";
        public float controlHoldTime = 0.5f;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemLightsaber>();
        }
    }
}