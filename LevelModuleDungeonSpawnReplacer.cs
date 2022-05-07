using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class LevelModuleDungeonSpawnReplacer : LevelModule {
        public string creatureTable;
        public Dictionary<string, string> groups = new Dictionary<string, string>();

        public override IEnumerator OnLoadCoroutine() {
            EventManager.onLevelLoad += OnLevelLoad;
            EventManager.onLevelUnload += OnLevelUnload;
            yield break;
        }

        public override void OnUnload() {
            base.OnUnload();
            EventManager.onLevelLoad -= OnLevelLoad;
            EventManager.onLevelUnload -= OnLevelUnload;
        }

        void OnLevelLoad(LevelData levelData, EventTime eventTime) {
            if (eventTime == EventTime.OnEnd) {
                groups.Clear();
                foreach (Room room in Level.current.dungeon.rooms) {
                    var creatures = new List<Creature>(room.creatures);
                    foreach (var creature in creatures) {
                        creature.Despawn();
                    }
                    room.spawnerNPCCount = 0;

                    var spawners = room.GetComponentsInChildren<CreatureSpawner>(true).Shuffle();
                    foreach (var spawner in spawners) {
                        spawner.creatureTableID = creatureTable;
                        if (spawner.ignoreRoomMaxNPC) {
                            spawner.Spawn();
                            if (spawner.spawning) room.spawnerNPCCount++;
                        } else if (room.spawnerNPCCount < Mathf.Min(Catalog.gameData.platformParameters.maxRoomNpc, room.spawnerMaxNPC)) {
                            spawner.Spawn();
                            if (spawner.spawning) room.spawnerNPCCount++;
                        }
                    }

                    foreach (WaveSpawner spawner in room.GetComponentsInChildren<WaveSpawner>(true)) {
                        var data = Catalog.GetData<WaveData>(spawner.startWaveId, true);
                        groups.Add(data.id, JsonUtility.ToJson(data));
                        foreach (var group in data.groups) {
                            group.reference = WaveData.Group.Reference.Table;
                            group.referenceID = creatureTable;
                            group.overrideContainer = false;
                            group.overrideBrain = false;
                        }
                        spawner.waveData = data;
                    }
                }
            }
        }

        private void OnLevelUnload(LevelData levelData, EventTime eventTime) {
            foreach (var group in groups) {
                var data = Catalog.GetData<WaveData>(group.Key, true);
                data.groups = JsonUtility.FromJson<WaveData>(group.Value).groups;
            }
            groups.Clear();
        }
    }
}
