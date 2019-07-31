using UnityEngine;

namespace TOR {
    class Utils {
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

        public static void PlayRandomSound(AudioSource[] sounds) {
            if (sounds != null) {
                sounds[Random.Range(0, sounds.Length)].Play();
            }
        }
    }
}
