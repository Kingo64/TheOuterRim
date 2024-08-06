using ThunderRoad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThunderRoad.Skill.SpellPower;

namespace TOR {
    [Serializable]
    public class ItemLightsaberSaveData : ContentCustomData {
        public string[] kyberCrystals = {};
        public float[] bladeLengths = {};
        public SavedLightsaber coupledLightsaber;
    }

    public class ItemLightsaber : ThunderBehaviour {
        public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update | ManagedLoops.FixedUpdate;

        public static List<ItemLightsaber> all = new List<ItemLightsaber>();
        protected Item item;
        public ItemModuleLightsaber module;
        protected Rigidbody body;

        public LightsaberBlade[] blades;
        internal bool isActive;
        bool isHelicoptering;
        bool isHolding;
        public bool isSnapped;
        bool isOpen;
        float originalMass;
        bool fixBrokenPhysics;

        RagdollHand leftInteractor;
        RagdollHand rightInteractor;
        SpellTelekinesis telekinesis;

        Animator[] animators = { };
        string[] kyberCrystals = { };
        float[] bladeLengths = { };
        bool thrown;
        bool returning;
        bool tapReturning;
        bool tapToReturn;
        float ignoreCrystalTime;
        public float ignoreCoupleTime;
        float primaryControlHoldTime;
        float secondaryControlHoldTime;
        float deactivateOnDropTime;
        ItemModuleAI.WeaponClass originalWeaponClass;
        PlayerHand playerHand;
        Coroutine destabiliseGrabbedCoroutine;
        Coroutine unpenetrateCoroutine;
        Coroutine unpenetrateCoroutineNPC;

        Transform itemTrans;
        Collider couplerCollider;
        Transform couplerTrans;
        FixedJoint couplerJoint;
        Item coupledItem;
        string coupledItemOriginalSlot;
        ItemLightsaber coupledLightsaber;

        Rigidbody playerBody;

        protected void Awake() {
            all.Add(this);
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLightsaber>();
            body = item.GetComponent<Rigidbody>();
            itemTrans = item.transform;
            originalMass = body.mass;

            if (!string.IsNullOrEmpty(module.animatorId)) animators = item.GetCustomReference(module.animatorId).GetComponentsInChildren<Animator>();

            item.TryGetCustomData(out ItemLightsaberSaveData saveData);
            if (saveData != null) {
                kyberCrystals = saveData.kyberCrystals;
                bladeLengths = saveData.bladeLengths;
            }

            if (module.hasCoupler) {
                couplerCollider = item.GetCustomReference("CouplerCollider").GetComponent<Collider>();
                couplerTrans = item.GetCustomReference("CouplerHolder");

                if (saveData != null && saveData.coupledLightsaber != null) {
                    Catalog.GetData<ItemData>(saveData.coupledLightsaber.itemId, true).SpawnAsync((Item item) => {
                        Couple(item);
                        var coupledKyberCrystals = saveData.coupledLightsaber.saveData.kyberCrystals;
                        for (int i = 0, l = coupledKyberCrystals.Count(); i < l; i++) {
                            coupledLightsaber.blades[i].AddCrystal(coupledKyberCrystals[i]);
                        }

                        var lengths = saveData.coupledLightsaber.saveData.bladeLengths;
                        for (int i = 0, l = lengths.Count(); i < l; i++) {
                            coupledLightsaber.blades[i].SetBladeLength(lengths[i] / coupledLightsaber.blades[i].saberBodyTrans.parent.localScale.z * 0.1f);
                        }

                        coupledLightsaber.UpdateCustomData();
                    });
                }
            }

            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnTelekinesisGrabEvent += OnTeleGrabEvent;
            item.OnTelekinesisReleaseEvent += OnTeleUngrabEvent;
            item.OnHeldActionEvent += OnHeldAction;
            item.OnSnapEvent += OnSnapEvent;
            item.OnUnSnapEvent += OnUnSnapEvent;

            blades = module.lightsaberBlades.Select(x => JsonUtility.FromJson<LightsaberBlade>(JsonUtility.ToJson(x))).ToArray();

            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].Initialise(item, module.ignitionDuration, kyberCrystals.ElementAtOrDefault(i) ?? null, bladeLengths.ElementAtOrDefault(i));
            }

            if (module.startActive) TurnOn(true);
            else TurnOff(false);

            tapToReturn = !GameManager.options.GetController().holdGripForHandles;
            originalWeaponClass = item.data.moduleAI.primaryClass;

