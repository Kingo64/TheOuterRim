using UnityEngine;
using ThunderRoad;
using System.Linq;
using System.Collections.Generic;

namespace TOR {
    [RequireComponent(typeof(CollisionIgnoreHandler))]
    public class ItemBlasterBolt : ThunderBehaviour {
        public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

        protected Item item;
        protected ItemModuleBlasterBolt module;
        protected Rigidbody body;
        protected Collider blasterCollider;
        protected int blasterColliderMaterial;

        public TrailRenderer trail;
        public Color trailColor;
        public Light light;

        bool markForDeletion;
        bool destroyNextTick;
        float despawnTime;
        int lastItemHitId;
        int ricochets;

        readonly List<DamagerData> originalDamagers = new List<DamagerData>();

        MaterialPropertyBlock _propBlock;
        public MaterialPropertyBlock PropBlock {
            get {
                _propBlock = _propBlock ?? new MaterialPropertyBlock();
                return _propBlock;
            }
        }

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBlasterBolt>();
            body = GetComponent<Rigidbody>();
            blasterCollider = GetComponentInChildren<Collider>();
            blasterColliderMaterial = Utils.HashString(blasterCollider.material.name, false);

            ResetValues();

            body.drag = module.drag;
            body.freezeRotation = module.lockRotation;

            foreach (CollisionHandler collisionHandler in item.collisionHandlers) {
                foreach (Damager damager in collisionHandler.damagers) {
                    originalDamagers.Add(damager.data);
                }
                collisionHandler.OnCollisionStartEvent += CollisionHandler;
                collisionHandler.checkMinVelocity = false;
            }

            if (!string.IsNullOrEmpty(module.effectID)) {
                trail = item.GetCustomReference(module.effectID).GetComponent<TrailRenderer>();
                light = item.GetCustomReference(module.effectID).GetComponent<Light>();

                UpdateHue(module.boltHue);
            }

            if (!Mathf.Approximately(module.colliderScale, 1.0f)) UpdateColliderScale(module.colliderScale);
        }

        protected override void ManagedOnEnable() {
            if (trail) trail.emitting = true;
        }

        public void UpdateValues(ref ProjectileData data) {
            module.drag = data.drag;
            body.drag = data.drag;
            if (module.colliderScale != data.colliderScale) UpdateColliderScale(data.colliderScale);
            module.colliderScale = data.colliderScale;
            module.deflectionMaterials = data.deflectionMaterials;
            module.despawnTime = data.despawnTime;
            module.lockRotation = data.lockRotation;
            module.useGravity = data.useGravity;

            module.ricochetLimit = data.ricochetLimit;
            module.ricochetMaxAngle = data.ricochetMaxAngle;
            module.applyGlow = data.applyGlow;
            module.disintegrate = data.disintegrate;
            module.effectID = data.effectID;
            module.boltHue = data.boltHue;
            module.impactEffectID = data.impactEffectID;
            module.impactEffect = data.impactEffect;

            UpdateHue(data.boltHue);
        }

        void ResetValues() {
            despawnTime = module.despawnTime;
            lastItemHitId = 0;
            ricochets = module.ricochetLimit;
            destroyNextTick = false;
            markForDeletion = false;
        }

        void UpdateColliderScale(float scale) {
            if (scale > 0) {
                var colliders = item.GetComponentsInChildren<Collider>();
                for (int i = 0, l = colliders.Count(); i < l; i++) {
                    var colTrans = colliders[i].transform;
                    var colScale = colTrans.localScale;
                    colTrans.localScale = new Vector3(colScale.x * scale, colScale.y * scale, colScale.z * scale);
                }
            }
        }

        void UpdateHue(float hue) {
            if (light != null) {
                light.color = Utils.UpdateHue(light.color, hue);
            }

            if (trail != null) {
                if (trailColor == Color.clear) trailColor = trail.material.GetColor("_Color");
                trail.GetPropertyBlock(PropBlock);
                PropBlock.SetColor("_Color", Utils.UpdateHue(trailColor, module.boltHue));
                trail.SetPropertyBlock(PropBlock);
            }
        }

