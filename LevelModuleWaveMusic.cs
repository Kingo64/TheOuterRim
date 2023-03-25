using System.Collections;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace TOR {
    public class LevelModuleWaveMusic : LevelModule {
		public string musicWavePath;
		AudioContainer musicWaveAsset;
		Coroutine observe;

		AudioMixerGroup musicMixer;

		public override IEnumerator OnLoadCoroutine() {
			yield return Catalog.LoadAssetCoroutine(musicWavePath, delegate (AudioContainer value) {
				musicWaveAsset = value;
			}, "ModuleWaveMusic");

			musicMixer = GameManager.GetAudioMixerGroup(AudioMixerName.Music);
			observe = level.StartCoroutine(Observe());
			yield break;
		}

		IEnumerator Observe() {
			var canExit = false;
			while (true) {
				yield return Utils.waitSeconds_01;

				foreach (var waveSpawner in WaveSpawner.instances) {
					var musicSelector = waveSpawner.gameObject.AddComponent<MusicSelector>();
					musicSelector.musicMixer = musicMixer;
					musicSelector.musicWaveAsset = musicWaveAsset;
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

		public class MusicSelector : MonoBehaviour {
			public AudioMixerGroup musicMixer;
			public WaveSpawner waveSpawner;
			public AudioSource audioSource;
			public AudioContainer musicWaveAsset;


			public void Setup() {
				var sources = GetComponents<AudioSource>();
				foreach (var source in sources) {
					if (source.outputAudioMixerGroup == musicMixer) {
						audioSource = source;
						break;
					}
				}
				waveSpawner.OnWaveBeginEvent.AddListener(new UnityAction(OnWaveBeginEvent));
			}

			void OnWaveBeginEvent() {
				if (musicWaveAsset && audioSource) {
					audioSource.clip = GetTrack();
					audioSource.loop = false;
					audioSource.Play();
				}
			}

			private AudioClip GetTrack() {
				return musicWaveAsset.GetRandomAudioClip(musicWaveAsset.sounds);
			}

			protected void Update() {
				if (waveSpawner.isRunning && musicWaveAsset && audioSource && !audioSource.isPlaying) {
					audioSource.clip = GetTrack();
					audioSource.Play();
				}
			}
		}
	}
}
