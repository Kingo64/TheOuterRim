using UnityEngine;
using BS;

namespace TOR {
    public class ItemKyberCrystal : MonoBehaviour {
        protected Item item;
        protected ItemModuleKyberCrystal module;

        public Material bladeMaterial;
        public AudioSource idleSound;
        public Light glowLight;
        public Material glowMaterial;
        public Transform startSounds;
        public Transform stopSounds;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleKyberCrystal>();

            bladeMaterial = item.definition.GetCustomReference("BladeMaterial").GetComponent<MeshRenderer>().material;
            idleSound = item.definition.GetCustomReference("IdleSound").GetComponent<AudioSource>();
            glowMaterial = item.definition.GetCustomReference("GlowMaterial").GetComponent<MeshRenderer>().material;
            glowLight = item.definition.GetCustomReference("GlowLight").GetComponent<Light>();
            startSounds = item.definition.GetCustomReference("StartSounds");
            stopSounds = item.definition.GetCustomReference("StopSounds");
        }
    }
}