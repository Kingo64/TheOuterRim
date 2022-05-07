using ThunderRoad;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections;
using IngameDebugConsole;
using System.Reflection;

namespace TOR {
    [Description("Global settings for The Outer Rim|Global Settings")]
    public class GlobalSettings : LevelModule {

        [Description("Automatically reload blaster when empty|Automatic Reload"), Category("Blasters")]
        public static bool BlasterAutomaticReload { get; set; }
        public bool blasterAutomaticReload = true;

        [Description("Global blaster bolt speed multiplier|Bolt Speed"), Category("Blasters")]
        public static float BlasterBoltSpeed { get; set; }
        public float blasterBoltSpeed = 1f;

        [Description("Blaster bolt speed multiplier for NPCs|NPC Bolt Speed"), Category("Blasters")]
        public static float BlasterBoltSpeedNPC { get; set; }
        public float blasterBoltSpeedNPC = 1f;

        [Description("Time multiplier between NPC shots|NPC Attack Interval"), Category("Blasters")]
        public static float BlasterNPCAttackInterval { get; set; }
        public float blasterNPCAttackInterval = 1f;

        [Description("Inaccuracy multiplier for NPCs|NPC Inaccuracy"), Category("Blasters")]
        public static float BlasterNPCInaccuracy { get; set; }
        public float blasterNPCInaccuracy = 1f;

        [Description("Can only reload via a manual power cell refill|Require Refill"), Category("Blasters")]
        public static bool BlasterRequireRefill { get; set; }
        public bool blasterRequireRefill;

        [Description("Use 3D simulated scopes|3D Scopes"), Category("Blasters")]
        public static bool BlasterScope3D { get; set; }
        public bool blasterScope3D = true;

        [Description("Scope resolution - Width * Height pixels|Scope Resolution"), Category("Blasters")]
        public static int[] BlasterScopeResolution { get; set; }
        public int[] blasterScopeResolution = { 512, 512 };

        [Description("Use scopes reticles|Scope Reticles"), Category("Blasters")]
        public static bool BlasterScopeReticles { get; set; }
        public bool blasterScopeReticles = true;

        [Description("Duration to hold button to detect a long press in seconds (s)"), Category("General")]
        public static float ControlsHoldDuration { get; set; }
        public float controlsHoldDuration = 0.3f;

        [Description("Automatically activate lightsaber when recalling|Activate On Recall"), Category("Lightsabers")]
        public static bool SaberActivateOnRecall { get; set; }
        public bool saberActivateOnRecall;

        [Description("Lightsaber blade thickness multiplier - will impact gameplay.|Blade Thickness"), Category("Lightsabers")]
        public static float SaberBladeThickness { get; set; }
        public float saberBladeThickness = 1f;

        [Description("Automatically deactivate lightsaber when dropped|Deactivate On Drop"), Category("Lightsabers")]
        public static bool SaberDeactivateOnDrop { get; set; }
        public bool saberDeactivateOnDrop;

        [Description("Time in seconds (s) until a lightsaber will automatically deactivate itself after being dropped.|Deactivate On Drop Delay"), Category("Lightsabers")]
        public static float SaberDeactivateOnDropDelay { get; set; }
        public float saberDeactivateOnDropDelay = 3f;

        [Description("Enable Deflect Assist module. Increases the deflection radius of lightsabers. Applies to both player and NPCs.|Deflect Assist"), Category("Lightsabers")]
        public static bool SaberDeflectAssist { get; set; }
        public bool saberDeflectAssist = true;

        [Description("Deflect assist detection radius in metres|Deflect Assist Distance"), Category("Lightsabers")]
        public static float SaberDeflectAssistDistance { get; set; }
        public float saberDeflectAssistDistance = 0.25f;

        [Description("Percent chance deflect assist will return bolts to the shooter|Deflect Return Chance"), Category("Lightsabers")]
        public static float SaberDeflectAssistReturnChance { get; set; }
        public float saberDeflectAssistReturnChance = 0.2f;

        [Description("Percent chance that saber NPCs will be able to return bolts to the shooter|Deflect NPC Return Chance"), Category("Lightsabers")]
        public static float SaberDeflectAssistReturnNPCChance { get; set; }
        public float saberDeflectAssistReturnNPCChance = 0.05f;

        [Description("Reduces instances of lightsabers passing through each other. It uses Unity's most accurate collision detection system available.|Use Expensive Collisions"), Category("Lightsabers")]
        public static bool SaberExpensiveCollisions { get; set; }
        public bool saberExpensiveCollisions = true;

