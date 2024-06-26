﻿using ThunderRoad;
using UnityEngine;

namespace TOR {
    public class ItemDiscoverable: ThunderBehaviour {
        protected Item item;

        AudioSource audio;
        AudioContainer audioContainer;

        protected void Awake() {
            item = GetComponent<Item>();

            if (LevelModuleSaveManager.saveData.discoveredItems.Contains(item.itemId)) {
                Destroy(this);
                return;
            };

            var fx = Catalog.GetData<EffectData>("ItemDiscovered", true);
            audioContainer = ((EffectModuleAudio)fx.modules[0]).audioContainer;

            audio = item.gameObject.AddComponent<AudioSource>();
            audio.spatialBlend = 0.5f;
            audio.volume = 0.5f;
            audio.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Effect);

            item.OnGrabEvent += OnGrabEvent;
        }

        private void OnGrabEvent(Handle handle, RagdollHand ragdollHand) {
            item.OnGrabEvent -= OnGrabEvent;
            if (LevelModuleSaveManager.saveData.discoveredItems.Contains(item.itemId)) {
                Destroy(this);
            } else {
                Utils.PlaySound(audio, audioContainer, item);
                LevelModuleSaveManager.saveData.discoveredItems.Add(item.itemId);
                LevelModuleSaveManager.Save();
                LevelModuleSaveManager.ProcessDiscoveredItems();
            }
        }
    }
}