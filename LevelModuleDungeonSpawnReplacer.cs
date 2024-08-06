using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using static ThunderRoad.CreatureSpawner;

namespace TOR {
    public class LevelModuleDungeonSpawnReplacer : LevelModule {
        public string creatureTable;
        public Dictionary<string, string> waveBackups = new Dictionary<string, string>();
        public int factionId;

        readonly Type creatureSpawnerType = typeof(CreatureSpawner);

        public override IEnumerator OnLoadCoroutine() {
            EventManager.onLevelLoad += OnLevelLoad;
            EventManager.onLevelUnload += OnLevelUnload;
            AreaManager.Instance.OnPlayerChangeAreaEvent += OnPlayerChangeAreaEvent; 
            yield break;
        }

        public override void OnUnload() {
            base.OnUnload();
            EventManager.onLevelLoad -= OnLevelLoad;
            EventManager.onLevelUnload -= OnLevelUnload;
            AreaManager.Instance.OnPlayerChangeAreaEvent -= OnPlayerChangeAreaEvent;
        }

        private void OnPlayerChangeAreaEvent(SpawnableArea newArea, SpawnableArea previousArea) {
            PatchArea(newArea.SpawnedArea);
        }

        private void OnLevelLoad(LevelData levelData, LevelData.Mode mode, EventTime eventTime) {
            if (eventTime == EventTime.OnEnd) {
                var tableData = Catalog.GetData<CreatureTable>(creatureTable);
                tableData.TryPick(out var creatureData);
                factionId = creatureData.factionId;

                waveBackups.Clear();
                foreach (SpawnableArea spawnableArea in AreaManager.Instance.CurrentTree) {
                    var area = spawnableArea.SpawnedArea;
                    while (area.creatures.Count > 0) {
                        Creature creature = area.creatures[0];
                        area.creatures.RemoveAt(0);
                        if (creature && creature != Player.currentCreature) creature.Despawn();
                    }

                    PatchArea(area);

                    var random = new System.Random(Level.seed + spawnableArea.managedId);
                    Level.current.StartCoroutine(InitCreature(area, random));
                }

                var allWaves = Catalog.GetDataList(Category.Wave);
                foreach (var wave in allWaves) {
                    if (wave.id.StartsWith("Dungeon")) {
                        var data = Catalog.GetData<WaveData>(wave.id);
                        PatchWave(data);
                    }
                }
            }
        }

        private void PatchArea(Area area) {
            foreach (var spawner in area.creatureSpawners) {
                spawner.creatureTableID = creatureTable;
            }

            foreach (var spawner in area.creatureNoLimiteSpawners) {
                spawner.creatureTableID = creatureTable;
            }

            foreach (WaveSpawner spawner in area.GetComponentsInChildren<WaveSpawner>(true)) {
                var data = Catalog.GetData<WaveData>(spawner.startWaveId, true);
                PatchWave(data);
                spawner.waveData = data;
                if (!spawner.isRunning) {
                    spawner.creatureQueue.Clear();
                    spawner.spawnedCreatures.Clear();
                }
            }
        }

        private void PatchWave(WaveData data) {
            if (!waveBackups.ContainsKey(data.id)) {
                waveBackups.Add(data.id, JsonUtility.ToJson(data));
            }
            foreach (var faction in data.factions) {
                faction.factionID = factionId;
            }
            foreach (var group in data.groups) {
                group.reference = WaveData.Group.Reference.Table;
                group.referenceID = creatureTable;
                group.creatureTableID = creatureTable;
                group.overrideContainer = false;
                group.overrideBrain = false;
            }
            data.OnCatalogRefresh();
        }

