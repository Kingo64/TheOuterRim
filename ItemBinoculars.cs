using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemBinoculars : ThunderBehaviour {
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

        MaterialInstance _scopeMaterialInstanceL;
        public MaterialInstance scopeMaterialInstanceL {
            get {
                if (_scopeMaterialInstanceL == null) {
                    scopeL.gameObject.TryGetOrAddComponent(out MaterialInstance mi);
                    _scopeMaterialInstanceL = mi;
                }
                return _scopeMaterialInstanceL;
            }
        }

        MaterialInstance _scopeMaterialInstanceR;
        public MaterialInstance scopeMaterialInstanceR {
            get {
                if (_scopeMaterialInstanceR == null) {
                    scopeR.gameObject.TryGetOrAddComponent(out MaterialInstance mi);
                    _scopeMaterialInstanceR = mi;
                }
                return _scopeMaterialInstanceR;
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

            scopeL = item.GetCustomReference(module.leftScopeID).GetComponent<Renderer>();
            scopeR = item.GetCustomReference(module.rightScopeID).GetComponent<Renderer>();
            SetupScope(item.GetCustomReference(module.leftScopeCameraID).GetComponent<Camera>(), scopeMaterialInstanceL, ref scopeCameraL, ref renderScopeTextureL);
            SetupScope(item.GetCustomReference(module.rightScopeCameraID).GetComponent<Camera>(), scopeMaterialInstanceR, ref scopeCameraR, ref renderScopeTextureR);
            if (!string.IsNullOrEmpty(module.zoomSoundsID)) zoomSounds = item.GetCustomReference(module.zoomSoundsID).GetComponents<AudioSource>();
        }

        protected void OnDestroy() {
            if (scopeCameraL) scopeCameraL.targetTexture = null;
            if (scopeCameraR) scopeCameraR.targetTexture = null;
            if (renderScopeTextureL) {
                if (renderScopeTextureL.IsCreated()) {
                    renderScopeTextureL.Release();
                }
                Destroy(renderScopeTextureL);
                renderScopeTextureL = null;
            }
            if (renderScopeTextureR) {
                if (renderScopeTextureR.IsCreated()) {
                    renderScopeTextureR.Release();
                }
                Destroy(renderScopeTextureR);
                renderScopeTextureR = null;
            }
        }

        void SetupScope(Camera scopeCamera, MaterialInstance scopeMaterial, ref Camera storedCamera, ref RenderTexture renderTexture) {
            scopeCamera.enabled = false;
            storedCamera = scopeCamera;
            scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
            renderTexture = new RenderTexture(
                module.scopeResolution != null ? module.scopeResolution[0] : GlobalSettings.BlasterScopeResolution,
                module.scopeResolution != null ? module.scopeResolution[1] : GlobalSettings.BlasterScopeResolution,
                module.scopeDepth, RenderTextureFormat.DefaultHDR);
            scopeMaterial.material.SetTexture("_RenderTexture", renderTexture);
            if (GlobalSettings.BlasterScope3D) scopeMaterial.material.EnableKeyword("_3D_SCOPE"); else scopeMaterial.material.DisableKeyword("_3D_SCOPE");
        }

        void SetScopeRender(MaterialInstance scopeMaterial, Camera scopeCamera, bool state, ref RenderTexture renderTexture) {
            if (scopeMaterial == null) return;
            scopeCamera.enabled = state;
            if (state) {
                if (!renderTexture.IsCreated()) renderTexture.Create();
                scopeCamera.targetTexture = renderTexture;
                scopeMaterial.material.EnableKeyword("_SCOPE_ACTIVE");
            } else {
                scopeCamera.targetTexture = null;
                renderTexture.Release();
                scopeMaterial.material.DisableKeyword("_SCOPE_ACTIVE");
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
                SetScopeRender(scopeMaterialInstanceL, scopeCameraL, true, ref renderScopeTextureL);
                SetScopeRender(scopeMaterialInstanceR, scopeCameraR, true, ref renderScopeTextureR);
            }
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            // toggle scope for performance reasons
            SetScopeRender(scopeMaterialInstanceL, scopeCameraL, false, ref renderScopeTextureL);
            SetScopeRender(scopeMaterialInstanceR, scopeCameraR, false, ref renderScopeTextureR);
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