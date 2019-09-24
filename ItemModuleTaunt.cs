using BS;

namespace TOR {
    // This create an item module that can be referenced in the item JSON
    public class ItemModuleTaunt : ItemModule
    {
        public string gripID;
        public string tauntID;
        public string taunt2ID;
        public float aiTauntChance = 0.5f;

        // controls
        public string gripPrimaryAction = "";
        public string gripSecondaryAction = "playTaunt";

        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<ItemTaunt>();
        }
    }
}
