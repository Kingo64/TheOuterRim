using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemAutoDrawHolder : ThunderBehaviour {
        public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

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
                if (!holderTransform) continue;

                var holder = holderTransform.GetComponent<Holder>();
                if (holder) holders.Add(holder);
            }

            item.OnSnapEvent += OnSnap;
            item.OnUnSnapEvent += OnUnsnap;

            if (item.holder != null) SetupDrawToHolders(item.holder);
        }

        protected void OnDestroy() {
            if (item != null) {
                item.OnSnapEvent -= OnSnap;
                item.OnUnSnapEvent -= OnUnsnap;
            }
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
            if (!creature) return;
            if (!module.aiOnly || creature != Player.local.creature) {
                drawToHolders.Clear();

                foreach (var holderPath in module.drawToHolders) {
                    var foundHolder = creature.holders.Find(x => x.name == holderPath);
                    if (foundHolder) drawToHolders.Add(foundHolder);
                }
                isDrawing = drawToHolders.Count > 0;
            }
        }

        protected override void ManagedUpdate() {
            drawTime -= Time.deltaTime;
            if (!isDrawing || drawTime >= 0) return;
            drawTime = 1;

            if (drawToHolders == null || drawToHolders.Count == 0) {
                isDrawing = false;
                return;
            }

            Holder vacantHolder = null;
            foreach (var holder in drawToHolders) {
                if (holder && holder.HasSlotFree()) {
                    vacantHolder = holder;
                    break;
                }
            }

            if (!vacantHolder) return;
            foreach (var holder in holders) {
                if (holder.items.Count > 0) {
                    vacantHolder.Snap(holder.UnSnapOne());
                    break;
                }
            }
        }
    }
}
