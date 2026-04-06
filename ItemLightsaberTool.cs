using UnityEngine;
using ThunderRoad;

namespace TOR {
    public class ItemLightsaberTool : ThunderBehaviour {
        protected Item item;
        protected ItemModuleLightsaberTool module;

        public int currentMode = 0;
        public readonly string[] modes = { "TryEjectCrystal", "IncreaseBladeLength", "DecreaseBladeLength", "ResetBladeLength" };
        public readonly Color[] modeColours = {
            new Color(5.7f, 6f, 0.3f, 1f),
            new Color(0.47f, 6f, 0.3f, 1f),
            new Color(6f, 0.3f, 0.5f, 1f),
            new Color(0.3f, 0.95f, 6f, 1f)
        };
        MeshRenderer mesh;
        bool holdingLeft;
        bool holdingRight;

        MaterialInstance _materialInstance;
        public MaterialInstance materialInstance {
            get {
                if (_materialInstance == null && mesh != null) {
                    mesh.gameObject.TryGetOrAddComponent(out MaterialInstance mi);
                    _materialInstance = mi;
                }
                return _materialInstance;
            }
        }
        static readonly int emissionColorId = Shader.PropertyToID("_EmissionColor");

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLightsaberTool>();

            foreach (var handler in item.collisionHandlers) {
                handler.OnCollisionStartEvent += CollisionHandler;
            }

            item.OnHeldActionEvent += OnHeldAction;
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;

            mesh = item.GetCustomReference("Mesh").GetComponent<MeshRenderer>();
            materialInstance.material.SetColor(emissionColorId, modeColours[currentMode]);
        }

        protected void OnDestroy() {
            if (item != null) {
                item.OnHeldActionEvent -= OnHeldAction;
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
                if (collisionInstance.sourceColliderGroup.name == "CollisionLightsaberTool" && collisionInstance.targetColliderGroup.name == "CollisionHilt") {
                    if (currentMode == 0) {
                        collisionInstance.targetColliderGroup.transform.root.SendMessage(modes[currentMode]);
                    } else {
                        collisionInstance.targetColliderGroup.transform.root.SendMessage(modes[currentMode], GlobalSettings.LightsaberToolAdjustIncrement * 0.1f);
                    }
                    Utils.PlayHaptic(holdingLeft, holdingRight, Utils.HapticIntensity.Major);
                }
            }
            catch { }
        }

        public void ExecuteAction(string action, RagdollHand interactor = null) {
            if (action == "cycleMode") {
                CycleMode(interactor);
            }
        }

        public void CycleMode(RagdollHand interactor = null) {
            currentMode = (currentMode >= modes.Length - 1) ? 0 : currentMode + 1;
            materialInstance.material.SetColor(emissionColorId, modeColours[currentMode]);
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action) {
            if (action == Interactable.Action.UseStart) {
                ExecuteAction(module.gripPrimaryAction, interactor);
            } else if (action == Interactable.Action.AlternateUseStart) {
                ExecuteAction(module.gripSecondaryAction, interactor);
            }
        }
    }
}