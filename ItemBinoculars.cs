using BS;
using UnityEngine;

namespace TOR {
    public class ItemBinoculars : MonoBehaviour {
        protected Item item;
        protected ItemModuleBinoculars module;

        protected Rigidbody body;

        protected Handle leftGrip;
        protected Handle rightGrip;
        protected AudioSource[] zoomSounds;

        Renderer scopeL;
        Camera scopeCameraL;
        Material scopeMaterialL;
        Material originalScopeMaterialL;
        RenderTexture renderScopeTextureL;

        Renderer scopeR;
        Camera scopeCameraR;
        Material scopeMaterialR;
        Material originalScopeMaterialR;
        RenderTexture renderScopeTextureR;

        public bool holdingGunGripLeft;
        public bool holdingGunGripRight;
        public bool holdingForeGripLeft;
        public bool holdingForeGripRight;
        public bool holdingScopeGripLeft;
        public bool holdingScopeGripRight;
        public bool holdingSecondaryGripLeft;
        public bool holdingSecondaryGripRight;

        int currentScopeZoom;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBinoculars>();
            body = this.GetComponent<Rigidbody>();

            // setup item events
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnHeldActionEvent += OnHeldAction;

            SetupScopeLeft(item.definition.GetCustomReference(module.leftScopeID), item.definition.GetCustomReference(module.leftScopeCameraID).GetComponent<Camera>());
            SetupScopeRight(item.definition.GetCustomReference(module.rightScopeID), item.definition.GetCustomReference(module.rightScopeCameraID).GetComponent<Camera>());
            if (!string.IsNullOrEmpty(module.zoomSoundsID)) zoomSounds = item.definition.GetCustomReference(module.zoomSoundsID).GetComponents<AudioSource>();
        }

        void SetupScopeLeft(Transform scopeTransform, Camera scopeCamera) {
            if (scopeTransform != null) {
                scopeL = scopeTransform.GetComponent<Renderer>();
                originalScopeMaterialL = scopeL.materials[0];
                scopeMaterialL = scopeL.materials[1];
                scopeL.materials = new Material[] { originalScopeMaterialL };

                scopeCameraL = scopeCamera;
                scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
                renderScopeTextureL = new RenderTexture(module.scopeResolution[0], module.scopeResolution[1], module.scopeDepth, RenderTextureFormat.Default);
                renderScopeTextureL.Create();
                scopeCamera.targetTexture = renderScopeTextureL;
                scopeMaterialL.SetTexture("_BaseMap", renderScopeTextureL);
            }
        }

        void SetupScopeRight(Transform scopeTransform, Camera scopeCamera) {
            if (scopeTransform != null) {
                scopeR = scopeTransform.GetComponent<Renderer>();
                originalScopeMaterialR = scopeR.materials[0];
                scopeMaterialR = scopeR.materials[1];
                scopeR.materials = new Material[] { originalScopeMaterialR };

                scopeCameraR = scopeCamera;
                scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
                renderScopeTextureR = new RenderTexture(module.scopeResolution[0], module.scopeResolution[1], module.scopeDepth, RenderTextureFormat.Default);
                renderScopeTextureR.Create();
                scopeCamera.targetTexture = renderScopeTextureR;
                scopeMaterialR.SetTexture("_BaseMap", renderScopeTextureR);
            }
        }

        void SetScopeRender(Renderer scope, Camera scopeCamera, Material material, bool state) {
            if (scope == null) return;
            scopeCamera.enabled = state;
            scope.material = material;
        }

        void CycleScope() {
            if (scopeL == null || scopeCameraL == null || scopeR == null || scopeCameraR == null) return;
            currentScopeZoom = (currentScopeZoom >= module.scopeZoom.Length - 1) ? -1 : currentScopeZoom;
            currentScopeZoom++;
            scopeCameraL.fieldOfView = module.scopeZoom[currentScopeZoom];
            scopeCameraR.fieldOfView = module.scopeZoom[currentScopeZoom];
            if (zoomSounds != null) Utils.PlayRandomSound(zoomSounds);
        }

        void CycleScopeBack() {
            if (scopeL == null || scopeCameraL == null || scopeR == null || scopeCameraR == null) return;
            currentScopeZoom = (currentScopeZoom <= 0) ? module.scopeZoom.Length : currentScopeZoom;
            currentScopeZoom--;
            scopeCameraL.fieldOfView = module.scopeZoom[currentScopeZoom];
            scopeCameraR.fieldOfView = module.scopeZoom[currentScopeZoom];
            if (zoomSounds != null) Utils.PlayRandomSound(zoomSounds);
        }

        public void ExecuteAction(string action) {
            if (action == "cycleScope") {
                CycleScope();
            } else if (action == "cycleScopeBack") {
                CycleScopeBack();
            }
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            // toggle scope for performance reasons
            if (interactor.playerHand == Player.local.handRight || interactor.playerHand == Player.local.handLeft) {
                SetScopeRender(scopeL, scopeCameraL, scopeMaterialL, true);
                SetScopeRender(scopeR, scopeCameraR, scopeMaterialR, true);
            }
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool throwing) {
            // toggle scope for performance reasons
            SetScopeRender(scopeL, scopeCameraL, originalScopeMaterialL, false);
            SetScopeRender(scopeR, scopeCameraR, originalScopeMaterialR, false);
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
            if (action == Interactable.Action.UseStart) {
                if (interactor.side == Side.Right) ExecuteAction(module.rightGripPrimaryAction);
                else ExecuteAction(module.leftGripPrimaryAction);
            } else if (action == Interactable.Action.AlternateUseStart) {
                if (interactor.side == Side.Right) ExecuteAction(module.rightGripSecondaryAction);
                else ExecuteAction(module.leftGripSecondaryAction);
            }
        }
    }
}