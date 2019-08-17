using BS;

namespace TOR {
    // This create an item module that can be referenced in the item JSON
    public class ItemModuleKyberCrystal : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemKyberCrystal>();
        }
    }
}
