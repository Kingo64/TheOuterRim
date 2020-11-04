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
        public float[] scopeReticleColour = { 0, 0, 0, 0};
        public float scopeReticleContrast = 1f;
        public bool scopeReticleUseBoltHue;

        public float[] scopeZoom = { 10f, 6f, 18f };

        // AI settings
        public float aiShootDistanceMult = 1.0f;
        public bool aiMeleeEnabled;

        public override void OnItemDataRefresh() {
            base.OnItemDataRefresh();
            if (!string.IsNullOrEmpty(altFireSoundPath)) altFireSoundAsset = CatalogData.GetPrefab<AudioContainer>("", altFireSoundPath);
            if (!string.IsNullOrEmpty(altFireSoundPath2)) altFireSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", altFireSoundPath2);

            if (!string.IsNullOrEmpty(chargeFireSoundPath)) chargeFireSoundAsset = CatalogData.GetPrefab<AudioContainer>("", chargeFireSoundPath);
            if (!string.IsNullOrEmpty(chargeFireSoundPath2)) chargeFireSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", chargeFireSoundPath2);
            if (!string.IsNullOrEmpty(chargeFireSoundPath3)) chargeFireSoundAsset3 = CatalogData.GetPrefab<AudioContainer>("", chargeFireSoundPath3);

            if (!string.IsNullOrEmpty(chargeSoundPath)) chargeSoundAsset = CatalogData.GetPrefab<AudioContainer>("", chargeSoundPath);
            if (!string.IsNullOrEmpty(chargeSoundPath2)) chargeSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", chargeSoundPath2);

            if (!string.IsNullOrEmpty(chargeReadySoundPath)) chargeReadySoundAsset = CatalogData.GetPrefab<AudioContainer>("", chargeReadySoundPath);
            if (!string.IsNullOrEmpty(chargeReadySoundPath2)) chargeReadySoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", chargeReadySoundPath2);
            
            if (!string.IsNullOrEmpty(chargeStartSoundPath)) chargeStartSoundAsset = CatalogData.GetPrefab<AudioContainer>("", chargeStartSoundPath);
            if (!string.IsNullOrEmpty(chargeStartSoundPath2)) chargeStartSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", chargeStartSoundPath2);

            if (!string.IsNullOrEmpty(emptySoundPath)) emptySoundAsset = CatalogData.GetPrefab<AudioContainer>("", emptySoundPath);
            if (!string.IsNullOrEmpty(emptySoundPath2)) emptySoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", emptySoundPath2);

            if (!string.IsNullOrEmpty(fireSoundPath)) fireSoundAsset = CatalogData.GetPrefab<AudioContainer>("", fireSoundPath);
            if (!string.IsNullOrEmpty(fireSoundPath2)) fireSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", fireSoundPath2);
            if (!string.IsNullOrEmpty(fireSoundPath3)) fireSoundAsset3 = CatalogData.GetPrefab<AudioContainer>("", fireSoundPath3);

            if (!string.IsNullOrEmpty(fireModeSoundPath)) fireModeSoundAsset = CatalogData.GetPrefab<AudioContainer>("", fireModeSoundPath);
            if (!string.IsNullOrEmpty(fireModeSoundPath2)) fireModeSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", fireModeSoundPath2);

            if (!string.IsNullOrEmpty(overheatSoundPath)) overheatSoundAsset = CatalogData.GetPrefab<AudioContainer>("", overheatSoundPath);
            if (!string.IsNullOrEmpty(overheatSoundPath2)) overheatSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", overheatSoundPath2);

            if (!string.IsNullOrEmpty(preFireSoundPath)) preFireSoundAsset = CatalogData.GetPrefab<AudioContainer>("", preFireSoundPath);
            if (!string.IsNullOrEmpty(preFireSoundPath2)) preFireSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", preFireSoundPath2);

            if (!string.IsNullOrEmpty(reloadSoundPath)) reloadSoundAsset = CatalogData.GetPrefab<AudioContainer>("", reloadSoundPath);
            if (!string.IsNullOrEmpty(reloadSoundPath2)) reloadSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", reloadSoundPath2);

            if (!string.IsNullOrEmpty(reloadEndSoundPath)) reloadEndSoundAsset = CatalogData.GetPrefab<AudioContainer>("", reloadEndSoundPath);
            if (!string.IsNullOrEmpty(reloadEndSoundPath2)) reloadEndSoundAsset2 = CatalogData.GetPrefab<AudioContainer>("", reloadEndSoundPath2);

            if (!string.IsNullOrEmpty(scopeReticle)) scopeReticleTexture = CatalogData.GetPrefab<Texture2D>("", scopeReticle);
        }

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemBlaster>();
        }
    }
}
