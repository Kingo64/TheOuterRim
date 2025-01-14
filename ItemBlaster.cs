using ThunderRoad;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ThunderRoad.Skill.SpellPower;

namespace TOR {
    [System.Serializable]
    public class ItemBlasterSaveData : ContentCustomData {
        public int ammo;
        public bool altFire;
        public int firemode;
        public int firerate;
        public int scopeZoom;
        public string projectileID;
    }

    public class ItemBlaster : ThunderBehaviour {
        public override ManagedLoops EnabledManagedLoops => ManagedLoops.LateUpdate;

        public static List<ItemBlaster> all = new List<ItemBlaster>();
        internal Item item;
        internal ItemModuleBlaster module;

        protected Rigidbody body;
        protected RagdollHand rightInteractor;
        protected RagdollHand leftInteractor;
        public RagdollHand overrideInteractor;
        protected AudioSource altFireSound;
        protected AudioSource altFireSound2;
        protected AudioSource chargeFireSound;
        protected AudioSource chargeFireSound2;
        protected AudioSource chargeFireSound3;
        protected AudioSource chargeSound;
        protected AudioSource chargeSound2;
        protected AudioSource chargeReadySound;
        protected AudioSource chargeReadySound2;
        protected AudioSource chargeStartSound;
        protected AudioSource chargeStartSound2;
        protected AudioSource emptySound;
        protected AudioSource emptySound2;
        protected AudioSource fireSound;
        protected AudioSource fireSound2;
        protected AudioSource fireSound3;
        protected AudioSource fireModeSound;
        protected AudioSource fireModeSound2;
        protected AudioSource overheatSound;
        protected AudioSource overheatSound2;
        protected AudioSource preFireSound;
        protected AudioSource preFireSound2;
        protected AudioSource reloadSound;
        protected AudioSource reloadSound2;
        protected AudioSource reloadEndSound;
        protected AudioSource reloadEndSound2;
        protected AudioSource spinStartSound;
        protected AudioSource spinStartSound2;
        protected AudioSource spinLoopSound;
        protected AudioSource spinLoopSound2;
        protected AudioSource spinStopSound;
        protected AudioSource spinStopSound2;
        protected NoiseManager.Noise spinLoopNoise;
        protected NoiseManager.Noise spinLoopNoise2;
        protected ParticleSystem altFireEffect;
        protected Text ammoDisplay;
        protected ParticleSystem chargeEffect;
        protected ParticleSystem fireEffect;
        protected ParticleSystem preFireEffect;
        protected ParticleSystem overheatEffect;
        protected Animator spinAnimator;

        protected Transform[] bulletSpawns;
        public Handle gunGrip;
        protected Handle foreGrip;
        protected Handle scopeGrip;
        protected Handle secondaryGrip;

        bool hasRefillPort;

        internal Renderer scope;
        Camera scopeCamera;
        RenderTexture renderScopeTexture;

        public bool holdingGunGripLeft;
        public bool holdingGunGripRight;
        public bool holdingForeGripLeft;
        public bool holdingForeGripRight;
        public bool holdingScopeGripLeft;
        public bool holdingScopeGripRight;
        public bool holdingSecondaryGripLeft;
        public bool holdingSecondaryGripRight;

        ProjectileData projectileData;
        ItemData projectileItemData;
        ProjectileData projectileAltData;
        ItemData projectileAltItemData;
        ItemModuleBlasterBolt boltModule;
        ItemModuleBlasterBolt boltAltModule;
        DamagerData boltDamager;
        DamagerData boltAltDamager;
        DamagerData boltChargeDamager;
        public int ammoLeft = -1;
        int shotsLeftInBurst;
        public float currentHeat;
        public float currentInstability;
        public int currentFiremode;
        int currentFiremodeIndex;
        public float currentFirerate;
        int currentFirerateIndex;
        int currentScopeZoom;
        float altFireTime;
        float chargeTime;
        float fireDelayTime;
        float fireTime;
        float reloadTime;
        float spinTime;
        bool isChargedFire;
        bool isDelayingFire;
        bool isOverheated;
        bool isReloading;
        bool isSpinning;
        bool altFireEnabled;
        Transform chargeEffectTrans;
        Vector3 originalChargeEffectScale;
        SpellTelekinesis telekinesis;

        float gunGripControlPrimaryHoldTime;
        float gunGripControlSecondaryHoldTime;
        float foreGripControlPrimaryHoldTime;
        float foreGripControlSecondaryHoldTime;
        float scopeGripControlPrimaryHoldTime;
        float scopeGripControlSecondaryHoldTime;
        float secondaryGripControlPrimaryHoldTime;
        float secondaryGripControlSecondaryHoldTime;

        // AI settings
        Creature currentAI;
        BrainData currentAIBrain;
        ItemModuleAI.WeaponHandling aiOriginalWeaponHandling;

        internal ItemModuleAI moduleAI;

        MaterialInstance _scopeMaterialInstance;
        public MaterialInstance scopeMaterialInstance {
            get {
                if (_scopeMaterialInstance == null) {
                    scope.gameObject.TryGetOrAddComponent(out MaterialInstance mi);
                    _scopeMaterialInstance = mi;
                }
                return _scopeMaterialInstance;
            }
        }

