using UnityEngine;
using BS;

namespace TOR {
    public class ItemKyberCrystal : MonoBehaviour {
        protected Item item;
        protected ItemModuleKyberCrystal module;

        public Color bladeColour;
        public AudioSource idleSound;
        public bool isUnstable;
        public Color glowColour;
        public float glowIntensity;
        public float glowRange;
        public Transform startSounds;
        public Transform stopSounds;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleKyberCrystal>();

            bladeColour = new Color(module.bladeColour[0], module.bladeColour[1], module.bladeColour[2], module.bladeColour[3]);
            idleSound = item.definition.GetCustomReference("IdleSound").GetComponent<AudioSource>();
            isUnstable = module.isUnstable;
            glowColour = new Color(module.glowColour[0], module.glowColour[1], module.glowColour[2], module.glowColour[3]);
            glowIntensity = module.glowIntensity;
            glowRange = module.glowRange;
            startSounds = item.definition.GetCustomReference("StartSounds");
            stopSounds = item.definition.GetCustomReference("StopSounds");
        }
    }
}