        void CollisionHandler(CollisionInstance collisionInstance) {
            bool bounced = false;
            if (ricochets != 0) {
                bounced = (Vector3.Angle(body.velocity, -collisionInstance.contactNormal) - 90) < module.ricochetMaxAngle;
                ricochets--;
            }
            if (!bounced) {
                Collider collider = collisionInstance.targetCollider;
                if (!module.deflectionMaterials.Any(material => collider.material.name == material + " (Instance)")) {
                    body.velocity = new Vector3(0, 0, 0);
                    markForDeletion = true;
                }
            }

            if (module.disintegrate) {
                var creature = collisionInstance.targetCollider.GetComponentInParent<Creature>();
                if (creature && (!creature.isPlayer || !Player.invincibility)) {
                    try {
                        foreach (var holder in creature.holders) {
                            holder.UnSnapAll();
                        }
                    }
                    catch { }
                    creature?.handLeft?.TryRelease();
                    creature?.handRight?.TryRelease();
                    creature.Kill(collisionInstance);

                    module.impactEffect?.Spawn(collisionInstance.contactPoint, new Quaternion()).Play();

                    creature.Despawn();
                }
            }
            if (module.applyGlow) {
                var creature = collisionInstance.targetCollider.GetComponentInParent<Creature>();
                if (creature) {
                    if (!StunGlow.stunMaterial) {
                        StunGlow.stunMaterial = item.GetCustomReference("StunEffect").GetComponent<MeshRenderer>().materials[0];
                    }
                    var stunGlow = creature.gameObject.AddComponent<StunGlow>();
                    stunGlow.hue = module.boltHue;
                    stunGlow.Glow();
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
                    if (lastItemHitId == colliders.Key) hasBeenAssisted = true;
                    if (hasBeenAssisted) break;
                    foreach (var collider in colliders.Value) {
                        if (!collider) {
                            if (toRemove == null) toRemove = new List<int> { colliders.Key };
                            else toRemove.Add(colliders.Key);
                            break;
                        } else if (collider.enabled) {
                            var closest = collider.ClosestPoint(currentPos);
                            if (Mathf.Abs((currentPos - closest).sqrMagnitude) < GlobalSettings.SaberDeflectAssistDistance) {
                                hasBeenAssisted = true;
                                if (ShouldReturnBolt()) {
                                    if (item.lastHandler) {
                                        var parts = item.lastHandler.creature.ragdoll.parts;
                                        var randomPart = parts[Random.Range(0, parts.Count)];
                                        var direction = randomPart.transform.position - transform.position;
                                        body.velocity = direction.normalized * body.velocity.magnitude;
                                    } else {
                                        body.velocity *= -1;
                                    }
                                    var collision = new CollisionInstance {
                                        sourceCollider = blasterCollider,
                                        targetCollider = collider,
                                        contactPoint = closest
                                    };
                                    if (Physics.Linecast(transform.position, closest, out RaycastHit hit)) {
                                        collision.contactNormal = hit.normal;
                                    }
                                    MaterialData.TryGetMaterials(blasterColliderMaterial, Utils.HashString(collider.material.name, false), out MaterialData sourceMaterial, out MaterialData targetMaterial);
                                    if (collision.SpawnEffect(sourceMaterial, targetMaterial, false, out EffectInstance effect)) {
                                        effect.SetIntensity(Random.Range(0.06f, 0.12f));
                                        effect.Play();
                                    }
                                } else {
                                    if (Time.timeScale >= 1) body.position = closest;
                                    body.velocity = (currentPos - closest).normalized * body.velocity.magnitude;
                                }

                                item.lastHandler = null;
                                lastItemHitId = colliders.Key;
                                despawnTime = module.despawnTime;
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

        bool ShouldReturnBolt() {
            var deflectRoll = Random.Range(0, 1f);
            if (!item.lastHandler || item.lastHandler.creature == Player.currentCreature) return GlobalSettings.SaberDeflectAssistReturnNPCChance > deflectRoll;
            return GlobalSettings.SaberDeflectAssistReturnChance > deflectRoll;
        }

        void DisableDamagers() {
            foreach (CollisionHandler collisionHandler in item.collisionHandlers) {
                collisionHandler.enabled = false;
                foreach (Damager damager in collisionHandler.damagers) {
                    damager.enabled = false;
                }
            }
        }

        void ResetDamagers() {
            var i = 0;
            foreach (CollisionHandler collisionHandler in item.collisionHandlers) {
                collisionHandler.enabled = true;
                foreach (Damager damager in collisionHandler.damagers) {
                    damager.data = originalDamagers[i++];
                    damager.enabled = true;
                }
            }
        }

        protected override void ManagedUpdate() {
            if (destroyNextTick) {
                Recycle();
                return;
            }

            destroyNextTick = markForDeletion;
            if (destroyNextTick) {
                if (GlobalSettings.BlasterBoltInstantDespawn) Recycle();
                else DisableDamagers();
                return;
            }

            if (item.isTelekinesisGrabbed) despawnTime = module.despawnTime;
            despawnTime -= Time.deltaTime;
            markForDeletion |= despawnTime <= 0;

            if (GlobalSettings.SaberDeflectAssist) {
                DeflectAssist();
            }
        }

        void Recycle() {
            if (trail) {
                trail.emitting = false;
                trail.Clear();
            }
            ResetDamagers();
            ResetValues();
            var ignoreHandler = item.GetComponent<CollisionIgnoreHandler>();
            if (ignoreHandler) ignoreHandler.ClearIgnoredCollisions();
            item.Despawn();
        }
    }

    public class StunGlow : ThunderBehaviour {
        public float hue = 0.62f;
        public Dictionary<int, Material[]> originalMaterials = new Dictionary<int, Material[]>();
        public Creature creature;
        public static Material stunMaterial;

        protected void Awake() {
            if (GetComponents<StunGlow>().Length > 1) {
                Destroy(this);
            }
        }

        public void Glow() {
            creature = GetComponent<Creature>();
            if (creature) {
                stunMaterial.SetColor("_EmissionColor", Color.HSVToRGB(hue, 1f, 0.75f) * 4);

                foreach (var rendererData in creature.renderers) {
                    originalMaterials.Add(rendererData.GetHashCode(), rendererData.renderer.materials);
                }
                AssignMaterials(creature.renderers);
            }
            if (creature.state == Creature.State.Alive) {
                if (creature != Player.currentCreature) creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                else {
                    Player.currentCreature.handLeft.TryRelease();
                    Player.currentCreature.handRight.TryRelease();
                }
            }
            Destroy(this, 0.1f);
        }

        protected void OnDestroy() {
            if (creature) {
                RestoreMaterials(creature.renderers);
            }
        }

        public void AssignMaterials(MeshRenderer[] renderers) {
            for (int i = 0, l = renderers.Length; i < l; i++) {
                Material[] tempMaterials = renderers[i].materials;
                for (int j = 0, k = tempMaterials.Length; j < k; j++) {
                    tempMaterials[j] = stunMaterial;
                }
                renderers[i].materials = tempMaterials;
            }
        }

        public void AssignMaterials(List<Creature.RendererData> renderers) {
            for (int i = 0, l = renderers.Count; i < l; i++) {
                Material[] tempMaterials = renderers[i].renderer.materials;
                for (int j = 0, k = tempMaterials.Length; j < k; j++) {
                    tempMaterials[j] = stunMaterial;
                }
                renderers[i].renderer.materials = tempMaterials;
            }
        }

        public void RestoreMaterials(MeshRenderer[] renderers) {
            for (int i = 0, l = renderers.Length; i < l; i++) {
                if (originalMaterials.TryGetValue(renderers[i].GetHashCode(), out Material[] original)) {
                    renderers[i].materials = original;
                }
            }
        }

        public void RestoreMaterials(List<Creature.RendererData> renderers) {
            for (int i = 0, l = renderers.Count; i < l; i++) {
                if (originalMaterials.TryGetValue(renderers[i].GetHashCode(), out Material[] original)) {
                    renderers[i].renderer.materials = original;
                }
            }
        }
    }
}