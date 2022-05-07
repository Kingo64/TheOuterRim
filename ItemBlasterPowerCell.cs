using UnityEngine;
using ThunderRoad;

namespace TOR {
    public class ItemBlasterPowerCell : MonoBehaviour {
        protected Item item;
        protected ItemModuleBlasterPowerCell module;

        AudioSource audio;
        ParticleSystem particle;
        bool holdingLeft;
        bool holdingRight;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBlasterPowerCell>();
            audio = item.GetCustomReference("Particle").GetComponent<AudioSource>();
            particle = item.GetCustomReference("Particle").GetComponent<ParticleSystem>();

            for (int i = 0, l = item.collisionHandlers.Count; i < l; i++) {
                item.collisionHandlers[i].OnCollisionStartEvent += CollisionHandler;
            }

            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;

            var boltModule = Catalog.GetData<ItemData>(module.projectileID, true).GetModule<ItemModuleBlasterBolt>();
            if (boltModule != null) {
                var mesh = item.GetCustomReference("Mesh").GetComponent<MeshRenderer>();
                mesh.materials[0].SetColor("_BaseColor", Utils.UpdateHue(mesh.materials[0].GetColor("_BaseColor"), boltModule.boltHue));

                if (particle) {
                    var main = particle.main;
                    main.startColor = Utils.UpdateHue(main.startColor.color, boltModule.boltHue);
                }
            }
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            holdingRight |= interactor.playerHand == Player.local.handRight;
            holdingLeft |= interactor.playerHand == Player.local.handLeft;
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            holdingRight &= interactor.playerHand != Player.local.handRight;
            holdingLeft &= interactor.playerHand == Player.local.handLeft;
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