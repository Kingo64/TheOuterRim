﻿using ThunderRoad;
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
        public float[] scopeReticleColour = { 0, 0, 0, 0};
        public float scopeReticleContrast = 1f;
        public bool scopeReticleUseBoltHue;

        public float[] scopeZoom = { 10f, 6f, 18f };

        // AI settings
        public int aiBurstAmount;
        public float aiShootDistanceMult = 1.0f;
        public bool aiMeleeEnabled;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemBlaster>(item.gameObject);
        }

        public override void OnItemDataRefresh(ItemData data) {
            base.OnItemDataRefresh(data);
            if (!string.IsNullOrEmpty(altFireSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(altFireSoundPath, x => altFireSoundAsset = x, null);
            if (!string.IsNullOrEmpty(altFireSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(altFireSoundPath2, x => altFireSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(chargeFireSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(chargeFireSoundPath, x => chargeFireSoundAsset = x, null);
            if (!string.IsNullOrEmpty(chargeFireSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(chargeFireSoundPath2, x => chargeFireSoundAsset2 = x, null);
            if (!string.IsNullOrEmpty(chargeFireSoundPath3)) Catalog.LoadAssetAsync<AudioContainer>(chargeFireSoundPath3, x => chargeFireSoundAsset3 = x, null);

            if (!string.IsNullOrEmpty(chargeSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(chargeSoundPath, x => chargeSoundAsset = x, null);
            if (!string.IsNullOrEmpty(chargeSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(chargeSoundPath2, x => chargeSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(chargeReadySoundPath)) Catalog.LoadAssetAsync<AudioContainer>(chargeReadySoundPath, x => chargeReadySoundAsset = x, null);
            if (!string.IsNullOrEmpty(chargeReadySoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(chargeReadySoundPath2, x => chargeReadySoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(chargeStartSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(chargeStartSoundPath, x => chargeStartSoundAsset = x, null);
            if (!string.IsNullOrEmpty(chargeStartSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(chargeStartSoundPath2, x => chargeStartSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(emptySoundPath)) Catalog.LoadAssetAsync<AudioContainer>(emptySoundPath, x => emptySoundAsset = x, null);
            if (!string.IsNullOrEmpty(emptySoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(emptySoundPath2, x => emptySoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(fireSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(fireSoundPath, x => fireSoundAsset = x, null);
            if (!string.IsNullOrEmpty(fireSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(fireSoundPath2, x => fireSoundAsset2 = x, null);
            if (!string.IsNullOrEmpty(fireSoundPath3)) Catalog.LoadAssetAsync<AudioContainer>(fireSoundPath3, x => fireSoundAsset3 = x, null);

            if (!string.IsNullOrEmpty(fireModeSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(fireModeSoundPath, x => fireModeSoundAsset = x, null);
            if (!string.IsNullOrEmpty(fireModeSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(fireModeSoundPath2, x => fireModeSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(overheatSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(overheatSoundPath, x => overheatSoundAsset = x, null);
            if (!string.IsNullOrEmpty(overheatSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(overheatSoundPath2, x => overheatSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(preFireSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(preFireSoundPath, x => preFireSoundAsset = x, null);
            if (!string.IsNullOrEmpty(preFireSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(preFireSoundPath2, x => preFireSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(reloadSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(reloadSoundPath, x => reloadSoundAsset = x, null);
            if (!string.IsNullOrEmpty(reloadSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(reloadSoundPath2, x => reloadSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(reloadEndSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(reloadEndSoundPath, x => reloadEndSoundAsset = x, null);
            if (!string.IsNullOrEmpty(reloadEndSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(reloadEndSoundPath2, x => reloadEndSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(spinStartSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(spinStartSoundPath, x => spinStartSoundAsset = x, null);
            if (!string.IsNullOrEmpty(spinStartSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(spinStartSoundPath2, x => spinStartSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(spinLoopSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(spinLoopSoundPath, x => spinLoopSoundAsset = x, null);
            if (!string.IsNullOrEmpty(spinLoopSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(spinLoopSoundPath2, x => spinLoopSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(spinStopSoundPath)) Catalog.LoadAssetAsync<AudioContainer>(spinStopSoundPath, x => spinStopSoundAsset = x, null);
            if (!string.IsNullOrEmpty(spinStopSoundPath2)) Catalog.LoadAssetAsync<AudioContainer>(spinStopSoundPath2, x => spinStopSoundAsset2 = x, null);

            if (!string.IsNullOrEmpty(scopeReticle)) Catalog.LoadAssetAsync<Texture2D>(scopeReticle, x => scopeReticleTexture = x, null);
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