        [Description("Minimum velocity (m/s) for lightsabers expensive collisions to enable|Expensive Collisions Min Velocity"), Category("Lightsabers")]
        public static float SaberExpensiveCollisionsMinVelocity { get; set; }
        public float saberExpensiveCollisionsMinVelocity = 8.0f;

        [Description("Attack speed for force sensitive lightsaber wielders. High values will cause animation/physics anomalies.|Saber NPC Attack Speed"), Category("Lightsabers")]
        public static float SaberNPCAttackSpeed { get; set; }
        public float saberNPCAttackSpeed = 1.3f;

        [Description("Activate the 'NPC Recoil On Parry' setting|Saber NPC Override Recoil On Parry"), Category("Lightsabers")]
        public static bool SaberNPCOverrideRecoilOnParry { get; set; }
        public bool saberNPCOverrideRecoilOnParry = false;

        [Description("Force sensitive lightsaber wielders will recoil upon being parried rather than following through with the attack. Disable for more difficult gameplay.|Saber NPC Recoil On Parry"), Category("Lightsabers")]
        public static bool SaberNPCRecoilOnParry { get; set; }
        public bool saberNPCRecoilOnParry = true;

        [Description("Lightsabers are able to be thrown and recalled|Saber Throwing"), Category("Lightsabers")]
        public static bool SaberThrowable { get; set; }
        public bool saberThrowable = true;

        [Description("Minimum velocity (m/s) for a thrown lightsaber to be able to be recalled|Throw Min Velocity"), Category("Lightsabers")]
        public static float SaberThrowMinVelocity { get; set; }
        public float saberThrowMinVelocity = 7.0f;

        [Description("Enable lightsaber trails|Use Trails"), Category("Lightsabers")]
        public static bool SaberTrailEnabled { get; set; }
        public bool saberTrailEnabled = true;

        [Description("Time in seconds (s) a lightsaber trail will be visible|Trail Duration"), Category("Lightsabers")]
        public static float SaberTrailDuration { get; set; }
        public float saberTrailDuration = 0.04f;

        [Description("Minimum velocity (m/s) a lightsaber must be moving to generate a trail|Trail Min Velocity"), Category("Lightsabers")]
        public static float SaberTrailMinVelocity { get; set; }
        public float saberTrailMinVelocity;

        [Description("Adds legacy (U8.3 and earlier) helmet support to the game - may conflict with U8.4+ helmets"), Category("General")]
        public static bool UseLegacyHelmets { get; set; }
        public bool useLegacyHelmets = true;

        [Description("[Non-Dungeon Only] Disables the problematic physics culling on creatures introduced in U10 - fixes NPC vs NPC combat")]
        public static bool DisableCreaturePhysicsCulling { get; set; }
        public bool disableCreaturePhysicsCulling;

        [Description("[Dungeon Only] Disables the problematic physics culling on creatures introduced in U10 - fixes NPC vs NPC combat")]
        public static bool DisableCreaturePhysicsCullingDungeon { get; set; }
        public bool disableCreaturePhysicsCullingDungeon;

        public List<string> loadingTips;

        internal static AudioContainer SaberRecallSound { get; private set; }

        internal static AudioSource HandAudioLeft { get; private set; }
        internal static AudioSource HandAudioRight { get; private set; }

        internal static Dictionary<int, Collider[]> LightsaberColliders { get; private set; }

        public override IEnumerator OnLoadCoroutine() {
            SetJsonValues();
            SetupConsoleCommands();
            SetupLoadingTips();
            SceneManager.sceneLoaded += OnNewSceneLoaded;
            EventManager.onPossess += OnPossessionEvent;
            EventManager.onReloadJson += OnReloadJson;
            EventManager.onLevelLoad += OnLevelLoad;

            yield break;
        }

        private void OnLevelLoad(LevelData levelData, EventTime eventTime) {
            if (eventTime == EventTime.OnEnd) {
                SetupHelmets();
            }
        }

        private void OnReloadJson(EventTime eventTime) {
            if (eventTime == EventTime.OnEnd) {
                SetJsonValues();
            }
        }

        void OnPossessionEvent(Creature creature, EventTime eventTime) {
            if (eventTime == EventTime.OnStart) {
                SetupPlayerHelmet(creature);
            } else if (eventTime == EventTime.OnEnd) {
                SetupHandAudio(creature);
            }
        }

        void OnNewSceneLoaded(Scene scene, LoadSceneMode mode) {
            SetJsonValues();
            LightsaberColliders = new Dictionary<int, Collider[]>();
        }

