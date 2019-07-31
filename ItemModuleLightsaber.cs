using BS;

namespace TOR {
    // This create an item module that can be referenced in the item JSON
    public class ItemModuleLightsaber : ItemModule
    {
        public bool canThrow = true;
        public float ignitionDuration = 0.1f;
        public float throwSpeed = 7f;
        public LightsaberBlade[] lightsaberBlades = { new LightsaberBlade() };

        // controls
        public string primaryGripPrimaryAction = "";
        public string primaryGripSecondaryAction = "toggleIgnition";

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemLightsaber>();
        }
    }
}