        public IEnumerator InitCreature(Area area, System.Random rng) {
            int ignoreMaxCount = area.creatureNoLimiteSpawners.Count;
            int num;
            for (int indexCreatureSpawnerNoLimit = 0; indexCreatureSpawnerNoLimit < ignoreMaxCount; indexCreatureSpawnerNoLimit = num + 1) {
                area.creatureNoLimiteSpawners[indexCreatureSpawnerNoLimit].Spawn(null, rng);
                Creature spawnedCreature = area.creatureNoLimiteSpawners[indexCreatureSpawnerNoLimit].GetSpawnedCreature(0);
                if (!(spawnedCreature == null)) {
                    while (!spawnedCreature.loaded) {
                        yield return null;
                    }
                    List<Holder> creatureholders = spawnedCreature.holders;
                    if (creatureholders != null && creatureholders.Count > 0) {
                        for (int i = 0; i < creatureholders.Count; i = num + 1) {
                            Holder holder = creatureholders[i];
                            while (holder.spawningItem) {
                                yield return null;
                            }
                            num = i;
                        }
                    }
                    area.RegisterCreature(spawnedCreature);
                    spawnedCreature.SetCull(area.spawnableArea.IsCulled);
                }
                num = indexCreatureSpawnerNoLimit;
            }
            if (area.spawnableArea.NumberCreature > 0) {
                int indexCreatureSpawnerNoLimit = Math.Min(area.spawnableArea.NumberCreature, area.creatureSpawners.Count);
                if (area.spawnableArea.isCreatureSpawnedExist == null) {
                    area.spawnableArea.isCreatureSpawnedExist = new bool[indexCreatureSpawnerNoLimit];
                }
                if (!area.spawnableArea.ResapawnDeadCreature && area.spawnableArea.isCreatureDead == null) {
                    area.spawnableArea.isCreatureDead = new bool[indexCreatureSpawnerNoLimit];
                }
                area.Shuffle(area.creatureSpawners, rng);
                for (int i = 0; i < indexCreatureSpawnerNoLimit; i = num + 1) {
                    if (!area.spawnableArea.isCreatureSpawnedExist[i] && (area.spawnableArea.ResapawnDeadCreature || !area.spawnableArea.isCreatureDead[i])) {
                        if (area.creatureSpawners[i].CurrentState == State.Spawned) {
                            creatureSpawnerType.GetProperty("CurrentState").SetValue(area.creatureSpawners[i], State.Init);
                        }
                        area.creatureSpawners[i].Spawn(null, rng);
                        if (area.creatureSpawners[i].CurrentState != State.Init) {
                            while (area.creatureSpawners[i].CurrentState != State.Spawned) {
                                yield return null;
                            }
                            Creature spawnedCreature = area.creatureSpawners[i].GetSpawnedCreature(0);
                            // area.spawnableArea.isCreatureSpawnedExist[i] = true;
                            spawnedCreature.currentArea = area.spawnableArea;
                            spawnedCreature.initialArea = area.spawnableArea;
                            spawnedCreature.areaSpawnerIndex = i;
                            area.RegisterCreature(spawnedCreature);
                            while (!spawnedCreature.loaded) {
                                yield return null;
                            }
                            List<Holder> creatureholders = spawnedCreature.holders;
                            if (creatureholders != null && creatureholders.Count > 0) {
                                for (int j = 0; j < creatureholders.Count; j = num + 1) {
                                    Holder holder = creatureholders[j];
                                    while (holder.spawningItem) {
                                        yield return null;
                                    }
                                    num = j;
                                }
                            }
                            spawnedCreature.SetCull(area.spawnableArea.IsCulled);
                        }
                    }
                    num = i;
                }
            }
            yield break;
        }

        private void OnLevelUnload(LevelData levelData, LevelData.Mode mode, EventTime eventTime) {
            foreach (var backup in waveBackups) {
                var data = Catalog.GetData<WaveData>(backup.Key, true);
                var waveData = JsonUtility.FromJson<WaveData>(backup.Value);
                data.groups = waveData.groups;
                data.factions = waveData.factions;
                data.OnCatalogRefresh();
            }
            waveBackups.Clear();
        }
    }
}