        void SetJsonValues() {
            BlasterAutomaticReload = blasterAutomaticReload;
            BlasterBoltSpeed = blasterBoltSpeed;
            BlasterBoltSpeedNPC = blasterBoltSpeedNPC;
            BlasterNPCAttackInterval = blasterNPCAttackInterval;
            BlasterNPCInaccuracy = blasterNPCInaccuracy;
            BlasterRequireRefill = blasterRequireRefill;
            BlasterScope3D = blasterScope3D;
            BlasterScopeResolution = blasterScopeResolution;
            BlasterScopeReticles = blasterScopeReticles;
            ControlsHoldDuration = Mathf.Abs(controlsHoldDuration);
            SaberActivateOnRecall = saberActivateOnRecall;
            SaberBladeThickness = saberBladeThickness;
            SaberDeactivateOnDrop = saberDeactivateOnDrop;
            SaberDeactivateOnDropDelay = Mathf.Abs(saberDeactivateOnDropDelay);
            SaberDeflectAssist = saberDeflectAssist;
            SaberDeflectAssistDistance = Mathf.Abs(saberDeflectAssistDistance);
            SaberDeflectAssistReturnChance = Mathf.Clamp(saberDeflectAssistReturnChance, 0, 1);
            SaberDeflectAssistReturnNPCChance = Mathf.Clamp(saberDeflectAssistReturnNPCChance, 0, 1);
            SaberExpensiveCollisions = saberExpensiveCollisions;
            SaberExpensiveCollisionsMinVelocity = Mathf.Abs(saberExpensiveCollisionsMinVelocity);
            SaberNPCAttackSpeed = Mathf.Max(saberNPCAttackSpeed, 0.01f);
            SaberNPCOverrideRecoilOnParry = saberNPCOverrideRecoilOnParry;
            SaberNPCRecoilOnParry = saberNPCRecoilOnParry;
            SaberThrowable = saberThrowable;
            SaberThrowMinVelocity = Mathf.Abs(saberThrowMinVelocity);
            SaberTrailEnabled = saberTrailEnabled;
            SaberTrailDuration = Mathf.Abs(saberTrailDuration);
            SaberTrailMinVelocity = Mathf.Abs(saberTrailMinVelocity);
            UseLegacyHelmets = useLegacyHelmets;
            DisableCreaturePhysicsCulling = disableCreaturePhysicsCulling;
            DisableCreaturePhysicsCullingDungeon = disableCreaturePhysicsCullingDungeon;

            Debug.Log("The Outer Rim v" + Assembly.GetExecutingAssembly().GetName().Version + ": Settings file loaded successfully");
        }

