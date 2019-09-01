using BS;

namespace TOR {
    public class ItemModuleBlasterBolt : ItemModule {
        public string[] deflectionMaterials = { "Lightsaber" };
        public float despawnTime = 2f;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemBlasterBolt>();
        }
    }
}
