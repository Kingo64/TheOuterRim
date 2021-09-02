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

        [Description("Automatically reload blaster when empty"), Category("Blasters")]
        public static bool BlasterAutomaticReload { get; private set; }
        public bool blasterAutomaticReload = true;

        [Description("Can only reload via a manual power cell refill"), Category("Blasters")]
        public static bool BlasterRequireRefill { get; private set; }
        public bool blasterRequireRefill;

        [Description("Scope resolution - Width * Height pixels"), Category("Blasters")]
        public static int[] BlasterScopeResolution { get; private set; }
        public int[] blasterScopeResolution = { 512, 512 };

        [Description("Duration to hold button to detect a long press in seconds (s)"), Category("General")]
        public static float ControlsHoldDuration { get; private set; }
        public float controlsHoldDuration = 0.3f;

        [Description("Automatically activate lightsaber when recalling"), Category("Lightsabers")]
        public static bool SaberActivateOnRecall { get; private set; }
        public bool saberActivateOnRecall;

        [Description("Automatically deactivate lightsaber when dropped"), Category("Lightsabers")]
        public static bool SaberDeactivateOnDrop { get; private set; }
        public bool saberDeactivateOnDrop;

        [Description("Time in seconds (s) until a lightsaber will automatically deactivate itself after being dropped."), Category("Lightsabers")]
        public static float SaberDeactivateOnDropDelay { get; private set; }
        public float saberDeactivateOnDropDelay = 3f;

        [Description("Deflect Assist increases the deflection radius of lightsabers. Applies to both player and NPCs."), Category("Lightsabers")]
        public static bool SaberDeflectAssist { get; private set; }
        public bool saberDeflectAssist = true;

        [Description("Deflect assist detection radius in metres"), Category("Lightsabers")]
        public static float SaberDeflectAssistDistance { get; private set; }
        public float saberDeflectAssistDistance = 0.2f;

        [Description("Deflect assist will attempt to return bolts to the shooter"), Category("Lightsabers")]
        public static bool SaberDeflectAssistAlwaysReturn { get; private set; }
        public bool saberDeflectAssistAlwaysReturn;

        [Description("Saber NPCs will be able to return bolts to the shooter - The regular 'Always Return' must also be enabled"), Category("Lightsabers")]
        public static bool SaberDeflectAssistAlwaysReturnNPC { get; private set; }
        public bool saberDeflectAssistAlwaysReturnNPC;

        [Description("Reduces instances of lightsabers passing through each other. It uses Unity's most accurate collision detection system available."), Category("Lightsabers")]
        public static bool SaberExpensiveCollisions { get; private set; }
        public bool saberExpensiveCollisions = true;

        [Description("Minimum velocity (m/s) for lightsabers expensive collisions to enable"), Category("Lightsabers")]
        public static float SaberExpensiveCollisionsMinVelocity { get; private set; }
        public float saberExpensiveCollisionsMinVelocity = 10.0f;

        [Description("Attack speed for force sensitive lightsaber wielders. High values will cause animation/physics anomalies.|Saber NPC Attack Speed"), Category("Lightsabers")]
        public static float SaberNPCAttackSpeed { get; private set; }
        public float saberNPCAttackSpeed = 1.5f;

        [Description("Force sensitive lightsaber wielders will recoil upon being parried rather than following through with the attack. Disable for more difficult gameplay.|Saber NPC Recoil On Parry"), Category("Lightsabers")]
        public static bool SaberNPCRecoilOnParry { get; private set; }
        public bool saberNPCRecoilOnParry = true;

        [Description("Lightsabers are able to be thrown and recalled"), Category("Lightsabers")]
        public static bool SaberThrowable { get; private set; }
        public bool saberThrowable = true;

        [Description("Minimum velocity (m/s) for a thrown lightsaber to be able to be recalled"), Category("Lightsabers")]
        public static float SaberThrowMinVelocity { get; private set; }
        public float saberThrowMinVelocity = 7.0f;

        [Description("Enable lightsaber trails"), Category("Lightsabers")]
        public static bool SaberTrailEnabled { get; private set; }
        public bool saberTrailEnabled = true;

        [Description("Time in seconds (s) a lightsaber trail will be visible"), Category("Lightsabers")]
        public static float SaberTrailDuration { get; private set; }
        public float saberTrailDuration = 0.04f;

        [Description("Minimum velocity (m/s) a lightsaber must be moving to generate a trail"), Category("Lightsabers")]
        public static float SaberTrailMinVelocity { get; private set; }
        public float saberTrailMinVelocity;

        [Description("Adds legacy (U8.3 and earlier) helmet support to the game - may conflict with U8.4+ helmets"), Category("General")]
        public static bool UseLegacyHelmets { get; private set; }
        public bool useLegacyHelmets = true;

        public static AudioContainer SaberRecallSound { get; private set; }

        public static AudioSource HandAudioLeft { get; private set; }
        public static AudioSource HandAudioRight { get; private set; }

        public static Dictionary<int, Collider[]> LightsaberColliders { get; private set; }

        public override IEnumerator OnLoadCoroutine(Level level) {
            SetJsonValues();
            SetupConsoleCommands();
            SetupHelmets();
            SceneManager.sceneLoaded += OnNewSceneLoaded;
            EventManager.onPossess += OnPossessionEvent;
            EventManager.onReloadJson += OnReloadJson;

            yield break;
        }

        private void OnReloadJson(EventTime eventTime) {
            if (eventTime == EventTime.OnEnd) {
                SetJsonValues();
                SetupHelmets();
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
            BlasterRequireRefill = BlasterRequireRefill;
            BlasterScopeResolution = blasterScopeResolution;
            ControlsHoldDuration = Mathf.Abs(controlsHoldDuration);
            SaberActivateOnRecall = saberActivateOnRecall;
            SaberDeactivateOnDrop = saberDeactivateOnDrop;
            SaberDeactivateOnDropDelay = Mathf.Abs(saberDeactivateOnDropDelay);
            SaberDeflectAssist = saberDeflectAssist;
            SaberDeflectAssistDistance = Mathf.Abs(saberDeflectAssistDistance);
            SaberDeflectAssistAlwaysReturn = saberDeflectAssistAlwaysReturn;
            SaberDeflectAssistAlwaysReturnNPC = saberDeflectAssistAlwaysReturnNPC;
            SaberExpensiveCollisions = saberExpensiveCollisions;
            SaberExpensiveCollisionsMinVelocity = Mathf.Abs(saberExpensiveCollisionsMinVelocity);
            SaberNPCAttackSpeed = Mathf.Max(saberNPCAttackSpeed, 0.01f);
            SaberNPCRecoilOnParry = saberNPCRecoilOnParry;
            SaberThrowable = saberThrowable;
            SaberThrowMinVelocity = Mathf.Abs(saberThrowMinVelocity);
            SaberTrailEnabled = saberTrailEnabled;
            SaberTrailDuration = Mathf.Abs(saberTrailDuration);
            SaberTrailMinVelocity = Mathf.Abs(saberTrailMinVelocity);
            UseLegacyHelmets = useLegacyHelmets;

            Debug.Log("The Outer Rim: Settings file loaded successfully");
        }

        void SetupConsoleCommands() {
            string GetDescription(string property) {
                var attribute = typeof(GlobalSettings).GetProperty(property).GetCustomAttribute<DescriptionAttribute>();
                return attribute.Description;
            }

            DebugLogConsole.AddCommand("tor_blaster_automatic_reload", GetDescription("BlasterAutomaticReload"), (bool enabled) => blasterAutomaticReload = BlasterAutomaticReload = enabled);
            DebugLogConsole.AddCommand("tor_blaster_require_refill", GetDescription("BlasterRequireRefill"), (bool enabled) => blasterRequireRefill = BlasterRequireRefill = enabled);
            DebugLogConsole.AddCommand("tor_blaster_scope_resolution", GetDescription("BlasterScopeResolution"), (int x, int y) => blasterScopeResolution = BlasterScopeResolution = new int[] { x, y });
            DebugLogConsole.AddCommand("tor_controls_hold_duration", GetDescription("ControlsHoldDuration"), (float duration) => controlsHoldDuration = ControlsHoldDuration = duration);
            DebugLogConsole.AddCommand("tor_saber_activate_on_recall", GetDescription("SaberActivateOnRecall"), (bool enabled) => saberActivateOnRecall = SaberActivateOnRecall = enabled);
            DebugLogConsole.AddCommand("tor_saber_deactivate_on_drop", GetDescription("SaberDeactivateOnDrop"), (bool enabled) => saberDeactivateOnDrop = SaberDeactivateOnDrop = enabled);
            DebugLogConsole.AddCommand("tor_saber_deactivate_on_drop_delay", GetDescription("SaberDeactivateOnDropDelay"), (float delay) => saberDeactivateOnDropDelay = SaberDeactivateOnDropDelay = delay);
            DebugLogConsole.AddCommand("tor_saber_deflect_assist", GetDescription("SaberDeflectAssist"), (bool enabled) => saberDeflectAssist = SaberDeflectAssist = enabled);
            DebugLogConsole.AddCommand("tor_saber_deflect_assist_distance", GetDescription("SaberDeflectAssistDistance"), (float distance) => saberDeflectAssistDistance = SaberDeflectAssistDistance = distance);
            DebugLogConsole.AddCommand("tor_saber_deflect_assist_always_return", GetDescription("SaberDeflectAssistAlwaysReturn"), (bool enabled) => saberDeflectAssistAlwaysReturn = SaberDeflectAssistAlwaysReturn = enabled);
            DebugLogConsole.AddCommand("tor_saber_deflect_assist_always_return_npc", GetDescription("SaberDeflectAssistAlwaysReturnNPC"), (bool enabled) => saberDeflectAssistAlwaysReturnNPC = SaberDeflectAssistAlwaysReturnNPC = enabled);
            DebugLogConsole.AddCommand("tor_saber_expensive_collisions", GetDescription("SaberExpensiveCollisions"), (bool enabled) => saberExpensiveCollisions = SaberExpensiveCollisions = enabled);
            DebugLogConsole.AddCommand("tor_saber_expensive_collisions_min_velocity", GetDescription("SaberExpensiveCollisionsMinVelocity"), (float velocity) => saberExpensiveCollisionsMinVelocity = SaberExpensiveCollisionsMinVelocity = velocity);
            DebugLogConsole.AddCommand("tor_saber_npc_attack_speed", GetDescription("SaberNPCAttackSpeed"), (float speed) => saberNPCAttackSpeed = SaberNPCAttackSpeed = speed);
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
        static Vector3 HAT_POSITION = new Vector3(-0.14f, 0, 0.02f);
        static Quaternion HAT_ROTATION = Quaternion.Euler(0, 90, 90);

        void SetupHelmet(Creature creature, HolderData holderData) {
            Holder holder = new GameObject(HAT_HOLDER_NAME).AddComponent<Holder>();
            holder.transform.SetParent(creature.ragdoll.headPart.transform);
            holder.transform.localPosition = HAT_POSITION;
            holder.transform.localRotation = HAT_ROTATION;
            holder.Load(holderData);
            creature.equipment.holders = new List<Holder>(creature.GetComponentsInChildren<Holder>());
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