using ThunderRoad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;

namespace TOR {
    public class ItemLightsaber : MonoBehaviour {
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
        bool useExpensiveCollisions;
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
        readonly char listSeperator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleLightsaber>();
            body = item.GetComponent<Rigidbody>();
            itemTrans = item.transform;
            originalMass = body.mass;

            if (!string.IsNullOrEmpty(module.animatorId)) animators = item.GetCustomReference(module.animatorId).GetComponentsInChildren<Animator>();

            if (module.hasCoupler) {
                couplerCollider = item.GetCustomReference("CouplerCollider").GetComponent<Collider>();
                couplerTrans = item.GetCustomReference("CouplerHolder");

                item.TryGetSavedValue("coupledLightsaberProps", out string coupledLightsaberProps);
                if (!string.IsNullOrEmpty(coupledLightsaberProps)) {
                    var decodedBytes = Convert.FromBase64String(coupledLightsaberProps);
                    var savedLightsaber = JsonUtility.FromJson<SavedLightsaber>(Encoding.UTF8.GetString(decodedBytes));
                    SpawnCoupledItem(savedLightsaber.itemId);

                    var coupledKyberCrystals = savedLightsaber.kyberCrystals.Split(listSeperator) ?? null;
                    for (int i = 0, l = coupledKyberCrystals.Count(); i < l; i++) {
                        coupledLightsaber.blades[i].AddCrystal(coupledKyberCrystals[i]);
                    }

                    var lengths = savedLightsaber.bladeLengths.Split(listSeperator);
                    for (int i = 0, l = lengths.Count(); i < l; i++) {
                        coupledLightsaber.blades[i].SetBladeLength(float.Parse(lengths[i]) / coupledLightsaber.blades[i].saberBodyTrans.parent.localScale.z * 0.1f);
                    }

                    coupledLightsaber.StoreSaberState();
                }
            }

            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnTelekinesisGrabEvent += OnTeleGrabEvent;
            item.OnTelekinesisReleaseEvent += OnTeleUngrabEvent;
            item.OnHeldActionEvent += OnHeldAction;
            item.OnSnapEvent += OnSnapEvent;
            item.OnUnSnapEvent += OnUnSnapEvent;

