using System.Collections;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Events;

namespace TOR {
    public class LevelModuleWaveMusic : LevelModule {
		public string musicWavePath;
		AudioContainer musicWaveAsset;
		Coroutine observe;

		GameObject obj;
        AudioSource audioSource;

		public override IEnumerator OnLoadCoroutine() {
			yield return Catalog.LoadAssetCoroutine(musicWavePath, delegate (AudioContainer value) {
				musicWaveAsset = value;

                obj = new GameObject("TOR_MusicPlayer");
                audioSource = obj.AddComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Music);
                audioSource.spatialBlend = 0;
                audioSource.playOnAwake = false;

                observe = level.StartCoroutine(Observe());
            }, "ModuleWaveMusic");
			yield break;
		}

		IEnumerator Observe() {
			var canExit = false;
			while (true) {
				yield return Utils.waitSeconds_01;

				foreach (var waveSpawner in WaveSpawner.instances) {
					var musicSelector = waveSpawner.gameObject.AddComponent<MusicSelector>();
					musicSelector.musicWaveAsset = musicWaveAsset;
					musicSelector.audioSource = audioSource;
					musicSelector.waveSpawner = waveSpawner;
					musicSelector.Setup();
					canExit = true;
				}
				if (canExit) {
					level.StopCoroutine(observe);
					yield break;
				}
			}
		}

		public override void OnUnload() {
			if (musicWaveAsset) {
				Catalog.ReleaseAsset(musicWaveAsset);
			}
		}

		public class MusicSelector : ThunderBehaviour {
            public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

            public WaveSpawner waveSpawner;
			public AudioSource audioSource;
			public AudioContainer musicWaveAsset;

            public void Setup() {
				waveSpawner.OnWaveBeginEvent.AddListener(new UnityAction(OnWaveBeginEvent));
				waveSpawner.OnWaveAnyEndEvent.AddListener(new UnityAction(OnWaveAnyEndEvent));
            }

			void OnWaveBeginEvent() {
				if (musicWaveAsset && audioSource) {
					audioSource.clip = musicWaveAsset.GetRandomAudioClip();
                    audioSource.loop = false;
					audioSource.Play();
				}
			}

			void OnWaveAnyEndEvent() {
				audioSource?.Stop();
			}

            protected override void ManagedUpdate() {
                if (waveSpawner.isRunning && musicWaveAsset && audioSource && !audioSource.isPlaying) {
					audioSource.clip = musicWaveAsset.GetRandomAudioClip();
                    audioSource.Play();
				}
			}
		}
	}
}
