using ThunderRoad;

namespace TOR {
    public class ItemModuleBlasterBolt : ItemModule {
        public float colliderScale;
        public string[] deflectionMaterials = { "Lightsaber", "Blaster" };
        public float despawnTime = 3f;
        public bool lockRotation;
        public bool useGravity;

        public int ricochetLimit;
        public float ricochetMaxAngle;

        public bool applyGlow;
        public string effectID;
        public float boltHue;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemBlasterBolt>(item.gameObject);
        }
    }
}