        protected void Awake() {
            all.Add(this);
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBlaster>();
            body = GetComponent<Rigidbody>();

            moduleAI = item.data.GetModule<ItemModuleAI>();

            // setup custom references
            if (!string.IsNullOrEmpty(module.altFireSoundID)) altFireSound = item.GetCustomReference(module.altFireSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.altFireSoundID2)) altFireSound2 = item.GetCustomReference(module.altFireSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeFireSoundID)) chargeFireSound = item.GetCustomReference(module.chargeFireSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeFireSoundID2)) chargeFireSound2 = item.GetCustomReference(module.chargeFireSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeFireSoundID3)) chargeFireSound3 = item.GetCustomReference(module.chargeFireSoundID3).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeSoundID)) chargeSound = item.GetCustomReference(module.chargeSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeSoundID2)) chargeSound2 = item.GetCustomReference(module.chargeSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeReadySoundID)) chargeReadySound = item.GetCustomReference(module.chargeReadySoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeReadySoundID2)) chargeReadySound2 = item.GetCustomReference(module.chargeReadySoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeStartSoundID)) chargeStartSound = item.GetCustomReference(module.chargeStartSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeStartSoundID2)) chargeStartSound2 = item.GetCustomReference(module.chargeStartSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.emptySoundID)) emptySound = item.GetCustomReference(module.emptySoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.emptySoundID2)) emptySound2 = item.GetCustomReference(module.emptySoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireSoundID)) fireSound = item.GetCustomReference(module.fireSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireSoundID2)) fireSound2 = item.GetCustomReference(module.fireSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireSoundID3)) fireSound3 = item.GetCustomReference(module.fireSoundID3).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireModeSoundID)) fireModeSound = item.GetCustomReference(module.fireModeSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireModeSoundID2)) fireModeSound2 = item.GetCustomReference(module.fireModeSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.overheatSoundID)) overheatSound = item.GetCustomReference(module.overheatSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.overheatSoundID2)) overheatSound2 = item.GetCustomReference(module.overheatSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.preFireSoundID)) preFireSound = item.GetCustomReference(module.preFireSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.preFireSoundID2)) preFireSound2 = item.GetCustomReference(module.preFireSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadSoundID)) reloadSound = item.GetCustomReference(module.reloadSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadSoundID2)) reloadSound2 = item.GetCustomReference(module.reloadSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadEndSoundID)) reloadEndSound = item.GetCustomReference(module.reloadEndSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadEndSoundID2)) reloadEndSound2 = item.GetCustomReference(module.reloadEndSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.spinStartSoundID)) spinStartSound = item.GetCustomReference(module.spinStartSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.spinStartSoundID2)) spinStartSound2 = item.GetCustomReference(module.spinStartSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.spinLoopSoundID)) spinLoopSound = item.GetCustomReference(module.spinLoopSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.spinLoopSoundID2)) spinLoopSound2 = item.GetCustomReference(module.spinLoopSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.spinStopSoundID)) spinStopSound = item.GetCustomReference(module.spinStopSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.spinStopSoundID2)) spinStopSound2 = item.GetCustomReference(module.spinStopSoundID2).GetComponent<AudioSource>();

            if (!string.IsNullOrEmpty(module.altFireEffectID)) altFireEffect = item.GetCustomReference(module.altFireEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.ammoDisplayID)) ammoDisplay = item.GetCustomReference(module.ammoDisplayID).GetComponent<Text>();
            if (!string.IsNullOrEmpty(module.chargeEffectID)) chargeEffect = item.GetCustomReference(module.chargeEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.fireEffectID)) fireEffect = item.GetCustomReference(module.fireEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.preFireEffectID)) preFireEffect = item.GetCustomReference(module.preFireEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.overheatEffectID)) overheatEffect = item.GetCustomReference(module.overheatEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.spinAnimatorID)) spinAnimator = item.GetCustomReference(module.spinAnimatorID).GetComponent<Animator>();

            if (!string.IsNullOrEmpty(module.gunGripID)) gunGrip = item.GetCustomReference(module.gunGripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.foreGripID)) foreGrip = item.GetCustomReference(module.foreGripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.scopeGripID)) scopeGrip = item.GetCustomReference(module.scopeGripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.secondaryGripID)) secondaryGrip = item.GetCustomReference(module.secondaryGripID).GetComponent<Handle>();
            if (module.bulletSpawnIDs != null) {
                bulletSpawns = module.bulletSpawnIDs.Select(id => item.GetCustomReference(id)).ToArray();
            }

            var moduleAIFireable = GetComponent<AIFireable>();
            if (moduleAIFireable) {
                moduleAIFireable.fireEvent.AddListener(OnAIFire);
                moduleAIFireable.reloadEvent.AddListener(OnAITryReload);
            }

            // setup item events
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnHeldActionEvent += OnHeldAction;
            item.OnSnapEvent += OnSnapEvent;
            item.OnTelekinesisGrabEvent += OnTelekinesisGrabEvent;
            item.OnTelekinesisReleaseEvent += OnTelekinesisReleaseEvent;

            // setup grip events
            if (gunGrip) {
                gunGrip.Grabbed += OnGripGrabbed;
                gunGrip.UnGrabbed += OnGripUnGrabbed;
            }
            if (foreGrip) {
                foreGrip.Grabbed += OnForeGripGrabbed;
                foreGrip.UnGrabbed += OnForeGripUnGrabbed;
            }
            if (scopeGrip) {
                scopeGrip.Grabbed += OnScopeGripGrabbed;
                scopeGrip.UnGrabbed += OnScopeGripUnGrabbed;
            }
            if (secondaryGrip) {
                secondaryGrip.Grabbed += OnSecondaryGripGrabbed;
                secondaryGrip.UnGrabbed += OnSecondaryGripUnGrabbed;
            }

            if (chargeEffect) {
                chargeEffectTrans = item.GetCustomReference(module.chargeEffectID);
                originalChargeEffectScale = chargeEffectTrans.localScale;
            }

            string foundProjectile = null;
            item.TryGetCustomData(out ItemBlasterSaveData saveData);
            if (saveData != null) {
                ammoLeft = saveData.ammo;
                altFireEnabled = saveData.altFire;
                currentFiremodeIndex = saveData.firemode;
                currentFirerateIndex = saveData.firerate;
                currentScopeZoom = saveData.scopeZoom;
                foundProjectile = saveData.projectileID;
            } else {
                ammoLeft = module.magazineSize;
                UpdateAmmoDisplay();
            }

            if (!string.IsNullOrEmpty(module.projectileID)) {
                projectileData = Catalog.GetData<ProjectileData>(!string.IsNullOrEmpty(foundProjectile) ? foundProjectile : module.projectileID, true);
                projectileItemData = Catalog.GetData<ItemData>(projectileData.item);
                if (projectileItemData != null) boltModule = projectileItemData.GetModule<ItemModuleBlasterBolt>();
            }

            if (!string.IsNullOrEmpty(module.altFireProjectileID)) {
                projectileAltData = Catalog.GetData<ProjectileData>(module.altFireProjectileID, true);
                projectileAltItemData = Catalog.GetData<ItemData>(projectileAltData.item, true);
                if (projectileAltItemData != null) boltAltModule = projectileAltItemData.GetModule<ItemModuleBlasterBolt>();
            }

            if (!string.IsNullOrEmpty(module.overrideBoltDamager)) boltDamager = Catalog.GetData<DamagerData>(module.overrideBoltDamager, true);
            if (!string.IsNullOrEmpty(module.overrideBoltAltDamager)) boltAltDamager = Catalog.GetData<DamagerData>(module.overrideBoltAltDamager, true);
            if (!string.IsNullOrEmpty(module.overrideBoltChargeDamager)) boltChargeDamager = Catalog.GetData<DamagerData>(module.overrideBoltChargeDamager, true);

            if (module.hasScope) SetupScope();
            hasRefillPort = transform.Find("CollisionBlasterRefill");

            currentFiremode = module.fireModes[currentFiremodeIndex];
            currentFirerate = module.gunRPM[currentFirerateIndex];
            currentInstability = module.handlingBaseAccuracy;
            aiOriginalWeaponHandling = moduleAI.weaponHandling;
            moduleAI.weaponHandling = ItemModuleAI.WeaponHandling.OneHanded;

            UpdateFireEffectColour();
        }

        protected void OnDestroy() {
            if (scopeCamera?.targetTexture) {
                scopeCamera.targetTexture.Release();
                scopeCamera.targetTexture = null;
            }
            if (renderScopeTexture) {
                if (renderScopeTexture.IsCreated()) renderScopeTexture.Release();
                Destroy(renderScopeTexture);
                renderScopeTexture = null;
            }

            if (all.Contains(this)) {
                all.Remove(this);
            }
        }

        public void UpdateCustomData() {
            Utils.UpdateCustomData(item, new ItemBlasterSaveData {
                altFire = altFireEnabled,
                ammo = ammoLeft,
                firemode = currentFiremodeIndex,
                firerate = currentFirerateIndex,
                scopeZoom = currentScopeZoom,
                projectileID = projectileData.id
            });
        }

        DamagerData GetActiveBoltDamagerData() {
            if (altFireEnabled) return boltAltDamager;
            if (isChargedFire) return boltChargeDamager;
            return boltDamager;
        }

        ItemModuleBlasterBolt GetActiveBoltModule() {
            return altFireEnabled ? boltAltModule : boltModule;
        }

        ItemData GetActiveProjectile() {
            return altFireEnabled ? projectileAltItemData : projectileItemData;
        }

        ProjectileData GetActiveProjectileData() {
            return altFireEnabled ? projectileAltData : projectileData;
        }

        void UpdateFireEffectColour() {
            if (module.fireEffectUseBoltHue && fireEffect) {
                var activeProjectileData = GetActiveProjectileData();
                if (activeProjectileData == null) return;
                var fireEffectTrans = item.GetCustomReference(module.fireEffectID);
                _UpdateFireEffectColour(fireEffectTrans, activeProjectileData.boltHue);
                _updateRecursively(fireEffectTrans, activeProjectileData.boltHue);

                if (chargeEffectTrans) {
                    _UpdateFireEffectColour(chargeEffectTrans, activeProjectileData.boltHue);
                    _updateRecursively(chargeEffectTrans, activeProjectileData.boltHue);
                }
                UpdateScopeReticleColour();
            }

            void _UpdateFireEffectColour(Transform trans, float hue) {
                var particle = trans.GetComponent<ParticleSystem>();
                if (particle) {
                    var main = particle.main;
                    main.startColor = Utils.UpdateHue(main.startColor.color, hue);
                }
                var light = trans.GetComponent<Light>();
                if (light) {
                    light.color = Utils.UpdateHue(light.color, hue);
                }
            }

            void _updateRecursively(Transform parent, float hue) {
                foreach (Transform child in parent) {
                    _UpdateFireEffectColour(child, hue);
                    _updateRecursively(child, hue);
                }
            }
        }

        public void SetupScope() {
            if (!module.hasScope) return;
            var scopeTransform = item.GetCustomReference(module.scopeID);
            if (scopeTransform) {
                scope = scopeTransform.GetComponent<Renderer>();
                scopeCamera = item.GetCustomReference(module.scopeCameraID).GetComponent<Camera>();
                scopeCamera.enabled = false;
                scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
                renderScopeTexture = new RenderTexture(
                    module.scopeResolution != null ? module.scopeResolution[0] : GlobalSettings.BlasterScopeResolution,
                    module.scopeResolution != null ? module.scopeResolution[1] : GlobalSettings.BlasterScopeResolution,
                    module.scopeDepth, RenderTextureFormat.DefaultHDR);

                scopeMaterialInstance.material.SetTexture("_RenderTexture", renderScopeTexture);
                if (module.scopeReticleTexture) scopeMaterialInstance.material.SetTexture("_Reticle", module.scopeReticleTexture);
                scopeMaterialInstance.material.SetFloat("_ReticleContrast", module.scopeReticleContrast);
                scopeMaterialInstance.material.SetFloat("_EdgeWarp", module.scopeEdgeWarp);

                UpdateScopeReticleColour();
                if (GlobalSettings.BlasterScope3D) scopeMaterialInstance.material.EnableKeyword("_3D_SCOPE"); else scopeMaterialInstance.material.DisableKeyword("_3D_SCOPE");
                if (GlobalSettings.BlasterScopeReticles) scopeMaterialInstance.material.EnableKeyword("_USE_RETICLE"); else scopeMaterialInstance.material.DisableKeyword("_USE_RETICLE");
            }
        }

        public void UpdateScopeReticleColour() {
            if (scope) {
                var activeProjectileData = GetActiveProjectileData();
                if (module.scopeReticleUseBoltHue && activeProjectileData != null) {
                    scopeMaterialInstance.material.SetColor("_ReticleColour", Color.HSVToRGB(activeProjectileData.boltHue, 1, 1));
                } else {
                    scopeMaterialInstance.material.SetColor("_ReticleColour", new Color(module.scopeReticleColour[0], module.scopeReticleColour[1], module.scopeReticleColour[2], 1));
                }
            }
        }

        void EnableScopeRender() {
            if (scope == null) return;
            if (!renderScopeTexture.IsCreated()) renderScopeTexture.Create();
            scopeCamera.targetTexture = renderScopeTexture;
            scopeCamera.enabled = true;
            scopeMaterialInstance.material.EnableKeyword("_SCOPE_ACTIVE");
        }

        void DisableScopeRender() {
            if (scope == null) return;
            scopeCamera.enabled = false;
            scopeCamera.targetTexture = null;
            scopeMaterialInstance.material.DisableKeyword("_SCOPE_ACTIVE");
            renderScopeTexture.Release();
        }

        public void CycleFiremode(RagdollHand interactor = null) {
            if (module.fireModes.Length > 1) {
                currentFiremodeIndex = (currentFiremodeIndex >= module.fireModes.Length - 1) ? -1 : currentFiremodeIndex;
                currentFiremode = module.fireModes[++currentFiremodeIndex];
                Utils.PlaySound(fireModeSound, module.fireModeSoundAsset, item);
                Utils.PlaySound(fireModeSound2, module.fireModeSoundAsset2, item);
                Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
            }
        }

        public void CycleFirerate(RagdollHand interactor = null) {
            if (module.gunRPM.Length > 1) {
                currentFirerateIndex = (currentFirerateIndex >= module.gunRPM.Length - 1) ? -1 : currentFirerateIndex;
                currentFirerate = module.gunRPM[++currentFirerateIndex];
                Utils.PlaySound(fireModeSound, module.fireModeSoundAsset, item);
                Utils.PlaySound(fireModeSound2, module.fireModeSoundAsset2, item);
                Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
            }
        }

        public void CycleScope(RagdollHand interactor = null) {
            if (scope == null || scopeCamera == null) return;
            currentScopeZoom = (currentScopeZoom >= module.scopeZoom.Length - 1) ? -1 : currentScopeZoom;
            scopeCamera.fieldOfView = module.scopeZoom[++currentScopeZoom];
            var scopeSound = scope.GetComponent<AudioSource>();
            if (scopeSound) Utils.PlaySound(scopeSound, null, item);
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
        }

        public void ResetScope(RagdollHand interactor = null) {
            if (scope == null || scopeCamera == null) return;
            currentScopeZoom = 0;
            scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
            var scopeSound = scope.GetComponent<AudioSource>();
            if (scopeSound) Utils.PlaySound(scopeSound, null, item);
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
        }

        public void ToggleAltFire(RagdollHand interactor = null) {
            ChargedFireStop();
            altFireEnabled = !altFireEnabled;
            UpdateFireEffectColour();
            shotsLeftInBurst = 0;
            leftInteractor = null;
            rightInteractor = null;
            overrideInteractor = null;
            isDelayingFire = false;

            Utils.PlaySound(fireModeSound, module.fireModeSoundAsset, item);
            Utils.PlaySound(fireModeSound2, module.fireModeSoundAsset2, item);
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            // toggle scope for performance reasons
            if (module.hasScope && interactor.playerHand) EnableScopeRender();
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            // toggle scope for performance reasons
            if (module.hasScope) DisableScopeRender();
        }

        public void OnSnapEvent(Holder holder) {
            Utils.UpdateCustomData(item, new ItemBlasterSaveData {
                altFire = altFireEnabled,
                ammo = ammoLeft,
                firemode = currentFiremodeIndex,
                firerate = currentFirerateIndex,
                scopeZoom = currentScopeZoom,
                projectileID = projectileData.id
            });
        }

        public void ExecuteAction(string action, RagdollHand interactor = null) {
            if (action == "cycleScope") CycleScope(interactor);
            else if (action == "cycleFiremode") CycleFiremode(interactor);
            else if (action == "cycleFirerate") CycleFirerate(interactor);
            else if (action == "chargedFire") ChargedFireStart(interactor);
            else if (action == "fire") {
                if (interactor.side == Side.Right) {
                    rightInteractor = interactor;
                } else {
                    leftInteractor = interactor;
                }
            } else if (action == "reload") Reload(interactor);
            else if (action == "resetScope") ResetScope(interactor);
            else if (action == "spinBarrel") SpinStart(interactor);
            else if (action == "toggleAltFire") ToggleAltFire(interactor);
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action) {
            if (handle == gunGrip) {
                HandleControl(interactor, action, true, module.gunGripPrimaryAction, module.gunGripPrimaryActionHold, ref gunGripControlPrimaryHoldTime);
                HandleControl(interactor, action, false, module.gunGripSecondaryAction, module.gunGripSecondaryActionHold, ref gunGripControlSecondaryHoldTime);
            } else if (handle == foreGrip) {
                HandleControl(interactor, action, true, module.foreGripPrimaryAction, module.foreGripPrimaryActionHold, ref foreGripControlPrimaryHoldTime);
                HandleControl(interactor, action, false, module.foreGripSecondaryAction, module.foreGripSecondaryActionHold, ref foreGripControlSecondaryHoldTime);
            } else if (handle == scopeGrip) {
                HandleControl(interactor, action, true, module.scopeGripPrimaryAction, module.scopeGripPrimaryActionHold, ref scopeGripControlPrimaryHoldTime);
                HandleControl(interactor, action, false, module.scopeGripSecondaryAction, module.scopeGripSecondaryActionHold, ref scopeGripControlSecondaryHoldTime);
            } else if (handle == secondaryGrip) {
                HandleControl(interactor, action, true, module.secondaryGripPrimaryAction, module.scopeGripPrimaryActionHold, ref secondaryGripControlPrimaryHoldTime);
                HandleControl(interactor, action, false, module.secondaryGripSecondaryAction, module.secondaryGripSecondaryActionHold, ref secondaryGripControlSecondaryHoldTime);
            }
            if (action == Interactable.Action.Ungrab) {
                if (interactor.side == Side.Right) {
                    rightInteractor = null;
                } else {
                    leftInteractor = null;
                }
                overrideInteractor = null;
            }
        }

        public void HandleControl(RagdollHand interactor, Interactable.Action action, bool isPrimary, string controlAction, string controlActionHold, ref float controlHoldTime) {
            var startAction = isPrimary ? Interactable.Action.UseStart : Interactable.Action.AlternateUseStart;
            var stopAction = isPrimary ? Interactable.Action.UseStop : Interactable.Action.AlternateUseStop;

            if (action == stopAction) {
                if (controlAction == "fire") {
                    if (interactor.side == Side.Right) {
                        rightInteractor = null;
                    } else {
                        leftInteractor = null;
                    }
                    overrideInteractor = null;
                }
                if (controlActionHold == "chargedFire") {
                    ChargedFireStop();
                } else if (controlActionHold == "spinBarrel") {
                    SpinStop();
                }
            }

            if (!string.IsNullOrEmpty(controlActionHold)) {
                if (action == startAction) {
                    controlHoldTime = GlobalSettings.ControlsHoldDuration;
                } else if (action == stopAction) {
                    if (controlHoldTime > 0 && controlHoldTime > (controlHoldTime / 2)) {
                        ExecuteAction(controlAction, interactor);
                    }
                    controlHoldTime = 0;
                }
            } else {
                if (action == startAction) ExecuteAction(controlAction, interactor);
            }
        }

        public void OnGripGrabbed(RagdollHand interactor, Handle handle, EventTime eventTime) {
            holdingGunGripRight = interactor.playerHand == Player.local.handRight;
            holdingGunGripLeft = interactor.playerHand == Player.local.handLeft;

            if (eventTime == EventTime.OnEnd && !holdingGunGripLeft && !holdingGunGripRight) {
                currentAI = interactor.creature;
                currentAIBrain = currentAI.brain.instance;
                currentAI.OnKillEvent += NPCDeathFire;
            }
        }

        private void NPCDeathFire(CollisionInstance collisionInstance, EventTime eventTime) {
            if (eventTime == EventTime.OnStart) {
                if (currentAIBrain != null && currentAIBrain.isActive && Random.value <= GlobalSettings.BlasterNPCFireUponDeathChance) {
                    AIFire(true);
                }
                if (currentAI) currentAI.OnKillEvent -= NPCDeathFire;
            }
        }

        public void OnGripUnGrabbed(RagdollHand interactor, Handle handle, EventTime eventTime) {
            if (interactor.playerHand == Player.local.handRight) holdingGunGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingGunGripLeft = false;

            if (eventTime == EventTime.OnStart) {
                if (currentAI) {
                    currentAI.OnKillEvent -= NPCDeathFire;
                    currentAI = null;
                    currentAIBrain = null;
                }
                ChargedFireStop();
                SpinStop();
            }
        }

        public void OnForeGripGrabbed(RagdollHand interactor, Handle handle, EventTime eventTime) {
            holdingForeGripRight = interactor.playerHand == Player.local.handRight;
            holdingForeGripLeft = interactor.playerHand == Player.local.handLeft;
        }

        public void OnForeGripUnGrabbed(RagdollHand interactor, Handle handle, EventTime eventTime) {
            if (interactor.playerHand == Player.local.handRight) holdingForeGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingForeGripLeft = false;
        }

        public void OnScopeGripGrabbed(RagdollHand interactor, Handle handle, EventTime eventTime) {
            holdingScopeGripRight = interactor.playerHand == Player.local.handRight;
            holdingScopeGripLeft = interactor.playerHand == Player.local.handLeft;
        }

        public void OnScopeGripUnGrabbed(RagdollHand interactor, Handle handle, EventTime eventTime) {
            if (interactor.playerHand == Player.local.handRight) holdingScopeGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingScopeGripLeft = false;
        }

        public void OnSecondaryGripGrabbed(RagdollHand interactor, Handle handle, EventTime eventTime) {
            holdingSecondaryGripRight = interactor.playerHand == Player.local.handRight;
            holdingSecondaryGripLeft = interactor.playerHand == Player.local.handLeft;
        }

        public void OnSecondaryGripUnGrabbed(RagdollHand interactor, Handle handle, EventTime eventTime) {
            if (interactor.playerHand == Player.local.handRight) holdingSecondaryGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingSecondaryGripLeft = false;
        }

        public void OnTelekinesisReleaseEvent(Handle handle, SpellTelekinesis teleGrabber, bool tryThrow, bool isGrabbing) {
            telekinesis = null;
        }

        public void OnTelekinesisGrabEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = teleGrabber;
        }

        public int GetAIBurstAmount(bool uncontrolledFire = false) {
            if (module.aiBurstAmounts?.Length > 0 && (!uncontrolledFire || (uncontrolledFire && module.fireModes.Contains(-1))))
                return module.aiBurstAmounts[Random.Range(0, module.aiBurstAmounts.Length)];
            return Mathf.Abs(module.fireModes.Max());
        }

        public void OnAIFire() {
            AIFire();
        }

        public void AIFire(bool uncontrolledFire = false) {
            if (module.fireDelay > 0) {
                fireDelayTime = module.fireDelay;
                isDelayingFire = true;
                Utils.PlaySound(preFireSound, module.preFireSoundAsset, item);
                Utils.PlaySound(preFireSound2, module.preFireSoundAsset2, item);
                Utils.PlayParticleEffect(preFireEffect, module.preFireEffectDetachFromParent);
            } else if (module.spinTime > 0) {
                if (GetSpinSpeed() >= module.spinSpeedMinToFire) {
                    shotsLeftInBurst = GetAIBurstAmount(uncontrolledFire);
                    Fire();
                }
            } else {
                shotsLeftInBurst = GetAIBurstAmount(uncontrolledFire);
                Fire();
            }
        }

        public void OnAITryReload() {
            Reload();
        }

        public void DropBlaster() {
            if (gunGrip) gunGrip.Release();
            if (foreGrip) foreGrip.Release();
            if (scopeGrip) scopeGrip.Release();
            if (secondaryGrip) secondaryGrip.Release();
            leftInteractor = null;
            rightInteractor = null;
            overrideInteractor = null;
        }

        public void Fire() {
            var shooterHand = gunGrip.handlers.FirstOrDefault() ?? gunGrip.telekinesisHandlers.FirstOrDefault()?.ragdollHand;
            var shooter = shooterHand?.creature;

            if (ammoLeft == 0 || isOverheated) {
                Utils.PlaySound(emptySound, module.emptySoundAsset, shooter);
                Utils.PlaySound(emptySound2, module.emptySoundAsset2, shooter);
                leftInteractor = null;
                rightInteractor = null;
                overrideInteractor = null;
                shotsLeftInBurst = 0;
                return;
            }

            // Create and fire bullet
            var activeProjectileData = GetActiveProjectileData();
            var activeProjectileItemData = GetActiveProjectile();
            if (activeProjectileData == null || activeProjectileItemData == null) {
                Utils.LogError("Couldn't retrieve ProjectileData/ItemData for " + item.name);
                return;
            }
            var activeDamager = GetActiveProjectileData().damager ?? GetActiveBoltDamagerData();

            if (module.spinTime > 0) {
                currentFirerate = module.gunRPM[currentFirerateIndex] * GetSpinSpeed();
            }

            Transform[] spawns = module.multishot || (isChargedFire && module.chargeMultishot) ? bulletSpawns : new Transform[] { bulletSpawns[0] };
            List<Item> projectileClones = spawns.Length > 1 ? new List<Item>() : null;
            bool usePooled = GlobalSettings.BlasterNPCUsePooledBolts || holdingGunGripLeft || holdingGunGripRight;
            foreach (var bulletSpawn in spawns) {
                activeProjectileItemData.SpawnAsync(projectile => {
                    var boltData = projectile.gameObject.GetComponent<ItemBlasterBolt>();
                    if (boltData != null) {
                        boltData.UpdateValues(ref activeProjectileData);
                    }
                    var ignoreHandler = projectile.gameObject.GetComponent<CollisionIgnoreHandler>();
                    if (!ignoreHandler) ignoreHandler = projectile.gameObject.AddComponent<CollisionIgnoreHandler>();
                    if (!projectile.gameObject.activeInHierarchy) projectile.gameObject.SetActive(true);
                    ignoreHandler.item = projectile;
                    ignoreHandler.IgnoreCollision(item);
                    if (projectileClones != null) {
                        foreach (var toIgnore in projectileClones) {
                            ignoreHandler.IgnoreCollision(toIgnore);
                        }
                        projectileClones.Add(projectile);
                    }
                    try {
                        foreach (var pt in projectile.parryTargets) {
                            pt.owner = shooter;
                            if (!ParryTarget.list.Contains(pt)) ParryTarget.list.Add(pt);
                        }
                        projectile.lastHandler = shooterHand;
                    }
                    catch { }

                    foreach (CollisionHandler collisionHandler in projectile.collisionHandlers) {
                        collisionHandler.SetPhysicModifier(this, gravityMultiplier: activeProjectileData.useGravity ? 1 : 0, massMultiplier: 1f, drag: activeProjectileData.drag);

                        if (activeDamager != null) {
                            foreach (Damager damager in collisionHandler.damagers) {
                                damager.data = activeDamager;
                            }
                        }
                    }

                    // match new projectile inertia with current gun motion inertia
                    var projTransform = projectile.transform;
                    projTransform.position = bulletSpawn.position;
                    projTransform.rotation = Quaternion.Euler(CalculateInaccuracy(bulletSpawn.rotation.eulerAngles));
                    var projectileBody = projectile.GetComponent<Rigidbody>();
                    projectileBody.mass /= GlobalSettings.BlasterBoltSpeed * (currentAI ? GlobalSettings.BlasterBoltSpeedNPC : 1f);
                    projectile.Throw(1f, Item.FlyDetection.Forced);
                    projectileBody.AddForce(projectileBody.transform.forward * module.bulletForce);
                    boltData.trail?.Clear();
                }, pooled: usePooled);
            }

            // Apply haptic feedback
            Utils.PlayHaptic(holdingGunGripLeft, holdingGunGripRight, module.fireHaptic);

            // Apply recoil
            if (module.recoil) ApplyRecoil();

            if (module.magazineSize > 0) ammoLeft--;
            if (module.overheatRate > 0) currentHeat += module.overheatRate * GlobalSettings.BlasterOverheatRate;
            if (module.handlingInstabilityRate > 0 && currentInstability < module.handlingInstabilityMax) {
                var handlers = item.handlers.Count > 0 ? item.handlers.Count : 1;
                var handlerRate = Mathf.Clamp(module.handlingInstabilityRate / (handlers * 0.8f), 1, float.MaxValue);
                currentInstability = Mathf.Clamp(currentInstability + handlerRate, module.handlingBaseAccuracy, module.handlingInstabilityMax);
            }
            if (module.handlingInstabilityThreshold > 0 && module.handlingInstabilityThreshold < currentInstability && currentAI == null) DropBlaster();

            if (--shotsLeftInBurst == 0) {
                leftInteractor = null;
                rightInteractor = null;
                overrideInteractor = null;
            }

            var fireModifier = (module.burstRPM > 0 && shotsLeftInBurst > 0) ? module.burstRPM : currentFirerate;
            fireTime = 1 / (fireModifier / 60) / (altFireEnabled ? module.altFireRateMultiplier : 1);

            UpdateAmmoDisplay();
            Utils.PlayParticleEffect(fireEffect, module.fireEffectDetachFromParent);

            if (altFireEnabled) {
                Utils.PlaySoundOneShot(altFireSound, module.altFireSoundAsset, shooter);
                Utils.PlaySoundOneShot(altFireSound2, module.altFireSoundAsset2, shooter);
            } else if (isChargedFire) {
                Utils.PlaySoundOneShot(chargeFireSound, module.chargeFireSoundAsset, shooter);
                Utils.PlaySoundOneShot(chargeFireSound2, module.chargeFireSoundAsset2, shooter);
                Utils.PlaySoundOneShot(chargeFireSound3, module.chargeFireSoundAsset3, shooter);
            } else {
                Utils.PlaySoundOneShot(fireSound, module.fireSoundAsset, shooter);
                Utils.PlaySoundOneShot(fireSound2, module.fireSoundAsset2, shooter);
                Utils.PlaySoundOneShot(fireSound3, module.fireSoundAsset3, shooter);
            }

            if (isChargedFire) {
                shotsLeftInBurst = 0;
                leftInteractor = null;
                rightInteractor = null;
                overrideInteractor = null;
                isChargedFire = false;
            }
        }

        void ChargedFireStart(RagdollHand interactor = null) {
            if (ammoLeft == 0 || isOverheated) {
                Utils.PlaySound(emptySound, module.emptySoundAsset, item);
                Utils.PlaySound(emptySound2, module.emptySoundAsset2, item);
                leftInteractor = null;
                rightInteractor = null;
                overrideInteractor = null;
                shotsLeftInBurst = 0;
                return;
            }

            if (altFireEnabled) {
                if (interactor.side == Side.Right) {
                    rightInteractor = interactor;
                } else {
                    leftInteractor = interactor;
                }
                return;
            }

            chargeTime = module.chargeTime;
            Utils.PlayParticleEffect(chargeEffect);
            Utils.PlaySound(chargeSound, module.chargeSoundAsset, item);
            Utils.PlaySound(chargeSound2, module.chargeSoundAsset2, item);
            Utils.PlaySound(chargeStartSound, module.chargeStartSoundAsset, item);
            Utils.PlaySound(chargeStartSound2, module.chargeStartSoundAsset2, item);
        }

        void ChargedFireStop() {
            if (chargeEffect) chargeEffect.Stop();
            if (chargeSound) chargeSound.Stop();
            if (chargeSound2) chargeSound2.Stop();
            if (chargeReadySound) chargeReadySound.Stop();
            if (chargeReadySound2) chargeReadySound2.Stop();
            if (chargeStartSound) chargeStartSound.Stop();
            if (chargeStartSound2) chargeStartSound2.Stop();
            if (chargeTime < 0) {
                isChargedFire = true;
                Fire();
            }
            chargeTime = 0;
        }

        void SpinStart(RagdollHand interactor = null) {
            isSpinning = true;
            Utils.PlaySound(spinStartSound, module.spinStartSoundAsset, item);
            Utils.PlaySound(spinStartSound2, module.spinStartSoundAsset2, item);
        }

        void SpinStop() {
            if (isSpinning) {
                Utils.PlaySound(spinStopSound, module.spinStopSoundAsset, item);
                Utils.PlaySound(spinStopSound2, module.spinStopSoundAsset2, item);
            }
            isSpinning = false;
        }

        float GetSpinSpeed() {
            return Mathf.SmoothStep(0, module.spinSpeedMax, spinTime / module.spinTime);
        }

        void ApplyRecoil() {
            // Add angular + positional recoil to the gun
            if (module.recoilAngle != null) {
                body.AddRelativeTorque(new Vector3(
                    Random.Range(module.recoilAngle[0], module.recoilAngle[1]),
                    Random.Range(module.recoilAngle[2], module.recoilAngle[3]),
                    Random.Range(module.recoilAngle[4], module.recoilAngle[5])
                    ), ForceMode.VelocityChange);
            }
            if (module.recoilForce != null) {
                Vector3 recoilForce = new Vector3(
                    Random.Range(module.recoilForce[0], module.recoilForce[1]),
                    Random.Range(module.recoilForce[2], module.recoilForce[3]),
                    Random.Range(module.recoilForce[4], module.recoilForce[5])
                    );
                body.AddRelativeForce(recoilForce);
            }
        }

        public void Reload(RagdollHand interactor = null) {
            if (!isReloading && !(GlobalSettings.BlasterRequireRefill && hasRefillPort && currentAI == null)) {
                isReloading = true;
                ammoLeft = 0;
                shotsLeftInBurst = 0;
                leftInteractor = null;
                rightInteractor = null;
                overrideInteractor = null;
                reloadTime = module.reloadTime;
                Utils.PlaySound(reloadSound, module.reloadSoundAsset, item);
                Utils.PlaySound(reloadSound2, module.reloadSoundAsset2, item);
                Utils.PlayHaptic(interactor, Utils.HapticIntensity.Moderate);
            }
        }

        void ReloadComplete() {
            isReloading = false;
            ammoLeft = module.magazineSize;
            shotsLeftInBurst = 0;
            leftInteractor = null;
            rightInteractor = null;
            overrideInteractor = null;
            UpdateAmmoDisplay();
            Utils.PlaySound(reloadEndSound, module.reloadEndSoundAsset, item);
            Utils.PlaySound(reloadEndSound2, module.reloadEndSoundAsset2, item);
            Utils.PlayHaptic(holdingGunGripLeft, holdingGunGripRight, Utils.HapticIntensity.Moderate);
            UpdateCustomData();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Called from BlasterPowerCell")]
        void RechargeFromPowerCell(string newProjectile) {
            if (!string.IsNullOrEmpty(newProjectile)) {
                projectileData = Catalog.GetData<ProjectileData>(newProjectile, true);
                if (projectileData != null) projectileItemData = Catalog.GetData<ItemData>(projectileData.item, true);
                if (projectileItemData != null) boltModule = projectileItemData.GetModule<ItemModuleBlasterBolt>();
                ChargedFireStop();
                UpdateFireEffectColour();
                ReloadComplete();
                UpdateCustomData();
            }
        }

        void UpdateAmmoDisplay() {
            if (ammoDisplay) {
                var digits = module.magazineSize == 0 ? 1 : 1 + (int)System.Math.Log10(System.Math.Abs(module.magazineSize));
                ammoDisplay.text = ammoLeft.ToString("D" + digits);
            }
        }

        Vector3 CalculateInaccuracy(Vector3 initial, bool addCurrent = true) {
            var instability = currentInstability;
            if (currentAI) instability /= Mathf.Clamp(GlobalSettings.BlasterNPCAccuracy, 0.01f, float.MaxValue);
            var baseInaccuracy = addCurrent ? new Vector3(
                        initial.x + Random.Range(-instability, instability),
                        initial.y + Random.Range(-instability, instability),
                        initial.z) : initial;
            return baseInaccuracy;
        }

        void UpdateHoldTime(string action, ref float holdTime, bool holdingLeft, bool holdingRight) {
            if (holdTime > 0) {
                holdTime -= Time.deltaTime;
                if (holdTime <= 0) {
                    RagdollHand interactor = null;
                    if (holdingLeft) interactor = Player.local.handLeft.ragdollHand;
                    if (holdingRight) interactor = Player.local.handRight.ragdollHand;
                    ExecuteAction(action, interactor);
                }
            }
        }

        protected override void ManagedLateUpdate() {
            // update timers
            if (altFireTime > 0) altFireTime -= Time.deltaTime;
            if (fireTime > 0) fireTime -= Time.deltaTime;
            if (fireDelayTime > 0) fireDelayTime -= Time.deltaTime;
            if (reloadTime > 0) reloadTime -= Time.deltaTime;
            if (currentHeat > 0) currentHeat -= Time.deltaTime * GlobalSettings.BlasterCoolingRate;
            if (currentInstability > module.handlingBaseAccuracy) {
                currentInstability -= Time.deltaTime * (module.handlingStabilityMultiplier * module.handlingInstabilityRate);
                if (item.handlers.Count == 0) currentInstability = module.handlingBaseAccuracy;
            }

            UpdateHoldTime(module.gunGripPrimaryActionHold, ref gunGripControlPrimaryHoldTime, holdingGunGripLeft, holdingGunGripRight);
            UpdateHoldTime(module.gunGripSecondaryActionHold, ref gunGripControlSecondaryHoldTime, holdingGunGripLeft, holdingGunGripRight);
            UpdateHoldTime(module.foreGripPrimaryActionHold, ref foreGripControlPrimaryHoldTime, holdingForeGripLeft, holdingForeGripRight);
            UpdateHoldTime(module.foreGripSecondaryActionHold, ref foreGripControlSecondaryHoldTime, holdingForeGripLeft, holdingForeGripRight);
            UpdateHoldTime(module.scopeGripPrimaryActionHold, ref scopeGripControlPrimaryHoldTime, holdingScopeGripLeft, holdingScopeGripRight);
            UpdateHoldTime(module.scopeGripSecondaryActionHold, ref scopeGripControlSecondaryHoldTime, holdingScopeGripLeft, holdingScopeGripRight);
            UpdateHoldTime(module.secondaryGripPrimaryActionHold, ref secondaryGripControlPrimaryHoldTime, holdingSecondaryGripLeft, holdingSecondaryGripRight);
            UpdateHoldTime(module.secondaryGripSecondaryActionHold, ref secondaryGripControlSecondaryHoldTime, holdingSecondaryGripLeft, holdingSecondaryGripRight);

            if (currentHeat > module.overheatThreshold && !isOverheated) {
                isOverheated = true;
                Utils.PlaySound(overheatSound, module.overheatSoundAsset, item);
                Utils.PlaySound(overheatSound2, module.overheatSoundAsset2, item);
                Utils.PlayParticleEffect(overheatEffect);
            } else if (currentHeat <= 0 && isOverheated) {
                isOverheated = false;
                overheatEffect.Stop();
            }

            // handle fire update
            if (fireTime <= 0 && !isReloading) {
                if (shotsLeftInBurst > 0) {
                    Fire();
                } else if (isDelayingFire && fireDelayTime <= 0) {
                    isDelayingFire = false;
                    shotsLeftInBurst = currentFiremode;
                    Fire();
                } else if (!isDelayingFire) {
                    if ((rightInteractor && holdingGunGripRight) || (leftInteractor && holdingGunGripLeft) || (telekinesis != null && telekinesis.spinMode) || overrideInteractor) {
                        if (module.fireDelay > 0) {
                            fireDelayTime = module.fireDelay;
                            isDelayingFire = true;
                            Utils.PlaySound(preFireSound, module.preFireSoundAsset, item);
                            Utils.PlaySound(preFireSound2, module.preFireSoundAsset2, item);
                            Utils.PlayParticleEffect(preFireEffect, module.preFireEffectDetachFromParent);
                        } else if (module.spinTime > 0) {
                            if (GetSpinSpeed() >= module.spinSpeedMinToFire) {
                                shotsLeftInBurst = currentFiremode;
                                Fire();
                            }
                        } else {
                            shotsLeftInBurst = currentFiremode;
                            Fire();
                        }
                    }
                }
            }
            if (telekinesis != null) telekinesis.SetSpinMode(false);

            // handle reload update if needed
            if (reloadTime <= 0 && module.magazineSize > 0) {
                // finish reload
                if (isReloading) {
                    ReloadComplete();
                }
                // start reloading if auto and out of ammo
                else if (ammoLeft == 0 && (module.automaticReload || GlobalSettings.BlasterAutomaticReload || currentAI) && !isReloading) Reload();
            }

            if (chargeTime > 0) {
                chargeTime -= Time.deltaTime;
                var adjustedScale = Mathf.SmoothStep(originalChargeEffectScale.z, originalChargeEffectScale.z * 0.1f, chargeTime / module.chargeTime);
                var adjustedVolume = Mathf.SmoothStep(1, 0.2f, chargeTime / module.chargeTime);
                chargeEffectTrans.localScale = new Vector3(adjustedScale, adjustedScale, adjustedScale);
                if (chargeSound) chargeSound.volume = adjustedVolume;
                if (chargeSound2) chargeSound2.volume = adjustedVolume;
                if (chargeTime <= 0) {
                    if (chargeReadySound && !chargeReadySound.isPlaying) Utils.PlaySound(chargeReadySound, module.chargeReadySoundAsset, item);
                    if (chargeReadySound2 && !chargeReadySound2.isPlaying) Utils.PlaySound(chargeReadySound2, module.chargeReadySoundAsset2, item);
                    Utils.PlayHaptic(holdingGunGripLeft || holdingForeGripLeft, holdingGunGripRight || holdingForeGripRight, module.fireHaptic);
                }
            }

            // handle spinning barrels
            if (module.spinTime > 0) {
                if (!isSpinning && spinTime > 0) {
                    spinTime -= Time.deltaTime;
                    if (spinTime <= 0) {
                        if (spinLoopSound) Utils.StopSoundLoop(spinLoopSound, ref spinLoopNoise);
                        if (spinLoopSound2) Utils.StopSoundLoop(spinLoopSound2, ref spinLoopNoise2);
                    }
                }
                if (isSpinning && spinTime < module.spinTime) {
                    spinTime = Mathf.Clamp(spinTime + Time.deltaTime, 0, module.spinTime);
                }

                if (isSpinning) {
                    if (spinLoopSound && !spinLoopSound.isPlaying) spinLoopNoise = Utils.PlaySoundLoop(spinLoopSound, module.spinLoopSoundAsset, item);
                    if (spinLoopSound2 && !spinLoopSound2.isPlaying) spinLoopNoise2 = Utils.PlaySoundLoop(spinLoopSound2, module.spinLoopSoundAsset2, item);
                    Utils.PlayHaptic(holdingGunGripLeft || holdingForeGripLeft, holdingGunGripRight || holdingForeGripRight, Utils.HapticIntensity.Minor);
                }

                var spinSpeed = GetSpinSpeed();
                var spinVolume = Mathf.SmoothStep(0, module.spinSpeedMax, spinSpeed);
                if (spinLoopSound) spinLoopSound.volume = spinVolume;
                if (spinLoopSound2) spinLoopSound2.volume = spinVolume;

                if (spinAnimator) {
                    spinAnimator.SetFloat("speed", spinSpeed);
                }
            }

            // Run AI logic
            if (currentAI && currentAIBrain != null && module.spinTime > 0) {
                if (currentAI.brain.state == Brain.State.Combat || currentAI.brain.state == Brain.State.Alert || currentAI.brain.state == Brain.State.Grappled) {
                    if (!isSpinning) SpinStart();
                } else SpinStop();
            }
        }
    }
}