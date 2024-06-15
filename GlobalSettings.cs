using ThunderRoad;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using IngameDebugConsole;
using System.Reflection;

namespace TOR {
    public class GlobalSettings : ThunderScript {
        public static GlobalSettings instance;

        public static ModOptionFloat[] OptionsPercentage() {
            int length = 101;
            float increment = 0.01f;
            float val = 0;

            ModOptionFloat[] options = new ModOptionFloat[length];
            for (int i = 0; i < options.Length; i++) {
                options[i] = new ModOptionFloat(val.ToString("0%"), val);
                val += increment;
            }
            return options;
        }

        public static ModOptionFloat[] OptionsFloatCenti() {
            // 0.0, 0.01, 0.02, 0.03 etc
            int length = 201;
            float increment = 0.01f;
            float val = 0;

            ModOptionFloat[] options = new ModOptionFloat[length];
            for (int i = 0; i < options.Length; i++) {
                options[i] = new ModOptionFloat(val.ToString("0.##"), val);
                val += increment;
            }
            return options;
        }

        public static ModOptionFloat[] OptionsFloatDeci() {
            // 0.0, 0.1, 0.2, 0.3 etc
            int length = 201;
            float increment = 0.1f;
            float val = 0;

            ModOptionFloat[] options = new ModOptionFloat[length];
            for (int i = 0; i < options.Length; i++) {
                options[i] = new ModOptionFloat(val.ToString("0.#"), val);
                val += increment;
            }
            return options;
        }

        public static ModOptionFloat[] OptionsFloatQuarter() {
            // 0.0, 0.25, 0.5, 0.75 etc
            int length = 201;
            float increment = 0.25f;
            float val = 0;

            ModOptionFloat[] options = new ModOptionFloat[length];
            for (int i = 0; i < options.Length; i++) {
                options[i] = new ModOptionFloat(val.ToString("0.##"), val);
                val += increment;
            }
            return options;
        }

        public static ModOptionInt[] OptionsResolution = {
            new ModOptionInt("64", 64),
            new ModOptionInt("128", 128),
            new ModOptionInt("256", 256),
            new ModOptionInt("512", 512),
            new ModOptionInt("1024", 1024),
            new ModOptionInt("2048", 2048)
        };

        [ModOption(name: "Automatic Reload", tooltip: "Automatically reload blaster when empty. (Default: True)", category = "Blasters", defaultValueIndex = 1)]
        public static bool BlasterAutomaticReload { get; set; }

