using ThunderRoad;

namespace TOR {
    public class ItemModuleAutoDrawHolder : ItemModule {
        public bool aiOnly = true;
        public string[] holders;
        public string[] drawToHolders = { "HipsLeft", "HipsRight", "BackLeft" };

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            Utils.AddModule<ItemAutoDrawHolder>(item.gameObject);
        }
    }
}
