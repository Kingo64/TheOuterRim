using BS;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TOR {
    // The item module will add a unity component to the item object. See unity monobehaviour for more information: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    // This component will apply a force on the player rigidbody to the direction of an item transform when the trigger is pressed (see custom reference in the item definition component of the item prefab)
    public class ItemBlaster : MonoBehaviour {
        protected Item item;
        protected ItemModuleBlaster module;

        protected Rigidbody body;
        protected Interactor rightInteractor;
        protected Interactor leftInteractor;
        protected AudioSource[] altFireSounds;
        protected AudioSource[] altFireSounds2;
        protected AudioSource[] emptySounds;
        protected AudioSource[] emptySounds2;
        protected AudioSource[] fireSounds;
        protected AudioSource[] fireSounds2;
        protected AudioSource[] fireModeSounds;
        protected AudioSource[] fireModeSounds2;
        protected AudioSource[] overheatSounds;
        protected AudioSource[] overheatSounds2;
        protected AudioSource[] preFireSounds;
        protected AudioSource[] reloadSounds;
        protected AudioSource[] reloadSounds2;
        protected AudioSource[] reloadEndSounds;
        protected AudioSource[] reloadEndSounds2;
        protected ParticleSystem altFireEffect;
        protected Text ammoDisplay;
        protected ParticleSystem fireEffect;
        protected ParticleSystem preFireEffect;
        protected ParticleSystem overheatEffect;

        protected Transform bulletSpawn;
        protected Handle gunGrip;
        protected Handle foreGrip;
        protected Handle scopeGrip;
        protected Handle secondaryGrip;

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

        public int ammoLeft = -1;
        int shotsLeftInBurst;
        public float currentHeat;
        public int currentFiremode;
        int currentFiremodeIndex;
        public float currentFirerate;
        int currentFirerateIndex;
        int currentScopeZoom;
        float altFireTime;
        float fireDelayTime;
        float fireTime;
        float reloadTime;
        bool isDelayingFire;
        bool isOverheated;
        bool isReloading;

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

        protected void Awake() {
            item = this.GetComponent<Item>();
            module = item.data.GetModule<ItemModuleBlaster>();
            body = this.GetComponent<Rigidbody>();

            // setup custom references
            if (!string.IsNullOrEmpty(module.altFireSoundsID)) altFireSounds = item.definition.GetCustomReference(module.altFireSoundsID).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.altFireSoundsID2)) altFireSounds2 = item.definition.GetCustomReference(module.altFireSoundsID2).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.emptySoundsID)) emptySounds = item.definition.GetCustomReference(module.emptySoundsID).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.emptySoundsID2)) emptySounds2 = item.definition.GetCustomReference(module.emptySoundsID2).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireSoundsID)) fireSounds = item.definition.GetCustomReference(module.fireSoundsID).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireSoundsID2)) fireSounds2 = item.definition.GetCustomReference(module.fireSoundsID2).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireModeSoundsID)) fireModeSounds = item.definition.GetCustomReference(module.fireModeSoundsID).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.fireModeSoundsID2)) fireModeSounds2 = item.definition.GetCustomReference(module.fireModeSoundsID2).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.overheatSoundsID)) overheatSounds = item.definition.GetCustomReference(module.overheatSoundsID).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.overheatSoundsID2)) overheatSounds2 = item.definition.GetCustomReference(module.overheatSoundsID2).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.preFireSoundsID)) preFireSounds = item.definition.GetCustomReference(module.preFireSoundsID).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadSoundsID)) reloadSounds = item.definition.GetCustomReference(module.reloadSoundsID).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadSoundsID2)) reloadSounds2 = item.definition.GetCustomReference(module.reloadSoundsID2).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadEndSoundsID)) reloadEndSounds = item.definition.GetCustomReference(module.reloadEndSoundsID).GetComponents<AudioSource>();
            if (!string.IsNullOrEmpty(module.reloadEndSoundsID2)) reloadEndSounds2 = item.definition.GetCustomReference(module.reloadEndSoundsID2).GetComponents<AudioSource>();

            if (!string.IsNullOrEmpty(module.altFireEffectID)) altFireEffect = item.definition.GetCustomReference(module.altFireEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.ammoDisplayID)) ammoDisplay = item.definition.GetCustomReference(module.ammoDisplayID).GetComponent<Text>();
            if (!string.IsNullOrEmpty(module.fireEffectID)) fireEffect = item.definition.GetCustomReference(module.fireEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.preFireEffectID)) preFireEffect = item.definition.GetCustomReference(module.preFireEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.overheatEffectID)) overheatEffect = item.definition.GetCustomReference(module.overheatEffectID).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(module.gunGripID)) gunGrip = item.definition.GetCustomReference(module.gunGripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.foreGripID)) foreGrip = item.definition.GetCustomReference(module.foreGripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.scopeGripID)) scopeGrip = item.definition.GetCustomReference(module.scopeGripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.secondaryGripID)) secondaryGrip = item.definition.GetCustomReference(module.secondaryGripID).GetComponent<Handle>();
            if (!string.IsNullOrEmpty(module.bulletSpawnID)) bulletSpawn = item.definition.GetCustomReference(module.bulletSpawnID);

            // setup item events
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnHeldActionEvent += OnHeldAction;
            item.OnSnapEvent += OnSnapEvent;

            // setup grip events
            if (gunGrip != null) {
                gunGrip.Grabbed += OnGripGrabbed;
                gunGrip.UnGrabbed += OnGripUnGrabbed;
            }
            if (foreGrip != null) {
                foreGrip.Grabbed += OnForeGripGrabbed;
                foreGrip.UnGrabbed += OnForeGripUnGrabbed;
            }
            if (scopeGrip != null) {
                scopeGrip.Grabbed += OnScopeGripGrabbed;
                scopeGrip.UnGrabbed += OnScopeGripUnGrabbed;
            }
            if (secondaryGrip != null) {
                secondaryGrip.Grabbed += OnSecondaryGripGrabbed;
                secondaryGrip.UnGrabbed += OnSecondaryGripUnGrabbed;
            }

            if (module.hasScope) SetupScope();
            currentFiremode = module.fireModes[0];
            currentFirerate = module.gunRPM[0];
            aiBurstAmount = Mathf.Abs(module.fireModes.Max());
            
            item.definition.TryGetSavedValue("Ammo", out string foundAmmo);
            if(!int.TryParse(foundAmmo, out ammoLeft)) {
                ammoLeft = module.magazineSize;
                UpdateAmmoDisplay();
            }
        }

        void SetupScope() {
            if (module.hasScope == false) return;
            var scopeTransform = item.definition.GetCustomReference(module.scopeID);
            if (scopeTransform != null) {
                scope = scopeTransform.GetComponent<Renderer>();
                originalScopeMaterial = scope.materials[0];
                scopeMaterial = scope.materials[1];
                scope.materials = new Material[] { originalScopeMaterial };

                scopeCamera = item.definition.GetCustomReference(module.scopeCameraID).GetComponent<Camera>();
                scopeCamera.fieldOfView = module.scopeZoom[currentScopeZoom];
                renderScopeTexture = new RenderTexture(module.scopeResolution[0], module.scopeResolution[1], module.scopeDepth, RenderTextureFormat.Default);
                renderScopeTexture.Create();
                scopeCamera.targetTexture = renderScopeTexture;
                scopeMaterial.SetTexture("_BaseMap", renderScopeTexture);
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

        void CycleFiremode() {
            if (module.fireModes.Length > 1) {
                currentFiremodeIndex = (currentFiremodeIndex >= module.fireModes.Length - 1) ? -1 : currentFiremodeIndex;
                currentFiremode = module.fireModes[++currentFiremodeIndex];
                Utils.PlayRandomSound(fireModeSounds); Utils.PlayRandomSound(fireModeSounds2);
            }
        }

        void CycleFirerate() {
            if (module.gunRPM.Length > 1) {
                currentFirerateIndex = (currentFirerateIndex >= module.gunRPM.Length - 1) ? -1 : currentFirerateIndex;
                currentFirerate = module.gunRPM[++currentFirerateIndex];
                Utils.PlayRandomSound(fireModeSounds); Utils.PlayRandomSound(fireModeSounds2);
            }
        }

        void CycleScope() {
            if (scope == null || scopeCamera == null) return;
            currentScopeZoom = (currentScopeZoom >= module.scopeZoom.Length - 1) ? -1 : currentScopeZoom;
            scopeCamera.fieldOfView = module.scopeZoom[++currentScopeZoom];
            var scopeSound = scope.GetComponent<AudioSource>();
            if (scopeSound != null) scopeSound.Play();
        }

        public void OnGrabEvent(Handle handle, Interactor interactor) {
            // toggle scope for performance reasons
            if (module.hasScope && (interactor.playerHand == Player.local.handRight || interactor.playerHand == Player.local.handLeft)) EnableScopeRender();
        }

        public void OnUngrabEvent(Handle handle, Interactor interactor, bool throwing) {
            // toggle scope for performance reasons
            if (module.hasScope) DisableScopeRender();
        }
        
        public void OnSnapEvent(ObjectHolder holder) {
            item.definition.SetSavedValue("Ammo", ammoLeft.ToString());
        }

        public void ExecuteAction(string action) {
            if (action == "cycleScope") CycleScope();
            else if (action == "cycleFiremode") CycleFiremode();
            else if (action == "cycleFirerate") CycleFirerate();
            else if (action == "reload") Reload();
            else if (action == "altFire") {
                if (altFireTime <= 0) {
                    altFireTime = module.altFireCooldown;
                    Stun();
                }
            }
        }

        public void OnHeldAction(Interactor interactor, Handle handle, Interactable.Action action) {
            if (action == Interactable.Action.UseStart) {
                if (interactor.side == Side.Right) {
                    rightInteractor = interactor;
                } else {
                    leftInteractor = interactor;
                }
            } else if (action == Interactable.Action.AlternateUseStart) {
                // trigger all the secondary actions
                if ((interactor.side == Side.Right && holdingGunGripRight) || (interactor.side == Side.Left && holdingGunGripLeft)) ExecuteAction(module.gunGripSecondaryAction);
                else if ((interactor.side == Side.Right && holdingForeGripRight) || (interactor.side == Side.Left && holdingForeGripLeft)) ExecuteAction(module.foreGripSecondaryAction);
                else if ((interactor.side == Side.Right && holdingScopeGripRight) || (interactor.side == Side.Left && holdingScopeGripLeft)) ExecuteAction(module.scopeGripSecondaryAction);
                else if ((interactor.side == Side.Right && holdingSecondaryGripRight) || (interactor.side == Side.Left && holdingSecondaryGripLeft)) ExecuteAction(module.secondaryGripSecondaryAction);
            } else if (action == Interactable.Action.UseStop || action == Interactable.Action.Ungrab) {
                if (interactor.side == Side.Right) {
                    rightInteractor = null;
                } else {
                    leftInteractor = null;
                }
            }
        }

        public void OnGripGrabbed(Interactor interactor, EventTime arg2) {
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
                        currentAIBrain.parryDetectionRadius = currentAIBrain.bowDist * module.aiShootDistanceMult;
                        currentAIBrain.parryMaxDistance = currentAIBrain.bowDist * module.aiShootDistanceMult;
                    }
                }
                if (item.data.moduleAI.weaponHandling == ItemModuleAI.WeaponHandling.TwoHanded && foreGrip != null) {
                    aiGrabForegripTime = 1.0f;
                }
            }
        }

        public void OnGripUnGrabbed(Interactor interactor, EventTime arg2) {
            if (interactor.playerHand == Player.local.handRight) holdingGunGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingGunGripLeft = false;

            if (currentAI != null) {
                if (aiOriginalMeleeEnabled) {
                    currentAIBrain.meleeEnabled = aiOriginalMeleeEnabled;
                    currentAIBrain.meleeDistMult = aiOriginalMeleeDistMult;
                    currentAIBrain.parryMaxDistance = aiOriginalParryMaxDist;
                }
                if (item.data.moduleAI.weaponHandling == ItemModuleAI.WeaponHandling.TwoHanded && foreGrip != null) {
                    currentAI.body.handLeft.interactor.TryRelease();
                }
                currentAI = null;
            }
        }

        public void OnForeGripGrabbed(Interactor interactor, EventTime arg2) {
            holdingForeGripRight = interactor.playerHand == Player.local.handRight;
            holdingForeGripLeft = interactor.playerHand == Player.local.handLeft;
        }

        public void OnForeGripUnGrabbed(Interactor interactor, EventTime arg2) {
            if (interactor.playerHand == Player.local.handRight) holdingForeGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingForeGripLeft = false;

            if (currentAI != null && !currentAI.body.handLeft.interactor.grabbedHandle) {
                if (item.data.moduleAI.weaponHandling == ItemModuleAI.WeaponHandling.TwoHanded && foreGrip != null) {
                    aiGrabForegripTime = 1.0f;
                }
            }
        }

        public void OnScopeGripGrabbed(Interactor interactor, EventTime arg2) {
            holdingScopeGripRight = interactor.playerHand == Player.local.handRight;
            holdingScopeGripLeft = interactor.playerHand == Player.local.handLeft;
        }

        public void OnScopeGripUnGrabbed(Interactor interactor, EventTime arg2) {
            if (interactor.playerHand == Player.local.handRight) holdingScopeGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingScopeGripLeft = false;
        }

        public void OnSecondaryGripGrabbed(Interactor interactor, EventTime arg2) {
            holdingSecondaryGripRight = interactor.playerHand == Player.local.handRight;
            holdingSecondaryGripLeft = interactor.playerHand == Player.local.handLeft;
        }

        public void OnSecondaryGripUnGrabbed(Interactor interactor, EventTime arg2) {
            if (interactor.playerHand == Player.local.handRight) holdingSecondaryGripRight = false;
            else if (interactor.playerHand == Player.local.handLeft) holdingSecondaryGripLeft = false;
        }

        void Fire() {
            if (ammoLeft == 0 || isOverheated) {
                Utils.PlayRandomSound(emptySounds); Utils.PlayRandomSound(emptySounds2);
                leftInteractor = null;
                rightInteractor = null;
                shotsLeftInBurst = 0;
                return;
            }

            // Create and fire bullet
            var projectileData = Catalog.current.GetData<ItemData>(module.projectileID, true);
            if (projectileData == null) return;
            var projectile = projectileData.Spawn(true);
            if (!projectile.gameObject.activeInHierarchy) projectile.gameObject.SetActive(true);
            try {
                var shooter = gunGrip.handlers.First();
                projectile.parryTargets.Select(target => target.owner = shooter.bodyHand.body.creature);
                projectile.lastHandler = shooter;
            }
            catch { }

            // match new projectile inertia with current gun motion inertia
            var projTransform = projectile.transform;
            projTransform.position = bulletSpawn.position;
            projTransform.rotation = Quaternion.Euler(CalculateInaccuracy(bulletSpawn.rotation.eulerAngles));
            var projectileBody = projectile.GetComponent<Rigidbody>();
            projectileBody.velocity = body.velocity;
            projectile.Throw(1f);
            projectileBody.AddForce(projectileBody.transform.forward * module.bulletForce);

            // Apply haptic feedback
            if (rightInteractor && holdingGunGripRight && module.fireHaptic > 0) PlayerControl.handRight.HapticShort(module.fireHaptic);
            else if (leftInteractor && holdingGunGripLeft && module.fireHaptic > 0) PlayerControl.handLeft.HapticShort(module.fireHaptic);

            // Apply recoil
            if (module.recoil) ApplyRecoil();

            if (module.magazineSize > 0) ammoLeft--;
            if (module.overheatRate > 0) currentHeat += module.overheatRate;
            if (--shotsLeftInBurst == 0) {
                leftInteractor = null;
                rightInteractor = null;
            }
            var fireModifier = (module.burstRPM > 0 && shotsLeftInBurst > 0) ? module.burstRPM : currentFirerate;
            fireTime = 1 / (fireModifier / 60);

            UpdateAmmoDisplay();
            Utils.PlayParticleEffect(fireEffect, module.fireEffectDetachFromParent);
            Utils.PlayRandomSound(fireSounds); Utils.PlayRandomSound(fireSounds2);
        }

        void Stun() {
            Utils.PlayParticleEffect(altFireEffect, module.altFireEffectDetachFromParent);
            Utils.PlayRandomSound(altFireSounds); Utils.PlayRandomSound(altFireSounds2);
            if (Physics.Raycast(bulletSpawn.position, bulletSpawn.TransformDirection(Vector3.forward), out RaycastHit hit, module.altFireRange)) {
                var target = hit.collider.transform.root.GetComponent<Creature>();
                if (target != null && target != Creature.player && target.state != Creature.State.Dead) {
                    target.ragdoll.SetState(Creature.State.Destabilized);
                }
            }
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

        void Reload() {
            if (!isReloading) {
                isReloading = true;
                reloadTime = module.reloadTime;
                Utils.PlayRandomSound(reloadSounds); Utils.PlayRandomSound(reloadSounds2);
            }
        }

        void UpdateAmmoDisplay() {
            if (ammoDisplay != null) {
                var digits = module.magazineSize == 0 ? 1 : 1 + (int)System.Math.Log10(System.Math.Abs(module.magazineSize));
                ammoDisplay.text = ammoLeft.ToString("D" + digits);
            }
        }
        
        void AIShoot() {
            if (currentAI != null && currentAIBrain != null && currentAIBrain.targetCreature != null) {
                if (!module.aiMeleeEnabled) {
                    currentAIBrain.meleeEnabled = Vector3.Distance(body.position, currentAIBrain.targetCreature.transform.position) <= (gunGrip.definition.reach + 3f);
                }
                var aiAimAngle = CalculateInaccuracy(bulletSpawn.TransformDirection(Vector3.forward));
                if (Physics.Raycast(bulletSpawn.position, aiAimAngle, out RaycastHit hit, currentAIBrain.detectionRadius)) {
                    Creature target = null;
                    if (hit.collider.material.name == "Lightsaber (Instance)") {
                        var handles = hit.collider.transform.root.GetComponentsInChildren<Handle>();
                        var handedHandle = handles.FirstOrDefault(handle => handle.IsHanded());
                        if (handedHandle != null) target = handedHandle.handlers[0].bodyHand.body.creature;
                    } else {
                        target = hit.collider.transform.root.GetComponent<Creature>();
                    }
                    if (target != null && currentAI != target
                        && currentAI.faction.attackBehaviour != Creature.Faction.AttackBehaviour.Ignored && currentAI.faction.attackBehaviour != Creature.Faction.AttackBehaviour.Passive 
                        && target.faction.attackBehaviour != Creature.Faction.AttackBehaviour.Ignored && (currentAI.faction.attackBehaviour == Creature.Faction.AttackBehaviour.Agressive || currentAI.factionId != target.factionId)) {
                            shotsLeftInBurst = aiBurstAmount;
                            Fire();
                            aiShootTime = Random.Range(currentAIBrain.bowAimMinMaxDelay.x, currentAIBrain.bowAimMinMaxDelay.y) * ((currentAIBrain.bowDist / module.aiShootDistanceMult + hit.distance / module.aiShootDistanceMult) / currentAIBrain.bowDist);
                    }
                }
            }
        }

        Vector3 CalculateInaccuracy(Vector3 initial) {
            if (currentAIBrain == null) return initial;
            var inaccuracyMult = 0.2f * (currentAIBrain.actionBow.aimSpreadCone / module.aiShootDistanceMult);
            return new Vector3(
                        initial.x + (Random.Range(-inaccuracyMult, inaccuracyMult)),
                        initial.y + (Random.Range(-inaccuracyMult, inaccuracyMult)),
                        initial.z);
        }

        protected void LateUpdate() {
            // update timers
            if (altFireTime > 0) altFireTime -= Time.deltaTime;
            if (fireTime > 0) fireTime -= Time.deltaTime;
            if (fireDelayTime > 0) fireDelayTime -= Time.deltaTime;
            if (reloadTime > 0) reloadTime -= Time.deltaTime;
            if (currentHeat > 0) currentHeat -= Time.deltaTime;
            if (aiShootTime > 0) aiShootTime -= Time.deltaTime;

            if (currentHeat > module.overheatThreshold && !isOverheated) {
                isOverheated = true;
                Utils.PlayRandomSound(overheatSounds); Utils.PlayRandomSound(overheatSounds2);
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
                    if ((rightInteractor && holdingGunGripRight) || (leftInteractor && holdingGunGripLeft)) {
                        if (module.fireDelay > 0) {
                            fireDelayTime = module.fireDelay;
                            isDelayingFire = true;
                            Utils.PlayRandomSound(preFireSounds);
                            Utils.PlayParticleEffect(preFireEffect, module.preFireEffectDetachFromParent);
                        } else {
                            shotsLeftInBurst = currentFiremode;
                            Fire();
                        }
                    }
                    else if (aiShootTime <= 0) { AIShoot(); }
                }
            }

            // handle reload update if needed
            if (reloadTime <= 0 && module.magazineSize > 0) {
                // finish reload
                if (isReloading) {
                    isReloading = false;
                    ammoLeft = module.magazineSize;
                    UpdateAmmoDisplay();
                    Utils.PlayRandomSound(reloadEndSounds); Utils.PlayRandomSound(reloadEndSounds2);
                }
                // start reloading if auto and out of ammo
                else if (ammoLeft == 0 && module.automaticReload && !isReloading) Reload();
            }

            // Run AI logic
            if (aiGrabForegripTime > 0) {
                aiGrabForegripTime -= Time.deltaTime;
                if (aiGrabForegripTime <= 0 && currentAI != null) {
                    currentAI.body.handLeft.interactor.TryRelease();
                    currentAI.body.handLeft.interactor.Grab(foreGrip);
                }
            }
        }
    }
}