        [ModOption(name: "Bolt Speed", tooltip: "Global blaster bolt speed multiplier. (Default: 1.0)", category = "Blasters", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float BlasterBoltSpeed { get; set; }

        [ModOption(name: "NPC Bolt Speed", tooltip: "Blaster bolt speed multiplier for NPCs. (Default: 1.0)", category = "Blasters", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float BlasterBoltSpeedNPC { get; set; }

        [ModOption(name: "Bolt instant despawn", tooltip: "Blaster bolts will instantly despawn upon collision detection rather than on next tick. Minor performance improvement but bolts may not be fully rendered. (Default: False)", category = "Blasters", defaultValueIndex = 0)]
        public static bool BlasterBoltInstantDespawn { get; set; }

        [ModOption(name: "Require Refill", tooltip: "Can only reload via a manual power cell refill. (Default: False)", category = "Blasters", defaultValueIndex = 0)]
        public static bool BlasterRequireRefill { get; set; }

        [ModOption(name: "3D Scopes", tooltip: "Use 3D simulated scope. (Default: True)", category = "Blasters", defaultValueIndex = 1)]
        public static bool BlasterScope3D {
            get => _blasterScope3D; 
            set {
                _blasterScope3D = value;
                ItemBlaster.all.ForEach(blaster => {
                    if (value) blaster.scope?.material.EnableKeyword("_3D_SCOPE"); else blaster.scope?.material.DisableKeyword("_3D_SCOPE");
                });
            }
        }
        public static bool _blasterScope3D;

        [ModOption(name: "Scope Resolution", tooltip: "Resolution of scope render in pixels. (Default: 512x512)", category = "Blasters", valueSourceName = nameof(OptionsResolution), defaultValueIndex = 3)]
        public static int BlasterScopeResolution {
            get => _blasterScopeResolution;
            set {
                _blasterScopeResolution = value;
                ItemBlaster.all.ForEach(blaster => blaster.SetupScope());
            }
        }
        public static int _blasterScopeResolution = 512;

        [ModOption(name: "Scope Reticles", tooltip: "Use scopes reticles. (Default: True)", category = "Blasters", defaultValueIndex = 1)]
        public static bool BlasterScopeReticles {
            get => _blasterScopeReticles;
            set {
                _blasterScopeReticles = value;
                ItemBlaster.all.ForEach(blaster => {
                    if (value) blaster.scope?.material.EnableKeyword("_USE_RETICLE"); else blaster.scope?.material.DisableKeyword("_USE_RETICLE");
                });
            }
        }
        private static bool _blasterScopeReticles = true;

        [ModOption(name: "Controls Hold Duration", tooltip: "Duration to hold button to detect a long press in seconds (s). (Default: 0.3s)", category = "General", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 3)]
        public static float ControlsHoldDuration { get; set; }

        [ModOption(name: "Activate On Recall", tooltip: "Automatically activate lightsaber when recalling. (Default: False)", category = "Lightsabers", defaultValueIndex = 0)]
        public static bool SaberActivateOnRecall { get; set; }

        [ModOption(name: "Blade Thickness", tooltip: "Lightsaber blade thickness multiplier - will impact gameplay. (Default: 1.0)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float SaberBladeThickness {
            get => _saberBladeThickness;
            set {
                _saberBladeThickness = value;
                ItemLightsaber.all.ForEach(lightsaber => {
                    for (int i = 0, l = lightsaber.blades.Length; i < l; i++) {
                        lightsaber.blades[i].UpdateBladeThickness(value);
                    }
                });
            }
        }
        public static float _saberBladeThickness = 1f;

        [ModOption(name: "Deactivate On Drop", tooltip: "Automatically deactivate lightsaber when dropped. (Default: False)", category = "Lightsabers", defaultValueIndex = 0)]
        public static bool SaberDeactivateOnDrop { get; set; }

        [ModOption(name: "Deactivate On Drop Delay", tooltip: "Time in seconds (s) until a lightsaber will automatically deactivate itself after being dropped. (Default: 3s)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatQuarter), defaultValueIndex = 12)] // def: 3f
        public static float SaberDeactivateOnDropDelay { get; set; }

        [ModOption(name: "Deflect Assist", tooltip: "Enable Deflect Assist module. Increases the deflection radius of lightsabers for both the player and NPCs. (Default: True)", category = "Lightsabers", defaultValueIndex = 1)]
        public static bool SaberDeflectAssist { get; set; }

        [ModOption(name: "Deflect Assist Distance", tooltip: "Deflect assist detection radius in metres. (Default: 0.25m)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatCenti), defaultValueIndex = 25)]
        public static float SaberDeflectAssistDistance { get; set; }

        [ModOption(name: "Deflect Return Chance", tooltip: "Percent chance deflect assist will return bolts to the shooter. (Default: 20%)", category = "Lightsabers", valueSourceName = nameof(OptionsPercentage), defaultValueIndex = 20)]
        public static float SaberDeflectAssistReturnChance { get; set; }

        [ModOption(name: "Deflect NPC Return Chance", tooltip: "Percent chance that saber NPCs will be able to perfectly return bolts to the shooter. (Default: 5%)", category = "Lightsabers", valueSourceName = nameof(OptionsPercentage), defaultValueIndex = 5)]
        public static float SaberDeflectAssistReturnNPCChance { get; set; }

        [ModOption(name: "Use Expensive Collisions", tooltip: "Reduces instances of lightsabers passing through each other. It uses Unity's most accurate collision detection system available. (Default: True)", category = "Lightsabers", defaultValueIndex = 1)]
        public static bool SaberExpensiveCollisions { get; set; }

        [ModOption(name: "Expensive Collisions Min Velocity", tooltip: "Minimum velocity (m/s) for lightsabers expensive collisions to enable. (Default: 8.0)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatQuarter), defaultValueIndex = 32)] // def: 8f
        public static float SaberExpensiveCollisionsMinVelocity { get; set; }

        [ModOption(name: "Saber NPC Attack Speed", tooltip: "Attack speed for force sensitive lightsaber wielders. High values will cause animation/physics anomalies. Applies to newly spawned NPCs. (Default: 1.2)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 12)]
        public static float SaberNPCAttackSpeed { get; set; }

        [ModOption(name: "Saber Throwing", tooltip: "Lightsabers are able to be thrown and recalled. (Default: True)", category = "Lightsabers", defaultValueIndex = 1)]
        public static bool SaberThrowable { get; set; }

        [ModOption(name: "Throw Min Velocity", tooltip: "Minimum velocity (m/s) for a thrown lightsaber to be able to be recalled. (Default: 7.0)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatQuarter), defaultValueIndex = 28)] // def: 7f
        public static float SaberThrowMinVelocity { get; set; }

        [ModOption(name: "Use Trails", tooltip: "Enable lightsaber trails. (Default: True)", category = "Lightsabers", defaultValueIndex = 1)]
        public static bool SaberTrailEnabled { get; set; }

        [ModOption(name: "Trail Duration", tooltip: "Time in seconds (s) a lightsaber trail will be visible. (Default: 0.04s)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatCenti), defaultValueIndex = 4)]
        public static float SaberTrailDuration { get; set; }

        [ModOption(name: "Length Adjust Increment", tooltip: "Amount of length to adjust on a lightsaber blade per use. (Default: 0.05m)", category = "Lightsaber Tool", valueSourceName = nameof(OptionsFloatCenti), defaultValueIndex = 5)]
        public static float LightsaberToolAdjustIncrement { get; set; }


        internal static AudioContainer SaberRecallSound { get; private set; }

        internal static AudioSource HandAudioLeft { get; private set; }
        internal static AudioSource HandAudioRight { get; private set; }

        internal static Dictionary<int, Collider[]> LightsaberColliders { get; private set; }

        public override void ScriptLoaded(ModManager.ModData modData) {
            base.ScriptLoaded(modData);
            instance = this;
            RunPrechecks();
            SetupConsoleCommands();
            Utils.Log("v" + Assembly.GetExecutingAssembly().GetName().Version + " - Settings file loaded successfully");
        }

        public override void ScriptEnable() {
            base.ScriptEnable();
            SceneManager.sceneLoaded += OnNewSceneLoaded;
            EventManager.onPossess += OnPossessionEvent;
        }

        public override void ScriptDisable() {
            base.ScriptDisable();
            SceneManager.sceneLoaded -= OnNewSceneLoaded;
            EventManager.onPossess -= OnPossessionEvent;
        }

        void OnPossessionEvent(Creature creature, EventTime eventTime) {
            if (eventTime == EventTime.OnEnd) {
                SetupHandAudio(creature);
            }
        }

        void OnNewSceneLoaded(Scene scene, LoadSceneMode mode) {
            LightsaberColliders = new Dictionary<int, Collider[]>();
        }

        void RunPrechecks() {
            if (!GameManager.options.postProcessing) Utils.LogWarning("Post-processing is currently disabled! Lightsabers will not render correctly unless it is turned on.");
            if (!GameManager.options.bloomEnabled) Utils.LogWarning("Bloom is currently disabled! Lightsabers will not render correctly unless it is turned on.");
            if (GameManager.options.bloomIntensity <= 0.05f) Utils.LogWarning("Bloom is currently set to a very low intensity. Lightsabers may not look correct as they are tuned for default game settings.");
        }

        void SetupConsoleCommands() {
            DebugLogConsole.AddCommand("tor_test_items", "Regression test: Spawn all items from TOR", () => {
                var items = Catalog.GetDataList<ItemData>();
                List<ContentCustomData> testData = new List<ContentCustomData> {
                     new ItemLightsaberSaveData {
                         kyberCrystals = new string[] { "" }
                     }
                };
                foreach (var itemData in items) {
                    if (string.IsNullOrEmpty(itemData.prefabAddress)) continue;
                    if (itemData.prefabAddress.Contains("theouterrim")) {
                        itemData.SpawnAsync(delegate (Item item) {
                            Utils.Log(itemData.id);
                            item.Despawn();
                        }, null, null, null, true, testData);
                    }
                }
            });
        }

        void SetupHandAudio(Creature creature) {
            var spatialBlend = 0.9f;
            var volume = 0.15f;

            HandAudioLeft = SetupHandAudioCreate(creature.handLeft.transform, "handAudioLeft", spatialBlend, volume);
            HandAudioRight = SetupHandAudioCreate(creature.handRight.transform, "handAudioRight", spatialBlend, volume);

            var fx = Catalog.GetData<EffectData>("LightsaberRecall", true);
            SaberRecallSound = ((EffectModuleAudio)fx.modules[0]).audioContainer;
        }

        AudioSource SetupHandAudioCreate(Transform parent, string name, float spatialBlend, float volume) {
            var audioSource = new GameObject(name, typeof(AudioSource));
            var handAudio = audioSource.GetComponent<AudioSource>();
            handAudio.spatialBlend = spatialBlend;
            handAudio.volume = volume;
            handAudio.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Effect);
            audioSource.transform.parent = parent;
            audioSource.transform.localPosition = Vector3.zero;
            return handAudio;
        }
    }
}