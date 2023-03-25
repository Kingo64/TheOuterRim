using UnityEngine;
using ThunderRoad;
using System;

namespace TOR {
    public class ItemBottle : MonoBehaviour {
        protected Item item;
        protected ItemModuleBottle module;
        protected ItemModulePotion potion;
        protected LiquidContainer liquidContainer;

        public MeshRenderer[] renderers;
        public LiquidData.Content liquid;
        readonly float bottleHeight = 0.25f;

        MaterialPropertyBlock _propBlock;
        public MaterialPropertyBlock PropBlock {
            get {
                _propBlock = _propBlock ?? new MaterialPropertyBlock();
                return _propBlock;
            }
        }

        Vector3 lastPos;
        Vector3 lastRot;
        float wobbleAmountToAddX;
        float wobbleAmountToAddZ;
        readonly float wobbleMax = 0.02f;
        readonly float wobbleSpeed = 2f;
        readonly float wobbleRecovery = 3f;
        float time;
        float deferRenderTime;

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBottle>();
            potion = item.data.GetModule<ItemModulePotion>();
            liquidContainer = item.GetComponent<LiquidContainer>();
            liquid = liquidContainer.contents[0];

            renderers = item.GetCustomReference("Liquid").GetComponentsInChildren<MeshRenderer>();
            for (int i = 0, l = renderers.Length; i < l; i++) {
                renderers[i].GetPropertyBlock(PropBlock);
                PropBlock.SetColor("_BaseColor", liquid.liquidData.color);
                renderers[i].SetPropertyBlock(PropBlock);
            }
        }

        protected void Update() {
            UpdateLiquidRender();
        }

        float GetCurrentLevel() {
            return liquid.level / potion.maxLevel;
        }

        void UpdateLiquidRender() {
            if (renderers == null || liquid == null || !PlayerControl.userPresence || !PlayerControl.vrEnabled || !PlayerControl.userPresence || GameManager.timeStopped) {
                deferRenderTime = 1f;
                return;
            }

            if (deferRenderTime > 0) {
                deferRenderTime -= Time.deltaTime;
                return;
            }

            var angle = Mathf.Abs(Vector3.SignedAngle(Vector3.up, transform.up, transform.forward)) / 180;
            var tiltLevel = Mathf.Lerp(angle * ((GetCurrentLevel() + 0.4f) * 2f), 1f, Mathf.Abs(1 - (angle / 0.5f)));
            var adjustedLevel = liquid.level > 0 ? GetCurrentLevel() * bottleHeight - (angle * bottleHeight * tiltLevel) : -1f;

            time = time > 1 ? Time.deltaTime : time + Time.deltaTime;
            wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * wobbleRecovery);
            wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * wobbleRecovery);

            var pulse = 2 * Mathf.PI * wobbleSpeed;
            var wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time) * GetCurrentLevel();
            var wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time) * GetCurrentLevel();

            var pos = transform.position;
            var rot = transform.rotation.eulerAngles;
            var velocity = (lastPos - pos) / Time.deltaTime;
            var angularVelocity = rot - lastRot;
            lastPos = pos;
            lastRot = rot;

            wobbleAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * wobbleMax, -wobbleMax, wobbleMax);
            wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * wobbleMax, -wobbleMax, wobbleMax);

            for (int i = 0, l = renderers.Length; i < l; i++) {
                renderers[i].GetPropertyBlock(PropBlock);
                PropBlock.SetFloat("Level", adjustedLevel);
                PropBlock.SetFloat("WobbleX", wobbleAmountX);
                PropBlock.SetFloat("WobbleZ", wobbleAmountZ);
                renderers[i].SetPropertyBlock(PropBlock);
            }
        }
    }
}