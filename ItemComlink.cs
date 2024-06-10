using UnityEngine;
using ThunderRoad;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TOR {
    [Serializable]
    public class ItemComlinkSaveData : ContentCustomData {
        public int faction = 0;
        public int target = 0;
    }

    public class ItemComlink : ThunderBehaviour {
        public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

        internal Item item;
        internal ItemModuleComlink module;

        int currentFaction;
        int currentTarget;
        FactionData factionData;
        ReinforcementData reinforcementData;
        CreatureTable creatureTable;

        AudioSource idleSound;
        AudioSource pingSound;
        AudioSource startSound;
        AudioSource stopSound;
        AudioSource useSound;
        GameObject hologram;
        Text text;
        Material[] materials;
        MeshRenderer hologramLight;
        MeshRenderer hologramLogo;
        Light light;

        NoiseManager.Noise idleNoise;

        float primaryControlHoldTime;
        float secondaryControlHoldTime;
        RagdollHand leftInteractor;
        RagdollHand rightInteractor;

        readonly List<Transform> spawnLocations = new List<Transform>();

        MaterialPropertyBlock _propBlock;
        public MaterialPropertyBlock PropBlock {
            get {
                _propBlock = _propBlock ?? new MaterialPropertyBlock();
                return _propBlock;
            }
        }

        protected void Awake() {
            item = GetComponent<Item>();
            module = item.data.GetModule<ItemModuleComlink>();

            item.OnHeldActionEvent += OnHeldAction;
            item.OnGrabEvent += OnGrabEvent;
            item.OnUngrabEvent += OnUngrabEvent;
            item.OnSnapEvent += OnSnapEvent;

            idleSound = item.GetCustomReference("IdleSound").GetComponent<AudioSource>();
            pingSound = item.GetCustomReference("PingSound").GetComponent<AudioSource>();
            startSound = item.GetCustomReference("StartSound").GetComponent<AudioSource>();
            stopSound = item.GetCustomReference("StopSound").GetComponent<AudioSource>();
            useSound = item.GetCustomReference("UseSound").GetComponent<AudioSource>();

            hologram = item.GetCustomReference("Hologram").gameObject;
            hologramLight = item.GetCustomReference("HologramLight").GetComponent<MeshRenderer>();
            hologramLogo = item.GetCustomReference("HologramLogo").GetComponent<MeshRenderer>();
            light = item.GetCustomReference("Light").GetComponent<Light>();
            text = item.GetCustomReference("Text").GetComponent<Text>();
            materials = item.GetCustomReference("Materials").GetComponent<MeshRenderer>().materials;

            hologram.SetActive(false);
            var waveSpawners = new List<WaveSpawner>(FindObjectsOfType<WaveSpawner>());
            foreach (var spawner in waveSpawners) {
                spawnLocations.AddRange(spawner.spawns);
            }

            var creatureSpawners = new List<CreatureSpawner>(FindObjectsOfType<CreatureSpawner>());
            foreach (var spawner in waveSpawners) {
                spawnLocations.AddRange(spawner.spawns);
            }

            item.TryGetCustomData<ItemComlinkSaveData>(out var savedData);
            if (savedData != null) {
                currentFaction = savedData.faction;
                currentTarget = savedData.target;
            }

            factionData = module.factions[currentFaction];
            reinforcementData = module.reinforcements[currentTarget];
            creatureTable = Catalog.GetData<CreatureTable>(reinforcementData.creatureTable, true);
            SetGraphic();
            SetColour();
        }

        public void UpdateCustomData() {
            Utils.UpdateCustomData(item, new ItemComlinkSaveData {
                faction = currentFaction,
                target = currentTarget
            });
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            hologram.SetActive(true);
            Utils.PlaySound(startSound, null, item);
            idleNoise = Utils.PlaySoundLoop(idleSound, null, item);
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            hologram.SetActive(false);
            Utils.PlaySound(stopSound, null, item);
            Utils.StopSoundLoop(idleSound, ref idleNoise);
        }

        public void OnSnapEvent(Holder holder) {
            UpdateCustomData();
        }

        public void ExecuteAction(string action, RagdollHand interactor = null) {
            if (action == "nextFaction") {
                CycleFaction(interactor, true);
            } else if (action == "prevFaction") {
                CycleFaction(interactor, false);
            } else if (action == "nextTarget") {
                CycleTarget(interactor, true);
            } else if (action == "prevTarget") {
                CycleTarget(interactor, false);
            } else if (action == "summonTarget") {
                SummonTarget();
            }
        }

        public void CycleFaction(RagdollHand interactor = null, bool inc = true) {
            if (inc) currentFaction = (currentFaction >= module.factions.Count - 1) ? 0 : currentFaction + 1;
            else currentFaction = (currentFaction <= 0) ? module.factions.Count - 1 : currentFaction - 1;
            factionData = module.factions[currentFaction];
            SetColour();
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
            Utils.PlaySound(useSound, module.useSoundAsset, item);
        }

        public void CycleTarget(RagdollHand interactor = null, bool inc = true) {
            if (inc) currentTarget = (currentTarget >= module.reinforcements.Count - 1) ? 0 : currentTarget + 1;
            else currentTarget = (currentTarget <= 0) ? module.reinforcements.Count - 1 : currentTarget - 1;
            reinforcementData = module.reinforcements[currentTarget];
            creatureTable = Catalog.GetData<CreatureTable>(reinforcementData.creatureTable, true);
            SetGraphic();
            SetColour();
            Utils.PlayHaptic(interactor, Utils.HapticIntensity.Minor);
            Utils.PlaySound(useSound, module.useSoundAsset, item);
        }

        public void SetGraphic() {
            hologramLogo.material = materials[(int)reinforcementData.hologramLogo];
            text.text = reinforcementData.description;
        }

        public void SetColour() {
            var newColour = Color.HSVToRGB(factionData.colour[0], factionData.colour[1], 1);
            hologramLogo.GetPropertyBlock(PropBlock);
            PropBlock.SetColor("Colour", newColour);
            hologramLogo.SetPropertyBlock(PropBlock);
            
            hologramLight.GetPropertyBlock(PropBlock);
            PropBlock.SetColor("Colour", newColour);
            hologramLight.SetPropertyBlock(PropBlock);

            light.color = newColour;
            text.color = newColour;
            text.material.SetColor("Colour", newColour);
        }

        public void SummonTarget() {
            Vector3 currentPos = transform.position;
            Transform closestSpawn = null;
            Vector3 spawnPos = Vector3.zero;
            float spawnRot = 0;

            float closestDistance = float.MaxValue;

            if (spawnLocations.Count > 0) {
                foreach (var spawn in spawnLocations) {
                    if (!closestSpawn) {
                        closestSpawn = spawn;
                        continue;
                    }
                    var distance = Mathf.Abs((currentPos - spawn.position).sqrMagnitude);
                    if (distance < closestDistance) {
                        closestSpawn = spawn;
                        closestDistance = distance;
                    }
                }
                spawnPos = closestSpawn.position;
                spawnRot = closestSpawn.rotation.y;
            } else if (Player.local) {
                var head = Player.local.head.transform;
                spawnPos = head.position + head.forward * 2f;
                spawnRot = head.rotation.eulerAngles.y + 180f;
            }

            if (creatureTable != null && spawnPos != Vector3.zero) {
                if (creatureTable.TryPick(out CreatureData creatureData)) {
                    StartCoroutine(creatureData.SpawnCoroutine(spawnPos, spawnRot, null, delegate (Creature creature) {
                        if (factionData.factionId != -999) creature.SetFaction(factionData.factionId);
                        if (creature.factionId == Player.currentCreature.factionId) {
                            creature.brain.instance.tree.blackboard.UpdateVariable("FollowTarget", Player.currentCreature);
                        } else {
                            creature.spawnGroup = new WaveData.Group();
                        }
                    }, true));
                    Utils.PlayHaptic(leftInteractor, rightInteractor, Utils.HapticIntensity.Moderate);
                    Utils.PlaySound(pingSound, null, item);
                }
            }
        }

        public void OnHeldAction(RagdollHand interactor, Handle handle, Interactable.Action action) {
            // If primary hold action available
            if (!string.IsNullOrEmpty(module.gripPrimaryActionHold)) {
                // start primary control timer
                if (action == Interactable.Action.UseStart) {
                    primaryControlHoldTime = GlobalSettings.ControlsHoldDuration;
                } else if (action == Interactable.Action.UseStop) {
                    // if not held for long run standard action
                    if (primaryControlHoldTime > 0 && primaryControlHoldTime > (primaryControlHoldTime / 2)) {
                        ExecuteAction(module.gripPrimaryAction, interactor);
                    }
                    primaryControlHoldTime = 0;
                }
            } else if (action == Interactable.Action.UseStart) ExecuteAction(module.gripPrimaryAction, interactor);

            // If secondary hold action available
            if (!string.IsNullOrEmpty(module.gripSecondaryActionHold)) {
                // start secondary control timer
                if (action == Interactable.Action.AlternateUseStart) {
                    secondaryControlHoldTime = GlobalSettings.ControlsHoldDuration;
                } else if (action == Interactable.Action.AlternateUseStop) {
                    // if not held for long run standard action
                    if (secondaryControlHoldTime > 0 && secondaryControlHoldTime > (secondaryControlHoldTime / 2)) {
                        ExecuteAction(module.gripSecondaryAction, interactor);
                    }
                    secondaryControlHoldTime = 0;
                }
            } else if (action == Interactable.Action.AlternateUseStart) ExecuteAction(module.gripSecondaryAction, interactor);

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

        protected override void ManagedUpdate() {
            if (primaryControlHoldTime > 0) {
                primaryControlHoldTime -= Time.deltaTime;
                if (primaryControlHoldTime <= 0) ExecuteAction(module.gripPrimaryActionHold);
            }
            if (secondaryControlHoldTime > 0) {
                secondaryControlHoldTime -= Time.deltaTime;
                if (secondaryControlHoldTime <= 0) ExecuteAction(module.gripSecondaryActionHold);
            }
        }

        [Serializable]
        public class FactionData {
            public int factionId = 2;
            public float[] colour = { 0.53f, 1 };
        }

        [Serializable]
        public class ReinforcementData {
            public string creatureTable;
            public string description;
            public HologramLogo hologramLogo;
        }

        public enum HologramLogo {
            BlackSun,
            Empire,
            Jedi,
            Mercenary,
            Rebel,
            Republic,
            Sith 
        }
    }
}