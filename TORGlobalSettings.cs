using ThunderRoad;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.ComponentModel;

namespace TOR {
    [Description("Global settings for The Outer Rim|Global Settings")]
    public class TORGlobalSettings : LevelModule {

        [Description("Automatically reload blaster when empty"), Category("Blasters")]
        public bool blasterAutomaticReload = true;
        public static bool BlasterAutomaticReload { get; private set; }

        [Description("Can only reload via a manual power cell refill"), Category("Blasters")]
        public bool blasterRequireRefill;
        public static bool BlasterRequireRefill { get; private set; }

        [Description("Scope resolution - Width * Height pixels"), Category("Blasters")]
        public int[] blasterScopeResolution = { 512, 512 };
        public static int[] BlasterScopeResolution { get; private set; }

        [Description("Duration to hold button to detect a long press in seconds (s)"), Category("General")]
        public float controlsHoldDuration = 0.3f;
        public static float ControlsHoldDuration { get; private set; }

        [Description("Automatically activate lightsaber when recalling"), Category("Lightsabers")]
        public bool saberActivateOnRecall;
        public static bool SaberActivateOnRecall { get; private set; }

        [Description("Deflect Assist increases the deflection radius of lightsabers. Applies to both player and NPCs."), Category("Lightsabers")]
        public bool saberDeflectAssist = true;
        public static bool SaberDeflectAssist { get; private set; }

        [Description("Deflect assist detection radius in metres"), Category("Lightsabers")]
        public float saberDeflectAssistDistance = 0.2f;
        public static float SaberDeflectAssistDistance { get; private set; }

        [Description("Deflect assist will attempt to return bolts to their originating positions"), Category("Lightsabers")]
        public bool saberDeflectAssistAlwaysReturn;
        public static bool SaberDeflectAssistAlwaysReturn { get; private set; }

        [Description("Reduces instances of lightsabers passing through each other. It uses Unity's most accurate collision detection system available."), Category("Lightsabers")]
        public bool saberExpensiveCollisions = true;
        public static bool SaberExpensiveCollisions { get; private set; }

        [Description("Minimum velocity (m/s) for lightsabers expensive collisions to enable"), Category("Lightsabers")]
        public float saberExpensiveCollisionsMinVelocity = 10.0f;
        public static float SaberExpensiveCollisionsMinVelocity { get; private set; }

        [Description("Attack speed for force sensitive lightsaber wielders. High values will cause animation/physics anomalies.|Saber NPC Attack Speed"), Category("Lightsabers")]
        public float saberNPCAttackSpeed = 1.5f;
        public static float SaberNPCAttackSpeed { get; private set; }

        [Description("Force sensitive lightsaber wielders will recoil upon being parried rather than following through with the attack. Disable for more difficult gameplay.|Saber NPC Recoil On Parry"), Category("Lightsabers")]
        public bool saberNPCRecoilOnParry = true;
        public static bool SaberNPCRecoilOnParry { get; private set; }

        [Description("Lightsabers are able to be thrown and recalled"), Category("Lightsabers")]
        public bool saberThrowable = true;
        public static bool SaberThrowable { get; private set; }

        [Description("Minimum velocity (m/s) for a thrown lightsaber to be able to be recalled"), Category("Lightsabers")]
        public float saberThrowMinVelocity = 7.0f;
        public static float SaberThrowMinVelocity { get; private set; }

        [Description("Enable lightsaber trails"), Category("Lightsabers")]
        public bool saberTrailEnabled = true;
        public static bool SaberTrailEnabled { get; private set; }

        [Description("Time in seconds (s) a lightsaber trail will be visible"), Category("Lightsabers")]
        public float saberTrailDuration = 0.04f;
        public static float SaberTrailDuration { get; private set; }

        [Description("Minimum velocity (m/s) a lightsaber must be moving to generate a trail"), Category("Lightsabers")]
        public float saberTrailMinVelocity;
        public static float SaberTrailMinVelocity { get; private set; }

        public static AudioContainer SaberRecallSound { get; private set; }

        public static AudioSource HandAudioLeft { get; private set; }
        public static AudioSource HandAudioRight { get; private set; }

        public static Dictionary<int, Collider[]> lightsaberColliders { get; private set; }

        public override void OnLevelLoaded(LevelDefinition levelDefinition) {
            SetJsonValues();
            Debug.Log("The Outer Rim: Settings file loaded successfully");
            SceneManager.sceneLoaded += OnNewSceneLoaded;
            EventManager.onPossessionEvent += OnPossessionEvent;
            initialized = true;
        }

        void OnPossessionEvent(Body oldBody, Body newBody) {
            SetupHandAudio();
        }

        public override void OnLevelUnloaded(LevelDefinition levelDefinition) {
            initialized = false;
        }

        void OnNewSceneLoaded(Scene scene, LoadSceneMode mode) {
            SetJsonValues();
            lightsaberColliders = new Dictionary<int, Collider[]>();
        }

        void SetJsonValues() {
            BlasterAutomaticReload = blasterAutomaticReload;
            BlasterRequireRefill = BlasterRequireRefill;
            BlasterScopeResolution = blasterScopeResolution;
            ControlsHoldDuration = Mathf.Abs(controlsHoldDuration);
            SaberActivateOnRecall = saberActivateOnRecall;
            SaberDeflectAssist = saberDeflectAssist;
            SaberDeflectAssistDistance = Mathf.Abs(saberDeflectAssistDistance);
            SaberDeflectAssistAlwaysReturn = saberDeflectAssistAlwaysReturn;
            SaberExpensiveCollisions = saberExpensiveCollisions;
            SaberExpensiveCollisionsMinVelocity = Mathf.Abs(saberExpensiveCollisionsMinVelocity);
            SaberNPCAttackSpeed = Mathf.Max(saberNPCAttackSpeed, 0.01f);
            SaberNPCRecoilOnParry = saberNPCRecoilOnParry;
            SaberThrowable = saberThrowable;
            SaberThrowMinVelocity = Mathf.Abs(saberThrowMinVelocity);
            SaberTrailEnabled = saberTrailEnabled;
            SaberTrailDuration = Mathf.Abs(saberTrailDuration);
            SaberTrailMinVelocity = Mathf.Abs(saberTrailMinVelocity);
        }

        void SetupHandAudio() {
            var spatialBlend = 0.9f;
            var volume = 0.15f;

            HandAudioLeft = SetupHandAudioCreate(Player.local.handLeft.transform, "handAudioLeft", HandAudioLeft, spatialBlend, volume);
            HandAudioRight = SetupHandAudioCreate(Player.local.handRight.transform, "handAudioRight", HandAudioRight, spatialBlend, volume);

            var fx = Catalog.GetData<EffectData>("LightsaberRecall", true);
            SaberRecallSound = ((EffectModuleAudio)fx.modules[0]).audioContainer;
        }

        AudioSource SetupHandAudioCreate(Transform parent, string name, AudioSource handAudio, float spatialBlend, float volume) {
            var audioSource = new GameObject(name, typeof(AudioSource));
            handAudio = audioSource.GetComponent<AudioSource>();
            handAudio.spatialBlend = spatialBlend;
            handAudio.volume = volume;
            handAudio.outputAudioMixerGroup = GameManager.local.audioMixer.FindMatchingGroups("Effect")[0];
            audioSource.transform.parent = parent;
            audioSource.transform.localPosition = Vector3.zero;
            return handAudio;
        }
    }
}