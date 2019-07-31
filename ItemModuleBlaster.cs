using BS;

namespace TOR {
    // This create an item module that can be referenced in the item JSON
    public class ItemModuleBlaster : ItemModule {
        // gun specs
        public bool automaticReload = true;
        public float altFireCooldown = 2f;
        public float bulletForce = 5000f;
        public float fireHaptic = 1f;
        public int[] fireModes = { -1, 1 };
        public float[] gunRPM = { 300 };
        public int magazineSize = 30;
        public float overheatRate = 1f;
        public float overheatThreshold = 50f;
        public float reloadTime = 1f;
        public bool recoil = true;
        public float[] recoilAngle = { -1000f, 1000f, 4000f, 6000f, -1000f, 1000f }; // x-min, x-max, y-min, y-max, z-min, z-max
        public float[] recoilForce = { 0f, 0f, 0f, 0f, -90f, -100f }; // x-min, x-max, y-min, y-max, z-min, z-max

        // controls
        public string gunGripPrimaryAction = "fire";
        public string gunGripSecondaryAction = "cycleFiremode";
        public string foreGripPrimaryAction = "";
        public string foreGripSecondaryAction = "altFire";
        public string scopeGripPrimaryAction = "";
        public string scopeGripSecondaryAction = "cycleScope";
        public string secondaryGripPrimaryAction = "";
        public string secondaryGripSecondaryAction = "";

        // standard refs
        public string altFireEffectID = "AltFireEffect";
        public string bulletSpawnID = "BulletSpawn";
        public string fireEffectID = "FireEffect";
        public string overheatEffectID = "OverheatEffect";
        public string projectileID = "BlasterBolt";
        public string scopeID = "Scope";
        public string scopeCameraID = "ScopeCamera";

        // grips
        public string foreGripID = "ForeGrip";
        public string gunGripID = "GunGrip";
        public string scopeGripID = "ScopeGrip";
        public string secondaryGripID;

        // alt fire settings
        public float altFireRange = 2f;

        // sounds
        public string altFireSoundsID = "AltFireSounds";
        public string altFireSoundsID2;
        public string emptySoundsID = "EmptySounds";
        public string emptySoundsID2;
        public string fireSoundsID = "FireSounds";
        public string fireSoundsID2;
        public string fireModeSoundsID = "FiremodeSounds";
        public string fireModeSoundsID2;
        public string overheatSoundsID = "OverheatSounds";
        public string overheatSoundsID2;
        public string reloadSoundsID = "ReloadSounds";
        public string reloadSoundsID2 = "ReloadFoleySounds";
        public string reloadEndSoundsID;
        public string reloadEndSoundsID2;

        // particles
        public bool altFireEffectDetachFromParent = true;
        public bool fireEffectDetachFromParent;

        // scope
        public bool hasScope = true;
        public int scopeDepth;
        public int[] scopeResolution = { 512, 512 };
        public float[] scopeZoom = { 10f, 6f, 18f };

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemBlaster>();
        }
    }
}
