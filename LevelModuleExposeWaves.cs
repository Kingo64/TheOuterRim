using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;

namespace TOR {
    public class LevelModuleExposeWaves : LevelModule {
        public HashSet<int> waveBackups;
        public int factionId;

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

        void OnLevelLoad(LevelData levelData, LevelData.Mode mode, EventTime eventTime) {
            if (eventTime == EventTime.OnStart) {
                UnrestrictWaves();
            }
        }

        private void OnLevelUnload(LevelData levelData, LevelData.Mode mode, EventTime eventTime) {
            if (eventTime == EventTime.OnStart) {
                RestoreWaves();
            }
        }

        public void UnrestrictWaves() {
            waveBackups = new HashSet<int>();
            var waves = Catalog.GetDataList(Category.Wave);
            foreach (var wave in waves.Cast<WaveData>()) {
                if (!wave.alwaysAvailable && wave.waveSelectors != null && wave.waveSelectors.Count > 0) {
                    waveBackups.Add(wave.hashId);
                    wave.alwaysAvailable = true;
                }
            }
        }

        public void RestoreWaves() {
            if (waveBackups == null) return;
            var waves = Catalog.GetDataList(Category.Wave);
            foreach (var wave in waves.Cast<WaveData>()) {
                wave.alwaysAvailable = false;
            }
            waveBackups = null;
        }
    }
}
