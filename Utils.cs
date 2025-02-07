using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace TOR {
    class Utils {
        public static readonly WaitForSeconds waitSeconds_001 = new WaitForSeconds(0.01f);
        public static readonly WaitForSeconds waitSeconds_01 = new WaitForSeconds(0.1f);
        public static readonly WaitForSeconds waitSeconds_1 = new WaitForSeconds(1f);

        public static void AddModule<T>(GameObject gameObject) {
            if (!gameObject.TryGetComponent(typeof(T), out _)) {
                gameObject.AddComponent(typeof(T));
            }
        }

        public static void ApplyStandardMixer(AudioSource[] audioSource) {
            if (audioSource != null) {
                try {
                    var defaultAudioMixer = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Effect);
                    foreach (var a in audioSource) a.outputAudioMixerGroup = defaultAudioMixer;
                }
                catch {
                    Utils.LogWarning("Couldn't find AudioMixerGroup 'Effect'");
                }
            }
        }

        public static float GetElevation(Transform origin, Transform target) {
            float horizontalDist = Vector3.Distance(origin.transform.position, new Vector3(target.transform.position.x, origin.transform.position.y, target.transform.position.z));
            float verticalDist = target.transform.position.y - origin.transform.position.y;
            return Mathf.Atan(verticalDist / horizontalDist) * Mathf.Rad2Deg;
        }

        public static int HashString(string str, bool toLower = true) {
            if (toLower) str = str.ToLower();
            return Animator.StringToHash(str);
        }

        public static HashSet<int> HashArray(string[] strings, bool toLower = true) {
            HashSet<int> hashed = new HashSet<int>();
            for (int i = 0, l = strings.Length; i < l; i++) hashed.Add(HashString(strings[i], toLower));
            return hashed;
        }

        readonly static string LogPrefix = "The Outer Rim: ";
        public static void Log(object message) {
            Debug.Log(LogPrefix + message.ToString());
        }

        public static void LogError(object message) { 
            Debug.LogError(LogPrefix + message.ToString());
        }

        public static void LogWarning(object message) {
            Debug.LogWarning(LogPrefix + message.ToString());
        }

        public static Dictionary<string, DamageModifierData> originalDamageModifiers = new Dictionary<string, DamageModifierData>();
        public static void ModifyDamageModifiers(string name, float multiplier) {
            var damageModifier = Catalog.GetData<DamageModifierData>(name, true);
            if (!originalDamageModifiers.ContainsKey(name)) {
                originalDamageModifiers[name] = damageModifier.CloneJson();
            }
            damageModifier.collisions = originalDamageModifiers[name].CloneJson().collisions;
            foreach (var collision in damageModifier.collisions) {
                foreach (var modifier in collision.modifiers) {
                    modifier.damageMultiplier *= multiplier;
                }
            }
            damageModifier.OnCatalogRefresh();
        }

        public static class HapticIntensity {
            public const float Minor = 0.3f;
            public const float Moderate = 0.6f;
            public const float Major = 1f;
        };

        public static void PlayHaptic(RagdollHand interactor = null, float intensity = HapticIntensity.Minor) {
            if (intensity > 0 && interactor) {
                PlayerControl.GetHand(interactor.side).HapticShort(intensity);
            }
        }

        public static void PlayHaptic(bool left, bool right, float intensity = HapticIntensity.Minor) {
            if (intensity > 0) {
                if (left) PlayerControl.GetHand(Side.Left).HapticShort(intensity);
                if (right) PlayerControl.GetHand(Side.Right).HapticShort(intensity);
            }
        }

        public static void PlayParticleEffect(ParticleSystem effect, bool detachFromParent = false) {
            if (effect) {
                if (detachFromParent) {
                    var clone = Object.Instantiate(effect);
                    var cloneTrans = clone.transform;
                    cloneTrans.parent = null;
                    cloneTrans.position = effect.transform.position;
                    cloneTrans.rotation = effect.transform.rotation;
                    clone.Play();
                    Object.Destroy(clone, 10f);
                } else effect.Play();
            }
        }

        public static void PlayRandomSound(AudioSource[] sounds, float volume = 0, Creature sourceCreature = null) {
            if (sounds != null) {
                var randomSource = sounds[Random.Range(0, sounds.Length)];
                randomSource.PlayOneShot(randomSource.clip, volume <= 0 ? randomSource.volume : volume);
                NoiseManager.AddNoise(randomSource.transform.position, volume <= 0 ? randomSource.volume : volume, sourceCreature);
            }
        }

        public static void PlaySound(AudioSource source, AudioContainer audioContainer = null, Creature sourceCreature = null, float volume = 0) {
            if (source) {
                if (audioContainer != null) source.clip = audioContainer.PickAudioClip();
                source.Play();
                NoiseManager.AddNoise(source.transform.position, volume > 0 ? volume : source.volume, sourceCreature);
            }
        }

        public static void PlaySound(AudioSource source, AudioContainer audioContainer, Item item = null, float volume = 0) {
            PlaySound(source, audioContainer, item?.lastHandler?.creature, volume);
        }

        public static NoiseManager.Noise PlaySoundLoop(AudioSource source, AudioContainer audioContainer = null, Creature sourceCreature = null, float volume = 0) {
            if (source) {
                if (audioContainer != null) source.clip = audioContainer.PickAudioClip();
                source.Play();
                var noise = NoiseManager.AddLoopNoise(source, sourceCreature);
                if (noise != null && volume > 0) noise.UpdateVolume(volume);
                return noise;
            }
            return null;
        }

        public static NoiseManager.Noise PlaySoundLoop(AudioSource source, AudioContainer audioContainer = null, Item item = null, float volume = 0) {
            return PlaySoundLoop(source, audioContainer, item?.lastHandler?.creature, volume);
        }

        public static void PlaySoundOneShot(AudioSource source, AudioContainer audioContainer = null, Creature sourceCreature = null, float volume = 0) {
            if (source) {
                source.PlayOneShot(audioContainer ? audioContainer.PickAudioClip() : source.clip);
                NoiseManager.AddNoise(source.transform.position, volume > 0 ? volume : source.volume, sourceCreature);
            }
        }

        public static void PlaySoundOneShot(AudioSource source, AudioContainer audioContainer, Item item = null, float volume = 0) {
            PlaySoundOneShot(source, audioContainer, item?.lastHandler?.creature, volume);
        }

        public static void StopSoundLoop(AudioSource source, ref NoiseManager.Noise noise) {
            if (source) {
                source.Stop();
                NoiseManager.RemoveLoopNoise(source);
                noise = null;
            }
        }

        public static class NoiseLevel {
            public const float MODERATE = 0.8f;
            public const float LOUD = 1f;
            public const float VERY_LOUD = 2f;
        }

        public static void ReleaseAsset<T>(T asset) where T : Object {
            try {
                if (asset != null) Catalog.ReleaseAsset(asset);
            }
            catch { }
        }

        public static readonly System.Random random = new System.Random();
        public static T RandomEnum<T>() {
            var v = System.Enum.GetValues(typeof(T));
            return (T)v.GetValue(random.Next(v.Length));
        }

        public static T UpdateCustomData<T>(Item item, T data) where T : ContentCustomData {
            if (item.HasCustomData<T>()) item.RemoveCustomData<T>();
            item.AddCustomData(data);
            return data;
        }

        public static Color UpdateHue(Color color, float hue) {
            Color.RGBToHSV(color, out float _, out float s, out float v);
            var newColour = Color.HSVToRGB(hue, s, v);
            newColour.a = color.a;
            return newColour;
        }
    }

    public class CollisionIgnoreHandler : ThunderBehaviour {
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
                            try {
                                Physics.IgnoreCollision(sourceCollider, targetCollider, ignore);
                            }
                            catch (System.Exception e) {
                                Utils.LogError(e);
                            }
                        }
                    }
                }
            }
        }
    }

    public class SkyController : ThunderBehaviour {
        public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

        public float speed = 0.2f;
        public float maxX = 1.1f;
        public float maxZ = 1.1f;
        public float minX = 0.9f;
        public float minZ = 0.9f;
        public float xSpeed = -0.001f;
        public float zSpeed = 0.002f;

        // Update is called once per frame
        protected override void ManagedUpdate() {
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
