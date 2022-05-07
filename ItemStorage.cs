using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemStorage : MonoBehaviour {
        protected Item item;
        protected ItemModuleStorage module;

        readonly Dictionary<Holder, Container> holderContainer = new Dictionary<Holder, Container>();

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

                item.TryGetSavedValue(holderPath, out string containerData);
                if (!string.IsNullOrEmpty(containerData)) {
                    try {
                        var contents = JsonConvert.DeserializeObject<List<ContainerData.Content>>(containerData, Catalog.GetJsonNetSerializerSettings());
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
                    catch (Exception e) {
                        Debug.LogError(e);
                    }
                }
            }
        }

        void OnHolderSnapped(Item snappedItem) {
            if (ignoreSnaps) {
                return;
            }

            var holder = snappedItem.holder;
            holderContainer.TryGetValue(holder, out var container);
            container.contents.Clear();
            foreach (var storedItem in snappedItem.holder.items) {
                var containerData = new ContainerData.Content(storedItem.data, (storedItem.savedValues != null) ? new List<Item.SavedValue>(storedItem.savedValues) : null);
                container.contents.Add(containerData);
            }
            item.SetSavedValue(container.transform.name, JsonConvert.SerializeObject(container.contents, Catalog.GetJsonNetSerializerSettings()));
        }
    }
}