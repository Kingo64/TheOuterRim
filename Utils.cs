using BS;
using UnityEngine;

namespace TOR {
    class Utils {
        public static void ApplyStandardMixer(AudioSource[] audioSource) {
            if (audioSource != null) {
                try {
                    var defaultAudioMixer = GameManager.local.audioMixer.FindMatchingGroups("Effect")[0];
                    foreach (var a in audioSource) a.outputAudioMixerGroup = defaultAudioMixer;
                }
                catch {
                    Debug.LogWarning("The Outer Rim: Couldn't find AudioMixerGroup 'Effect'.");
                }
            }
        }

        public static void PlayParticleEffect(ParticleSystem effect, bool detachFromParent = false) {
            if (effect != null) {
                if (detachFromParent) {
                    var clone = Object.Instantiate(effect);
                    clone.transform.parent = null;
                    clone.transform.position = effect.transform.position;
                    clone.transform.rotation = effect.transform.rotation;
                    clone.Play();
                } else effect.Play();
            }
        }

        public static void PlayRandomSound(AudioSource[] sounds, float volume = 0) {
            if (sounds != null) {
                var randomSource = sounds[Random.Range(0, sounds.Length)];
                randomSource.PlayOneShot(randomSource.clip, volume <= 0 ? randomSource.volume : volume);
            }
        }
    }
}
