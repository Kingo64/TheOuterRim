using BS;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TOR {
    public class TORGlobalSettings : LevelModule {

        public static float SaberControlsHoldDuration { get; private set; }
        public float saberControlsHoldDuration = 0.3f;

        public static bool SaberExpensiveCollisions { get; private set; }
        public bool saberExpensiveCollisions = true;

        public static float SaberExpensiveCollisionsMinVelocity { get; private set; }
        public float saberExpensiveCollisionsMinVelocity = 10.0f;

        public static bool SaberThrowable { get; private set; }
        public bool saberThrowable = true;

        public static float SaberThrowMinVelocity { get; private set; }
        public float saberThrowMinVelocity = 7.0f;

        public static AudioContainer SaberRecallSound { get; private set; }

        public static bool SaberTrailEnabled { get; private set; }
        public bool saberTrailEnabled = true;

        public static float SaberTrailDuration { get; private set; }
        public float saberTrailDuration = 0.04f;

        public static float SaberTrailMinVelocity { get; private set; }
        public float saberTrailMinVelocity = 0;

        public static AudioSource HandAudioLeft { get; private set; }
        public static AudioSource HandAudioRight { get; private set; }

        public override void OnLevelLoaded(LevelDefinition levelDefinition) {
            SetJsonValues();
            Debug.Log("The Outer Rim: Settings file loaded successfully");
            SceneManager.sceneLoaded += OnNewSceneLoaded;
            initialized = true;
        }

        public override void OnLevelUnloaded(LevelDefinition levelDefinition) {
            initialized = false;
        }

        void OnNewSceneLoaded(Scene scene, LoadSceneMode mode) {
            SetJsonValues();
            SetupHandAudio();
        }

        void SetJsonValues() {
            SaberControlsHoldDuration = saberControlsHoldDuration;
            SaberExpensiveCollisions = saberExpensiveCollisions;
            SaberExpensiveCollisionsMinVelocity = saberExpensiveCollisionsMinVelocity;
            SaberThrowable = saberThrowable;
            SaberThrowMinVelocity = saberThrowMinVelocity;
            SaberTrailEnabled = saberTrailEnabled;
            SaberTrailDuration = saberTrailDuration;
            SaberTrailMinVelocity = saberTrailMinVelocity;
        }

        public override void Update(LevelDefinition levelDefinition) {
            if (HandAudioLeft == null || HandAudioLeft == null) {
                SetupHandAudio();
            }
        }

        void SetupHandAudio() {
            if (Player.local) {
                var spatialBlend = 0.9f;
                var volume = 0.15f;

                var handAudioLeft = new GameObject("handAudioLeft", typeof(AudioSource));
                HandAudioLeft = handAudioLeft.GetComponent<AudioSource>();
                HandAudioLeft.spatialBlend = spatialBlend;
                HandAudioLeft.volume = volume;
                HandAudioLeft.outputAudioMixerGroup = GameManager.local.audioMixer.FindMatchingGroups("Effect")[0];
                handAudioLeft.transform.parent = Player.local.handLeft.transform;
                handAudioLeft.transform.localPosition = Vector3.zero;

                var handAudioRight = new GameObject("handAudioRight", typeof(AudioSource));
                HandAudioRight = handAudioRight.GetComponent<AudioSource>();
                HandAudioRight.spatialBlend = spatialBlend;
                HandAudioRight.volume = volume;
                HandAudioRight.outputAudioMixerGroup = GameManager.local.audioMixer.FindMatchingGroups("Effect")[0];
                handAudioRight.transform.parent = Player.local.handRight.transform;
                handAudioRight.transform.localPosition = Vector3.zero;

                var fx = Catalog.current.GetData<FXData>("LightsaberRecall", true);
                SaberRecallSound = fx.audioContainer;
            }
        }
    }
}
