using ThunderRoad;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using System;

namespace TOR {
    public class LevelModuleSurvivalExtended : LevelModuleSurvival {
        public override IEnumerator OnLoadCoroutine() {
            if (Level.current.options != null) {
                if (Level.current.options.TryGetValue("rewardsToSpawn", out string val)) rewardsToSpawn = int.Parse(val);
            }
            yield return Catalog.LoadAssetCoroutine(rewardPillarAddress, new Action<GameObject>(OnPillarSpawn), "LevelModuleSurvival");
            waitingToChooseReward = false;
            currentWaveNumberForReward = 0;
            waveIndex = -1;
            waveDisplay = -1;
            DisableSandboxItems();
            SpawnRewardPillar();
            if (WaveSpawner.instances.Count > 0) {
                var customWaveSpawner = level.customReferences.Find(x => x.name == "SurvivalWaveSpawner");
                if (customWaveSpawner != null) {
                    waveSpawner = customWaveSpawner.transforms[0].GetComponent<WaveSpawner>();
                } else {
                    waveSpawner = WaveSpawner.instances[0];
                }
                waveSpawner.OnWaveWinEvent.AddListener(new UnityAction(OnWaveEnded));
                waveSpawner.OnWaveLossEvent.AddListener(new UnityAction(OnWaveEnded));
                waveSpawner.OnWaveCancelEvent.AddListener(new UnityAction(OnWaveEnded));
                EventManager.onCreatureKill += OnCreatureKill;
                EventManager.onPossess += OnPossessionEvent;
                EventManager.onUnpossess += OnUnpossessionEvent;
                yield break;
            }
            Debug.LogError("No wave spawner available for survival module!");
            yield break;
        }

        private void OnUnpossessionEvent(Creature creature, EventTime eventTime) {
            if (eventTime == EventTime.OnEnd) {
                Player local = Player.local;
                if (local?.creature) {
                    Holder[] holders = Player.local.creature.GetComponentsInChildren<Holder>();
                    for (int i = 0; i < holders.Length; i++) {
                        holders[i].Snapped -= OnHolderSnapped;
                    }
                }
            }
        }

        private void OnHolderSnapped(Item item) {
            if (waitingToChooseReward && canOnlyUseRewards) {
                item.Despawn();
            }
        }

        private void OnPillarSpawn(GameObject obj) {
            if (obj) {
                rewardPillarPrefab = obj.GetComponent<ArenaPillar>();
            }
        }
    }
}
