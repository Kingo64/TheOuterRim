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

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemKyberCrystal>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }

        public override void OnItemDataRefresh(ItemData data) {
            base.OnItemDataRefresh(data);
            if (!string.IsNullOrEmpty(whooshFX)) whoosh = Catalog.GetData<EffectData>(whooshFX, true);
        }

        public override System.Collections.IEnumerator LoadAddressableAssetsCoroutine(ItemData data) {
            if (!string.IsNullOrEmpty(idleSound)) yield return Catalog.LoadAssetCoroutine(idleSound, delegate (AudioContainer x) { idleSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(startSound)) yield return Catalog.LoadAssetCoroutine(startSound, delegate (AudioContainer x) { startSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(stopSound)) yield return Catalog.LoadAssetCoroutine(stopSound, delegate (AudioContainer x) { stopSoundAsset = x; }, GetType().Name);
            yield return base.LoadAddressableAssetsCoroutine(data);
            yield break;
        }

        public override void ReleaseAddressableAssets() {
            base.ReleaseAddressableAssets();
            Utils.ReleaseAsset(idleSoundAsset);
            Utils.ReleaseAsset(startSoundAsset);
            Utils.ReleaseAsset(stopSoundAsset);
        }
    }
}
