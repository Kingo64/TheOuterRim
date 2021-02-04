using UnityEngine;
using ThunderRoad;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace TOR {
    [RequireComponent(typeof(CollisionIgnoreHandler))]
    public class ItemBlasterBolt : MonoBehaviour {
        protected Item item;
        protected ItemModuleBlasterBolt module;
        protected Rigidbody body;

        protected TrailRenderer trail;
        protected Light light;

        bool markForDeletion;
        bool destroyNextTick;
        float despawnTime;
        int ricochets;

        List<DamagerData> originalDamagers = new List<DamagerData>();

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBlasterBolt>();
            body = GetComponent<Rigidbody>();

            ResetValues();

            body.freezeRotation = module.lockRotation;

            if (!string.IsNullOrEmpty(module.effectID)) {
                trail = item.GetCustomReference(module.effectID).GetComponent<TrailRenderer>();
                light = item.GetCustomReference(module.effectID).GetComponent<Light>();

                if (light != null) {
                    light.color = Utils.UpdateHue(light.color, module.boltHue);
                }

                if (trail != null) {
                    trail.material.SetColor("_Color", Utils.UpdateHue(trail.material.GetColor("_Color"), module.boltHue));
                }
            }

            foreach (CollisionHandler collisionHandler in item.collisionHandlers) {
                foreach (Damager damager in collisionHandler.damagers) {
                    originalDamagers.Add(damager.data);
                }
                collisionHandler.OnCollisionStartEvent += CollisionHandler;
                collisionHandler.checkMinVelocity = false;
            }
            
            if (module.colliderScale > 0 && !Mathf.Approximately(module.colliderScale, 1.0f)) {
                var colliders = item.GetComponentsInChildren<Collider>();
                for (int i = 0, l = colliders.Count(); i < l; i++) {
                    var colTrans = colliders[i].transform;
                    var colScale = colTrans.localScale;
                    colTrans.localScale = new Vector3(colScale.x * module.colliderScale, colScale.y * module.colliderScale, colScale.z * module.colliderScale);
                }
            }
        }

        void ResetValues() {
            despawnTime = module.despawnTime;
            ricochets = module.ricochetLimit;
            destroyNextTick = false;
            markForDeletion = false;
        }

        void CollisionHandler(ref CollisionStruct collisionInstance) {
            bool bounced = false;
            if (ricochets != 0) {
                bounced = (Vector3.Angle(body.velocity, -collisionInstance.contactNormal) - 90) < module.ricochetMaxAngle;
                ricochets--;
            }
            if (!bounced) {
                Collider collider = collisionInstance.targetCollider;
                if (!module.deflectionMaterials.Any(material => collider.material.name == material + " (Instance)")) {
                    GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                    markForDeletion = true;
                }
            }
            if (module.applyGlow) {
                var creature = collisionInstance.targetCollider.GetComponentInParent<Creature>();
                if (creature) {
                    creature.gameObject.AddComponent<StunGlow>().hue = module.boltHue;
                }
            }
            item.ResetObjectCollision();
        }

        void DeflectAssist() {
            if (GlobalSettings.LightsaberColliders.Count > 0) {
                List<int> toRemove = null;
                var currentPos = body.transform.position;
                var hasBeenAssisted = false;
                foreach (KeyValuePair<int, Collider[]> colliders in GlobalSettings.LightsaberColliders) {
                    if (hasBeenAssisted) return;
                    foreach (var collider in colliders.Value) {
                        if (!collider) {
                            if (toRemove == null) toRemove = new List<int> { colliders.Key };
                            else toRemove.Add(colliders.Key);
                            break;
                        }
                        if (collider.enabled) {
                            var closest = collider.ClosestPoint(currentPos);
                            if (Mathf.Abs((currentPos - closest).sqrMagnitude) < GlobalSettings.SaberDeflectAssistDistance) {
                                hasBeenAssisted = true;
                                if (GlobalSettings.SaberDeflectAssistAlwaysReturn) {
                                    body.velocity *= -1;
                                } else {
                                    if (Time.timeScale >= 1 ) body.position = closest;
                                    body.velocity = (currentPos - closest).normalized * body.velocity.magnitude;
                                }
                            }
                        }
                    }
                }
                if (toRemove != null) {
                    foreach (var key in toRemove) {
                        GlobalSettings.LightsaberColliders.Remove(key);
                    }
                }
            }
        }

        void ResetDamagers() {
            var i = 0;
            foreach (CollisionHandler collisionHandler in item.collisionHandlers) {
                foreach (Damager damager in collisionHandler.damagers) {
                    damager.data = originalDamagers[i++];
                }
            }
        }

        void Update() {
            if (destroyNextTick) {
                if (item.isPooled && trail != null) {
                    trail.Clear();
                }
                ResetDamagers();
                ResetValues();
                var ignoreHandler = item.GetComponent<CollisionIgnoreHandler>();
                if (ignoreHandler) ignoreHandler.ClearIgnoredCollisions();
                item.Despawn();
                return;
            }
            destroyNextTick = markForDeletion;

            despawnTime -= Time.deltaTime;
            markForDeletion |= despawnTime <= 0;

            if (GlobalSettings.SaberDeflectAssist) DeflectAssist();
        }
    }

    public class StunGlow : MonoBehaviour {
        public float hue = 0.62f;
        Dictionary<int, Material[]> originalMaterials = new Dictionary<int, Material[]>();
        MeshRenderer[] hatRenderers;
        Creature creature;
        Material stunMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        void Awake() {
            if (GetComponents<StunGlow>().Length > 1) {
                Destroy(this);
            } else {
                creature = GetComponent<Creature>();
                if (creature) {
                    stunMaterial.EnableKeyword("_EMISSION");
                    stunMaterial.SetColor("_EmissionColor", Color.HSVToRGB(hue, 1f, 0.75f) * 4);

                    foreach (var rendererData in creature.renderers) {
                        originalMaterials.Add(rendererData.GetHashCode(), rendererData.renderer.materials);
                    }
                    AssignMaterials(creature.renderers);

                    var holster = creature.equipment.GetHolster(GlobalSettings.HAT_HOLDER_NAME);
                    if (holster && holster.holdObjects.Count > 0) {
                        hatRenderers = holster.holdObjects[0].GetComponentsInChildren<MeshRenderer>();
                        foreach (var mesh in hatRenderers) {
                            originalMaterials.Add(mesh.GetHashCode(), mesh.materials);
                        }
                        AssignMaterials(hatRenderers);
                    }
                }
                Destroy(this, 0.1f);
            }
        }

        void OnDestroy() {
            if (creature) {
                RestoreMaterials(creature.renderers);
                if (hatRenderers != null) RestoreMaterials(hatRenderers);
            }
        }

        void AssignMaterials(MeshRenderer[] renderers) {
            for (int i = 0, l = renderers.Length; i < l; i++) {
                Material[] tempMaterials = renderers[i].materials;
                for (int j = 0, k = tempMaterials.Length; j < k; j++) {
                    tempMaterials[j] = stunMaterial;
                }
                renderers[i].materials = tempMaterials;
            }
        }

        void AssignMaterials(List<Creature.RendererData> renderers) {
            for (int i = 0, l = renderers.Count; i < l; i++) {
                Material[] tempMaterials = renderers[i].renderer.materials;
                for (int j = 0, k = tempMaterials.Length; j < k; j++) {
                    tempMaterials[j] = stunMaterial;
                }
                renderers[i].renderer.materials = tempMaterials;
            }
        }

        void RestoreMaterials(MeshRenderer[] renderers) {
            for (int i = 0, l = renderers.Length; i < l; i++) {
                if (originalMaterials.TryGetValue(renderers[i].GetHashCode(), out Material[] original)) {
                    renderers[i].materials = original;
                }
            }
        }

        void RestoreMaterials(List<Creature.RendererData> renderers) {
            for (int i = 0, l = renderers.Count; i < l; i++) {
                if (originalMaterials.TryGetValue(renderers[i].GetHashCode(), out Material[] original)) {
                    renderers[i].renderer.materials = original;
                }
            }
        }
    }
}