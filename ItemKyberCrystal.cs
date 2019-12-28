using UnityEngine;
using BS;

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

            mesh = item.definition.GetCustomReference("Mesh").GetComponent<MeshRenderer>();
            propBlock = new MaterialPropertyBlock();
            mesh.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", coreColour);
            mesh.SetPropertyBlock(propBlock);

            itemTrans = item.transform;
            leftHandTrans = Player.local.handLeft.transform;
            rightHandTrans = Player.local.handRight.transform;
        }

        public float getClosestHandDistance() {
            var distanceToHandLeft = Vector3.Distance(itemTrans.position, leftHandTrans.position);
            var distanceToHandRight = Vector3.Distance(itemTrans.position, rightHandTrans.position);
            return (distanceToHandLeft < distanceToHandRight) ? distanceToHandLeft : distanceToHandRight;
        }

        protected void Update() {
            var distanceToHand = getClosestHandDistance();
            var minGlow = 0.33f;
            var maxGlow = 1.5f;
            var flicker = module.isUnstable ? Random.Range(-0.1f, 0.1f) : Random.Range(-0.02f, 0.02f);
            var intensity = Mathf.Clamp(maxGlow - (8 * distanceToHand) + flicker, minGlow, maxGlow);
            mesh.GetPropertyBlock(propBlock);
            propBlock.SetColor("_EmissionColor", new Color(bladeColour.r * intensity, bladeColour.g * intensity, bladeColour.b * intensity, bladeColour.a));
            mesh.SetPropertyBlock(propBlock);
        }
    }
}