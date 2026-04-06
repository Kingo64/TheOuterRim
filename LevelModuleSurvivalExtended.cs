using System.Collections;
using System.Collections.Generic;
using ThunderRoad;

namespace TOR {
    public class LevelModuleSurvivalExtended : LevelModuleSurvival {
        public override IEnumerator OnLoadCoroutine() {
            if (Level.current.options != null) {
                if (Level.current.options.TryGetValue("rewardsToSpawn", out string val)) rewardsToSpawn = int.Parse(val);
                if (Level.current.options.TryGetValue("surivalWavePreset", out string wavePresetId)) {
                    var wavePreset = Catalog.GetData<SurvivalWavePreset>(wavePresetId);
                    waves = wavePreset.waves; 
                }
            }
            if (WaveSpawner.instances.Count > 0) {
                var customWaveSpawner = level.customReferences.Find(x => x.name == "SurvivalWaveSpawner");
                if (customWaveSpawner != null) {
                    waveSpawner = customWaveSpawner.transforms[0].GetComponent<WaveSpawner>();
                    WaveSpawner.instances.Remove(waveSpawner);
                    WaveSpawner.instances.Insert(0, waveSpawner);
                }
            }
            yield return base.OnLoadCoroutine();

            if (Level.current.options.TryGetValue("startingWave", out string startingWave)) {
                var i = int.Parse(startingWave);
                waveIndex = i - 1;
                waveDisplay = i - 1;
                currentWaveNumberForReward = i;
            }

            yield break;
        }
    }

    public class SurvivalWavePreset : CustomData {
        public List<LevelModuleSurvival.Waves> waves;
    }

    public class OptionSurvivalWavePreset : OptionStringList {
        public List<string> valueList = null;

        public override List<string> GetOptionList() {
            if (valueList == null) PopulateList();
            return valueList;
        }

        public void PopulateList() {
            valueList = new List<string>();
            var presets = Catalog.GetDataList<SurvivalWavePreset>();
            foreach (var preset in presets) {
                valueList.Add(preset.id);
            }
        }

        public override void SetOptionList(List<string> list) {
            valueList = list;
        }
    }
}