            RetrieveSaberState();
            blades = module.lightsaberBlades.Select(x => JsonUtility.FromJson<LightsaberBlade>(JsonUtility.ToJson(x))).ToArray();

            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].Initialise(item, module.ignitionDuration, kyberCrystals.ElementAtOrDefault(i) ?? null, bladeLengths.ElementAtOrDefault(i));
            }

            if (module.startActive) TurnOn(true);
            else TurnOff(false);

            tapToReturn = !GameManager.options.GetController().holdGripForHandles;
            originalWeaponClass = item.data.moduleAI.weaponClass;

            playerBody = Player.local.locomotion.rb;
            StoreSaberState();
        }

        void Destroy() {
            if (GlobalSettings.LightsaberColliders.ContainsKey(GetInstanceID())) {
                GlobalSettings.LightsaberColliders.Remove(GetInstanceID());
            }
        }

        void LoadGlobalSettings() {
            useExpensiveCollisions = GlobalSettings.SaberExpensiveCollisions;
        }

        public void ExecuteAction(string action, RagdollHand interactor = null) {
            if (action == "decouple") Decouple();
            else if (action == "nextPhase") NextPhase(interactor);
            else if (action == "toggleAnimation") ToggleAnimation(interactor);
            else if (action == "toggleHelicopter") ToggleHelicopter(interactor);
            else if (action == "toggleIgnition") ToggleLightsaber(interactor);
            else if (action == "toggleIgnitionAnimated") {
                ToggleLightsaber(interactor);
                ToggleAnimation(interactor);
            } else if (action == "toggleSingle") ToggleSingle(interactor);
            else if (action == "turnOn") TurnOn();
            else if (action == "turnOff") TurnOff();
        }

        public struct AdjustBladeLength {
            public bool allowDisarm;
            public float lengthChange;
        }

        void DecreaseBladeLength(AdjustBladeLength args) {
            if (args.allowDisarm && (item.leftNpcHand || item.rightNpcHand)) return;
            for (int i = 0, l = blades.Count(); i < l; i++) {
                if (blades[i].maxLength - args.lengthChange > 0) {
                    blades[i].SetBladeLength(blades[i].maxLength - args.lengthChange);
                }
            }
            StoreSaberState();
        }

        void IncreaseBladeLength(AdjustBladeLength args) {
            if (args.allowDisarm && (item.leftNpcHand || item.rightNpcHand)) return;
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].SetBladeLength(blades[i].maxLength + args.lengthChange);
            }
            StoreSaberState();
        }

        void ResetBladeLength(AdjustBladeLength args) {
            if (args.allowDisarm && (item.leftNpcHand || item.rightNpcHand)) return;
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].SetBladeLength(module.lightsaberBlades[i].bladeLength / blades[i].saberBodyTrans.parent.localScale.z * 0.1f);
            }
            StoreSaberState();
        }

        void NextPhase(RagdollHand interactor = null) {
            for (int i = 0, l = blades.Count(); i < l; i++) {
                blades[i].NextPhase();
            }
            if (interactor) PlayerControl.GetHand(interactor.playerHand.side).HapticShort(1f);
        }

        public void Couple(Item itemToCouple) {
            if (itemToCouple && (itemToCouple.itemId == item.itemId || module.couplingWhitelist.Contains(itemToCouple.itemId))) {
                coupledItem = itemToCouple;
                coupledItemOriginalSlot = coupledItem.data.slot;
                coupledItem.data.slot = "Cork";
                coupledItem.transform.MoveAlign(coupledItem.holderPoint, couplerTrans, couplerTrans);
                couplerJoint = itemTrans.gameObject.AddComponent<FixedJoint>();
                couplerJoint.connectedBody = coupledItem.rb;
                coupledLightsaber = coupledItem.GetComponent<ItemLightsaber>();
                SetCouplerCollider(false);
                StoreSaberState();
            }
        }

        public void Decouple() {
            if (couplerJoint) Destroy(couplerJoint);
            coupledItem.transform.SetParent(null);
            coupledItem.data.slot = coupledItemOriginalSlot;
            coupledItem = null;
            SetCouplerCollider(true);
            coupledLightsaber = null;
            StoreSaberState();
        }

        public void SetCouplerCollider(bool enabled) {
            couplerCollider.enabled = enabled;
            ignoreCoupleTime = 0.5f;
            if (coupledLightsaber) {
                coupledLightsaber.SetCouplerCollider(enabled);
                coupledLightsaber.ignoreCoupleTime = 1f;
            }
        }

        void SpawnCoupledItem(string itemId) {
            Catalog.GetData<ItemData>(itemId, true).SpawnAsync((Item item) => Couple(item));
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

        readonly WaitForSeconds destabliseGrabbedDelay = new WaitForSeconds(0.1f);
        IEnumerator DestabliseGrabbedCoroutine() {
            Creature GetCreature(Side side) {
                return Player.local?.GetHand(side)?.ragdollHand?.grabbedHandle?.gameObject?.GetComponentInParent<Creature>();
            }

            while (true) {
                yield return destabliseGrabbedDelay;
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
                    if (!firstEnabled || singleAlreadyActive && !blades[i].isActive) blades[i].TurnOn(!blades[i].isActive);
                    else blades[i].TurnOff(blades[i].isActive);
                    firstEnabled = true;
                }
                ResetCollisions();
            }
        }

        void TurnOn(bool playSound = true) {
            if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) {
                isActive = true;

                for (int i = 0, l = blades.Count(); i < l; i++) {
                    blades[i].TurnOn(playSound && !blades[i].isActive);
                }
                ResetCollisions();

                if (GlobalSettings.LightsaberColliders != null && !GlobalSettings.LightsaberColliders.ContainsKey(GetInstanceID())) {
                    GlobalSettings.LightsaberColliders.Add(GetInstanceID(), blades.Select(blade => blade.collisionBlade).ToArray());
                }
            }
        }

        void TurnOff(bool playSound = true) {
            isActive = false;

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
            if (!thrown && body.velocity.magnitude > module.throwSpeed && module.canThrow) {
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

        public void OnTeleUngrabEvent(Handle handle, SpellTelekinesis teleGrabber) {
            telekinesis = null;
            ResetCollisions();

            if (!item.IsHanded()) deactivateOnDropTime = GlobalSettings.SaberDeactivateOnDropDelay;
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action) {
            // If primary hold action available
            if (!string.IsNullOrEmpty(module.primaryGripPrimaryActionHold)) {
                // start primary control timer
                if (action == Interactable.Action.UseStart) {
                    primaryControlHoldTime = module.controlHoldTime;
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
                    secondaryControlHoldTime = module.controlHoldTime;
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

        void TryAddCrystal(ItemKyberCrystal crystal) {
            if (crystal && ignoreCrystalTime <= 0) {
                for (int i = 0, l = blades.Count(); i < l; i++) {
                    if (string.IsNullOrEmpty(blades[i].kyberCrystal)) {
                        blades[i].AddCrystal(crystal);
                        StoreSaberState();
                        break;
                    }
                }
                // Allow AI to use lightsaber again
                if (blades.All(blade => !string.IsNullOrEmpty(blade.kyberCrystal))) item.data.moduleAI.weaponClass = originalWeaponClass;
            }
        }

        void TryEjectCrystal(bool allowDisarm) {
            if (allowDisarm && (item.leftNpcHand || item.rightNpcHand)) return;
            for (int i = 0, l = blades.Count(); i < l; i++) {
                if (!string.IsNullOrEmpty(blades[i].kyberCrystal)) {
                    TurnOff(isActive);
                    item.data.moduleAI.weaponClass = 0; // Tell NPCs not to use lightsaber
                    blades[i].RemoveCrystal();
                    StoreSaberState();
                    ignoreCrystalTime = 0.5f;
                    break;
                }
            }
        }

        void OnTriggerEnter(Collider other) {
            if (other.name == "CouplerCollider" && module.hasCoupler && coupledItem == null && ignoreCoupleTime <= 0 && isSnapped == false) {
                var itemToCouple = other.attachedRigidbody.GetComponentInParent<Item>();
                var lightsaberToCouple = other.attachedRigidbody.GetComponentInParent<ItemLightsaber>();
                if (lightsaberToCouple && !lightsaberToCouple.isSnapped && lightsaberToCouple.coupledItem == null && lightsaberToCouple.item.data.slot != "Cork") {
                    Couple(itemToCouple);
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

        readonly WaitForSeconds unpenetrateLoopDelay = new WaitForSeconds(0.1f);
        IEnumerator UnpenetrateCoroutine() {
            while (true) {
                yield return unpenetrateLoopDelay;
                Unpenetrate();
            }
        }

        protected void Update() {
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
                if (useExpensiveCollisions) body.collisionDetectionMode = (body.velocity.magnitude > module.fastCollisionSpeed) ? (CollisionDetectionMode)module.fastCollisionMode : CollisionDetectionMode.Discrete;
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

        protected void FixedUpdate() {
            if (isHelicoptering && isActive) {
                float thrustLeft = 0;
                float thrustRight = 0;
                if (leftInteractor) thrustLeft = PlayerControl.GetHand(leftInteractor.side).useAxis;
                if (rightInteractor) thrustRight = PlayerControl.GetHand(rightInteractor.side).useAxis;
                float maxThrust = Mathf.Max(thrustLeft, thrustRight);
                if (!playerBody) playerBody = Player.local.locomotion.rb;
                playerBody.AddForce(itemTrans.right * Mathf.Lerp(module.helicopterThrust[0], module.helicopterThrust[1], maxThrust), ForceMode.Force);

                for (int i = 0, l = animators.Count(); i < l; i++) {
                    animators[i].speed = 1 + maxThrust;
                }
            }

            if (coupledItem) {
                coupledItem.transform.MoveAlign(coupledItem.holderPoint, couplerTrans, couplerTrans);
            }

            if (fixBrokenPhysics) {
                ResetCollisions();
                fixBrokenPhysics = false;
            }
        }

        void ReturnSaber() {
            var hand = PlayerControl.GetHand(playerHand.side);
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

        void RetrieveSaberState() {
            item.TryGetSavedValue("kyberCrystals", out string tempCrystals);
            if (!string.IsNullOrEmpty(tempCrystals)) {
                kyberCrystals = tempCrystals.Split(listSeperator);
            }
            item.TryGetSavedValue("bladeLengths", out string tempLengths);
            if (!string.IsNullOrEmpty(tempLengths)) {
                var lengths = tempLengths.Split(listSeperator);
                bladeLengths = new float[lengths.Count()];
                for (int i = 0, l = module.lightsaberBlades.Count(); i < l; i++) {
                    bladeLengths[i] = float.Parse(lengths[i]);
                }
            }
        }

        void StoreSaberState() {
            if (blades != null) {
                item.SetSavedValue("kyberCrystals", string.Join(listSeperator.ToString(), blades.Select(blade => blade.kyberCrystal)));
                item.SetSavedValue("bladeLengths", string.Join(listSeperator.ToString(), blades.Select(blade => (blade.maxLength * blade.saberBodyTrans.parent.localScale.z * 10f).ToString())));
            }
            if (coupledItem) {
                coupledItem.TryGetSavedValue("kyberCrystals", out string tempCrystals);
                coupledItem.TryGetSavedValue("bladeLengths", out string tempLengths);
                var savedLightsaber = new SavedLightsaber {
                    itemId = coupledItem.itemId,
                    kyberCrystals = tempCrystals,
                    bladeLengths = tempLengths
                };
                var bytesToEncode = Encoding.UTF8.GetBytes(JsonUtility.ToJson(savedLightsaber));
                item.SetSavedValue("coupledLightsaberProps", Convert.ToBase64String(bytesToEncode));
            } else {
                item.SetSavedValue("coupledLightsaberProps", null);
            }
        }
    }

    [Serializable]
    public class SavedLightsaber {
        public string itemId;
        public string kyberCrystals;
        public string bladeLengths;
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
        public MaterialPropertyBlock propBlock;
        float originalWhooshMaxVel;
        float originalWhooshMinVel;
        bool trailRescaleZ;

        public Transform saberBodyTrans;

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
                saberBodyTrans = tempSaberBody.transform;
                saberBodyTrans.localScale = new Vector3(saberBodyTrans.localScale.x * GlobalSettings.SaberBladeThickness, saberBodyTrans.localScale.y * GlobalSettings.SaberBladeThickness, saberBodyTrans.localScale.z);
                propBlock = new MaterialPropertyBlock();
                idleSoundSource = tempSaberBody.GetComponent<AudioSource>();
                var tempUnstable = saberBodyTrans.Find("UnstableParticles");
                if (tempUnstable) unstableParticles = tempUnstable.GetComponent<ParticleSystem>();
                if (GlobalSettings.SaberTrailEnabled) {
                    var trailTrans = saberBodyTrans.Find("Trail");
                    trail = trailTrans.gameObject.AddComponent<LightsaberTrail>();
                    trailMeshRenderer = trailTrans.gameObject.GetComponent<MeshRenderer>();

                    trailRescaleZ = saberBodyTrans.parent != parent.transform;
                }
            }
            if (!string.IsNullOrEmpty(saberTipGlowRef)) saberTipGlow = parent.GetCustomReference(saberTipGlowRef).GetComponent<Light>();
            if (!string.IsNullOrEmpty(saberGlowRef)) {
                saberGlow = parent.GetCustomReference(saberGlowRef).GetComponent<MeshRenderer>();
                saberGlowLight = parent.GetCustomReference(saberGlowRef).GetComponent<Light>();
                var mesh = parent.GetCustomReference(saberGlowRef).GetComponent<MeshFilter>();
                UnityEngine.Object.Destroy(mesh);
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
                if (state && isUnstable) unstableParticles.Play();
                else {
                    unstableParticles.Stop();
                    unstableParticles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); // destroy leftover beam particles
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
            saberBody.GetPropertyBlock(propBlock);
            propBlock.SetColor("_GlowColor", kyberCrystalObject.bladeColour);
            propBlock.SetColor("_Color", kyberCrystalObject.coreColour);
            propBlock.SetFloat("_InnerGlow", kyberCrystalObject.module.innerGlow);
            propBlock.SetFloat("_OuterGlow", kyberCrystalObject.module.outerGlow);
            propBlock.SetFloat("_CoreRadius", kyberCrystalObject.module.coreRadius);
            propBlock.SetFloat("_CoreStrength", kyberCrystalObject.module.coreStrength);
            propBlock.SetFloat("_Flicker", kyberCrystalObject.module.flicker);
            propBlock.SetFloat("_FlickerSpeed", kyberCrystalObject.module.flickerSpeed);
            propBlock.SetFloatArray("_FlickerScale", kyberCrystalObject.module.flickerScale);
            saberBody.SetPropertyBlock(propBlock);

            if (trailMeshRenderer) {
                trailMeshRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_GlowColor", kyberCrystalObject.bladeColour);
                propBlock.SetColor("_Color", kyberCrystalObject.coreColour);
                propBlock.SetFloat("_InnerGlow", kyberCrystalObject.module.innerGlow);
                propBlock.SetFloat("_OuterGlow", kyberCrystalObject.module.outerGlow * (kyberCrystalObject.module.innerGlow <= 0.01 ? 0.5f : 1f));
                propBlock.SetFloat("_CoreRadius", kyberCrystalObject.module.innerGlow <= 0.01 ? 0 : kyberCrystalObject.module.coreRadius);
                propBlock.SetFloat("_CoreStrength", kyberCrystalObject.module.innerGlow <= 0.01 ? 0 : kyberCrystalObject.module.coreStrength);
                propBlock.SetFloat("_Flicker", kyberCrystalObject.module.flicker);
                propBlock.SetFloat("_FlickerSpeed", kyberCrystalObject.module.flickerSpeed);
                propBlock.SetFloatArray("_FlickerScale", kyberCrystalObject.module.flickerScale);
                trailMeshRenderer.SetPropertyBlock(propBlock);
            }

            if (saberGlow) {
                saberGlow.material.SetColor("_DiffuseColor", kyberCrystalObject.bladeColour);
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

        public class LightsaberTrail : MonoBehaviour {
            public float height = 0.9f;
            public float time = GlobalSettings.SaberTrailDuration;
            public float desiredTime = GlobalSettings.SaberTrailDuration;
            public float minVelocity = GlobalSettings.SaberTrailMinVelocity;
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

            void Awake() {
                MeshFilter meshF = GetComponent<MeshFilter>();
                mesh = meshF.mesh;
                trans = transform;
            }

            void FixedUpdate() {
                Iterate(Time.time);
                UpdateTrail(Time.time, Time.deltaTime);
            }

            public void Iterate(float itterateTime) { // ** call everytime you sample animation **
                position = trans.position;
                now = itterateTime;

                var velocity = (lastRotation - trans.rotation.eulerAngles).sqrMagnitude;
                lastRotation = trans.rotation.eulerAngles;

                if (sections.Count == 0 || velocity > minVelocity) {
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

                while (sections.Count > 0 && currentTime > sections[sections.Count - 1].time + time) {
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
                    if (i != 0) u = Mathf.Clamp01((currentTime - currentSection.time) / time);

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

                if (time > desiredTime) {
                    time -= deltaTime * timeTransitionSpeed;
                    if (time <= desiredTime) time = desiredTime;
                } else if (time < desiredTime) {
                    time += deltaTime * timeTransitionSpeed;
                    if (time >= desiredTime) time = desiredTime;
                }
            }
        }
    }

    internal class LightsaberNPCAnimator : MonoBehaviour {
        public Creature creature;
        BrainData brain;
        BrainModuleMelee melee;
        bool originalRecoilOnParry;

        public void SetCreature(Creature newCreature) {
            creature = newCreature;
            if (creature.brain.instance.id == "ForceSensitive") {
                if (creature.animator) {
                    creature.animator.speed *= GlobalSettings.SaberNPCAttackSpeed;
                }
                brain = creature.brain.instance;
                melee = brain.GetModule<BrainModuleMelee>();
                if (melee != null) {
                    originalRecoilOnParry = melee.recoilOnParry;
                    if (GlobalSettings.SaberNPCOverrideRecoilOnParry) {
                        melee.recoilOnParry = GlobalSettings.SaberNPCRecoilOnParry;
                    }
                }
            }
            creature.OnKillEvent += OnKillEvent;
        }

        void OnKillEvent(CollisionInstance collisionStruct, EventTime eventTime) {
            creature.OnKillEvent -= OnKillEvent;
            Destroy(this);
        }

        protected void OnDestroy() {
            if (creature && brain != null) {
                if (creature.animator) {
                    creature.animator.speed /= GlobalSettings.SaberNPCAttackSpeed;
                }
                if (melee != null && GlobalSettings.SaberNPCOverrideRecoilOnParry) {
                    melee.recoilOnParry = originalRecoilOnParry;
                }
            }
        }
    }
}