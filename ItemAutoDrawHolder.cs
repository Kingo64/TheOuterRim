using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemAutoDrawHolder : MonoBehaviour {
        protected Item item;
        protected ItemModuleAutoDrawHolder module;

        readonly HashSet<Holder> holders = new HashSet<Holder>();
        readonly HashSet<Holder> drawToHolders = new HashSet<Holder>();

        float drawTime;
        bool isDrawing;

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleAutoDrawHolder>();

            foreach (var holderPath in module.holders) {
                var holderTransform = transform.Find(holderPath);
                var holder = holderTransform.GetComponent<Holder>();
                holders.Add(holder);
            }

            item.OnSnapEvent += OnSnap;
            item.OnUnSnapEvent += OnUnsnap;

            if (item.holder != null) SetupDrawToHolders(item.holder);
        }

        public void OnSnap(Holder holder) {
            SetupDrawToHolders(holder);
        }

        public void OnUnsnap(Holder holder) {
            isDrawing = false;
            drawToHolders.Clear();
        }

        public void SetupDrawToHolders(Holder holder) {
            if (!holder) return;
            var creature = holder.creature;
            if (!module.aiOnly || creature != Player.local.creature) {
                foreach (var holderPath in module.drawToHolders) {
                    var foundHolder = creature.holders.Find(x => x.name == holderPath);
                    if (foundHolder) drawToHolders.Add(foundHolder);
                }
                isDrawing = true;
            }
        }

        protected void Update() {
            drawTime -= Time.deltaTime;
            if (isDrawing && drawTime < 0) {
                drawTime = 1;

                if (drawToHolders == null || drawToHolders.Count == 0) {
                    isDrawing = false;
                }

                Holder vacantHolder = null;
                foreach (var holder in drawToHolders) {
                    if (holder.HasSlotFree()) {
                        vacantHolder = holder;
                        break;
                    }
                }

                if (vacantHolder) {
                    foreach (var holder in holders) {
                        if (holder.items.Count > 0) {
                            vacantHolder.Snap(holder.UnSnapOne());
                            break;
                        }
                    }
                }
            }
        }
    }
}