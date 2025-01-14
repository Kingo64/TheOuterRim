using ThunderRoad;

namespace TOR {
    public class ItemModuleHelmet : ItemModule
    {
        // controls
        public string leftGripPrimaryAction;
        public string leftGripSecondaryAction;
        public string rightGripPrimaryAction;
        public string rightGripSecondaryAction;

        // standard refs
        public string light1ID;
        public string light2ID;
        public string lightSoundID;
        public string playSoundID;
        public string toggleSoundID;
        public string primaryModelID;
        public string secondaryModelID;

        public override void OnItemLoaded(Item item) {
            base.OnItemLoaded(item);
            item.OnSpawnEvent += AddCustomModules;
        }

        public void AddCustomModules(EventTime eventTime) {
            if (eventTime == EventTime.OnStart) return;
            Utils.AddModule<ItemHelmet>(item.gameObject);
            item.OnSpawnEvent -= AddCustomModules;
        }
    }
}
