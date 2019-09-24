using BS;
using UnityEngine;

namespace TOR {
    // The item module will add a unity component to the item object. See unity monobehaviour for more information: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    // This component will apply a force on the player rigidbody to the direction of an item transform when the trigger is pressed (see custom reference in the item definition component of the item prefab)
    public class ItemBinoculars : MonoBehaviour {
        protected Item item;
        protected ItemModuleBinoculars module;

        protected Rigidbody body;

        protected Handle leftGrip;
        protected Handle rightGrip;
        protected AudioSource[] zoomSounds;

        Renderer scopeL;
        Camera scopeCameraL;
        Texture originalScopeTextureL;
        RenderTexture renderScopeTextureL;

        Renderer scopeR;
        Camera scopeCameraR;
        Texture originalScopeTextureR;
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
                originalScopeTextureL = scopeL.material.mainTexture;

                scopeCameraL = scopeCamera;
                scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
                renderScopeTextureL = new RenderTexture(module.scopeResolution[0], module.scopeResolution[1], module.scopeDepth, RenderTextureFormat.Default);
                renderScopeTextureL.Create();
                scopeCamera.targetTexture = renderScopeTextureL;
            }
        }

        void SetupScopeRight(Transform scopeTransform, Camera scopeCamera) {
            if (scopeTransform != null) {
                scopeR = scopeTransform.GetComponent<Renderer>();
                originalScopeTextureR = scopeR.material.mainTexture;

                scopeCameraR = scopeCamera;
                scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
                renderScopeTextureR = new RenderTexture(module.scopeResolution[0], module.scopeResolution[1], module.scopeDepth, RenderTextureFormat.Default);
                renderScopeTextureR.Create();
                scopeCamera.targetTexture = renderScopeTextureR;
            }
        }

        void EnableScopeRender(Renderer scope, RenderTexture renderTexture) {
            if (scope == null) return;
            scope.material.mainTexture = renderTexture;
            scope.material.SetTexture("_EmissionMap", renderTexture);
            scope.material.SetColor("_EmissionColor", new Color(0.9f, 0.9f, 0.9f, 0.9f));
        }

        void DisableScopeRender(Renderer scope, Texture originalTexture) {
            if (scope == null) return;
            scope.material.mainTexture = originalTexture;
            scope.material.SetTexture("_EmissionMap", null);
            scope.material.SetColor("_EmissionColor", new Color(0.0f, 0.0f, 0.0f, 0.0f));
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
            EnableScopeRender(scopeL, renderScopeTextureL);
            EnableScopeRender(scopeR, renderScopeTextureR);
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool throwing) {
            // toggle scope for performance reasons
            DisableScopeRender(scopeL, originalScopeTextureL);
            DisableScopeRender(scopeR, originalScopeTextureR);
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