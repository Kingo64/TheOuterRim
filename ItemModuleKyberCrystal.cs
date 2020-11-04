using ThunderRoad;

namespace TOR {
    public class ItemModuleKyberCrystal : ItemModule
    {
        public float[] bladeColour = { 1f, 1f, 1f, 1f };
        public float[] coreColour = { 1f, 1f, 1f, 1f };
        public float coreRadius;
        public float coreStrength;
        public float innerGlow = 0.25f;
        public float outerGlow = 5f;
        public float flicker = 0.4f;
        public float flickerSpeed = 2.68f;
        public float[] flickerScale = { 6f, 6f }; 

        public float[] glowColour = { 1f, 1f, 1f, 1f };
        public float glowIntensity = 1.0f;
        public float glowRange = 15f;

        public bool isUnstable;
        public string idleSound;
        public AudioContainer idleSoundAsset;
        public float idleSoundVolume;
        public float idleSoundPitch;
        public string startSound;
        public AudioContainer startSoundAsset;
        public float startSoundVolume;
        public float startSoundPitch;
        public string stopSound;
        public AudioContainer stopSoundAsset;
        public float stopSoundVolume;
        public float stopSoundPitch;
        public EffectData whoosh;
        public string whooshFX;

        public override void OnItemDataRefresh() {
            base.OnItemDataRefresh();
            if (!string.IsNullOrEmpty(idleSound)) idleSoundAsset = CatalogData.GetPrefab<AudioContainer>("", idleSound);
            if (!string.IsNullOrEmpty(startSound)) startSoundAsset = CatalogData.GetPrefab<AudioContainer>("", startSound);
            if (!string.IsNullOrEmpty(stopSound)) stopSoundAsset = CatalogData.GetPrefab<AudioContainer>("", stopSound);
            if (!string.IsNullOrEmpty(whooshFX)) whoosh = Catalog.GetData<EffectData>(whooshFX, true);
        }

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemKyberCrystal>();
        }
    }
}
