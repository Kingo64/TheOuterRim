using ThunderRoad;

namespace TOR {
    public class ItemModuleBlasterBolt : ItemModule {
        public float drag;
        public float colliderScale = 1f;
        public string[] deflectionMaterials = { "Lightsaber", "Blaster" };
        public float despawnTime = 3f;
        public bool lockRotation;
        public bool useGravity;

        public int ricochetLimit;
        public float ricochetMaxAngle;

        public bool applyGlow;
        public bool disintegrate;
        public string effectID;
        public float boltHue;

        public string impactEffectID;
        public EffectData impactEffect;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemBlasterBolt>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }

        public override void OnItemDataRefresh(ItemData data) {
            base.OnItemDataRefresh(data);
            if (!string.IsNullOrEmpty(impactEffectID)) impactEffect = Catalog.GetData<EffectData>(impactEffectID, true);
        }
    }

    public class ProjectileData : CustomData {
        public string item;

        public string damagerID;
        public DamagerData damager;

        public float drag;
        public float colliderScale = 1f;
        public string[] deflectionMaterials = { "Lightsaber", "Blaster" };
        public float despawnTime = 3f;
        public bool lockRotation;
        public bool useGravity;

        public int ricochetLimit;
        public float ricochetMaxAngle;

        public bool applyGlow;
        public bool disintegrate;
        public string effectID;
        public float boltHue;

        public string impactEffectID;
        public EffectData impactEffect;

        public override void OnCatalogRefresh() {
            base.OnCatalogRefresh();
            if (!string.IsNullOrEmpty(damagerID)) damager = Catalog.GetData<DamagerData>(damagerID, true);
            if (!string.IsNullOrEmpty(impactEffectID)) impactEffect = Catalog.GetData<EffectData>(impactEffectID, true);
        }
    }
}
