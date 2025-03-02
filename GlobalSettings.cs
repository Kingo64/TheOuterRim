﻿using ThunderRoad;
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

        public static ModOptionString[] OptionsFuncButton = {
            new ModOptionString("Submit", "Submit")
        };

        public static ModOptionInt[] OptionsResolution = {
            new ModOptionInt("64", 64),
            new ModOptionInt("128", 128),
            new ModOptionInt("256", 256),
            new ModOptionInt("512", 512),
            new ModOptionInt("1024", 1024),
            new ModOptionInt("2048", 2048)
        };

        // BLASTERS
        [ModOption(name: "Automatic Reload", tooltip: "Automatically reload blaster when empty. (Default: True)", category = "Blasters", defaultValueIndex = 1)]
        public static bool BlasterAutomaticReload = true;

        [ModOption(name: "Bolt Damage", tooltip: "Global blaster bolt damage multiplier. (Default: 1.0)", category = "Blasters", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float BlasterBoltDamage {
            get => _blasterBoltDamage;
            set {
                _blasterBoltDamage = value;
                Utils.ModifyDamageModifiers("Blaster", _blasterBoltDamage);
            }
        }
        public static float _blasterBoltDamage = 1f;

        [ModOption(name: "Bolt Speed", tooltip: "Global blaster bolt speed multiplier. (Default: 1.0)", category = "Blasters", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float BlasterBoltSpeed = 1.0f;

        [ModOption(name: "NPC Bolt Speed", tooltip: "Blaster bolt speed multiplier for NPCs. (Default: 1.0)", category = "Blasters", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float BlasterBoltSpeedNPC = 1.0f;

        [ModOption(name: "Bolt instant despawn", tooltip: "Blaster bolts will instantly despawn upon collision detection rather than on next tick. Minor performance improvement but bolts may not be fully rendered. (Default: False)", category = "Blasters", defaultValueIndex = 0)]
        public static bool BlasterBoltInstantDespawn = false;

        [ModOption(name: "Overheat Cooling Rate", tooltip: "Speed at which blasters cool down before overheating. (Default: 1.0)", category = "Blasters", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float BlasterCoolingRate = 1.0f;

        [ModOption(name: "Overheat Heat Rate", tooltip: "Speed at which blasters heat up before overheating. (Default: 1.0)", category = "Blasters", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float BlasterOverheatRate = 1.0f;

        [ModOption(name: "Require Manual Refill", tooltip: "Can only reload via a manual power cell refill. (Default: False)", category = "Blasters", defaultValueIndex = 0)]
        public static bool BlasterRequireRefill = false;

        public static bool BlasterScope3D = true;

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
                    if (blaster.scope != null) {
                        if (value) blaster.scopeMaterialInstance.material.EnableKeyword("_USE_RETICLE"); else blaster.scopeMaterialInstance.material.DisableKeyword("_USE_RETICLE");
                    }
                });
            }
        }
        private static bool _blasterScopeReticles = true;

        // GENERAL
        [ModOption(name: "Controls Hold Duration", tooltip: "Duration to hold button to detect a long press in seconds (s). (Default: 0.3s)", category = "General", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 3)]
        public static float ControlsHoldDuration = 0.3f;

        [ModOption(name: "Jetpack activates with Jump button only", tooltip: "Prevents pushing the aiming joystick Up from activating the jetpacks. Instead can only be activated with the Jump button when in the air. (Default: False)", category = "General", defaultValueIndex = 0)]
        public static bool JetpackJumpButtonOnly = false;

        [ModOption(name: "TOR Soundtrack", tooltip: "Use the TOR non-dynamic soundtrack instead of vanilla dynamic music on TOR levels. Requires a reloading level to apply. (Default: True)", category = "General", defaultValueIndex = 1)]
        public static bool TORSoundtrack = true;

        [ModOption(name: "Thermal Detonator Damage", tooltip: "Multiplies the damage of thermal detonators. Higher = more damage. (Default: 1.0x)", category = "General", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float ThermalDetonatorDamage = 1.0f;

        [ModOption(name: "Thermal Detonator Range", tooltip: "Multiplies the blast range of thermal detonators. Higher = wider radius. (Default: 1.0x)", category = "General", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float ThermalDetonatorRange = 1.0f;

        // LIGHTSABERS
        [ModOption(name: "Activate On Recall", tooltip: "Automatically activate lightsaber when recalling. (Default: False)", category = "Lightsabers", defaultValueIndex = 0)]
        public static bool SaberActivateOnRecall = false;

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
        public static bool SaberDeactivateOnDrop = false;

        [ModOption(name: "Deactivate On Drop Delay", tooltip: "Time in seconds (s) until a lightsaber will automatically deactivate itself after being dropped. (Default: 3s)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatQuarter), defaultValueIndex = 12)] // def: 3f
        public static float SaberDeactivateOnDropDelay = 3f;

        [ModOption(name: "Deflect Assist", tooltip: "Enable Deflect Assist module. Increases the deflection radius of lightsabers for both the player and NPCs. (Default: True)", category = "Lightsabers", defaultValueIndex = 1)]
        public static bool SaberDeflectAssist = true;

        [ModOption(name: "Deflect Assist Distance", tooltip: "Deflect assist detection radius in metres. (Default: 0.25m)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatCenti), defaultValueIndex = 25)]
        public static float SaberDeflectAssistDistance = 0.25f;

        [ModOption(name: "Deflect Return Chance", tooltip: "Percent chance deflect assist will return bolts to the shooter. (Default: 20%)", category = "Lightsabers", valueSourceName = nameof(OptionsPercentage), defaultValueIndex = 20)]
        [ModOptionSlider]
        public static float SaberDeflectAssistReturnChance = 0.2f;

        [ModOption(name: "Deflect NPC Return Chance", tooltip: "Percent chance that saber NPCs will be able to perfectly return bolts to the shooter. (Default: 5%)", category = "Lightsabers", valueSourceName = nameof(OptionsPercentage), defaultValueIndex = 5)]
        [ModOptionSlider]
        public static float SaberDeflectAssistReturnNPCChance = 0.05f;

        [ModOption(name: "Expensive Collisions", tooltip: "Reduces instances of lightsabers passing through each other. It uses Unity's most accurate collision detection system available. (Default: True)", category = "Lightsabers", defaultValueIndex = 1)]
        public static bool SaberExpensiveCollisions = true;

        [ModOption(name: "Expensive Collisions Min Velocity", tooltip: "Minimum velocity (m/s) for lightsabers expensive collisions to enable. (Default: 8.0)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatQuarter), defaultValueIndex = 32)] // def: 8f
        public static float SaberExpensiveCollisionsMinVelocity = 8f;

        [ModOption(name: "Lightsaber Damage", tooltip: "Global lightsaber blade damage multiplier. (Default: 1.0)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float LightsaberDamage {
            get => _lightsaberDamage;
            set {
                _lightsaberDamage = value;
                Utils.ModifyDamageModifiers("LightsaberBlunt", _lightsaberDamage);
                Utils.ModifyDamageModifiers("LightsaberPierce", _lightsaberDamage);
                Utils.ModifyDamageModifiers("LightsaberSlash", _lightsaberDamage);
            }
        }
        public static float _lightsaberDamage = 1f;

        [ModOption(name: "Saber Throwing", tooltip: "Lightsabers are able to be thrown and recalled. (Default: True)", category = "Lightsabers", defaultValueIndex = 1)]
        public static bool SaberThrowable = true;

        [ModOption(name: "Throw Min Velocity", tooltip: "Minimum velocity (m/s) for a thrown lightsaber to be able to be recalled. (Default: 7.0)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatQuarter), defaultValueIndex = 28)] // def: 7f
        public static float SaberThrowMinVelocity = 7f;

        [ModOption(name: "Trails", tooltip: "Enable lightsaber trails. (Default: True)", category = "Lightsabers", defaultValueIndex = 1)]
        public static bool SaberTrailEnabled = true;

        [ModOption(name: "Trail Duration", tooltip: "Time in seconds (s) a lightsaber trail will be visible. (Default: 0.04s)", category = "Lightsabers", valueSourceName = nameof(OptionsFloatCenti), defaultValueIndex = 4)]
        public static float SaberTrailDuration = 0.04f;

        [ModOption(name: "Length Adjust Increment", tooltip: "Amount of length to adjust on a lightsaber blade per use. (Default: 0.05m)", category = "Lightsaber Tool", valueSourceName = nameof(OptionsFloatCenti), defaultValueIndex = 5)]
        public static float LightsaberToolAdjustIncrement = 0.05f;

        // NPCs
        [ModOption(name: "Allow NPC force powers", tooltip: "Determines whether force users will be able to cast force powers. Will render weaponless 'caster' NPCs useless. Disabling it makes you a big baby.", category = "NPCs", defaultValueIndex = 1)]
        public static bool AllowNPCForcePowers = true;

        [ModOption(name: "Cast force power delay multiplier", tooltip: "Multiplies the delay before an NPC is allowed to cast a force ability. Lower = more frequently and higher = less frequently. Applies when NPC spawns or enters combat. (Default: 1.0x)", category = "NPCs", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float CastForcePowerDelayMultiplier = 1.0f;

        [ModOption(name: "Force Push/Pull/Throw strength multiplier", tooltip: "Strength multiplier for the NPC force push/pull/throw abilities. Lower = weaker and higher = stronger. Applies to newly spawned NPCs. (Default: 1.0x)", category = "NPCs", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float NPCForceGravityMultiplier = 1.0f;

        [ModOption(name: "Blaster NPC two handing", category = "NPCs", defaultValueIndex = 1)]
        public static bool BlasterNPCTwoHanding = true;

        [ModOption(name: "Blaster NPC accuracy", tooltip: "Adjusts the accuracy of blaster wielding NPCs. Higher values make them more accurate. (Default: 1.0)", category = "NPCs", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 10)]
        public static float BlasterNPCAccuracy = 1.0f;

        [ModOption(name: "Blaster NPC fire upon death chance", tooltip: "Chance that when a blaster wielding NPC dies that they accidentally fire their weapon. (Default: 33%)", category = "NPCs", valueSourceName = nameof(OptionsPercentage), defaultValueIndex = 33)]
        [ModOptionSlider]
        public static float BlasterNPCFireUponDeathChance = 0.33f;

        [ModOption(name: "NPCs fire pooled bolts (higher perf + unstable)", tooltip: "NPCs will fire pooled bolt instances which may improve CPU performance and reduce stuttering. Disabled by default as blaster bolts may glitch out with enough combat. If blaster bolts glitch out you can regenerate the pools by clicking the button below. (Default: Off)", category = "NPCs", defaultValueIndex = 0)]
        public static bool BlasterNPCUsePooledBolts = false;

        [ModOption(name: "Regenerate all pooled items (debug)", tooltip: "Will regenerate all pooled items instances in the game. Use this if blaster bolts are glitching out.", category = "NPCs", valueSourceName = nameof(OptionsFuncButton), defaultValueIndex = 0)]
        [ModOptionButton]
        [ModOptionDontSave]
        public static void RegenerateAllPooledItems(string _) {
            if (ModManager.gameModsLoaded && ModManager.modCatalogAddressablesLoaded && LevelManager.IsPoolsGenerated) {
                ItemData.GeneratePool().AsSynchronous();
                Utils.Log("All item pools have been regenerated");
            }
        }

        [ModOption(name: "Saber NPC Attack Speed", tooltip: "Attack speed for force sensitive lightsaber wielders. High values will cause animation/physics anomalies. Applies to newly spawned NPCs. (Default: 1.2)", category = "NPCs", valueSourceName = nameof(OptionsFloatDeci), defaultValueIndex = 12)]
        public static float SaberNPCAttackSpeed = 1.2f;

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
            DebugLogConsole.AddCommand("tor_unload_assets", "Manually trigger UnloadUnusedAssets()", () => Resources.UnloadUnusedAssets());
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