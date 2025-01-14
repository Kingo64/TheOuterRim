using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemModuleBlaster : ItemModule {
        // gun specs
        public bool automaticReload = true;
        public float bulletForce = 5000f;
        public float burstRPM;
        public float fireDelay;
        public float fireHaptic = 1f;
        public int[] fireModes = { -1, 1 };
        public float[] gunRPM = { 300 };
        public int magazineSize = 30;
        public bool multishot;
        public float overheatRate = 1f;
        public float overheatThreshold = 50f;
        public float reloadTime = 1f;

        // recoil
        public bool recoil = true;
        public float[] recoilAngle = { -1000f, 1000f, 4000f, 6000f, -1000f, 1000f }; // x-min, x-max, y-min, y-max, z-min, z-max
        public float[] recoilForce = { 0f, 0f, 0f, 0f, -90f, -100f }; // x-min, x-max, y-min, y-max, z-min, z-max

        // handling
        public float handlingBaseAccuracy;
        public float handlingInstabilityRate = 1f;
        public float handlingInstabilityMax = 5f;
        public float handlingInstabilityThreshold;
        public float handlingStabilityMultiplier = 10f;

        // damagers
        public string overrideBoltDamager;
        public string overrideBoltAltDamager;
        public string overrideBoltChargeDamager;

        // controls
        public string gunGripPrimaryAction;
        public string gunGripPrimaryActionHold;
        public string gunGripSecondaryAction;
        public string gunGripSecondaryActionHold;

        public string foreGripPrimaryAction;
        public string foreGripPrimaryActionHold;
        public string foreGripSecondaryAction;
        public string foreGripSecondaryActionHold;

        public string scopeGripPrimaryAction;
        public string scopeGripPrimaryActionHold;
        public string scopeGripSecondaryAction;
        public string scopeGripSecondaryActionHold;

        public string secondaryGripPrimaryAction;
        public string secondaryGripPrimaryActionHold;
        public string secondaryGripSecondaryAction;
        public string secondaryGripSecondaryActionHold;

        // standard refs
        public string altFireEffectID;
        public string ammoDisplayID;
        public string[] bulletSpawnIDs;
        public string chargeEffectID;
        public string fireEffectID;
        public string preFireEffectID;
        public string overheatEffectID;
        public string projectileID;
        public string scopeID;
        public string scopeCameraID;
        public string spinAnimatorID;

        // grips
        public string foreGripID;
        public string gunGripID;
        public string scopeGripID;
        public string secondaryGripID;

        // alt fire settings
        public float altFireRateMultiplier = 1f;
        public string altFireProjectileID;

        // charged fire settings
        public float chargeTime = 1f;
        public bool chargeMultishot = true;

        // spin up settings
        public float spinTime;
        public float spinSpeedMax = 1.5f;
        public float spinSpeedMinToFire = 1f;

        // sounds
        public AudioContainer altFireSoundAsset; public string altFireSoundPath; public string altFireSoundID;
        public AudioContainer altFireSoundAsset2; public string altFireSoundPath2; public string altFireSoundID2;

        public AudioContainer chargeFireSoundAsset; public string chargeFireSoundPath; public string chargeFireSoundID;
        public AudioContainer chargeFireSoundAsset2; public string chargeFireSoundPath2; public string chargeFireSoundID2;
        public AudioContainer chargeFireSoundAsset3; public string chargeFireSoundPath3; public string chargeFireSoundID3;

        public AudioContainer chargeSoundAsset; public string chargeSoundPath; public string chargeSoundID;
        public AudioContainer chargeSoundAsset2; public string chargeSoundPath2; public string chargeSoundID2;

        public AudioContainer chargeReadySoundAsset; public string chargeReadySoundPath; public string chargeReadySoundID;
        public AudioContainer chargeReadySoundAsset2; public string chargeReadySoundPath2; public string chargeReadySoundID2;

        public AudioContainer chargeStartSoundAsset; public string chargeStartSoundPath; public string chargeStartSoundID;
        public AudioContainer chargeStartSoundAsset2; public string chargeStartSoundPath2; public string chargeStartSoundID2;

        public AudioContainer emptySoundAsset; public string emptySoundPath; public string emptySoundID;
        public AudioContainer emptySoundAsset2; public string emptySoundPath2; public string emptySoundID2;

        public AudioContainer fireSoundAsset; public string fireSoundPath; public string fireSoundID;
        public AudioContainer fireSoundAsset2; public string fireSoundPath2; public string fireSoundID2;
        public AudioContainer fireSoundAsset3; public string fireSoundPath3; public string fireSoundID3;

        public AudioContainer fireModeSoundAsset; public string fireModeSoundPath; public string fireModeSoundID;
        public AudioContainer fireModeSoundAsset2; public string fireModeSoundPath2; public string fireModeSoundID2;

        public AudioContainer overheatSoundAsset; public string overheatSoundPath; public string overheatSoundID;
        public AudioContainer overheatSoundAsset2; public string overheatSoundPath2; public string overheatSoundID2;

        public AudioContainer preFireSoundAsset; public string preFireSoundPath; public string preFireSoundID;
        public AudioContainer preFireSoundAsset2; public string preFireSoundPath2; public string preFireSoundID2;

        public AudioContainer reloadSoundAsset; public string reloadSoundPath; public string reloadSoundID;
        public AudioContainer reloadSoundAsset2; public string reloadSoundPath2; public string reloadSoundID2;

        public AudioContainer reloadEndSoundAsset; public string reloadEndSoundPath; public string reloadEndSoundID;
        public AudioContainer reloadEndSoundAsset2; public string reloadEndSoundPath2; public string reloadEndSoundID2;

        public AudioContainer spinStartSoundAsset; public string spinStartSoundPath; public string spinStartSoundID;
        public AudioContainer spinStartSoundAsset2; public string spinStartSoundPath2; public string spinStartSoundID2;

        public AudioContainer spinLoopSoundAsset; public string spinLoopSoundPath; public string spinLoopSoundID;
        public AudioContainer spinLoopSoundAsset2; public string spinLoopSoundPath2; public string spinLoopSoundID2;

        public AudioContainer spinStopSoundAsset; public string spinStopSoundPath; public string spinStopSoundID;
        public AudioContainer spinStopSoundAsset2; public string spinStopSoundPath2; public string spinStopSoundID2;

        // particles
        public bool fireEffectUseBoltHue;
        public bool altFireEffectDetachFromParent = true;
        public bool fireEffectDetachFromParent;
        public bool preFireEffectDetachFromParent;

        // scope
        public bool hasScope;
        public int scopeDepth = 24;
        public float scopeEdgeWarp;
        public int[] scopeResolution;

        public string scopeReticle;
        public Texture2D scopeReticleTexture;
        public float[] scopeReticleColour = { 0, 0, 0, 0 };
        public float scopeReticleContrast = 1f;
        public bool scopeReticleUseBoltHue;

        public float[] scopeZoom = { 10f, 6f, 18f };

        // AI settings
        public int[] aiBurstAmounts;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemBlaster>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }

        public override System.Collections.IEnumerator LoadAddressableAssetsCoroutine(ItemData data) {
            if (!string.IsNullOrEmpty(altFireSoundPath)) yield return Catalog.LoadAssetCoroutine(altFireSoundPath, delegate (AudioContainer x) { altFireSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(altFireSoundPath2)) yield return Catalog.LoadAssetCoroutine(altFireSoundPath2, delegate (AudioContainer x) { altFireSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(chargeFireSoundPath)) yield return Catalog.LoadAssetCoroutine(chargeFireSoundPath, delegate (AudioContainer x) { chargeFireSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(chargeFireSoundPath2)) yield return Catalog.LoadAssetCoroutine(chargeFireSoundPath2, delegate (AudioContainer x) { chargeFireSoundAsset2 = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(chargeFireSoundPath3)) yield return Catalog.LoadAssetCoroutine(chargeFireSoundPath3, delegate (AudioContainer x) { chargeFireSoundAsset3 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(chargeSoundPath)) yield return Catalog.LoadAssetCoroutine(chargeSoundPath, delegate (AudioContainer x) { chargeSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(chargeSoundPath2)) yield return Catalog.LoadAssetCoroutine(chargeSoundPath2, delegate (AudioContainer x) { chargeSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(chargeReadySoundPath)) yield return Catalog.LoadAssetCoroutine(chargeReadySoundPath, delegate (AudioContainer x) { chargeReadySoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(chargeReadySoundPath2)) yield return Catalog.LoadAssetCoroutine(chargeReadySoundPath2, delegate (AudioContainer x) { chargeReadySoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(chargeStartSoundPath)) yield return Catalog.LoadAssetCoroutine(chargeStartSoundPath, delegate (AudioContainer x) { chargeStartSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(chargeStartSoundPath2)) yield return Catalog.LoadAssetCoroutine(chargeStartSoundPath2, delegate (AudioContainer x) { chargeStartSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(emptySoundPath)) yield return Catalog.LoadAssetCoroutine(emptySoundPath, delegate (AudioContainer x) { emptySoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(emptySoundPath2)) yield return Catalog.LoadAssetCoroutine(emptySoundPath2, delegate (AudioContainer x) { emptySoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(fireSoundPath)) yield return Catalog.LoadAssetCoroutine(fireSoundPath, delegate (AudioContainer x) { fireSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(fireSoundPath2)) yield return Catalog.LoadAssetCoroutine(fireSoundPath2, delegate (AudioContainer x) { fireSoundAsset2 = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(fireSoundPath3)) yield return Catalog.LoadAssetCoroutine(fireSoundPath3, delegate (AudioContainer x) { fireSoundAsset3 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(fireModeSoundPath)) yield return Catalog.LoadAssetCoroutine(fireModeSoundPath, delegate (AudioContainer x) { fireModeSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(fireModeSoundPath2)) yield return Catalog.LoadAssetCoroutine(fireModeSoundPath2, delegate (AudioContainer x) { fireModeSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(overheatSoundPath)) yield return Catalog.LoadAssetCoroutine(overheatSoundPath, delegate (AudioContainer x) { overheatSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(overheatSoundPath2)) yield return Catalog.LoadAssetCoroutine(overheatSoundPath2, delegate (AudioContainer x) { overheatSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(preFireSoundPath)) yield return Catalog.LoadAssetCoroutine(preFireSoundPath, delegate (AudioContainer x) { preFireSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(preFireSoundPath2)) yield return Catalog.LoadAssetCoroutine(preFireSoundPath2, delegate (AudioContainer x) { preFireSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(reloadSoundPath)) yield return Catalog.LoadAssetCoroutine(reloadSoundPath, delegate (AudioContainer x) { reloadSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(reloadSoundPath2)) yield return Catalog.LoadAssetCoroutine(reloadSoundPath2, delegate (AudioContainer x) { reloadSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(reloadEndSoundPath)) yield return Catalog.LoadAssetCoroutine(reloadEndSoundPath, delegate (AudioContainer x) { reloadEndSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(reloadEndSoundPath2)) yield return Catalog.LoadAssetCoroutine(reloadEndSoundPath2, delegate (AudioContainer x) { reloadEndSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(spinStartSoundPath)) yield return Catalog.LoadAssetCoroutine(spinStartSoundPath, delegate (AudioContainer x) { spinStartSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(spinStartSoundPath2)) yield return Catalog.LoadAssetCoroutine(spinStartSoundPath2, delegate (AudioContainer x) { spinStartSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(spinLoopSoundPath)) yield return Catalog.LoadAssetCoroutine(spinLoopSoundPath, delegate (AudioContainer x) { spinLoopSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(spinLoopSoundPath2)) yield return Catalog.LoadAssetCoroutine(spinLoopSoundPath2, delegate (AudioContainer x) { spinLoopSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(spinStopSoundPath)) yield return Catalog.LoadAssetCoroutine(spinStopSoundPath, delegate (AudioContainer x) { spinStopSoundAsset = x; }, GetType().Name);
            if (!string.IsNullOrEmpty(spinStopSoundPath2)) yield return Catalog.LoadAssetCoroutine(spinStopSoundPath2, delegate (AudioContainer x) { spinStopSoundAsset2 = x; }, GetType().Name);

            if (!string.IsNullOrEmpty(scopeReticle)) yield return Catalog.LoadAssetCoroutine(scopeReticle, delegate (Texture2D x) { scopeReticleTexture = x; }, GetType().Name);
            yield return base.LoadAddressableAssetsCoroutine(data);
            yield break;
        }

        public override void ReleaseAddressableAssets() {
            base.ReleaseAddressableAssets();
            Utils.ReleaseAsset(altFireSoundAsset);
            Utils.ReleaseAsset(altFireSoundAsset2);
            Utils.ReleaseAsset(chargeFireSoundAsset);
            Utils.ReleaseAsset(chargeFireSoundAsset2);
            Utils.ReleaseAsset(chargeFireSoundAsset3);
            Utils.ReleaseAsset(chargeSoundAsset);
            Utils.ReleaseAsset(chargeSoundAsset2);
            Utils.ReleaseAsset(chargeReadySoundAsset);
            Utils.ReleaseAsset(chargeReadySoundAsset2);
            Utils.ReleaseAsset(chargeStartSoundAsset);
            Utils.ReleaseAsset(chargeStartSoundAsset2);
            Utils.ReleaseAsset(emptySoundAsset);
            Utils.ReleaseAsset(emptySoundAsset2);
            Utils.ReleaseAsset(fireSoundAsset);
            Utils.ReleaseAsset(fireSoundAsset2);
            Utils.ReleaseAsset(fireSoundAsset3);
            Utils.ReleaseAsset(fireModeSoundAsset);
            Utils.ReleaseAsset(fireModeSoundAsset2);
            Utils.ReleaseAsset(overheatSoundAsset);
            Utils.ReleaseAsset(overheatSoundAsset2);
            Utils.ReleaseAsset(preFireSoundAsset);
            Utils.ReleaseAsset(preFireSoundAsset2);
            Utils.ReleaseAsset(reloadSoundAsset);
            Utils.ReleaseAsset(reloadSoundAsset2);
            Utils.ReleaseAsset(reloadEndSoundAsset);
            Utils.ReleaseAsset(reloadEndSoundAsset2);
            Utils.ReleaseAsset(spinStartSoundAsset);
            Utils.ReleaseAsset(spinStartSoundAsset2);
            Utils.ReleaseAsset(spinLoopSoundAsset);
            Utils.ReleaseAsset(spinLoopSoundAsset2);
            Utils.ReleaseAsset(spinStopSoundAsset);
            Utils.ReleaseAsset(spinStopSoundAsset2);

            Utils.ReleaseAsset(scopeReticleTexture);
        }
    }
}
