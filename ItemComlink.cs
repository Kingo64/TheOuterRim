using UnityEngine;
using ThunderRoad;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

namespace TOR {
    public class ItemComlink : MonoBehaviour {
        protected Item item;
        protected ItemModuleComlink module;

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

        float primaryControlHoldTime;
        float secondaryControlHoldTime;
        RagdollHand leftInteractor;
        RagdollHand rightInteractor;

        List<SpawnLocation> spawnLocations;

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
            spawnLocations = new List<SpawnLocation>(FindObjectsOfType<SpawnLocation>());

            item.TryGetSavedValue("faction", out string tempFaction);
            item.TryGetSavedValue("target", out string tempTarget);
            int.TryParse(tempFaction, out currentFaction);
            int.TryParse(tempTarget, out currentTarget);

            factionData = module.factions[currentFaction];
            reinforcementData = module.reinforcements[currentTarget];
            creatureTable = Catalog.GetData<CreatureTable>(reinforcementData.creatureTable, true);
            SetGraphic();
            SetColour();
        }

        public void OnGrabEvent(Handle handle, RagdollHand interactor) {
            hologram.SetActive(true);
            startSound.Play();
            idleSound.Play();
        }

        public void OnUngrabEvent(Handle handle, RagdollHand interactor, bool throwing) {
            hologram.SetActive(false);
            stopSound.Play();
            idleSound.Stop();
        }

        public void OnSnapEvent(Holder holder) {
            item.SetSavedValue("faction", currentFaction.ToString());
            item.SetSavedValue("target", currentTarget.ToString());
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
            if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
            useSound.clip = module.useSoundAsset.PickAudioClip();
            useSound.Play();
        }

        public void CycleTarget(RagdollHand interactor = null, bool inc = true) {
            if (inc) currentTarget = (currentTarget >= module.reinforcements.Count - 1) ? 0 : currentTarget + 1;
            else currentTarget = (currentTarget <= 0) ? module.reinforcements.Count - 1 : currentTarget - 1;
            reinforcementData = module.reinforcements[currentTarget];
            creatureTable = Catalog.GetData<CreatureTable>(reinforcementData.creatureTable, true);
            SetGraphic();
            SetColour();
            if (interactor) Utils.PlayHaptic(interactor.side == Side.Left, interactor.side == Side.Right, Utils.HapticIntensity.Minor);
            useSound.clip = module.useSoundAsset.PickAudioClip();
            useSound.Play();
        }

        public void SetGraphic() {
            hologramLogo.material = materials[(int)reinforcementData.hologramLogo];
            text.text = reinforcementData.description;
        }

        public void SetColour() {
            var newColour = Color.HSVToRGB(factionData.colour[0], factionData.colour[1], 1);
            hologramLogo.material.SetColor("Colour", newColour);
            hologramLight.material.SetColor("Colour", newColour);
            light.color = newColour;
            text.color = newColour;
            text.material.SetColor("Colour", newColour);
        }

        public void SummonTarget() {
            Vector3 currentPos = transform.position;
            Transform closestSpawn = null;
            float closestDistance = float.MaxValue;

            foreach (var spawnLocation in spawnLocations) {
                foreach (var spawn in spawnLocation.list) {
                    if (closestSpawn == null) {
                        closestSpawn = spawn;
                        continue;
                    }
                    var distance = Mathf.Abs((currentPos - spawn.position).sqrMagnitude);
                    if (distance < closestDistance) {
                        closestSpawn = spawn;
                        closestDistance = distance;
                    }
                }
            }

            if (closestSpawn != null && creatureTable != null) {
                StartCoroutine((creatureTable.Pick().Clone() as CreatureData).SpawnCoroutine(closestSpawn.transform.position, closestSpawn.transform.rotation, null, delegate (Creature creature) {
                    if (factionData.factionId != -999) creature.SetFaction(factionData.factionId);
                    if (creature.factionId == Player.currentCreature.factionId) {
                        AllyBehaviour ally = creature.gameObject.AddComponent<AllyBehaviour>();
                        ally.creature = creature;
                    }
                }, true));
                Utils.PlayHaptic(leftInteractor, rightInteractor, Utils.HapticIntensity.Moderate);
                pingSound.Play();
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

        public void Update() {
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

    public class AllyBehaviour : MonoBehaviour {
        public Creature creature;
        public IEnumerator follow;

        void Awake() {
            creature = creature ?? GetComponent<Creature>();
            if (!creature) {
                Destroy(this);
            }
            var behaviours = GetComponents<AllyBehaviour>();
            foreach (var behaviour in behaviours) {
                if (behaviour != this) {
                    Destroy(behaviour);
                }
            }
        }

        void OnEnable() {
            ((BrainHuman)creature.brain.instance).canLeave = false;
            follow = Follow();
            StartCoroutine(follow);
        }

        void OnDisable() {
            if (follow != null) StopCoroutine(follow);
            Destroy(this);
        }

        private IEnumerator Follow() {
            while (creature) {
                if (creature.brain.instance.isActive) {
                    creature.brain.TryAction(new ActionMove(Player.currentCreature.transform) {
                        reachDistance = 3f
                    });
                }
                yield return new WaitForSeconds(creature.brain.actionCycleSpeed);
            }
        }
    }
}