using UnityEngine;
using UnityEngine.UI;
using ThunderRoad;
using System.Collections.Generic;

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

        HashSet<ColliderGroup> playerColliders = new HashSet<ColliderGroup>();

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBactaStim>();

            for (int i = 0, l = item.definition.collisionHandlers.Count; i < l; i++) {
                item.definition.collisionHandlers[i].OnCollisionStartEvent += CollisionHandler;
            }

            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;

            tip = item.definition.GetCustomReference("Tip").GetComponent<Collider>();
            light = item.definition.GetCustomReference("Light").GetComponent<Light>();
            text = item.definition.GetCustomReference("Text").GetComponent<Text>();
            injectSound = item.definition.GetCustomReference("InjectSound").GetComponent<AudioSource>();
            rechargeSound = item.definition.GetCustomReference("RechargeSound").GetComponent<AudioSource>();

            light.enabled = false;
            text.enabled = false;
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            healer = interactor.bodyHand.body.creature;
            holdingRight |= interactor.playerHand == Player.local.handRight;
            holdingLeft |= interactor.playerHand == Player.local.handLeft;
            light.enabled = true;
            text.enabled = true;
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool throwing) {
            healer = null;
            holdingRight &= interactor.playerHand == Player.local.handRight;
            holdingLeft &= interactor.playerHand == Player.local.handLeft;
            light.enabled = false;
            text.enabled = false;
        }

        void CollisionHandler(ref CollisionStruct collisionInstance) {
            if (collisionInstance.sourceCollider == tip || collisionInstance.targetCollider == tip) {
                if (currentCharge >= 100) {
                    if (playerColliders.Contains(collisionInstance.targetColliderGroup) || playerColliders.Contains(collisionInstance.sourceColliderGroup)) {
                        Heal(Player.local.body.creature);
                        return;
                    }
                    var ragdollPart = collisionInstance.targetColliderGroup?.collisionHandler?.ragdollPart ?? collisionInstance.sourceColliderGroup?.collisionHandler?.ragdollPart;
                    if (ragdollPart) {
                        var creature = ragdollPart.ragdoll.creature;
                        Heal(creature);
                        return;
                    }
                }
            }
        }

        void Heal(Creature creature) {
            if (creature && currentCharge >= 100) {
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
            if (Player.local && playerColliders.Count < 1) {
                try {
                    playerColliders.UnionWith(Player.local.GetHand(Side.Left).itemHand.item.definition.colliderGroups);
                    playerColliders.UnionWith(Player.local.GetHand(Side.Right).itemHand.item.definition.colliderGroups);
                }
                catch { }
            }

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
            if (creature == null || creature.state == Creature.State.Dead || duration >= healDuration || creature.health.currentHealth >= creature.health.maxHealth) Destroy(this);
            creature.health.Heal(healAmount * Time.deltaTime, healer);
            duration += Time.deltaTime;
        }
    }
}