        void SetupConsoleCommands() {
            string GetDescription(string property) {
                var attribute = typeof(GlobalSettings).GetProperty(property).GetCustomAttribute<DescriptionAttribute>();
                return attribute.Description;
            }

            DebugLogConsole.AddCommand("tor_blaster_automatic_reload", GetDescription("BlasterAutomaticReload"), (bool enabled) => blasterAutomaticReload = BlasterAutomaticReload = enabled);
            DebugLogConsole.AddCommand("tor_blaster_bolt_speed", GetDescription("BlasterBoltSpeed"), (float multiplier) => blasterBoltSpeed = BlasterBoltSpeed = multiplier);
            DebugLogConsole.AddCommand("tor_blaster_bolt_speed_npc", GetDescription("BlasterBoltSpeedNPC"), (float multiplier) => blasterBoltSpeedNPC = BlasterBoltSpeedNPC = multiplier);
            DebugLogConsole.AddCommand("tor_blaster_npc_attack_interval", GetDescription("BlasterNPCAttackInterval"), (float multiplier) => blasterNPCAttackInterval = BlasterNPCAttackInterval = multiplier);
            DebugLogConsole.AddCommand("tor_blaster_npc_inaccuracy", GetDescription("BlasterNPCInaccuracy"), (float multiplier) => blasterNPCInaccuracy = BlasterNPCInaccuracy = multiplier);
            DebugLogConsole.AddCommand("tor_blaster_require_refill", GetDescription("BlasterRequireRefill"), (bool enabled) => blasterRequireRefill = BlasterRequireRefill = enabled);
            DebugLogConsole.AddCommand("tor_blaster_scope_3d", GetDescription("BlasterScope3D"), (bool enabled) => {
            blasterScope3D = BlasterScope3D = enabled;
                ItemBlaster.all.ForEach(blaster => {
                    if (enabled) blaster.scope?.material.EnableKeyword("_3D_SCOPE"); else blaster.scope?.material.DisableKeyword("_3D_SCOPE");
                });
            });
            DebugLogConsole.AddCommand("tor_blaster_scope_resolution", GetDescription("BlasterScopeResolution"), (int x, int y) => {
                blasterScopeResolution = BlasterScopeResolution = new int[] { x, y };
                ItemBlaster.all.ForEach(blaster => blaster.SetupScope());
            });
            DebugLogConsole.AddCommand("tor_blaster_scope_reticles", GetDescription("BlasterScopeReticles"), (bool enabled) => {
                blasterScopeReticles = BlasterScopeReticles = enabled;
                ItemBlaster.all.ForEach(blaster => {
                    blaster.scope?.material.SetTexture("_Reticle", enabled ? blaster.module.scopeReticleTexture : null);
                });
            });
            DebugLogConsole.AddCommand("tor_controls_hold_duration", GetDescription("ControlsHoldDuration"), (float duration) => controlsHoldDuration = ControlsHoldDuration = duration);
            DebugLogConsole.AddCommand("tor_disable_physics_culling", GetDescription("DisableCreaturePhysicsCulling"), (bool enabled) => {
                disableCreaturePhysicsCulling = DisableCreaturePhysicsCulling = enabled;
                LevelModuleFixCreaturePhysics.SetAllCreaturePhysics();
            });
            DebugLogConsole.AddCommand("tor_disable_physics_culling_dungeon", GetDescription("DisableCreaturePhysicsCullingDungeon"), (bool enabled) => {
                disableCreaturePhysicsCullingDungeon = DisableCreaturePhysicsCullingDungeon = enabled;
                LevelModuleFixCreaturePhysics.SetAllCreaturePhysics();
            });
            DebugLogConsole.AddCommand("tor_saber_activate_on_recall", GetDescription("SaberActivateOnRecall"), (bool enabled) => saberActivateOnRecall = SaberActivateOnRecall = enabled);
            DebugLogConsole.AddCommand("tor_saber_blade_thickness", GetDescription("SaberBladeThickness"), (float multiplier) => saberBladeThickness = SaberBladeThickness = multiplier);
            DebugLogConsole.AddCommand("tor_saber_deactivate_on_drop", GetDescription("SaberDeactivateOnDrop"), (bool enabled) => saberDeactivateOnDrop = SaberDeactivateOnDrop = enabled);
            DebugLogConsole.AddCommand("tor_saber_deactivate_on_drop_delay", GetDescription("SaberDeactivateOnDropDelay"), (float delay) => saberDeactivateOnDropDelay = SaberDeactivateOnDropDelay = delay);
            DebugLogConsole.AddCommand("tor_saber_deflect_assist", GetDescription("SaberDeflectAssist"), (bool enabled) => saberDeflectAssist = SaberDeflectAssist = enabled);
            DebugLogConsole.AddCommand("tor_saber_deflect_assist_distance", GetDescription("SaberDeflectAssistDistance"), (float distance) => saberDeflectAssistDistance = SaberDeflectAssistDistance = distance);
            DebugLogConsole.AddCommand("tor_saber_deflect_assist_return_chance", GetDescription("SaberDeflectAssistReturnChance"), (float percent) => saberDeflectAssistReturnChance = SaberDeflectAssistReturnChance = percent);
            DebugLogConsole.AddCommand("tor_saber_deflect_assist_return_npc_chance", GetDescription("SaberDeflectAssistReturnNPCChance"), (float percent) => saberDeflectAssistReturnNPCChance = SaberDeflectAssistReturnNPCChance = percent);
            DebugLogConsole.AddCommand("tor_saber_expensive_collisions", GetDescription("SaberExpensiveCollisions"), (bool enabled) => saberExpensiveCollisions = SaberExpensiveCollisions = enabled);
            DebugLogConsole.AddCommand("tor_saber_expensive_collisions_min_velocity", GetDescription("SaberExpensiveCollisionsMinVelocity"), (float velocity) => saberExpensiveCollisionsMinVelocity = SaberExpensiveCollisionsMinVelocity = velocity);
            DebugLogConsole.AddCommand("tor_saber_npc_attack_speed", GetDescription("SaberNPCAttackSpeed"), (float speed) => saberNPCAttackSpeed = SaberNPCAttackSpeed = speed);
            DebugLogConsole.AddCommand("tor_saber_npc_override_recoil_on_parry", GetDescription("SaberNPCOverrideRecoilOnParry"), (bool enabled) => saberNPCOverrideRecoilOnParry = SaberNPCOverrideRecoilOnParry = enabled);
            DebugLogConsole.AddCommand("tor_saber_npc_recoil_on_parry", GetDescription("SaberNPCRecoilOnParry"), (bool enabled) => saberNPCRecoilOnParry = SaberNPCRecoilOnParry = enabled);
            DebugLogConsole.AddCommand("tor_saber_throw", GetDescription("SaberThrowable"), (bool enabled) => saberThrowable = SaberThrowable = enabled);
            DebugLogConsole.AddCommand("tor_saber_throw_min_velocity", GetDescription("SaberThrowMinVelocity"), (float velocity) => saberThrowMinVelocity = SaberThrowMinVelocity = velocity);
            DebugLogConsole.AddCommand("tor_saber_trail", GetDescription("SaberTrailEnabled"), (bool enabled) => saberTrailEnabled = SaberTrailEnabled = enabled);
            DebugLogConsole.AddCommand("tor_saber_trail_duration", GetDescription("SaberTrailDuration"), (float duration) => saberTrailDuration = SaberTrailDuration = duration);
            DebugLogConsole.AddCommand("tor_saber_trail_min_velocity", GetDescription("SaberTrailMinVelocity"), (float velocity) => saberTrailMinVelocity = SaberTrailMinVelocity = velocity);
            DebugLogConsole.AddCommand("tor_use_legacy_helmets", GetDescription("UseLegacyHelmets"), (bool enabled) => useLegacyHelmets = UseLegacyHelmets = enabled);
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
            handAudio.outputAudioMixerGroup = GameManager.local.audioMixer.FindMatchingGroups("Effect")[0];
            audioSource.transform.parent = parent;
            audioSource.transform.localPosition = Vector3.zero;
            return handAudio;
        }

