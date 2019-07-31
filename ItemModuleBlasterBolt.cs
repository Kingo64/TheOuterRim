using BS;

namespace TOR {
    public class ItemModuleBlasterBolt : ItemModule {
        public string[] deflectionMaterials = { "Lightsaber" };

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemBlasterBolt>();
        }
    }
}
