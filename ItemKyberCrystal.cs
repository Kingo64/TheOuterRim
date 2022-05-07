using UnityEngine;
using ThunderRoad;

namespace TOR {
    public class ItemKyberCrystal : MonoBehaviour {
        public Item item;
        public ItemModuleKyberCrystal module;

        public Color bladeColour;
        public Color coreColour;
        public Color glowColour;
        MeshRenderer mesh;
        MaterialPropertyBlock propBlock;
        Transform itemTrans;
        Transform leftHandTrans;
        Transform rightHandTrans;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleKyberCrystal>();

            bladeColour = new Color(module.bladeColour[0], module.bladeColour[1], module.bladeColour[2], module.bladeColour[3]);
            coreColour = new Color(module.coreColour[0], module.coreColour[1], module.coreColour[2], module.coreColour[3]);
            glowColour = new Color(module.glowColour[0], module.glowColour[1], module.glowColour[2], module.glowColour[3]);

            mesh = item.GetCustomReference("Mesh").GetComponent<MeshRenderer>();
            propBlock = new MaterialPropertyBlock();
            mesh.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", coreColour);
            mesh.SetPropertyBlock(propBlock);

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

        public float getClosestHandDistance() {
            if (!Player.local) return float.MaxValue;
            if (!leftHandTrans) leftHandTrans = Player.local.handLeft.transform;
            if (!rightHandTrans) rightHandTrans = Player.local.handRight.transform;
            var distanceToHandLeft = Vector3.SqrMagnitude(itemTrans.position - leftHandTrans.position);
            var distanceToHandRight = Vector3.SqrMagnitude(itemTrans.position - rightHandTrans.position);
            return (distanceToHandLeft < distanceToHandRight) ? distanceToHandLeft : distanceToHandRight;
        }

        protected void Update() {
            var distanceToHand = getClosestHandDistance();
            var minGlow = 0.33f;
            var maxGlow = 3f;
            var flicker = module.isUnstable ? Random.Range(-0.2f, 0.2f) : Random.Range(-0.04f, 0.04f);
            var intensity = Mathf.Clamp(maxGlow - (10 * distanceToHand) + flicker, minGlow, maxGlow);
            mesh.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", new Color(bladeColour.r * intensity, bladeColour.g * intensity, bladeColour.b * intensity, bladeColour.a));
            mesh.SetPropertyBlock(propBlock);
        }
    }
}