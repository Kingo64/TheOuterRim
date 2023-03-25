using ThunderRoad;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace TOR {
    // DELETE WHEN ALL HELMETS ARE PORTED TO MANIKIN
    public class LevelModuleLegacyHelmets : LevelModule {
        public string[] creatures = new string[]{
            "TORMale",
            "TORFemale",
            "CloneTrooper",
            "Stormtrooper",
        };
        HashSet<int> creatureHash;

        public static string HAT_HOLDER_NAME = "HolderHead";

        public override IEnumerator OnLoadCoroutine() {
            EventManager.onPossess += OnPossessionEvent;
            EventManager.onLevelLoad += OnLevelLoad;

            yield break;
        }

        private void OnLevelLoad(LevelData levelData, EventTime eventTime) {
            if (eventTime == EventTime.OnEnd) {
                creatureHash = Utils.HashArray(creatures);
                SetupHelmets();
            }
        }

        void OnPossessionEvent(Creature creature, EventTime eventTime) {
            if (eventTime == EventTime.OnStart) {
                SetupPlayerHelmet(creature);
            }
        }

        void SetupPlayerHelmet(Creature creature) {
            SetupHelmet(creature, Catalog.GetData<HolderData>(HAT_HOLDER_NAME, true));
            var holder = creature.holders.Find(x => x.name == HAT_HOLDER_NAME);
            if (holder && holder.HasSlotFree()) {
                foreach (ContainerData.Content content in Player.characterData.inventory) {
                    if (content.TryGetState<ContentStateHolder>(out var contentStateHolder) && contentStateHolder.holderName == HAT_HOLDER_NAME) {
                        holder.spawningItem = true;
                        content.Spawn(delegate (Item item) {
                            if (item) holder.Snap(item, true, false);
                            holder.spawningItem = false;
                        }, true);
                    }
                }
            }
        }

        void SetupHelmets() {
            Utils.Log("Configuring pooled creatures");
            var holderNPCHead = Catalog.GetData<HolderData>("HolderNPCHead", true);
            foreach (var id in creatureHash) {
                var pool = CreatureData.pools.Find((CreatureData.Pool p) => p.id == id);
                if (pool != null) {
                    foreach (var obj in pool.list) {
                        var pooledCreature = obj.GetComponent<Creature>();
                        SetupHelmet(pooledCreature, holderNPCHead.Clone() as HolderData);
                    }
                }
            }
        }

        public static void SetupHelmet(Creature creature, HolderData holderData) {
            var HAT_POSITION = new Vector3(-0.14f, 0, 0.02f);
            var HAT_ROTATION = Quaternion.Euler(0, 90, 90);

            var holderObject = new GameObject(HAT_HOLDER_NAME);
            holderObject.transform.SetParent(creature.ragdoll.headPart.meshBone);
            holderObject.transform.localPosition = HAT_POSITION;
            holderObject.transform.localRotation = HAT_ROTATION;
            Holder holder = holderObject.AddComponent<Holder>();
            holder.Load(holderData);
            creature.holders.Add(holder);
        }
    }
}
