using ThunderRoad;
using System.Collections.Generic;

namespace TOR {
    public class LevelModuleSaveManager : ThunderScript {

        public static SaveData saveData = new SaveData();

        public override void ScriptLoaded(ModManager.ModData modData) {
            base.ScriptLoaded(modData);
            EventManager.onLevelLoad += OnLevelLoad;
            EventManager.onLevelUnload += OnLevelUnload;
            EventManager.onReloadJson += OnReloadJson;
        }

        public override void ScriptDisable() {
            base.ScriptDisable();
            EventManager.onLevelLoad -= OnLevelLoad;
            EventManager.onLevelUnload -= OnLevelUnload;
            EventManager.onReloadJson -= OnReloadJson;
        }

        private void OnLevelLoad(LevelData levelData, LevelData.Mode mode, EventTime eventTime) {
            if (eventTime == EventTime.OnStart) {
                Load();
                ProcessDiscoveredItems();
            }
        }

        private void OnLevelUnload(LevelData levelData, LevelData.Mode mode, EventTime eventTime) {
            if (eventTime == EventTime.OnStart) {
                Save();
            }
        }

        private void OnReloadJson(EventTime eventTime) {
            if (eventTime == EventTime.OnEnd) {
                ProcessDiscoveredItems();
            }
        }

        public static void ProcessDiscoveredItems() {
            foreach (var itemId in saveData.discoveredItems) {
                try {
                    var item = Catalog.GetData<ItemData>(itemId);
                    item.allowedStorage = item.allowedStorage | ItemData.Storage.SandboxAllItems;
                }
                catch { }
            }
            //var containers = Object.FindObjectsOfType<Container>();
            //if (containers != null) {
            //    var contents = new List<ContainerData.Content>();
            //    foreach (CatalogData catalogData in Catalog.GetDataList(Catalog.Category.Item)) {
            //        ItemData itemData = (ItemData)catalogData;
            //        if (itemData.purchasable && itemData != null && itemData.prefabLocation != null) {
            //            contents.Add(new ContainerData.Content(itemData, null, null, 1));
            //        }
            //    }

            //    foreach (var container in containers) {
            //        if (container.loadContent == Container.LoadContent.Purchasable) {
            //            container.Load(contents);
            //        }
            //    }
            //}
        }

        public static void Load() {
            if (Player.characterData != null) {
                var data = DataManager.LoadLocalFile<SaveData>(Player.characterData.ID + ".tor_save");
                if (data != null) {
                    saveData = data;
                }
            }
        }

        public static void Save() {
            if (Player.characterData != null) {
                DataManager.SaveLocalFile(saveData, Player.characterData.ID + ".tor_save");
            }
        }
    }

    public class SaveData {
        public HashSet<string> discoveredItems = new HashSet<string>();
    }
}