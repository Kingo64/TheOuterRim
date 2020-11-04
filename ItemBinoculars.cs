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
        Material scopeMaterialL;
        Material originalScopeMaterialL;
        RenderTexture renderScopeTextureL;

        Renderer scopeR;
        Camera scopeCameraR;
        Material scopeMaterialR;
        Material originalScopeMaterialR;
        RenderTexture renderScopeTextureR;

        int currentScopeZoom;

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBinoculars>();
            body = this.GetComponent<Rigidbody>();

            // setup item events
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnHeldActionEvent += OnHeldAction;

            SetupScope(item.definition.GetCustomReference(module.leftScopeID), item.definition.GetCustomReference(module.leftScopeCameraID).GetComponent<Camera>(), ref scopeL, ref scopeCameraL, ref originalScopeMaterialL, ref scopeMaterialL, ref renderScopeTextureL);
            SetupScope(item.definition.GetCustomReference(module.rightScopeID), item.definition.GetCustomReference(module.rightScopeCameraID).GetComponent<Camera>(), ref scopeR, ref scopeCameraR, ref originalScopeMaterialR, ref scopeMaterialR, ref renderScopeTextureR);
            if (!string.IsNullOrEmpty(module.zoomSoundsID)) zoomSounds = item.definition.GetCustomReference(module.zoomSoundsID).GetComponents<AudioSource>();
        }

        void SetupScope(Transform scopeTransform, Camera scopeCamera, ref Renderer scope, ref Camera storedCamera, ref Material originalScopeMaterial, ref Material scopeMaterial, ref RenderTexture renderTexture) {
            if (scopeTransform != null) {
                scope = scopeTransform.GetComponent<Renderer>();
                originalScopeMaterial = scopeL.materials[0];
                scopeMaterial = scope.materials[1];
                scope.materials = new Material[] { originalScopeMaterial };

                scopeCamera.enabled = false;
                storedCamera = scopeCamera;
                scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
                renderTexture = new RenderTexture(
                    module.scopeResolution != null ? module.scopeResolution[0] : TORGlobalSettings.BlasterScopeResolution[0],
                    module.scopeResolution != null ? module.scopeResolution[1] : TORGlobalSettings.BlasterScopeResolution[1],
                    module.scopeDepth, RenderTextureFormat.Default);
                renderTexture.Create();
                scopeCamera.targetTexture = renderTexture;
                scopeMaterial.SetTexture("_BaseMap", renderTexture);
            }
        }

        void SetScopeRender(Renderer scope, Camera scopeCamera, Material material, bool state) {
            if (scope == null) return;
            scopeCamera.enabled = state;
            scope.material = material;
        }

        void CycleScope(Interactor interactor = null) {
            if (scopeL == null || scopeCameraL == null || scopeR == null || scopeCameraR == null) return;
            currentScopeZoom = (currentScopeZoom >= module.scopeZoom.Length - 1) ? -1 : currentScopeZoom;
            currentScopeZoom++;
            scopeCameraL.fieldOfView = module.scopeZoom[currentScopeZoom];
            scopeCameraR.fieldOfView = module.scopeZoom[currentScopeZoom];
            if (zoomSounds != null) Utils.PlayRandomSound(zoomSounds);
            if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
        }

        void CycleScopeBack(Interactor interactor = null) {
            if (scopeL == null || scopeCameraL == null || scopeR == null || scopeCameraR == null) return;
            currentScopeZoom = (currentScopeZoom <= 0) ? module.scopeZoom.Length : currentScopeZoom;
            currentScopeZoom--;
            scopeCameraL.fieldOfView = module.scopeZoom[currentScopeZoom];
            scopeCameraR.fieldOfView = module.scopeZoom[currentScopeZoom];
            if (zoomSounds != null) Utils.PlayRandomSound(zoomSounds);
            if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
        }

        public void ExecuteAction(string action, Interactor interactor = null) {
            if (action == "cycleScope") {
                CycleScope(interactor);
            } else if (action == "cycleScopeBack") {
                CycleScopeBack(interactor);
            }
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            // toggle scope for performance reasons
            if (interactor.playerHand) {
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
                if (interactor.side == Side.Right) ExecuteAction(module.rightGripPrimaryAction, interactor);
                else ExecuteAction(module.leftGripPrimaryAction, interactor);
            } else if (action == Interactable.Action.AlternateUseStart) {
                if (interactor.side == Side.Right) ExecuteAction(module.rightGripSecondaryAction, interactor);
                else ExecuteAction(module.leftGripSecondaryAction, interactor);
            }
        }
    }
}