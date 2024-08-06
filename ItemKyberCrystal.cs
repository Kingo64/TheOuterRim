using UnityEngine;
using ThunderRoad;

namespace TOR {
    public class ItemKyberCrystal : ThunderBehaviour {
        public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

        public Item item;
        public ItemModuleKyberCrystal module;

        public Color bladeColour;
        public Color coreColour;
        public Color glowColour;
        MeshRenderer mesh;
        Transform itemTrans;
        Transform leftHandTrans;
        Transform rightHandTrans;

        MaterialInstance _materialInstance;
        public MaterialInstance materialInstance {
            get {
                if (_materialInstance == null) {
                    mesh.gameObject.TryGetOrAddComponent(out MaterialInstance mi);
                    _materialInstance = mi;
                }
                return _materialInstance;
            }
        }
        static readonly int emissionColorId = Shader.PropertyToID("_EmissionColor");

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleKyberCrystal>();

            bladeColour = new Color(module.bladeColour[0], module.bladeColour[1], module.bladeColour[2], module.bladeColour[3]);
            coreColour = new Color(module.coreColour[0], module.coreColour[1], module.coreColour[2], module.coreColour[3]);
            glowColour = new Color(module.glowColour[0], module.glowColour[1], module.glowColour[2], module.glowColour[3]);

            mesh = item.GetCustomReference("Mesh").GetComponent<MeshRenderer>();
            materialInstance.material.SetColor("_BaseColor", coreColour);

            itemTrans = item.transform;

            for (int i = 0, l = item.collisionHandlers.Count; i < l; i++) {
                item.collisionHandlers[i].OnCollisionStartEvent += CollisionHandler;
            }

        }

        void CollisionHandler(CollisionInstance collisionInstance) {
            try {
                if (collisionInstance.sourceColliderGroup.name == "KyberCrystalCollision" && collisionInstance.targetColliderGroup.name == "CollisionHilt") {
                    collisionInstance.targetColliderGroup.transform.root.SendMessage("TryAddCrystal", this);
                }
            }
            catch { }
        }

        public float GetClosestHandDistance() {
            if (!Player.local) return float.MaxValue;
            if (!leftHandTrans) leftHandTrans = Player.local.handLeft.transform;
            if (!rightHandTrans) rightHandTrans = Player.local.handRight.transform;
            var distanceToHandLeft = Vector3.SqrMagnitude(itemTrans.position - leftHandTrans.position);
            var distanceToHandRight = Vector3.SqrMagnitude(itemTrans.position - rightHandTrans.position);
            return (distanceToHandLeft < distanceToHandRight) ? distanceToHandLeft : distanceToHandRight;
        }

        protected override void ManagedUpdate() {
            var distanceToHand = GetClosestHandDistance();
            var minGlow = 0.33f;
            var maxGlow = 3f;
            var flicker = module.isUnstable ? Random.Range(-0.2f, 0.2f) : Random.Range(-0.04f, 0.04f);
            var intensity = Mathf.Clamp(maxGlow - (10 * distanceToHand) + flicker, minGlow, maxGlow);
            materialInstance.material.SetColor(emissionColorId, new Color(bladeColour.r * intensity, bladeColour.g * intensity, bladeColour.b * intensity, bladeColour.a));
        }
    }
}