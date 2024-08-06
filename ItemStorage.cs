﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<string, List<ContainerContent>> holderContents;

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
                        var holderContents = JsonConvert.DeserializeObject<Dictionary<string, List<ContainerContent>>>(savedData.data, Catalog.GetJsonNetSerializerSettings());
                        holderContents.TryGetValue(holderPath, out var contents);
                        if (contents != null) {
                            foreach (var content in contents) {
                                content.OnCatalogRefresh();
                            }
                            container.Load(contents);
                            var itemsSnapped = 0;
                            foreach (var content in container.contents.Cast<ItemContent>()) {
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
                    var containerData = new ItemContent(storedItem.data, null, (storedItem.contentCustomData != null) ? new List<ContentCustomData>(storedItem.contentCustomData) : null);
                    container.contents.Add(containerData);
                }
            }

            var saveData = new ItemStorageSaveData();
            var holderContents = new Dictionary<string, List<ContainerContent>>();

            foreach (var hc in holderContainer) {
                holderContents.Add(hc.Key.transform.name, hc.Value.contents);
            }
            saveData.data = JsonConvert.SerializeObject(holderContents, Catalog.GetJsonNetSerializerSettings());
            Utils.UpdateCustomData(item, saveData);
        }

        void OnHolderSnapped(Item snappedItem) {
            if (!snappedItem || ignoreSnaps) return;
            UpdateCustomData();
        }
    }
}