using UnityEngine;
using UnityEngine.UI;
using ThunderRoad;
using System.Collections.Generic;
using System.Collections;

namespace TOR {
    public class ItemBactaStim : MonoBehaviour {
        protected Item item;
        protected ItemModuleBactaStim module;

        public float currentCharge = 100;
        Collider tip;
        Light light;
        Text text;
        bool holdingLeft;
        bool holdingRight;
        Creature healer;

        AudioSource injectSound;
        AudioSource rechargeSound;

        HashSet<Collider> playerColliders = new HashSet<Collider>();

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBactaStim>();

            for (int i = 0, l = item.collisionHandlers.Count; i < l; i++) {
                item.collisionHandlers[i].OnCollisionStartEvent += CollisionHandler;
            }

            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;

            tip = item.GetCustomReference("Tip").GetComponent<Collider>();
            light = item.GetCustomReference("Light").GetComponent<Light>();
            text = item.GetCustomReference("Text").GetComponent<Text>();
            injectSound = item.GetCustomReference("InjectSound").GetComponent<AudioSource>();
            rechargeSound = item.GetCustomReference("RechargeSound").GetComponent<AudioSource>();

            light.enabled = false;
            text.enabled = false;

            var coroutine = GetPlayerColliders();
            StartCoroutine(coroutine);
        }

        private IEnumerator GetPlayerColliders() {
            while (Player.local && playerColliders.Count < 1) {
                try {
                    playerColliders.UnionWith(Player.local.handLeft.ragdollHand.colliderGroup.colliders);
                    playerColliders.UnionWith(Player.local.handRight.ragdollHand.colliderGroup.colliders);
                    playerColliders.UnionWith(Player.local.creature.ragdoll.GetPart(RagdollPart.Type.Neck).colliderGroup.colliders);
                    playerColliders.UnionWith(Player.local.creature.ragdoll.GetPart(RagdollPart.Type.LeftArm).colliderGroup.colliders);
                    playerColliders.UnionWith(Player.local.creature.ragdoll.GetPart(RagdollPart.Type.RightArm).colliderGroup.colliders);
                    playerColliders.UnionWith(Player.local.creature.ragdoll.GetPart(RagdollPart.Type.LeftLeg).colliderGroup.colliders);
                    playerColliders.UnionWith(Player.local.creature.ragdoll.GetPart(RagdollPart.Type.RightLeg).colliderGroup.colliders);
                    yield break;
                }
                catch { }
                yield return new WaitForSeconds(1f);
            }
            yield break;
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            healer = interactor.creature;
            holdingRight |= interactor.playerHand == Player.local.handRight;
            holdingLeft |= interactor.playerHand == Player.local.handLeft;
            light.enabled = true;
            text.enabled = true;
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            healer = null;
            holdingRight &= interactor.playerHand == Player.local.handRight;
            holdingLeft &= interactor.playerHand == Player.local.handLeft;
            light.enabled = false;
            text.enabled = false;
        }

        void CollisionHandler(ref CollisionStruct collisionInstance) {
            if (item.IsHanded(null) && (collisionInstance.sourceCollider == tip || collisionInstance.targetCollider == tip)) {
                if (currentCharge >= 100) {
                    var ragdollPart = collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart ?? collisionInstance.sourceColliderGroup?.collisionHandler?.ragdollPart;
                    if (ragdollPart) {
                        var creature = ragdollPart.ragdoll.creature;
                        Heal(creature);
                        return;
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other) {
            if (currentCharge >= 100 && playerColliders.Contains(other)) {
               Heal(Player.local.creature);
            }
        }

        void Heal(Creature creature) {
            if (creature && currentCharge >= 100 && creature.currentHealth < creature.maxHealth) {
                injectSound.Play();
                BactaStimHeal heal = creature.gameObject.AddComponent<BactaStimHeal>();
                heal.healAmount = module.healAmount;
                heal.healDuration = module.healDuration;
                heal.creature = creature;
                heal.healer = healer;
                currentCharge = 0;
                Utils.PlayHaptic(holdingLeft, holdingRight, Utils.HapticIntensity.Major);
            }
        }

        void Update() {
            if (currentCharge < 100) {
                currentCharge = Mathf.Clamp(currentCharge + (module.chargeRate * Time.deltaTime), 0, 100);
                text.text = Mathf.RoundToInt(currentCharge).ToString();
                if (currentCharge >= 100) rechargeSound.Play();
            }
        }
    }

    public class BactaStimHeal : MonoBehaviour {
        public float healAmount = 10f;
        public float healDuration = 10f;
        public Creature creature;
        public Creature healer;

        float duration;

        void Update() {
            if (creature == null || creature.state == Creature.State.Dead || duration >= healDuration || creature.currentHealth >= creature.maxHealth) Destroy(this);
            creature.Heal(healAmount * Time.deltaTime, healer);
            duration += Time.deltaTime;
        }
    }
}