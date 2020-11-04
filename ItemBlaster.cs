using ThunderRoad;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TOR {
    public class ItemBlaster : MonoBehaviour {
        protected Item item;
        protected ItemModuleBlaster module;

        protected Rigidbody body;
        protected Interactor rightInteractor;
        protected Interactor leftInteractor;
        public Interactor overrideInteractor;
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
        protected ParticleSystem altFireEffect;
        protected Text ammoDisplay;
        protected ParticleSystem chargeEffect;
        protected ParticleSystem fireEffect;
        protected ParticleSystem preFireEffect;
        protected ParticleSystem overheatEffect;

        protected Transform[] bulletSpawns;
        public Handle gunGrip;
        protected Handle foreGrip;
        protected Handle scopeGrip;
        protected Handle secondaryGrip;

        bool hasRefillPort;

        Renderer scope;
        Camera scopeCamera;
        Material originalScopeMaterial;
        Material scopeMaterial;
        RenderTexture renderScopeTexture;

        public bool holdingGunGripLeft;
        public bool holdingGunGripRight;
        public bool holdingForeGripLeft;
        public bool holdingForeGripRight;
        public bool holdingScopeGripLeft;
        public bool holdingScopeGripRight;
        public bool holdingSecondaryGripLeft;
        public bool holdingSecondaryGripRight;

        ItemPhysic projectileData;
        ItemPhysic projectileAltData;
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
        bool isChargedFire;
        bool isDelayingFire;
        bool isOverheated;
        bool isReloading;
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
        BrainHuman currentAIBrain;
        int aiBurstAmount;
        float aiGrabForegripTime;
        float aiShootTime;
        bool aiOriginalMeleeEnabled;
        float aiOriginalMeleeDistMult;
        float aiOriginalParryDetectionRadius;
        float aiOriginalParryMaxDist;
        float aiElevationSpeed;
        const int aiMask = ~(1 << 0);

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBlaster>();
            body = GetComponent<Rigidbody>();

            // setup custom references
            if (!string.IsNullOrEmpty(module.altFireSoundID)) altFireSound = item.definition.GetCustomReference(module.altFireSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.altFireSoundID2)) altFireSound2 = item.definition.GetCustomReference(module.altFireSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeFireSoundID)) chargeFireSound = item.definition.GetCustomReference(module.chargeFireSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeFireSoundID2)) chargeFireSound2 = item.definition.GetCustomReference(module.chargeFireSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeFireSoundID3)) chargeFireSound3 = item.definition.GetCustomReference(module.chargeFireSoundID3).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeSoundID)) chargeSound = item.definition.GetCustomReference(module.chargeSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeSoundID2)) chargeSound2 = item.definition.GetCustomReference(module.chargeSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeReadySoundID)) chargeReadySound = item.definition.GetCustomReference(module.chargeReadySoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeReadySoundID2)) chargeReadySound2 = item.definition.GetCustomReference(module.chargeReadySoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeStartSoundID)) chargeStartSound = item.definition.GetCustomReference(module.chargeStartSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.chargeStartSoundID2)) chargeStartSound2 = item.definition.GetCustomReference(module.chargeStartSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.emptySoundID)) emptySound = item.definition.GetCustomReference(module.emptySoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.emptySoundID2)) emptySound2 = item.definition.GetCustomReference(module.emptySoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireSoundID)) fireSound = item.definition.GetCustomReference(module.fireSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireSoundID2)) fireSound2 = item.definition.GetCustomReference(module.fireSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireSoundID3)) fireSound3 = item.definition.GetCustomReference(module.fireSoundID3).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireModeSoundID)) fireModeSound = item.definition.GetCustomReference(module.fireModeSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireModeSoundID2)) fireModeSound2 = item.definition.GetCustomReference(module.fireModeSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.overheatSoundID)) overheatSound = item.definition.GetCustomReference(module.overheatSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.overheatSoundID2)) overheatSound2 = item.definition.GetCustomReference(module.overheatSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.preFireSoundID)) preFireSound = item.definition.GetCustomReference(module.preFireSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.preFireSoundID2)) preFireSound2 = item.definition.GetCustomReference(module.preFireSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadSoundID)) reloadSound = item.definition.GetCustomReference(module.reloadSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadSoundID2)) reloadSound2 = item.definition.GetCustomReference(module.reloadSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadEndSoundID)) reloadEndSound = item.definition.GetCustomReference(module.reloadEndSoundID).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadEndSoundID2)) reloadEndSound2 = item.definition.GetCustomReference(module.reloadEndSoundID2).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(module.altFireEffectID)) altFireEffect = item.definition.GetCustomReference(module.altFireEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.ammoDisplayID)) ammoDisplay = item.definition.GetCustomReference(module.ammoDisplayID).GetComponent<Text>();
            if (!string.IsNullOrEmpty(module.chargeEffectID)) chargeEffect = item.definition.GetCustomReference(module.chargeEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.fireEffectID)) fireEffect = item.definition.GetCustomReference(module.fireEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.preFireEffectID)) preFireEffect = item.definition.GetCustomReference(module.preFireEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.overheatEffectID)) overheatEffect = item.definition.GetCustomReference(module.overheatEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.gunGripID)) gunGrip = item.definition.GetCustomReference(module.gunGripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.foreGripID)) foreGrip = item.definition.GetCustomReference(module.foreGripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.scopeGripID)) scopeGrip = item.definition.GetCustomReference(module.scopeGripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.secondaryGripID)) secondaryGrip = item.definition.GetCustomReference(module.secondaryGripID).GetComponent<Handle>();
            if (module.bulletSpawnIDs != null) {
                bulletSpawns = module.bulletSpawnIDs.Select(id => item.definition.GetCustomReference(id)).ToArray();
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
                chargeEffectTrans = item.definition.GetCustomReference(module.chargeEffectID);
                originalChargeEffectScale = chargeEffectTrans.localScale;
            }

            item.definition.TryGetSavedValue("ammo", out string foundAmmo);
            if (!int.TryParse(foundAmmo, out ammoLeft)) {
                ammoLeft = module.magazineSize;
                UpdateAmmoDisplay();
            }

            item.definition.TryGetSavedValue("altFire", out string foundAltFire);
            bool.TryParse(foundAltFire, out altFireEnabled);
            item.definition.TryGetSavedValue("firerate", out string foundFirerate);
            int.TryParse(foundFirerate, out currentFirerateIndex);
            item.definition.TryGetSavedValue("firemode", out string foundFiremode);
            int.TryParse(foundFiremode, out currentFiremodeIndex);
            item.definition.TryGetSavedValue("scopeZoom", out string foundScopeZoom);
            int.TryParse(foundScopeZoom, out currentScopeZoom);
            item.definition.TryGetSavedValue("projectileID", out string foundProjectile);

            if (!string.IsNullOrEmpty(module.projectileID)) projectileData = Catalog.GetData<ItemPhysic>(!string.IsNullOrEmpty(foundProjectile) ? foundProjectile : module.projectileID, true);
            if (projectileData != null) boltModule = projectileData.GetModule<ItemModuleBlasterBolt>();

            if (!string.IsNullOrEmpty(module.altFireProjectileID)) projectileAltData = Catalog.GetData<ItemPhysic>(module.altFireProjectileID, true);
            if (projectileAltData != null) boltAltModule = projectileAltData.GetModule<ItemModuleBlasterBolt>();

            if (!string.IsNullOrEmpty(module.overrideBoltDamager)) boltDamager = Catalog.GetData<DamagerData>(module.overrideBoltDamager, true);
            if (!string.IsNullOrEmpty(module.overrideBoltAltDamager)) boltAltDamager = Catalog.GetData<DamagerData>(module.overrideBoltAltDamager, true);
            if (!string.IsNullOrEmpty(module.overrideBoltChargeDamager)) boltChargeDamager = Catalog.GetData<DamagerData>(module.overrideBoltChargeDamager, true);

            if (module.hasScope) SetupScope();
            hasRefillPort = transform.Find("CollisionBlasterRefill");

            currentFiremode = module.fireModes[currentFiremodeIndex];
            currentFirerate = module.gunRPM[currentFirerateIndex];
            currentInstability = module.handlingBaseAccuracy;
            aiBurstAmount = Mathf.Abs(module.fireModes.Max());
            aiElevationSpeed = 10 / Mathf.Sqrt(item.data.mass);

            UpdateFireEffectColour();
        }

        DamagerData GetActiveBoltDamagerData() {
            if (altFireEnabled) return boltAltDamager;
            if (isChargedFire) return boltChargeDamager;
            return boltDamager;
        }

        ItemModuleBlasterBolt GetActiveBoltModule() {
            return altFireEnabled ? boltAltModule : boltModule;
        }

        ItemPhysic GetActiveProjectile() {
            return altFireEnabled ? projectileAltData : projectileData;
        }
        
        void UpdateFireEffectColour() {
            if (module.fireEffectUseBoltHue && fireEffect) {
                var activeBoltModule = GetActiveBoltModule();
                if (activeBoltModule == null) return;
                var fireEffectTrans = item.definition.GetCustomReference(module.fireEffectID);
                _UpdateFireEffectColour(fireEffectTrans, activeBoltModule.boltHue);
                _updateRecursively(fireEffectTrans, activeBoltModule.boltHue);

                if (chargeEffectTrans) {
                    _UpdateFireEffectColour(chargeEffectTrans, activeBoltModule.boltHue);
                    _updateRecursively(chargeEffectTrans, activeBoltModule.boltHue);
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

        void SetupScope() {
            if (module.hasScope == false) return;
            var scopeTransform = item.definition.GetCustomReference(module.scopeID);
            if (scopeTransform) {
                scope = scopeTransform.GetComponent<Renderer>();
                originalScopeMaterial = scope.materials[0];
                scopeMaterial = scope.materials[1];
                scope.materials = new Material[] { originalScopeMaterial };

                scopeCamera = item.definition.GetCustomReference(module.scopeCameraID).GetComponent<Camera>();
                scopeCamera.enabled = false;
                scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
                renderScopeTexture = new RenderTexture(
                    module.scopeResolution != null ? module.scopeResolution[0] : TORGlobalSettings.BlasterScopeResolution[0],
                    module.scopeResolution != null ? module.scopeResolution[1] : TORGlobalSettings.BlasterScopeResolution[1],
                    module.scopeDepth, RenderTextureFormat.DefaultHDR);
                renderScopeTexture.Create();
                scopeCamera.targetTexture = renderScopeTexture;

                scopeMaterial.SetTexture("_RenderTexture", renderScopeTexture);
                if (module.scopeReticleTexture != null) scopeMaterial.SetTexture("_Reticle", module.scopeReticleTexture);
                UpdateScopeReticleColour();
                scopeMaterial.SetFloat("_ReticleContrast", module.scopeReticleContrast);
                scopeMaterial.SetFloat("_EdgeWarp", module.scopeEdgeWarp);
            }
        }

        void UpdateScopeReticleColour() {
            if (scopeMaterial) {
                var activeBoltModule = GetActiveBoltModule();
                if (module.scopeReticleUseBoltHue && activeBoltModule != null) {
                    scopeMaterial.SetColor("_ReticleColour", Color.HSVToRGB(activeBoltModule.boltHue, 1, 1));
                } else {
                    scopeMaterial.SetColor("_ReticleColour", new Color(module.scopeReticleColour[0], module.scopeReticleColour[1], module.scopeReticleColour[2], 1));
                }
            }
        }

        void EnableScopeRender() {
            if (scope == null) return;
            scopeCamera.enabled = true;
            scope.material = scopeMaterial;
        }

        void DisableScopeRender() {
            if (scope == null) return;
            scopeCamera.enabled = false;
            scope.material = originalScopeMaterial;
        }

        void CycleFiremode(Interactor interactor = null) {
            if (module.fireModes.Length > 1) {
                currentFiremodeIndex = (currentFiremodeIndex >= module.fireModes.Length - 1) ? -1 : currentFiremodeIndex;
                currentFiremode = module.fireModes[++currentFiremodeIndex];
                Utils.PlaySound(fireModeSound, module.fireModeSoundAsset);
                Utils.PlaySound(fireModeSound2, module.fireModeSoundAsset2);
                if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
            }
        }

        void CycleFirerate(Interactor interactor = null) {
            if (module.gunRPM.Length > 1) {
                currentFirerateIndex = (currentFirerateIndex >= module.gunRPM.Length - 1) ? -1 : currentFirerateIndex;
                currentFirerate = module.gunRPM[++currentFirerateIndex];
                Utils.PlaySound(fireModeSound, module.fireModeSoundAsset);
                Utils.PlaySound(fireModeSound2, module.fireModeSoundAsset2);
                if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
            }
        }

        void CycleScope(Interactor interactor = null) {
            if (scope == null || scopeCamera == null) return;
            currentScopeZoom = (currentScopeZoom >= module.scopeZoom.Length - 1) ? -1 : currentScopeZoom;
            scopeCamera.fieldOfView = module.scopeZoom[++currentScopeZoom];
            var scopeSound = scope.GetComponent<AudioSource>();
            if (scopeSound) scopeSound.Play();
            if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
        }

        void ResetScope(Interactor interactor = null) {
            if (scope == null || scopeCamera == null) return;
            currentScopeZoom = 0;
            scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
            var scopeSound = scope.GetComponent<AudioSource>();
            if (scopeSound) scopeSound.Play();
            if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
        }

        void ToggleAltFire(Interactor interactor = null) {
            ChargedFireStop();
            altFireEnabled = !altFireEnabled;
            UpdateFireEffectColour();
            shotsLeftInBurst = 0;
            leftInteractor = null;
            rightInteractor = null;
            overrideInteractor = null;
            isDelayingFire = false;

            Utils.PlaySound(fireModeSound, module.fireModeSoundAsset);
            Utils.PlaySound(fireModeSound2, module.fireModeSoundAsset2);
            if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            // toggle scope for performance reasons
            if (module.hasScope && interactor.playerHand) EnableScopeRender();
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool throwing) {
            // toggle scope for performance reasons
            if (module.hasScope) DisableScopeRender();
        }
        
        public void OnSnapEvent(ObjectHolder holder) {
            item.definition.SetSavedValue("ammo", ammoLeft.ToString());
            item.definition.SetSavedValue("firemode", currentFiremodeIndex.ToString());
            item.definition.SetSavedValue("firerate", currentFirerateIndex.ToString());
            item.definition.SetSavedValue("altFire", altFireEnabled.ToString());
            if (module.hasScope) item.definition.SetSavedValue("scopeZoom", currentScopeZoom.ToString());
        }

        public void ExecuteAction(string action, Interactor interactor = null) {
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
            else if (action == "toggleAltFire") ToggleAltFire(interactor);
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
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

        public void HandleControl(Interactor interactor, Interactable.Action action, bool isPrimary, string controlAction, string controlActionHold, ref float controlHoldTime) {
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
                }
            }

            if (!string.IsNullOrEmpty(controlActionHold)) {
                if (action == startAction) {
                    controlHoldTime = TORGlobalSettings.ControlsHoldDuration;
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

        public void OnGripGrabbed(Interactor interactor, Handle handle, EventTime eventTime) {
            holdingGunGripRight = interactor.playerHand == Player.local.handRight;
            holdingGunGripLeft = interactor.playerHand == Player.local.handLeft;

            if (!holdingGunGripLeft && !holdingGunGripRight) {
                currentAI = interactor.bodyHand.body.creature;
                currentAIBrain = (BrainHuman)currentAI.brain;
                aiOriginalMeleeEnabled = currentAIBrain.meleeEnabled;
                if (aiOriginalMeleeEnabled) {
                    aiOriginalMeleeDistMult = currentAIBrain.meleeMax;
                    aiOriginalParryDetectionRadius = currentAIBrain.parryDetectionRadius;
                    aiOriginalParryMaxDist = currentAIBrain.parryMaxDistance;
                    currentAIBrain.meleeEnabled = module.aiMeleeEnabled;
                    if (!module.aiMeleeEnabled) {
                        currentAIBrain.meleeDistMult = currentAIBrain.bowDist * module.aiShootDistanceMult;
                        currentAIBrain.parryDetectionRadius = (currentAIBrain.bowDist + 1f) * module.aiShootDistanceMult;
                        currentAIBrain.parryMaxDistance = (currentAIBrain.bowDist + 1f) * module.aiShootDistanceMult;
                    }
                }
                if (item.data.moduleAI.weaponHandling == ItemModuleAI.WeaponHandling.TwoHanded && foreGrip) {
                    aiGrabForegripTime = 1.0f;
                }
            }
        }

        public void OnGripUnGrabbed(Interactor interactor, Handle handle, EventTime eventTime) {
            if (interactor.playerHand == Player.local.handRight) holdingGunGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingGunGripLeft = false;

            if (currentAI) {
                if (aiOriginalMeleeEnabled) {
                    currentAIBrain.meleeEnabled = aiOriginalMeleeEnabled;
                    currentAIBrain.meleeDistMult = aiOriginalMeleeDistMult;
                    currentAIBrain.parryDetectionRadius = aiOriginalParryDetectionRadius;
                    currentAIBrain.parryMaxDistance = aiOriginalParryMaxDist;
                }
                if (item.data.moduleAI.weaponHandling == ItemModuleAI.WeaponHandling.TwoHanded && foreGrip) {
                    currentAI.body.handLeft.interactor.TryRelease();
                }
                currentAI = null;
            }
            ChargedFireStop();
        }

        public void OnForeGripGrabbed(Interactor interactor, Handle handle, EventTime eventTime) {
            holdingForeGripRight = interactor.playerHand == Player.local.handRight;
            holdingForeGripLeft = interactor.playerHand == Player.local.handLeft;
        }

        public void OnForeGripUnGrabbed(Interactor interactor, Handle handle, EventTime eventTime) {
            if (interactor.playerHand == Player.local.handRight) holdingForeGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingForeGripLeft = false;

            if (currentAI && !currentAI.body.handLeft.interactor.grabbedHandle) {
                if (item.data.moduleAI.weaponHandling == ItemModuleAI.WeaponHandling.TwoHanded && foreGrip) {
                    aiGrabForegripTime = 1.0f;
                }
            }
        }

        public void OnScopeGripGrabbed(Interactor interactor, Handle handle, EventTime eventTime) {
            holdingScopeGripRight = interactor.playerHand == Player.local.handRight;
            holdingScopeGripLeft = interactor.playerHand == Player.local.handLeft;
        }

        public void OnScopeGripUnGrabbed(Interactor interactor, Handle handle, EventTime eventTime) {
            if (interactor.playerHand == Player.local.handRight) holdingScopeGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingScopeGripLeft = false;
        }

        public void OnSecondaryGripGrabbed(Interactor interactor, Handle handle, EventTime eventTime) {
            holdingSecondaryGripRight = interactor.playerHand == Player.local.handRight;
            holdingSecondaryGripLeft = interactor.playerHand == Player.local.handLeft;
        }

        public void OnSecondaryGripUnGrabbed(Interactor interactor, Handle handle, EventTime eventTime) {
            if (interactor.playerHand == Player.local.handRight) holdingSecondaryGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingSecondaryGripLeft = false;
        }

        public void OnTelekinesisReleaseEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = null;
        }

        public void OnTelekinesisGrabEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = teleGrabber;
        }

        void DropBlaster() {
            if (gunGrip) gunGrip.Release();
            if (foreGrip) foreGrip.Release();
            if (scopeGrip) scopeGrip.Release();
            if (secondaryGrip) secondaryGrip.Release();
            leftInteractor = null;
            rightInteractor = null;
            overrideInteractor = null;
        }

        public void Fire() {
            if (ammoLeft == 0 || isOverheated) {
                Utils.PlaySound(emptySound, module.emptySoundAsset);
                Utils.PlaySound(emptySound2, module.emptySoundAsset2);
                leftInteractor = null;
                rightInteractor = null;
                overrideInteractor = null;
                shotsLeftInBurst = 0;
                return;
            }

            // Create and fire bullet
            var activeProjectileData = GetActiveProjectile();
            if (activeProjectileData == null) return;

            Transform[] spawns = module.multishot || (isChargedFire && module.chargeMultishot) ? bulletSpawns : new Transform[] { bulletSpawns[0] };
            FastList<Item> projectileClones = spawns.Length > 1 ? new FastList<Item>() : null;
            foreach (var bulletSpawn in spawns) {
                var projectile = activeProjectileData.Spawn(true);
                if (!projectile.gameObject.activeInHierarchy) projectile.gameObject.SetActive(true);
                var ignoreHandler = projectile.gameObject.GetComponent<CollisionIgnoreHandler>();
                if (!ignoreHandler) ignoreHandler = projectile.gameObject.AddComponent<CollisionIgnoreHandler>();
                ignoreHandler.item = projectile;
                ignoreHandler.IgnoreCollision(item);
                if (projectileClones != null) {
                    if (projectileClones.Count > 0) {
                        foreach (var toIgnore in projectileClones) {
                            ignoreHandler.IgnoreCollision(toIgnore);
                        }
                    }
                    projectileClones.Add(projectile);
                }
                try {
                    var shooter = gunGrip.handlers.First();
                    foreach (var pt in projectile.parryTargets) {
                        pt.owner = shooter.bodyHand.body.creature;
                        if (!ParryTarget.list.Contains(pt)) ParryTarget.list.Add(pt);
                    }
                    projectile.lastHandler = shooter;
                }
                catch { }

                foreach (CollisionHandler collisionHandler in projectile.definition.collisionHandlers) {
                    var useGravity = altFireEnabled ? boltAltModule.useGravity : boltModule.useGravity;
                    collisionHandler.SetPhysicModifier(this, 0, useGravity ? 1 : 0);

                    foreach (Damager damager in collisionHandler.damagers) {
                        var activeDamager = GetActiveBoltDamagerData();
                        if (activeDamager != null) {
                            damager.data = activeDamager;
                        }
                    }
                }
               
                // match new projectile inertia with current gun motion inertia
                var projTransform = projectile.transform;
                projTransform.position = bulletSpawn.position;
                projTransform.rotation = Quaternion.Euler(CalculateInaccuracy(bulletSpawn.rotation.eulerAngles));
                var projectileBody = projectile.GetComponent<Rigidbody>();
                projectileBody.velocity = body.velocity;
                projectile.Throw(1f);
                projectileBody.AddForce(projectileBody.transform.forward * module.bulletForce);

            }

            // Apply haptic feedback
            Utils.PlayHaptic(holdingGunGripLeft, holdingGunGripRight, module.fireHaptic);

            // Apply recoil
            if (module.recoil) ApplyRecoil();

            if (module.magazineSize > 0) ammoLeft--;
            if (module.overheatRate > 0) currentHeat += module.overheatRate;
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
                Utils.PlaySound(altFireSound, module.altFireSoundAsset);
                Utils.PlaySound(altFireSound2, module.altFireSoundAsset2);
            } else if (isChargedFire) {
                Utils.PlaySound(chargeFireSound, module.chargeFireSoundAsset);
                Utils.PlaySound(chargeFireSound2, module.chargeFireSoundAsset2);
                Utils.PlaySound(chargeFireSound3, module.chargeFireSoundAsset3);
            } else {
                Utils.PlaySound(fireSound, module.fireSoundAsset);
                Utils.PlaySound(fireSound2, module.fireSoundAsset2);
                Utils.PlaySound(fireSound3, module.fireSoundAsset3);
            }

            if (isChargedFire) {
                shotsLeftInBurst = 0;
                leftInteractor = null;
                rightInteractor = null;
                overrideInteractor = null;
                isChargedFire = false;
            }
        }

        void ChargedFireStart(Interactor interactor = null) {
            if (ammoLeft == 0 || isOverheated) {
                Utils.PlaySound(emptySound, module.emptySoundAsset);
                Utils.PlaySound(emptySound2, module.emptySoundAsset2);
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
            Utils.PlaySound(chargeSound, module.chargeSoundAsset);
            Utils.PlaySound(chargeSound2, module.chargeSoundAsset2);
            Utils.PlaySound(chargeStartSound, module.chargeStartSoundAsset);
            Utils.PlaySound(chargeStartSound2, module.chargeStartSoundAsset2);
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

        void ApplyRecoil() {
            // Add angular + positional recoil to the gun
            if (module.recoilAngle != null) {
                body.AddRelativeTorque(new Vector3(
                    Random.Range(module.recoilAngle[0], module.recoilAngle[1]),
                    Random.Range(module.recoilAngle[2], module.recoilAngle[3]),
                    Random.Range(module.recoilAngle[4], module.recoilAngle[5])
                    ), ForceMode.Impulse);
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

        void Reload(Interactor interactor = null) {
            if (!isReloading && !(TORGlobalSettings.BlasterRequireRefill && hasRefillPort && currentAI == null)) {
                isReloading = true;
                ammoLeft = 0;
                shotsLeftInBurst = 0;
                leftInteractor = null;
                rightInteractor = null;
                overrideInteractor = null;
                reloadTime = module.reloadTime;
                Utils.PlaySound(reloadSound, module.reloadSoundAsset);
                Utils.PlaySound(reloadSound2, module.reloadSoundAsset2);
                if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Moderate);
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
            Utils.PlaySound(reloadEndSound, module.reloadEndSoundAsset);
            Utils.PlaySound(reloadEndSound2, module.reloadEndSoundAsset2);
            Utils.PlayHaptic(holdingGunGripLeft, holdingGunGripRight, Utils.HapticIntensity.Moderate);
            item.definition.SetSavedValue("ammo", ammoLeft.ToString());
        }

        void RechargeFromPowerCell(string newProjectile) {
            if (!string.IsNullOrEmpty(newProjectile)) {
                item.definition.SetSavedValue("projectileID", newProjectile);
                projectileData = Catalog.GetData<ItemPhysic>(newProjectile, true);
                if (projectileData != null) boltModule = projectileData.GetModule<ItemModuleBlasterBolt>();
                ChargedFireStop();
                UpdateFireEffectColour();
                ReloadComplete();
            }
        }

        void UpdateAmmoDisplay() {
            if (ammoDisplay) {
                var digits = module.magazineSize == 0 ? 1 : 1 + (int)System.Math.Log10(System.Math.Abs(module.magazineSize));
                ammoDisplay.text = ammoLeft.ToString("D" + digits);
            }
        }
        
        void AIShoot() {
            if (currentAI && currentAIBrain != null && currentAIBrain.targetCreature) {
                if (!module.aiMeleeEnabled) {
                    var reach = gunGrip.definition.reach + 3f;
                    currentAIBrain.meleeEnabled = Vector3.SqrMagnitude(body.position - currentAIBrain.targetCreature.transform.position) <= reach * reach;
                }
                var aiAimAngle = CalculateInaccuracy(bulletSpawns[0].forward);
                if (Physics.Raycast(bulletSpawns[0].position, aiAimAngle, out RaycastHit hit, currentAIBrain.detectionRadius, aiMask, QueryTriggerInteraction.Ignore)) {
                    Creature target = null;
                    var materialHash = Animator.StringToHash(hit.collider.material.name);
                    if (materialHash == 1740652790 || materialHash == 1655722809) {
                        var handles = hit.collider.transform.root.GetComponentsInChildren<Handle>();
                        var handedHandle = handles.FirstOrDefault(handle => handle.IsHanded());
                        if (handedHandle) target = handedHandle.handlers[0].bodyHand.body.creature;
                    } else {
                        target = hit.collider.transform.GetComponentInChildren<Creature>();
                    }
                    if (target && currentAI != target
                        && currentAI.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Ignored && currentAI.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Passive 
                        && target.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Ignored && (currentAI.faction.attackBehaviour == GameData.Faction.AttackBehaviour.Agressive || currentAI.factionId != target.factionId)) {
                            shotsLeftInBurst = aiBurstAmount;
                            Fire();
                            aiShootTime = Random.Range(currentAIBrain.bowAimMinMaxDelay.x, currentAIBrain.bowAimMinMaxDelay.y) * ((currentAIBrain.bowDist / module.aiShootDistanceMult + hit.distance / module.aiShootDistanceMult) / currentAIBrain.bowDist);
                    }
                }
            }
        }

        Vector3 CalculateInaccuracy(Vector3 initial) {
            var baseInaccuracy = new Vector3(
                        initial.x + Random.Range(-currentInstability, currentInstability),
                        initial.y + Random.Range(-currentInstability, currentInstability),
                        initial.z);
            if (currentAIBrain == null) {
                return baseInaccuracy;
            }
            var inaccuracyMult = 0.1f * (currentAIBrain.aimSpreadCone / module.aiShootDistanceMult);
            return new Vector3(
                        baseInaccuracy.x + Random.Range(-inaccuracyMult, inaccuracyMult),
                        baseInaccuracy.y + Random.Range(-inaccuracyMult, inaccuracyMult),
                        baseInaccuracy.z);
        }

        void UpdateHoldTime(string action, ref float holdTime, bool holdingLeft, bool holdingRight) {
            if (holdTime > 0) {
                holdTime -= Time.deltaTime;
                if (holdTime <= 0) {
                    Interactor interactor = null;
                    if (holdingLeft) interactor = Player.local.handLeft.bodyHand.interactor;
                    if (holdingRight) interactor = Player.local.handRight.bodyHand.interactor;
                    ExecuteAction(action, interactor);
                }
            }
        }

        protected void LateUpdate() {
            // update timers
            if (altFireTime > 0) altFireTime -= Time.deltaTime;
            if (fireTime > 0) fireTime -= Time.deltaTime;
            if (fireDelayTime > 0) fireDelayTime -= Time.deltaTime;
            if (reloadTime > 0) reloadTime -= Time.deltaTime;
            if (currentHeat > 0) currentHeat -= Time.deltaTime;
            if (aiShootTime > 0) aiShootTime -= Time.deltaTime;
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
                Utils.PlaySound(overheatSound, module.overheatSoundAsset);
                Utils.PlaySound(overheatSound2, module.overheatSoundAsset2);
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
                            Utils.PlaySound(preFireSound, module.preFireSoundAsset);
                            Utils.PlaySound(preFireSound2, module.preFireSoundAsset2);
                            Utils.PlayParticleEffect(preFireEffect, module.preFireEffectDetachFromParent);
                        } else {
                            shotsLeftInBurst = currentFiremode;
                            Fire();
                        }
                    }
                    else if (aiShootTime <= 0) { AIShoot(); }
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
                else if (ammoLeft == 0 && (module.automaticReload || TORGlobalSettings.BlasterAutomaticReload || currentAI) && !isReloading) Reload();
            }

            if (chargeTime > 0) {
                chargeTime -= Time.deltaTime;
                var adjustedScale = Mathf.Lerp(originalChargeEffectScale.z, originalChargeEffectScale.z * 0.1f, chargeTime / module.chargeTime);
                var adjustedVolume = Mathf.Lerp(1, 0.2f, chargeTime / module.chargeTime);
                chargeEffectTrans.localScale = new Vector3(adjustedScale, adjustedScale, adjustedScale);
                if (chargeSound) chargeSound.volume = adjustedVolume;
                if (chargeSound2) chargeSound2.volume = adjustedVolume;
                if (chargeTime <= 0) {
                    if (chargeReadySound && !chargeReadySound.isPlaying) Utils.PlaySound(chargeReadySound, module.chargeReadySoundAsset);
                    if (chargeReadySound2 && !chargeReadySound2.isPlaying) Utils.PlaySound(chargeReadySound2, module.chargeReadySoundAsset2);
                    Utils.PlayHaptic(holdingGunGripLeft || holdingForeGripLeft, holdingGunGripRight || holdingForeGripRight, module.fireHaptic);
                }
            }

            // Run AI logic
            if (aiGrabForegripTime > 0) {
                aiGrabForegripTime -= Time.deltaTime;
                if (aiGrabForegripTime <= 0 && currentAI) {
                    currentAI.body.handLeft.interactor.TryRelease();
                    currentAI.body.handLeft.interactor.Grab(foreGrip);
                }
            }

            if (currentAI && currentAIBrain != null && currentAIBrain.targetCreature && currentAIBrain.defenseCollider) {
                if (currentAI.state == Creature.State.Alive && !currentAI.IsAnimatorBusy()) {
                    var pivot = currentAIBrain.defenseCollider.transform;
                    var direction = (currentAIBrain.targetCreature.transform.position - pivot.position).normalized;
                    pivot.rotation = Quaternion.Slerp(pivot.rotation, Quaternion.LookRotation(direction), Time.deltaTime * aiElevationSpeed);
                }
            }
        }
    }
}