        void SetupPlayerHelmet(Creature creature) {
            if (UseLegacyHelmets) {
                SetupHelmet(creature, Catalog.GetData<HolderData>(HAT_HOLDER_NAME, true));
                var holder = creature.equipment.holders.Find(x => x.name == HAT_HOLDER_NAME);
                if (holder && holder.HasSlotFree()) {
                    foreach (ContainerData.Content content in Player.characterData.inventory) {
                        if (content.TryGetCustomValue(SavedValueID.Holder.ToString(), out string savedHolder)) {
                            if (savedHolder == HAT_HOLDER_NAME) {
                                content.Spawn(delegate (Item item) { if (item) holder.Snap(item, true); }, true);
                            }
                        }
                    }
                }
            }
        }

        void SetupHelmets() {
            Debug.Log("The Outer Rim: Configuring pooled creatures");
            var holderNPCHead = Catalog.GetData<HolderData>("HolderNPCHead", true);
            foreach (var id in Constants.CREATURE_IDS.Keys) {
                var pool = CreatureData.pools.Find((CreatureData.Pool p) => p.id == id);
                if (pool != null) {
                    foreach (var obj in pool.list) {
                        var pooledCreature = obj.GetComponent<Creature>();
                        SetupHelmet(pooledCreature, holderNPCHead.Clone() as HolderData);
                    }
                }
            }
        }

        public static string HAT_HOLDER_NAME = "HolderHead";

        public static void SetupHelmet(Creature creature, HolderData holderData) {
            var HAT_POSITION = new Vector3(-0.14f, 0, 0.02f);
            var HAT_ROTATION = Quaternion.Euler(0, 90, 90);

            var holderObject = new GameObject(HAT_HOLDER_NAME);
            holderObject.transform.SetParent(creature.ragdoll.headPart.meshBone);
            holderObject.transform.localPosition = HAT_POSITION;
            holderObject.transform.localRotation = HAT_ROTATION;
            Holder holder = holderObject.AddComponent<Holder>();
            holder.Load(holderData);
            creature.equipment.holders.Add(holder);
        }

        public void SetupLoadingTips() {
            if (loadingTips != null) { 
                var tips = Catalog.GetTextData()?.GetGroup("Tips");
                var count = tips.texts.Count;
                foreach (var tip in loadingTips) {
                    tips.texts.Add(new TextData.TextID {
                        id = (++count).ToString(),
                        text = tip
                    });
                }
            }
        }
    }

    public class Constants {
        internal readonly static Dictionary<int, string> CREATURE_IDS = new Dictionary<int, string>() {
            { -2046070811, "CloneTrooper" },
            { -1363703833, "ForceSensitiveMale" },
            { -145168602, "ForceSensitiveFemale" },
            { 121048391, "Stormtrooper" },
            //{ 190647430, "Gamorrean" },
            //{ 519019616, "TuskenRaider" }
        };
    }
}