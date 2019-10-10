using BS;

namespace TOR {
    // This create an item module that can be referenced in the item JSON
    public class ItemModuleKyberCrystal : ItemModule
    {
        public float[] bladeColour = { 1f, 1f, 1f, 1f };
        public float[] glowColour = { 1f, 1f, 1f, 1f };
        public float glowIntensity = 0.3f;
        public float glowRange = 5f;
        public bool isUnstable;

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemKyberCrystal>();
        }
    }
}
