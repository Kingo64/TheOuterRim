using ThunderRoad;
using System.Collections;
using System.Collections.Generic;

namespace TOR {
    public class LevelModuleSaveManager : LevelModule {

        public static SaveData saveData = new SaveData();

        public override IEnumerator OnLoadCoroutine() {
            EventManager.onLevelLoad += OnLevelLoad;
            EventManager.onLevelUnload += OnLevelUnload;
            EventManager.onReloadJson += OnReloadJson;
            yield break;
        }

        private void OnLevelLoad(LevelData levelData, EventTime eventTime) {
            if (eventTime == EventTime.OnEnd) {
                Load();
                ProcessDiscoveredItems();
            }
        }

        private void OnLevelUnload(LevelData levelData, EventTime eventTime) {
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
                    item.purchasable = true;
                }
                catch { }
            }
        }

        public static void Load() {
            if (DataManager.local.dataSource == DataManager.DataSource.Local) {
                if (Player.characterData != null) {
                    var data = DataManager.LoadLocalFile<SaveData>(Player.characterData.ID + ".tor_save");
                    if (data != null) {
                        saveData = data;
                    }
                }
            }
        }

        public static void Save() {
            if (DataManager.local.dataSource == DataManager.DataSource.Local) {
                if (Player.characterData != null) {
                    DataManager.SaveLocalFile(saveData, Player.characterData.ID + ".tor_save");
                }
            }
        }
    }

    public class SaveData {
        public HashSet<string> discoveredItems = new HashSet<string>();
    }
}