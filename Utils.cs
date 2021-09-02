using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace TOR {
    class Utils {
        public static void AddModule<T>(GameObject gameObject) {
            if (!gameObject.TryGetComponent(typeof(T), out _)) {
                gameObject.AddComponent(typeof(T));
            }
        }

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

        public static float GetElevation(Transform origin, Transform target) {
            float horizontalDist = Vector3.Distance(origin.transform.position, new Vector3(target.transform.position.x, origin.transform.position.y, target.transform.position.z));
            float verticalDist = target.transform.position.y - origin.transform.position.y;
            return Mathf.Atan(verticalDist / horizontalDist) * Mathf.Rad2Deg;
        }

        public static class HapticIntensity {
            public const float Minor = 0.3f;
            public const float Moderate = 0.6f;
            public const float Major = 1f;
        };

        public static void PlayHaptic(bool left, bool right, float intensity) {
            if (intensity > 0) {
                if (left) PlayerControl.GetHand(Side.Left).HapticShort(intensity);
                if (right) PlayerControl.GetHand(Side.Right).HapticShort(intensity);
            }
        }

        public static void PlayParticleEffect(ParticleSystem effect, bool detachFromParent = false) {
            if (effect != null) {
                if (detachFromParent) {
                    var clone = Object.Instantiate(effect);
                    var cloneTrans = clone.transform;
                    cloneTrans.parent = null;
                    cloneTrans.position = effect.transform.position;
                    cloneTrans.rotation = effect.transform.rotation;
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

        public static void PlaySound(AudioSource source, AudioContainer audioContainer) {
            if (source != null && audioContainer != null) {
                source.clip = audioContainer.PickAudioClip();
                source.Play();
            }
        }

        static System.Random random = new System.Random();
        public static T RandomEnum<T>() {
            var v = System.Enum.GetValues(typeof(T));
            return (T)v.GetValue(random.Next(v.Length));
        }

        public static Color UpdateHue(Color color, float hue) {
            Color.RGBToHSV(color, out float _, out float s, out float v);
            var newColour = Color.HSVToRGB(hue, s, v);
            newColour.a = color.a;
            return newColour;
        }
    }

    public class CollisionIgnoreHandler : MonoBehaviour {
        public List<Item> ignoredItems = new List<Item>();
        public Item item;

        public void ClearIgnoredCollisions() {
            foreach (var target in ignoredItems) {
                if (!target) continue;
                SetCollision(target, false);
            }
            ignoredItems.Clear();
        }

        public void IgnoreCollision(Item target) {
            SetCollision(target, true);
            ignoredItems.Add(target);
        }

        public void SetCollision(Item target, bool ignore) {
            foreach (ColliderGroup colliderGroup in item.colliderGroups) {
                foreach (Collider sourceCollider in colliderGroup.colliders) {
                    foreach (ColliderGroup colliderGroup2 in target.colliderGroups) {
                        foreach (Collider targetCollider in colliderGroup2.colliders) {
                            Physics.IgnoreCollision(sourceCollider, targetCollider, ignore);
                        }
                    }
                }
            }
        }
    }

    public class SkyController : MonoBehaviour {
        public float speed = 0.2f;
        public float maxX = 1.1f;
        public float maxZ = 1.1f;
        public float minX = 0.9f;
        public float minZ = 0.9f;
        public float xSpeed = -0.001f;
        public float zSpeed = 0.002f;

        // Update is called once per frame
        void Update() {
            var ls = transform.localScale;
            transform.Rotate(Vector3.up * speed * Time.deltaTime);
            if ((ls.x > maxX && xSpeed > 0) || (ls.x < minX && xSpeed < 0)) {
                xSpeed *= -1;
            }
            if ((ls.z > maxZ && zSpeed > 0) || (ls.z < minZ && zSpeed < 0)) {
                zSpeed *= -1;
            }
            transform.localScale = new Vector3(ls.x + (xSpeed * Time.deltaTime), ls.y, ls.z + (zSpeed * Time.deltaTime));
        }
    }
}
