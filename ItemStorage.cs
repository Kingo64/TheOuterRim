using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ThunderRoad;

namespace TOR {
    [Serializable]
    public class ItemStorageSaveData : ContentCustomData {
        public string data;
    }

    public class ItemStorage : ThunderBehaviour {
        protected Item item;
        protected ItemModuleStorage module;

        readonly Dictionary<Holder, Container> holderContainer = new Dictionary<Holder, Container>();
        public Dictionary<string, List<ContainerData.Content>> holderContents;

        bool ignoreSnaps;

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleStorage>();

            foreach (var holderPath in module.holders) {
                var holderTransform = transform.Find(holderPath);
                var holder = holderTransform.GetComponent<Holder>();
                var container = holderTransform.GetComponent<Container>();

                holderContainer.Add(holder, container);
                holder.Snapped += OnHolderSnapped;
                holder.UnSnapped += OnHolderSnapped;

                item.TryGetCustomData<ItemStorageSaveData>(out var savedData);
                if (savedData != null && !string.IsNullOrEmpty(savedData.data)) {
                    try {
                        var holderContents = JsonConvert.DeserializeObject<Dictionary<string, List<ContainerData.Content>>>(savedData.data, Catalog.GetJsonNetSerializerSettings());
                        holderContents.TryGetValue(holderPath, out var contents);
                        if (contents != null) {
                            foreach (var content in contents) {
                                content.OnCatalogRefresh();
                            }
                            container.Load(contents);
                            var itemsSnapped = 0;
                            foreach (var content in container.contents) {
                                ignoreSnaps = true;
                                content.OnCatalogRefresh();
                                content.Spawn(spawnedItem => {
                                    itemsSnapped++;
                                    holder.Snap(spawnedItem, true);
                                    if (itemsSnapped == container.contents.Count) {
                                        ignoreSnaps = false;
                                    }
                                });
                            }
                        }
                    }
                    catch (Exception e) {
                        Utils.LogError(e);
                    }
                }
            }
        }

        public void UpdateCustomData() {
            foreach (var entry in holderContainer) {
                if (!entry.Key) continue;
                holderContainer.TryGetValue(entry.Key, out var container);
                container.contents.Clear();
                foreach (var storedItem in entry.Key.items) {
                    var containerData = new ContainerData.Content(storedItem.data, null, (storedItem.contentCustomData != null) ? new List<ContentCustomData>(storedItem.contentCustomData) : null);
                    container.contents.Add(containerData);
                }
            }

            var saveData = new ItemStorageSaveData();
            var holderContents = new Dictionary<string, List<ContainerData.Content>>();

            foreach (var hc in holderContainer) {
                holderContents.Add(hc.Key.transform.name, hc.Value.contents);
            }
            saveData.data = JsonConvert.SerializeObject(holderContents, Catalog.GetJsonNetSerializerSettings());
            Utils.UpdateCustomData(item, saveData);
            var holder = item.holder;
            if (!holder) return;
            if (item.holder.linkedContainer) {
                for (int j = item.holder.linkedContainer.contents.Count - 1; j >= 0; j--) {
                    if (item.holder.linkedContainer.contents[j].TryGetState(out ContentStateHolder contentStateHolder) && contentStateHolder.holderName == item.holder.name) {
                        item.holder.linkedContainer.contents[j].customDataList = item.contentCustomData;
                    }
                }
            }
        }

        void OnHolderSnapped(Item snappedItem) {
            if (!snappedItem || ignoreSnaps) return;
            UpdateCustomData();
        }
    }
}