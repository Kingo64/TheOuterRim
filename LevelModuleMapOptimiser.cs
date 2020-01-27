using BS;
using UnityEngine;

namespace TOR {
    public class MapOptimiser : LevelModule {
        GameObject torOptimiser;

        public override void OnLevelLoaded(LevelDefinition levelDefinition) {
            foreach (GameObject obj in Object.FindObjectsOfType<GameObject>()) {
                if (obj.name == "ItemSelector") {
                    var comp = obj.AddComponent<OptimisedBook>();
                    comp.SetProps("ItemSpawner");
                } else if (obj.name == "WaveSelector") {
                    var comp = obj.AddComponent<OptimisedBook>();
                    comp.SetProps("WaveSelector");
                }
            }
            initialized = true;
        }

        public override void OnLevelUnloaded(LevelDefinition levelDefinition) {
            initialized = false;
        }

        public override void Update(LevelDefinition levelDefinition) {
            if (torOptimiser == null) {
                if (Player.local) {
                    torOptimiser = new GameObject("TOR_Optimiser", typeof(SphereCollider)) {
                        layer = 8
                    };
                    torOptimiser.GetComponent<SphereCollider>().isTrigger = true;
                    torOptimiser.transform.parent = Player.local.transform;
                    torOptimiser.transform.localPosition = Vector3.zero;
                }
            }
        }
    }

    public class OptimisedBook : MonoBehaviour {
        bool playerPresent;
        Collider triggerCollider;
        string selector;
        GameObject book;
        Inventory inventory;
        float multiplier = 8f;

        public void SetProps(string selector) {
            this.selector = selector;
            var optimiseCollider = transform.Find("OptimiseCollider");
            if (optimiseCollider != null) {
                triggerCollider = optimiseCollider.GetComponent<Collider>();
                book = transform.FindDeepChild(selector).gameObject;
                if (selector == "ItemSpawner") {
                    inventory = book.GetComponent<Inventory>();
                }
                DisableBook();
            }
        }

        void DisableBook() {
            try {
                if (selector == "ItemSpawner" && inventory != null) {
                    inventory.categoriesPage.SetActive(false);
                    inventory.itemsPage.SetActive(false);
                    inventory.itemStatsPage.SetActive(false);
                    inventory.itemPreviewPage.SetActive(false);
                } else book.SetActive(false);
            }
            catch { }
        }

        void EnableBook() {
            if (selector == "ItemSpawner" && inventory != null) {
                inventory.SetPageItemList();
            } else book.SetActive(true);
        }

        void OnTriggerEnter(Collider other) {
            if (!playerPresent) {
                if (other.name == "TOR_Optimiser") {
                    EnableBook();
                    playerPresent = true;
                    triggerCollider.transform.localScale *= multiplier;
                }
            }
        }

        void OnTriggerExit(Collider other) {
            if (playerPresent) {
                if (other.name == "TOR_Optimiser") {
                    DisableBook();
                    playerPresent = false;
                    triggerCollider.transform.localScale /= multiplier;
                }
            }
        }
    }
}
