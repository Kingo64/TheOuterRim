using ThunderRoad;
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
        RenderTexture renderScopeTextureL;

        Renderer scopeR;
        Camera scopeCameraR;
        RenderTexture renderScopeTextureR;

        int currentScopeZoom;

        MaterialPropertyBlock _propBlock;
        public MaterialPropertyBlock PropBlock {
            get {
                _propBlock = _propBlock ?? new MaterialPropertyBlock();
                return _propBlock;
            }
        }

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBinoculars>();
            body = this.GetComponent<Rigidbody>();

            // setup item events
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnHeldActionEvent += OnHeldAction;

            SetupScope(item.GetCustomReference(module.leftScopeID), item.GetCustomReference(module.leftScopeCameraID).GetComponent<Camera>(), ref scopeL, ref scopeCameraL, ref renderScopeTextureL);
            SetupScope(item.GetCustomReference(module.rightScopeID), item.GetCustomReference(module.rightScopeCameraID).GetComponent<Camera>(), ref scopeR, ref scopeCameraR, ref renderScopeTextureR);
            if (!string.IsNullOrEmpty(module.zoomSoundsID)) zoomSounds = item.GetCustomReference(module.zoomSoundsID).GetComponents<AudioSource>();
        }

        void SetupScope(Transform scopeTransform, Camera scopeCamera, ref Renderer scope, ref Camera storedCamera, ref RenderTexture renderTexture) {
            if (scopeTransform != null) {
                scope = scopeTransform.GetComponent<Renderer>();

                scopeCamera.enabled = false;
                storedCamera = scopeCamera;
                scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
                renderTexture = new RenderTexture(
                    module.scopeResolution != null ? module.scopeResolution[0] : GlobalSettings.BlasterScopeResolution,
                    module.scopeResolution != null ? module.scopeResolution[1] : GlobalSettings.BlasterScopeResolution,
                    module.scopeDepth, RenderTextureFormat.DefaultHDR);
                scopeCamera.targetTexture = renderTexture;
                scope.GetPropertyBlock(PropBlock);
                PropBlock.SetTexture("_BaseMap", renderTexture);
                scope.SetPropertyBlock(PropBlock);
                if (GlobalSettings.BlasterScope3D) scope.sharedMaterial.EnableKeyword("_3D_SCOPE"); else scope.sharedMaterial.DisableKeyword("_3D_SCOPE");
            }
        }

        void SetScopeRender(Renderer scope, Camera scopeCamera, bool state, ref RenderTexture renderTexture) {
            if (scope == null) return;
            scopeCamera.enabled = state;
            if (state) {
                if (!renderTexture.IsCreated()) renderTexture.Create();
                scope.material.EnableKeyword("_SCOPE_ACTIVE");
            } else {
                renderTexture.Release();
                scope.material.DisableKeyword("_SCOPE_ACTIVE");
            }
        }

        void CycleScope(RagdollHand interactor = null) {
            if (scopeL == null || scopeCameraL == null || scopeR == null || scopeCameraR == null) return;
            currentScopeZoom = (currentScopeZoom >= module.scopeZoom.Length - 1) ? -1 : currentScopeZoom;
            currentScopeZoom++;
            scopeCameraL.fieldOfView = module.scopeZoom[currentScopeZoom];
            scopeCameraR.fieldOfView = module.scopeZoom[currentScopeZoom];
            if (zoomSounds != null) Utils.PlayRandomSound(zoomSounds);
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
        }

        void CycleScopeBack(RagdollHand interactor = null) {
            if (scopeL == null || scopeCameraL == null || scopeR == null || scopeCameraR == null) return;
            currentScopeZoom = (currentScopeZoom <= 0) ? module.scopeZoom.Length : currentScopeZoom;
            currentScopeZoom--;
            scopeCameraL.fieldOfView = module.scopeZoom[currentScopeZoom];
            scopeCameraR.fieldOfView = module.scopeZoom[currentScopeZoom];
            if (zoomSounds != null) Utils.PlayRandomSound(zoomSounds);
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
        }

        public void ExecuteAction(string action, RagdollHand ragdollHand = null) {
            if (action == "cycleScope") {
                CycleScope(ragdollHand);
            } else if (action == "cycleScopeBack") {
                CycleScopeBack(ragdollHand);
            }
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            // toggle scope for performance reasons
            if (interactor.playerHand) {
                SetScopeRender(scopeL, scopeCameraL, true, ref renderScopeTextureL);
                SetScopeRender(scopeR, scopeCameraR, true, ref renderScopeTextureR);
            }
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            // toggle scope for performance reasons
            SetScopeRender(scopeL, scopeCameraL, false, ref renderScopeTextureL);
            SetScopeRender(scopeR, scopeCameraR, false, ref renderScopeTextureR);
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action) {
            if (action == Interactable.Action.UseStart) {
                if (interactor.side == Side.Right) ExecuteAction(module.rightGripPrimaryAction, interactor);
                else ExecuteAction(module.leftGripPrimaryAction, interactor);
            } else if (action == Interactable.Action.AlternateUseStart) {
                if (interactor.side == Side.Right) ExecuteAction(module.rightGripSecondaryAction, interactor);
                else ExecuteAction(module.leftGripSecondaryAction, interactor);
            }
        }
    }
}