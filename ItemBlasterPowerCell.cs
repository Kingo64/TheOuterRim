using UnityEngine;
using ThunderRoad;

namespace TOR {
    public class ItemBlasterPowerCell : ThunderBehaviour {
        protected Item item;
        protected ItemModuleBlasterPowerCell module;

        AudioSource audio;
        ParticleSystem particle;
        bool holdingLeft;
        bool holdingRight;

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBlasterPowerCell>();
            audio = item.GetCustomReference("Particle").GetComponent<AudioSource>();
            particle = item.GetCustomReference("Particle").GetComponent<ParticleSystem>();

            foreach (var handler in item.collisionHandlers) {
                handler.OnCollisionStartEvent += CollisionHandler;
            }

            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;

            var projectileData = Catalog.GetData<ProjectileData>(module.projectileID, true);
            if (projectileData != null) {
                var mesh = item.GetCustomReference("Mesh").GetComponent<MeshRenderer>();
                mesh.materials[0].SetColor("_BaseColor", Utils.UpdateHue(mesh.materials[0].GetColor("_BaseColor"), projectileData.boltHue));

                if (particle) {
                    var main = particle.main;
                    main.startColor = Utils.UpdateHue(main.startColor.color, projectileData.boltHue);
                }
            }
        }

        protected void OnDestroy() {
            if (item != null) {
                item.OnGrabEvent -= OnGrabEvent;
                item.OnUngrabEvent -= OnUngrabEvent;

                foreach (var handler in item.collisionHandlers) {
                    handler.OnCollisionStartEvent -= CollisionHandler;
                }
            }
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            if (interactor.playerHand == Player.local.handRight) holdingRight = true;
            if (interactor.playerHand == Player.local.handLeft) holdingLeft = true;
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            if (interactor.playerHand == Player.local.handRight) holdingRight = false;
            if (interactor.playerHand == Player.local.handLeft) holdingLeft = false;
        }

        void CollisionHandler(CollisionInstance collisionInstance) {
            try {
                if (collisionInstance.sourceColliderGroup.name == "CollisionBlasterPowerCell" && collisionInstance.targetColliderGroup.name == "CollisionBlasterRefill") {
                    collisionInstance.targetColliderGroup.transform.root.SendMessage("RechargeFromPowerCell", module.projectileID);
                    Utils.PlayParticleEffect(particle);
                    Utils.PlayHaptic(holdingLeft, holdingRight, Utils.HapticIntensity.Major);
                    Utils.PlaySound(audio, module.audioAsset, item);
                }
            }
            catch { }
        }
    }
}