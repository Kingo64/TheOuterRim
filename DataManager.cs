using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace ThunderRoad {
    public class DataManager : MonoBehaviour {
        public static DataManager local;

        public bool editorLoadAddressableBundles;

        public DataManager() {
            local = this;
        }

        private void OnValidate() {
            local = this;
        }

        private void Awake() {
            local = this;
        }

        public static T LoadLocalFile<T>(string fileName) {
            string localSavePath = GetLocalSavePath();
            if (File.Exists(localSavePath + fileName)) {
                try {
                    return JsonConvert.DeserializeObject<T>(File.ReadAllText(localSavePath + fileName), Catalog.GetJsonNetSerializerSettings());
                }
                catch (Exception ex) {
                    Debug.LogError(string.Concat(new string[] { "Cannot read file ", fileName, " (", ex.Message, ")" }));
                    return default;
                }
            }
            return default;
        }

        public static void SaveLocalFile(object obj, string fileName) {
            File.WriteAllText(GetLocalSavePath() + fileName, JsonConvert.SerializeObject(obj, Catalog.GetJsonNetSerializerSettings()));
        }

        public static string GetLocalSavePath() {
            string localMyGamesPath = GetLocalMyGamesPath("Saves/" + GameData.savesFolder);
            if (!Directory.Exists(localMyGamesPath)) {
                Directory.CreateDirectory(Path.GetDirectoryName(localMyGamesPath));
            }
            return localMyGamesPath;
        }

        public static string GetLocalMyGamesPath(string folderName) {
            string text2 = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/");
            text2 = string.Concat(new string[]
            {
                text2,
                "/My Games/",
                Application.productName,
                "/",
                folderName,
                "/"
            });
            try {
                if (!Directory.Exists(text2)) {
                    Directory.CreateDirectory(Path.GetDirectoryName(text2));
                }
                File.WriteAllText(text2 + "TestWrite.txt", "Test");
                File.Delete(text2 + "TestWrite.txt");
            }
            catch (Exception ex) {
                Debug.LogError(string.Concat(new string[]
                {
                    ex.Message,
                    "\nUnable to access ",
                    text2,
                    "\nFallback to persistentDataPath ",
                    Application.persistentDataPath
                }));
                text2 = Application.persistentDataPath + "/" + folderName + "/";
                if (!Directory.Exists(text2)) {
                    Directory.CreateDirectory(Path.GetDirectoryName(text2));
                }
            }
            return text2;
        }
    }
}
