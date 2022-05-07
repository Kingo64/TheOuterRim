using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemImbueToggle : MonoBehaviour {
        protected Item item;
        protected ItemModuleImbueToggle module;
        SpellCastCharge castCharge;

        Handle grip;
        Imbue imbue;

        float primaryControlHoldTime;
        float secondaryControlHoldTime;

        bool isEnabled;
        bool holdingGripRight;
        bool holdingGripLeft;

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleImbueToggle>();

            item.OnHeldActionEvent += OnHeldAction;

            if (!string.IsNullOrEmpty(module.gripID)) grip = item.GetCustomReference(module.gripID).GetComponent<Handle>();
            if (grip) {
                grip.Grabbed += OnGrabbed;
                grip.UnGrabbed += OnUngrabbed;
            }

            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;

            castCharge = Catalog.GetData<SpellCastCharge>(module.spell, true);
        }

        public void ExecuteAction(string action, RagdollHand interactor = null) {
            if (action == "toggleImbue") {
                if (isEnabled) TurnOff();
                else TurnOn();
            }
        }

        void TurnOn() {
            if (imbue && !isEnabled) {
                imbue.Transfer(castCharge, imbue.maxEnergy);
                Utils.PlayHaptic(holdingGripLeft, holdingGripRight, Utils.HapticIntensity.Moderate);
                isEnabled = true;
            }
        }

        void TurnOff() {
            if (imbue && isEnabled) {
                imbue.energy = 0;
                Utils.PlayHaptic(holdingGripLeft, holdingGripRight, Utils.HapticIntensity.Minor);
                isEnabled = false;
            }
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            if (!interactor.playerHand)
                TurnOn();
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool thrown) {
            if (!interactor.playerHand)
                TurnOff();
        }

        private void OnGrabbed(RagdollHand interactor, Handle handle, EventTime eventTime) {
            holdingGripRight = interactor.playerHand == Player.local.handRight;
            holdingGripLeft = interactor.playerHand == Player.local.handLeft;
        }

        private void OnUngrabbed(RagdollHand interactor, Handle handle, EventTime eventTime) {
            if (interactor.playerHand == Player.local.handRight) holdingGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingGripLeft = false;
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action) {
            if (!grip || handle == grip) {
                // If primary hold action available
                if (!string.IsNullOrEmpty(module.primaryActionHold)) {
                    // start primary control timer
                    if (action == Interactable.Action.UseStart) {
                        primaryControlHoldTime = GlobalSettings.ControlsHoldDuration;
                    } else if (action == Interactable.Action.UseStop) {
                        // if not held for long run standard action
                        if (primaryControlHoldTime > 0 && primaryControlHoldTime > (primaryControlHoldTime / 2)) {
                            ExecuteAction(module.primaryAction, interactor);
                        }
                        primaryControlHoldTime = 0;
                    }
                } else if (action == Interactable.Action.UseStart) ExecuteAction(module.primaryAction, interactor);

                // If secondary hold action available
                if (!string.IsNullOrEmpty(module.secondaryActionHold)) {
                    // start secondary control timer
                    if (action == Interactable.Action.AlternateUseStart) {
                        secondaryControlHoldTime = GlobalSettings.ControlsHoldDuration;
                    } else if (action == Interactable.Action.AlternateUseStop) {
                        // if not held for long run standard action
                        if (secondaryControlHoldTime > 0 && secondaryControlHoldTime > (secondaryControlHoldTime / 2)) {
                            ExecuteAction(module.secondaryAction, interactor);
                        }
                        secondaryControlHoldTime = 0;
                    }
                } else if (action == Interactable.Action.AlternateUseStart) ExecuteAction(module.secondaryAction, interactor);
            }
        }

        void Update() {
            if (primaryControlHoldTime > 0) {
                primaryControlHoldTime -= Time.deltaTime;
                if (primaryControlHoldTime <= 0) ExecuteAction(module.primaryActionHold);
            }
            if (secondaryControlHoldTime > 0) {
                secondaryControlHoldTime -= Time.deltaTime;
                if (secondaryControlHoldTime <= 0) ExecuteAction(module.secondaryActionHold);
            }

            if (!imbue && item.imbues.Count > 0) {
                imbue = item.imbues[0];
            }
        }
    }
}