            UpdateCustomData();            
        }

        protected void OnDestroy() {
            if (all.Contains(this)) {
                all.Remove(this);
            }
            if (GlobalSettings.LightsaberColliders.ContainsKey(GetInstanceID())) {
                GlobalSettings.LightsaberColliders.Remove(GetInstanceID());
            }
        }

        public ItemLightsaberSaveData UpdateCustomData() {
            var saveData = new ItemLightsaberSaveData();
            if (blades != null) {
                saveData.kyberCrystals = blades.Select(blade => blade.kyberCrystal).ToArray();
                saveData.bladeLengths = blades.Select(blade => blade.maxLength * blade.saberBodyTrans.parent.localScale.z * 10f).ToArray();
            }
            if (coupledItem) {
                saveData.coupledLightsaber = new SavedLightsaber {
                    itemId = coupledItem.itemId,
                    saveData = coupledLightsaber.UpdateCustomData()
                };
            }
            return Utils.UpdateCustomData(item, saveData);
        }

        public void ExecuteAction(string action, RagdollHand interactor = null) {
            if (action == "decouple") Decouple();
            else if (action == "nextPhase") NextPhase(interactor);
            else if (action == "toggleAnimation") ToggleAnimation(interactor);
            else if (action == "toggleHelicopter") ToggleHelicopter(interactor);
            else if (action == "toggleIgnition") ToggleLightsaber(interactor);
            else if (action == "toggleSingle") ToggleSingle(interactor);
            else if (action == "turnOn") TurnOn();
            else if (action == "turnOff") TurnOff();
        }

        public void DecreaseBladeLength(float lengthChange) {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                if (blades[i].maxLength - lengthChange > 0) {
                    blades[i].SetBladeLength(blades[i].maxLength - lengthChange);
                }
            }
            UpdateCustomData();
        }

        public void IncreaseBladeLength(float lengthChange) {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].SetBladeLength(blades[i].maxLength + lengthChange);
            }
            UpdateCustomData();
        }

        public void ResetBladeLength(float _) {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].SetBladeLength(module.lightsaberBlades[i].bladeLength / blades[i].saberBodyTrans.parent.localScale.z * 0.1f);
            }
            UpdateCustomData();
        }

        public void NextPhase(RagdollHand interactor = null) {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].NextPhase();
            }
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
        }

        public void Couple(Item itemToCouple) {
            if (itemToCouple && (itemToCouple.itemId == item.itemId || (module.couplingWhitelist != null && module.couplingWhitelist.Contains(itemToCouple.itemId)))) {
                coupledItem = itemToCouple;
                coupledItemOriginalSlot = coupledItem.data.slot;
                coupledItem.data.slot = "Cork";
                coupledItem.transform.MoveAlign(coupledItem.holderPoint, couplerTrans, couplerTrans);
                couplerJoint = itemTrans.gameObject.AddComponent<FixedJoint>();
                couplerJoint.connectedBody = coupledItem.physicBody.rigidBody;
                coupledLightsaber = coupledItem.GetComponent<ItemLightsaber>();
                SetCouplerCollider(false);
                UpdateCustomData();
            }
        }

        public void Decouple() {
            if (couplerJoint) Destroy(couplerJoint);
            if (coupledItem) {
                coupledItem.transform.SetParent(null);
                coupledItem.data.slot = coupledItemOriginalSlot;
                coupledItem = null;
            }
            SetCouplerCollider(true);
            coupledLightsaber = null;
            UpdateCustomData();
        }

        public void SetCouplerCollider(bool enabled) {
            if (couplerCollider) couplerCollider.enabled = enabled;
            ignoreCoupleTime = 0.5f;
            if (coupledLightsaber) {
                coupledLightsaber.SetCouplerCollider(enabled);
                coupledLightsaber.ignoreCoupleTime = 1f;
            }
        }

        void ToggleAnimation(RagdollHand interactor = null) {
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
            isOpen = !isOpen;
            for (int i = 0, l = animators.Count(); i < l; i++) {
                animators[i].SetTrigger(isOpen ? "open" : "close");
                animators[i].ResetTrigger(isOpen ? "close" : "open");
            }
        }

        void ToggleHelicopter(RagdollHand interactor = null) {
            ToggleAnimation(interactor);
            isHelicoptering = !isHelicoptering;
            if (isHelicoptering) {
                destabiliseGrabbedCoroutine = StartCoroutine(DestabliseGrabbedCoroutine());
                unpenetrateCoroutine = StartCoroutine(UnpenetrateCoroutine());
            } else {
                StopCoroutine(destabiliseGrabbedCoroutine);
                StopCoroutine(unpenetrateCoroutine);
            }
        }

        IEnumerator DestabliseGrabbedCoroutine() {
            Creature GetCreature(Side side) {
                return Player.local?.GetHand(side)?.ragdollHand?.grabbedHandle?.gameObject?.GetComponentInParent<Creature>();
            }

            while (true) {
                yield return Utils.waitSeconds_01;
                Creature creature = null;
                if (item.leftPlayerHand) {
                    if (Player.local?.handRight?.ragdollHand?.grabbedHandle)
                    creature = GetCreature(Side.Right);
                } else if (item.rightPlayerHand) {
                    if (Player.local?.handLeft?.ragdollHand?.grabbedHandle)
                    creature = GetCreature(Side.Left);
                } else {
                    StopCoroutine(destabiliseGrabbedCoroutine);
                }
                if (creature?.ragdoll && creature.ragdoll.state == Ragdoll.State.Standing) {
                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                }
            }
        }

        void ToggleLightsaber(RagdollHand interactor = null) {
            if (isActive) TurnOff();
            else TurnOn();
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
        }

        // Turn on only first blade - used for saber staff
        void ToggleSingle(RagdollHand interactor = null) {
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
            var singleAlreadyActive = blades[0].isActive;
            if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) {
                isActive = true;

                var firstEnabled = false;
                for (int i = 0, l = blades.Count(); i < l; i++) {
                    if (!firstEnabled || singleAlreadyActive && !blades[i].isActive) {
                        blades[i].TurnOn(!blades[i].isActive);
                        blades[i].TryPenetrate();
                    } else blades[i].TurnOff(blades[i].isActive);
                    firstEnabled = true;
                }
                ResetCollisions();

                if (GlobalSettings.LightsaberColliders != null) {
                    var activeBlades = blades.Select(blade => blade.collisionBlade).ToArray();
                    if (!GlobalSettings.LightsaberColliders.ContainsKey(GetInstanceID())) {
                        GlobalSettings.LightsaberColliders.Add(GetInstanceID(), activeBlades);
                    } else {
                        GlobalSettings.LightsaberColliders[GetInstanceID()] = activeBlades;
                    }
                }
            }
        }

        void TurnOn(bool playSound = true) {
            if (module.animateOnIgnition && !isOpen) ToggleAnimation();
            if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) {
                isActive = true;
                var tryPenetrate = true;

                for (int i = 0, l = blades.Count(); i < l; i++) {
                    blades[i].TurnOn(playSound && !blades[i].isActive);

                    if (tryPenetrate) tryPenetrate = !blades[i].TryPenetrate();
                }

                ResetCollisions();

                if (GlobalSettings.LightsaberColliders != null && !GlobalSettings.LightsaberColliders.ContainsKey(GetInstanceID())) {
                    GlobalSettings.LightsaberColliders.Add(GetInstanceID(), blades.Select(blade => blade.collisionBlade).ToArray());
                }
            }
        }

        void TurnOff(bool playSound = true) {
            isActive = false;

            if (module.animateOnIgnition && isOpen) ToggleAnimation();
            Unpenetrate();
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].TurnOff(playSound && blades[i].isActive);
            }
            ResetCollisions();

            if (GlobalSettings.LightsaberColliders != null && GlobalSettings.LightsaberColliders.ContainsKey(GetInstanceID())) {
                GlobalSettings.LightsaberColliders.Remove(GetInstanceID());
            }
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            playerHand = interactor.playerHand;

            // Turn on lightsaber automatically if NPC hand
            if (playerHand != Player.local.handRight && playerHand != Player.local.handLeft) {
                if (body.mass == originalMass) body.mass *= 0.25f;
                TurnOn();
                var creature = interactor.creature;
                if (!creature.gameObject.GetComponent<LightsaberNPCAnimator>()) {
                    creature.gameObject.AddComponent<LightsaberNPCAnimator>().SetCreature(creature);
                }
                unpenetrateCoroutineNPC = StartCoroutine(UnpenetrateCoroutine());
            }
            isHolding = true;
            ResetCollisions();
            thrown = false;
            returning = false;

            deactivateOnDropTime = -1;

            if (coupledItem) {
                item.IgnoreRagdollCollision(interactor.ragdoll);
                coupledItem.IgnoreRagdollCollision(interactor.ragdoll);
            }
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            // Turn off lightsaber automatically if NPC hand
            if (playerHand != Player.local.handRight && playerHand != Player.local.handLeft) {
                body.mass = originalMass;
                TurnOff();
                var otherHandle = interactor.otherHand?.grabbedHandle;
                if (!otherHandle || (otherHandle.item && !otherHandle.item.gameObject.GetComponent<ItemLightsaber>())) {
                    Destroy(interactor.creature.gameObject.GetComponent<LightsaberNPCAnimator>());
                }
                StopCoroutine(unpenetrateCoroutineNPC);
            }
            isHolding = false;
            ResetCollisions();

            // throw the lightsaber
            if (!thrown && body.velocity.magnitude > GlobalSettings.SaberThrowMinVelocity && GlobalSettings.SaberThrowable) {
                playerHand = interactor.playerHand;
                thrown = true;
            }

            leftInteractor = null;
            rightInteractor = null;

            if (!item.IsHanded()) deactivateOnDropTime = GlobalSettings.SaberDeactivateOnDropDelay;
        }

        public void OnSnapEvent(Holder holder) {
            isSnapped = true;
            if (playerHand) playerHand.ragdollHand.TryRelease();
        }

        public void OnUnSnapEvent(Holder holder) {
            isSnapped = false;

            if (!item.isGripped && !item.isTelekinesisGrabbed) {
                fixBrokenPhysics = true;
            }
        }

        public void OnTeleGrabEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = teleGrabber;
            ResetCollisions();

            if (isHelicoptering && !thrown) {
                ToggleHelicopter();
            }

            deactivateOnDropTime = -1;
        }

        public void OnTeleUngrabEvent(Handle handle, SpellTelekinesis teleGrabber, bool tryThrow, bool isGrabbing) {
            telekinesis = null;
            ResetCollisions();

            if (!item.IsHanded()) deactivateOnDropTime = GlobalSettings.SaberDeactivateOnDropDelay;
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action) {
            // If primary hold action available
            if (!string.IsNullOrEmpty(module.primaryGripPrimaryActionHold)) {
                // start primary control timer
                if (action == Interactable.Action.UseStart) {
                    primaryControlHoldTime = GlobalSettings.ControlsHoldDuration;
                    if (interactor.side == Side.Right) {
                        rightInteractor = interactor;
                    } else {
                        leftInteractor = interactor;
                    }
                } else if (action == Interactable.Action.UseStop) {
                    // if not held for long run standard action
                    if (primaryControlHoldTime > 0 && primaryControlHoldTime > (primaryControlHoldTime / 2)) {
                        ExecuteAction(module.primaryGripPrimaryAction, interactor);
                    }
                    primaryControlHoldTime = 0;
                }
            } else {
                if (action == Interactable.Action.UseStart) ExecuteAction(module.primaryGripPrimaryAction, interactor);
            }

            // If secondary hold action available
            if (!string.IsNullOrEmpty(module.primaryGripSecondaryActionHold)) {
                // start secondary control timer
                if (action == Interactable.Action.AlternateUseStart) {
                    secondaryControlHoldTime = GlobalSettings.ControlsHoldDuration;
                } else if (action == Interactable.Action.AlternateUseStop) {
                    // if not held for long run standard action
                    if (secondaryControlHoldTime > 0 && secondaryControlHoldTime > (secondaryControlHoldTime / 2)) {
                        ExecuteAction(module.primaryGripSecondaryAction, interactor);
                    }
                    secondaryControlHoldTime = 0;
                }
            } else {
                if (action == Interactable.Action.AlternateUseStart) ExecuteAction(module.primaryGripSecondaryAction, interactor);
            }

            if (action == Interactable.Action.UseStart) {
                if (interactor.side == Side.Right) {
                    rightInteractor = interactor;
                } else {
                    leftInteractor = interactor;
                }
            } else if (action == Interactable.Action.UseStop || action == Interactable.Action.Ungrab) {
                if (interactor.side == Side.Right) {
                    rightInteractor = null;
                } else {
                    leftInteractor = null;
                }
            }
        }

        public void TryAddCrystal(ItemKyberCrystal crystal) {
            if (crystal && ignoreCrystalTime <= 0) {
                for (int i = 0, l = blades.Count(); i < l; i++) {
                    if (string.IsNullOrEmpty(blades[i].kyberCrystal)) {
                        blades[i].AddCrystal(crystal);
                        UpdateCustomData();
                        break;
                    }
                }
                // Allow AI to use lightsaber again
                if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) item.data.moduleAI.primaryClass = originalWeaponClass;
            }
        }

        public void TryEjectCrystal() {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                if (!string.IsNullOrEmpty(blades[i].kyberCrystal)) {
                    TurnOff(isActive);
                    item.data.moduleAI.primaryClass = ItemModuleAI.WeaponClass.None; // Tell NPCs not to use lightsaber
                    blades[i].RemoveCrystal();
                    UpdateCustomData();
                    ignoreCrystalTime = 0.5f;
                    break;
                }
            }
        }

        protected void OnTriggerEnter(Collider other) {
            if (other.name == "CouplerCollider" && module.hasCoupler && coupledItem == null && ignoreCoupleTime <= 0 && isSnapped == false) {
                var itemToCouple = other.attachedRigidbody.GetComponentInParent<Item>();
                if ((item.leftPlayerHand || item.rightPlayerHand) && (itemToCouple.leftPlayerHand || itemToCouple.rightPlayerHand)) {
                    var lightsaberToCouple = other.attachedRigidbody.GetComponentInParent<ItemLightsaber>();
                    if (lightsaberToCouple && !lightsaberToCouple.isSnapped && lightsaberToCouple.coupledItem == null && lightsaberToCouple.item.data.slot != "Cork") {
                        Couple(itemToCouple);
                    }
                }
            }
        }

        void ResetCollisions() {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                if (blades[i].collisionBlade) blades[i].collisionBlade.enabled = blades[i].isActive;
            }
            body.ResetCenterOfMass();
        }

        void Unpenetrate() {
            // Unpenetrate all currently penetrated objects - fixes glitchy physics
            foreach (var handler in item.collisionHandlers) {
                for (int i = 0, l = handler.collisions.Count(); i < l; i++) {
                    var damageStruct = handler.collisions[i].damageStruct;
                    if (damageStruct.penetration != DamageStruct.Penetration.None) {
                        damageStruct.damager.UnPenetrateAll();
                        handler.collisions[i].active = false;
                    }
                }
            }
        }

        IEnumerator UnpenetrateCoroutine() {
            while (true) {
                yield return Utils.waitSeconds_01;
                Unpenetrate();
            }
        }

        protected override void ManagedUpdate() {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].UpdateSize();

                // Turn off blade completely if at minimum length and currently active
                if (blades[i].currentLength <= blades[i].minLength && (blades[i].saberBody.enabled)) {
                    Utils.StopSoundLoop(blades[i].idleSoundSource, ref blades[i].idleSoundNoise);
                    ResetCollisions();
                    blades[i].SetComponentState(false);
                }
            }

            if (playerHand && thrown && (PlayerControl.GetHand(playerHand.side).gripPressed || tapReturning) && !item.isGripped && telekinesis == null) {
                // forget hand if hand is currently holding something
                if (!playerHand.ragdollHand || playerHand.ragdollHand.grabbedHandle || playerHand.ragdollHand.caster.telekinesis?.catchedHandle) playerHand = null;
                else {
                    if (!returning) {
                        var handToPlay = (playerHand.side == Side.Left) ? GlobalSettings.HandAudioLeft : GlobalSettings.HandAudioRight;
                        handToPlay.PlayOneShot(GlobalSettings.SaberRecallSound.PickAudioClip());
                        if (GlobalSettings.SaberActivateOnRecall) TurnOn();
                        Unpenetrate();
                        returning = true;
                    }
                    ReturnSaber();
                }
            }

            if (isHolding && telekinesis == null) {
                thrown = false;
                returning = false;
                if (GlobalSettings.SaberExpensiveCollisions) body.collisionDetectionMode = (body.velocity.magnitude > GlobalSettings.SaberExpensiveCollisionsMinVelocity) ? (CollisionDetectionMode)module.fastCollisionMode : CollisionDetectionMode.ContinuousSpeculative;
            } else if (telekinesis != null && telekinesis.spinMode && !isActive) {
                TurnOn();
                telekinesis.SetSpinMode(false);
            }

            if (ignoreCrystalTime > 0) ignoreCrystalTime -= Time.deltaTime;
            if (ignoreCoupleTime > 0) ignoreCoupleTime -= Time.deltaTime;

            if (primaryControlHoldTime > 0) {
                primaryControlHoldTime -= Time.deltaTime;
                if (primaryControlHoldTime <= 0) ExecuteAction(module.primaryGripPrimaryActionHold);
            }
            if (secondaryControlHoldTime > 0) {
                secondaryControlHoldTime -= Time.deltaTime;
                if (secondaryControlHoldTime <= 0) ExecuteAction(module.primaryGripSecondaryActionHold);
            }

            if (GlobalSettings.SaberDeactivateOnDrop && deactivateOnDropTime > 0) {
                deactivateOnDropTime -= Time.deltaTime;
                if (deactivateOnDropTime <= 0) TurnOff();
            }
        }

        protected override void ManagedFixedUpdate() {
            if (isHelicoptering && isActive) {
                float thrustLeft = 0;
                float thrustRight = 0;
                if (leftInteractor) thrustLeft = PlayerControl.GetHand(leftInteractor.side).useAxis;
                if (rightInteractor) thrustRight = PlayerControl.GetHand(rightInteractor.side).useAxis;
                float maxThrust = Mathf.Max(thrustLeft, thrustRight);
                if (!playerBody) playerBody = Player.local.locomotion.physicBody.rigidBody;
                playerBody.AddForce(itemTrans.right * Mathf.Lerp(module.helicopterThrust[0], module.helicopterThrust[1], maxThrust), ForceMode.Force);

                for (int i = 0, l = animators.Count(); i < l; i++) {
                    animators[i].speed = 1 + maxThrust;
                }
            }

            if (coupledItem) {
                coupledItem.transform.MoveAlign(coupledItem.holderPoint, couplerTrans, couplerTrans);

                if (!item.IsHanded() && coupledItem.IsHanded()) ForceCollisionDetection(item);
                if (item.IsHanded() && !coupledItem.IsHanded()) ForceCollisionDetection(coupledItem);
            }

            if (fixBrokenPhysics) {
                ResetCollisions();
                fixBrokenPhysics = false;
            }
        }

        void ForceCollisionDetection(Item item) {
            item.isThrowed = true;
            item.IgnoreIsMoving();
            item.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem), false);
            item.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.grabbed;
            if (!Item.allThrowed.Contains(item)) {
                Item.allThrowed.Add(item);
            }
        }

        void ReturnSaber() {
            item.Throw(0);
            var hand = PlayerControl.GetHand(playerHand.side);
            item.IgnoreRagdollCollision(playerHand.ragdollHand.creature.ragdoll);

            float grabDistance = 0.3f;
            float returnSpeed = 10f;
            if (!tapToReturn) {
                var gripAxis = (hand.middleCurl + hand.ringCurl + hand.littleCurl) / 3f;
                returnSpeed *= gripAxis;
            }
            tapReturning = tapToReturn;

            if (Vector3.SqrMagnitude(itemTrans.position - playerHand.transform.position) < grabDistance * grabDistance) {
                playerHand.ragdollHand.TryRelease();
                playerHand.ragdollHand.Grab(item.mainHandleRight);
                tapReturning = false;
            } else {
                body.velocity = (playerHand.transform.position - body.position) * returnSpeed;
            }
        }
    }

    [Serializable]
    public class SavedLightsaber {
        public string itemId;
        public ItemLightsaberSaveData saveData;
    }

    [Serializable]
    public class LightsaberBlade {
        // sets blade length in metres - defaults to default mesh size if blank
        public float bladeLength;
        public float[] phaseLengths;
        public float audioPitch = 1f;
        public float glowIntensity = 1f;

        // custom reference strings
        public string kyberCrystal;
        public string crystalEjectRef;
        public string collisionRef;
        public string saberBodyRef;
        public string saberTipGlowRef;
        public string saberGlowRef;
        public string saberParticlesRef;
        public string startSoundsRef;
        public string stopSoundsRef;
        public string whooshRef;

        // primary objects for Lightsaber to interact with
        public LightsaberTrail trail;
        public Transform crystalEject;
        public MeshRenderer saberBody;
        public MeshRenderer saberGlow;
        public MeshRenderer trailMeshRenderer;
        public Light saberGlowLight;
        public Light saberTipGlow;
        public ParticleSystem saberParticles;
        public ParticleSystem unstableParticles;
        public WhooshPoint whooshBlade;
        public Collider collisionBlade;
        public AudioContainer idleSound;
        public AudioContainer startSound;
        public AudioContainer stopSound;
        public AudioSource idleSoundSource;
        public AudioSource startSoundSource;
        public AudioSource stopSoundSource;
        public NoiseManager.Noise idleSoundNoise;

        // internal properties
        public Item parent;
        public float currentLength;
        public int currentPhase;
        public float minLength;
        public float maxLength;
        public float extendDelta;
        public float ignitionDuration;
        public bool isActive;
        public bool isUnstable;
        float originalWhooshMaxVel;
        float originalWhooshMinVel;
        Vector3 originalSaberBodyTransLocalScale;
        bool trailRescaleZ;

        public Transform saberBodyTrans;

        MaterialInstance _saberBodyMaterialInstance;
        public MaterialInstance saberBodyMaterialInstance {
            get {
                if (_saberBodyMaterialInstance == null) {
                    saberBody.gameObject.TryGetOrAddComponent(out MaterialInstance mi);
                    _saberBodyMaterialInstance = mi;
                }
                return _saberBodyMaterialInstance;
            }
        }

        MaterialInstance _trailMaterialInstance;
        public MaterialInstance trailMaterialInstance {
            get {
                if (_trailMaterialInstance == null) {
                    trailMeshRenderer.gameObject.TryGetOrAddComponent(out MaterialInstance mi);
                    _trailMaterialInstance = mi;
                }
                return _trailMaterialInstance;
            }
        }

        public void Initialise(Item parent, float ignitionDuration, string kyberCrystalOverride = null, float bladeLengthOverride = 0) {
            this.parent = parent;
            this.ignitionDuration = ignitionDuration;
            if (!string.IsNullOrEmpty(collisionRef)) {
                collisionBlade = parent.GetCustomReference(collisionRef).GetComponent<Collider>();
                collisionBlade.enabled = true;
                collisionBlade.enabled = isActive;
            }
            if (!string.IsNullOrEmpty(crystalEjectRef)) crystalEject = parent.GetCustomReference(crystalEjectRef);
            if (!string.IsNullOrEmpty(startSoundsRef)) startSoundSource = parent.GetCustomReference(startSoundsRef).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(stopSoundsRef)) stopSoundSource = parent.GetCustomReference(stopSoundsRef).GetComponent<AudioSource>();
            if (!string.IsNullOrEmpty(saberBodyRef)) {
                var tempSaberBody = parent.GetCustomReference(saberBodyRef);
                saberBody = tempSaberBody.GetComponent<MeshRenderer>();
                parent.renderers.Remove(saberBody);
                saberBodyTrans = tempSaberBody.transform;
                originalSaberBodyTransLocalScale = saberBodyTrans.localScale;
                UpdateBladeThickness(GlobalSettings.SaberBladeThickness);
                idleSoundSource = tempSaberBody.GetComponent<AudioSource>();
                var tempUnstable = saberBodyTrans.Find("UnstableParticles");
                if (tempUnstable) unstableParticles = tempUnstable.GetComponent<ParticleSystem>();
                var trailTrans = saberBodyTrans.Find("Trail");
                trail = trailTrans.gameObject.AddComponent<LightsaberTrail>();
                trailMeshRenderer = trailTrans.gameObject.GetComponent<MeshRenderer>();

                trailRescaleZ = saberBodyTrans.parent != parent.transform;
            }
            if (!string.IsNullOrEmpty(saberTipGlowRef)) saberTipGlow = parent.GetCustomReference(saberTipGlowRef).GetComponent<Light>();
            if (!string.IsNullOrEmpty(saberGlowRef)) {
                var saberGlowTrans = parent.GetCustomReference(saberGlowRef);
                saberGlowLight = saberGlowTrans.GetComponent<Light>();
                var renderer = saberGlowTrans.GetComponent<MeshRenderer>();
                if (renderer) {
                    renderer.enabled = false;
                    parent.renderers.Remove(renderer);
                }
                UnityEngine.Object.Destroy(saberGlowTrans.GetComponent<MeshFilter>());
            }
            if (!string.IsNullOrEmpty(saberParticlesRef)) saberParticles = parent.GetCustomReference(saberParticlesRef).GetComponent<ParticleSystem>();
            if (!string.IsNullOrEmpty(whooshRef)) {
                whooshBlade = parent.GetCustomReference(whooshRef).GetComponent<WhooshPoint>();
                originalWhooshMaxVel = whooshBlade.maxVelocity;
                originalWhooshMinVel = whooshBlade.minVelocity;
            }
            float initialBladeLength = (bladeLengthOverride > 0) ? bladeLengthOverride : bladeLength;
            SetBladeLength((initialBladeLength > 0f) ? (initialBladeLength / saberBodyTrans.parent.localScale.z * 0.1f) : saberBodyTrans.localScale.z);
            extendDelta = -extendDelta;
            kyberCrystal = kyberCrystalOverride ?? kyberCrystal;
            AddCrystal(kyberCrystal);

            SetComponentState(false);

            // Always set this to false now, it is only used for item previews
            if (saberGlow) saberGlow.enabled = false;
        }

        public void SetComponentState(bool state) {
            if (whooshBlade) {
                whooshBlade.maxVelocity = state ? originalWhooshMaxVel : float.MaxValue;
                whooshBlade.minVelocity = state ? originalWhooshMinVel : float.MaxValue;
            }
            if (saberBody) saberBody.enabled = state;
            if (saberGlowLight) saberGlowLight.enabled = state;
            if (saberTipGlow) saberTipGlow.enabled = state;
            if (saberParticles) {
                if (state) saberParticles.Play();
                else {
                    saberParticles.Stop();
                    saberParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); // destroy leftover beam particles
                }
            }
            if (unstableParticles) {
                if (state && isUnstable) {
                    unstableParticles.gameObject.SetActive(true);
                    unstableParticles.Play();
                } else {
                    unstableParticles.Stop();
                    unstableParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); // destroy leftover beam particles
                    unstableParticles.gameObject.SetActive(false);
                }
            }
            if (trail) {
                trail.enabled = state;
                trailMeshRenderer.enabled = state;
            }
        }

        public void NextPhase() {
            currentPhase = (currentPhase >= phaseLengths.Length - 1) ? -1 : currentPhase;
            SetBladeLength(phaseLengths[++currentPhase] * 0.1f);
        }

        public void SetBladeLength(float length) {
            maxLength = length;
            if (trail) trail.height = length * saberBodyTrans.parent.localScale.z;
            CalculateUnstableParticleSize();
            extendDelta = maxLength / ignitionDuration;
        }

        public void UpdateBladeThickness(float thickness) {
            saberBodyTrans.localScale = new Vector3(originalSaberBodyTransLocalScale.x * thickness, originalSaberBodyTransLocalScale.y * thickness, originalSaberBodyTransLocalScale.z);
        }

        public void CalculateUnstableParticleSize() {
            if (unstableParticles) {
                var main = unstableParticles.main;
                main.startLifetimeMultiplier = 33.333f * maxLength * saberBodyTrans.parent.localScale.z;
                var shape = unstableParticles.GetComponentInChildren<ParticleSystem>().shape;
                shape.scale = new Vector3(shape.scale.x, maxLength, shape.scale.y);
            }
        }

        public void AddCrystal(string kyberCrystalId) {
            if (!string.IsNullOrEmpty(kyberCrystalId)) {
                var kyberCrystalData = Catalog.GetData<ItemData>(kyberCrystalId, true);
                if (kyberCrystalData == null) return;
                kyberCrystalData.SpawnAsync((Item item) => {
                    AddCrystal(item.GetComponent<ItemKyberCrystal>());
                });  
            }
        }

        public void AddCrystal(ItemKyberCrystal kyberCrystalObject) {
            if (!saberBody) return;
            saberBodyMaterialInstance.material.SetColor("_GlowColor", kyberCrystalObject.bladeColour);
            saberBodyMaterialInstance.material.SetColor("_Color", kyberCrystalObject.coreColour);
            saberBodyMaterialInstance.material.SetFloat("_InnerGlow", kyberCrystalObject.module.innerGlow);
            saberBodyMaterialInstance.material.SetFloat("_OuterGlow", kyberCrystalObject.module.outerGlow);
            saberBodyMaterialInstance.material.SetFloat("_CoreRadius", kyberCrystalObject.module.coreRadius);
            saberBodyMaterialInstance.material.SetFloat("_CoreStrength", kyberCrystalObject.module.coreStrength);
            saberBodyMaterialInstance.material.SetFloat("_Flicker", kyberCrystalObject.module.flicker);
            saberBodyMaterialInstance.material.SetFloat("_FlickerSpeed", kyberCrystalObject.module.flickerSpeed);
            saberBodyMaterialInstance.material.SetFloatArray("_FlickerScale", kyberCrystalObject.module.flickerScale);

            if (trailMeshRenderer) {
                trailMaterialInstance.material.SetColor("_GlowColor", kyberCrystalObject.bladeColour);
                trailMaterialInstance.material.SetColor("_Color", kyberCrystalObject.coreColour);
                trailMaterialInstance.material.SetFloat("_InnerGlow", kyberCrystalObject.module.innerGlow);
                trailMaterialInstance.material.SetFloat("_OuterGlow", kyberCrystalObject.module.outerGlow * (kyberCrystalObject.module.innerGlow <= 0.01 ? 0.5f : 1f));
                trailMaterialInstance.material.SetFloat("_CoreRadius", kyberCrystalObject.module.innerGlow <= 0.01 ? 0 : kyberCrystalObject.module.coreRadius);
                trailMaterialInstance.material.SetFloat("_CoreStrength", kyberCrystalObject.module.innerGlow <= 0.01 ? 0 : kyberCrystalObject.module.coreStrength);
                trailMaterialInstance.material.SetFloat("_Flicker", kyberCrystalObject.module.flicker);
                trailMaterialInstance.material.SetFloat("_FlickerSpeed", kyberCrystalObject.module.flickerSpeed);
                trailMaterialInstance.material.SetFloatArray("_FlickerScale", kyberCrystalObject.module.flickerScale);
            }

            if (unstableParticles) {
                var main = unstableParticles.main;
                main.startColor = kyberCrystalObject.bladeColour;
            }

            if (saberGlowLight) {
                saberGlowLight.color = kyberCrystalObject.glowColour;
                saberGlowLight.intensity = kyberCrystalObject.module.glowIntensity * glowIntensity * 0.1f;
                saberGlowLight.range = kyberCrystalObject.module.glowRange;
            }

            if (saberTipGlow) {
                saberTipGlow.color = kyberCrystalObject.glowColour;
                saberTipGlow.intensity = kyberCrystalObject.module.glowIntensity * glowIntensity;
                saberTipGlow.range = kyberCrystalObject.module.glowRange;
            }

            isUnstable = kyberCrystalObject.module.isUnstable;

            if (idleSoundSource) {
                idleSound = kyberCrystalObject.module.idleSoundAsset;
                idleSoundSource.volume = kyberCrystalObject.module.idleSoundVolume;
                idleSoundSource.pitch = kyberCrystalObject.module.idleSoundPitch * audioPitch;
            }
            if (startSoundSource) {
                startSound = kyberCrystalObject.module.startSoundAsset;
                startSoundSource.volume = kyberCrystalObject.module.startSoundVolume;
                startSoundSource.pitch = kyberCrystalObject.module.startSoundPitch * audioPitch;
            }

            if (stopSoundSource) {
                stopSound = kyberCrystalObject.module.stopSoundAsset;
                stopSoundSource.volume = kyberCrystalObject.module.stopSoundVolume;
                stopSoundSource.pitch = kyberCrystalObject.module.stopSoundPitch * audioPitch;
            }

            if (!string.IsNullOrEmpty(whooshRef)) {
                var fxData = kyberCrystalObject.module.whoosh;
                fxData.effectGroupData.globalMaxPitch *= audioPitch;
                fxData.effectGroupData.globalMinPitch *= audioPitch;
                ItemData.Whoosh whoosh = new ItemData.Whoosh {
                    minVelocity = whooshBlade.minVelocity,
                    maxVelocity = whooshBlade.maxVelocity,
                    stopOnSnap = whooshBlade.stopOnSnap,
                    trigger = whooshBlade.trigger
                };
                whooshBlade.Load(fxData, whoosh);
                fxData.effectGroupData.globalMaxPitch /= audioPitch;
                fxData.effectGroupData.globalMinPitch /= audioPitch;
            }

            kyberCrystal = kyberCrystalObject.item.data.id;
            var tempItem = kyberCrystalObject.GetComponent<Item>();
            if (tempItem.mainHandler) tempItem.mainHandler.TryRelease();
            tempItem.Despawn();
        }

        public void RemoveCrystal() {
            if (!string.IsNullOrEmpty(kyberCrystal)) {
                var kyberCrystalData = Catalog.GetData<ItemData>(kyberCrystal, true);
                if (kyberCrystalData == null) return;
                kyberCrystalData.SpawnAsync(item => {}, crystalEject.position, crystalEject.rotation);
                kyberCrystal = "";
            }
        }

        public void TurnOn(bool playSound = true) {
            if (!string.IsNullOrEmpty(kyberCrystal)) {
                isActive = true;
                if (playSound) Utils.PlaySoundOneShot(startSoundSource, startSound, parent);
                idleSoundNoise = Utils.PlaySoundLoop(idleSoundSource, idleSound, parent);
                SetComponentState(true);
            }
        }

        public void TurnOff(bool playSound = true) {
            if (!string.IsNullOrEmpty(kyberCrystal)) {
                isActive = false;
                if (playSound) Utils.PlaySoundOneShot(stopSoundSource, stopSound, parent);
            }
        }

        public void UpdateBladeDirection() {
            extendDelta = (!isActive | currentLength > maxLength) ? -Mathf.Abs(extendDelta) : Mathf.Abs(extendDelta);
        }

        public void UpdateSize() {
            // if blade is currently longer than expected (going from long to short blade phase)
            if (isActive && currentLength > maxLength) {
                UpdateBladeDirection();
                currentLength = Mathf.Clamp(currentLength + (extendDelta * Time.deltaTime), maxLength, currentLength);
                saberBodyTrans.localScale = new Vector3(saberBodyTrans.localScale.x, saberBodyTrans.localScale.y, currentLength);
                if (trail) trail.height = currentLength * 10 * (trailRescaleZ ? saberBodyTrans.parent.localScale.z : 1f);
                return;
            }

            // if blade still extending or retracting
            if ((isActive && (currentLength < maxLength || currentLength > maxLength)) || (!isActive && currentLength > minLength)) {
                UpdateBladeDirection();
                currentLength = Mathf.Clamp(currentLength + (extendDelta * Time.deltaTime), minLength, maxLength);
                saberBodyTrans.localScale = new Vector3(saberBodyTrans.localScale.x, saberBodyTrans.localScale.y, currentLength);
                if (trail) trail.height = currentLength * 10 * (trailRescaleZ ? saberBodyTrans.parent.localScale.z : 1f);
                return;
            }
        }

        public bool TryPenetrate() {
            if (!isActive) return false;
            if (Physics.Raycast(saberBodyTrans.position, -saberBodyTrans.forward, bladeLength, 201334784, QueryTriggerInteraction.Ignore)) {
                parent.physicBody.rigidBody.AddForce(saberBodyTrans.forward * 20f, ForceMode.Impulse);
                return true;
            }
            return false;
        }

        /* 
         * The following Lightsaber Trail code has been adapted from "PocketRPG weapon trail"
         * Source: https://www.assetstore.unity3d.com/en/#!/content/2458
         */
        public class TronTrailSection {
            public Vector3 point;
            public Vector3 upDir;
            public float time;
            public TronTrailSection() { }
            public TronTrailSection(Vector3 p, float t) {
                point = p;
                time = t;
            }
        }

        public class LightsaberTrail : ThunderBehaviour {
            public override ManagedLoops EnabledManagedLoops => ManagedLoops.FixedUpdate;

            public float height = 0.9f;
            public float time;
            public float timeTransitionSpeed = 5f;
            public Color startColor = Color.white;
            public Color endColor = new Color(1, 1, 1, 0);

            Vector3 position;
            Vector3 lastRotation;
            float now;
            TronTrailSection currentSection;
            Matrix4x4 localSpaceTransform;

            Transform trans;
            Mesh mesh;
            Vector3[] vertices;
            Color[] colors;
            Vector2[] uv;

            public List<TronTrailSection> sections = new List<TronTrailSection>();

            protected void Awake() {
                MeshFilter meshF = GetComponent<MeshFilter>();
                mesh = meshF.mesh;
                trans = transform;
            }

            protected override void ManagedFixedUpdate() {
                if (GlobalSettings.SaberTrailEnabled) {
                    Iterate(Time.time);
                    UpdateTrail(Time.time, Time.deltaTime);
                } else {
                    mesh.Clear();
                    sections.Clear();
                }
            }

            public void Iterate(float itterateTime) { // ** call everytime you sample animation **
                position = trans.position;
                now = itterateTime;

                var velocity = (lastRotation - trans.rotation.eulerAngles).sqrMagnitude;
                lastRotation = trans.rotation.eulerAngles;

                if (sections.Count == 0 || velocity > 0) {
                    TronTrailSection section = new TronTrailSection {
                        point = position,
                        upDir = trans.TransformDirection(Vector3.up),
                        time = now
                    };
                    sections.Insert(0, section);
                }
            }

            public void UpdateTrail(float currentTime, float deltaTime) {
                mesh.Clear();

                while (sections.Count > 0 && currentTime > sections[sections.Count - 1].time + GlobalSettings.SaberTrailDuration) {
                    sections.RemoveAt(sections.Count - 1);
                }

                if (sections.Count < 2) return;

                vertices = new Vector3[sections.Count * 2];
                colors = new Color[sections.Count * 2];
                uv = new Vector2[sections.Count * 2];

                currentSection = sections[0];
                localSpaceTransform = trans.worldToLocalMatrix;

                for (var i = 0; i < sections.Count; i++) {
                    currentSection = sections[i];
                    float u = 0.0f;
                    if (i != 0) u = Mathf.Clamp01((currentTime - currentSection.time) / GlobalSettings.SaberTrailDuration);

                    Vector3 upDir = currentSection.upDir;

                    vertices[i * 2 + 0] = localSpaceTransform.MultiplyPoint(currentSection.point);
                    vertices[i * 2 + 1] = localSpaceTransform.MultiplyPoint(currentSection.point + upDir * height);

                    uv[i * 2 + 0] = new Vector2(u, 0);
                    uv[i * 2 + 1] = new Vector2(u, 1);

                    Color interpolatedColor = Color.Lerp(startColor, endColor, (u - 0.3f) * 10f);
                    colors[i * 2 + 0] = interpolatedColor;
                    colors[i * 2 + 1] = interpolatedColor;
                }

                int[] triangles = new int[(sections.Count - 1) * 2 * 3];
                for (int i = 0; i < triangles.Length / 6; i++) {
                    triangles[i * 6 + 0] = i * 2;
                    triangles[i * 6 + 1] = i * 2 + 1;
                    triangles[i * 6 + 2] = i * 2 + 2;

                    triangles[i * 6 + 3] = i * 2 + 2;
                    triangles[i * 6 + 4] = i * 2 + 1;
                    triangles[i * 6 + 5] = i * 2 + 3;
                }

                mesh.vertices = vertices;
                mesh.colors = colors;
                mesh.uv = uv;
                mesh.triangles = triangles;

                if (time > GlobalSettings.SaberTrailDuration) {
                    time -= deltaTime * timeTransitionSpeed;
                    if (time <= GlobalSettings.SaberTrailDuration) time = GlobalSettings.SaberTrailDuration;
                } else if (time < GlobalSettings.SaberTrailDuration) {
                    time += deltaTime * timeTransitionSpeed;
                    if (time >= GlobalSettings.SaberTrailDuration) time = GlobalSettings.SaberTrailDuration;
                }
            }
        }
    }

    internal class LightsaberNPCAnimator : ThunderBehaviour {
        public Creature creature;
        float originalSpeed;

        public void SetCreature(Creature newCreature) {
            creature = newCreature;
            if (creature.brain.instance.id == "ForceSensitive") {
                if (creature.animator) {
                    originalSpeed = creature.animator.speed;
                    creature.animator.speed = originalSpeed * GlobalSettings.SaberNPCAttackSpeed;
                }
            }
            creature.OnKillEvent += OnKillEvent;
        }

        void OnKillEvent(CollisionInstance collisionStruct, EventTime eventTime) {
            creature.OnKillEvent -= OnKillEvent;
            Destroy(this);
        }

        protected void OnDestroy() {
            try {
                if (creature.brain.instance.id == "ForceSensitive") {
                    creature.animator.speed = originalSpeed;
                }
            } catch {}
        }
    }
}