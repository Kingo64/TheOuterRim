using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace TOR {
    class LevelModuleWaveMusic : LevelModule {
		public Level level;
		public LevelModuleWave wave;
		public AudioContainer musicWaveAsset;
		public string musicWavePath;
		public AudioSource audioSource;
		public AudioClip targetClip;
		Coroutine observe;

		public override IEnumerator OnLoadCoroutine(Level level) {
			this.level = level;
			wave = level.data.modes[0].GetModule<LevelModuleWave>();

			yield return Catalog.LoadAssetCoroutine(level.data.musicWaveLocation, delegate (AudioClip value) {
				targetClip = value;
			}, "ModuleWaveMusic");

			yield return Catalog.LoadAssetCoroutine(musicWavePath, delegate (AudioContainer value) {
				musicWaveAsset = value;
			}, "ModuleWaveMusic");

			observe = level.StartCoroutine(Observe());

			wave.OnWaveBeginEvent += Wave_OnWaveBeginEvent;
			yield break;
		}

		IEnumerator Observe() {
			while (true) {
				yield return new WaitForSeconds(0.1f);
				var sources = level.gameObject.GetComponents<AudioSource>();
				foreach (var source in sources) {
					if (source.clip == targetClip) {
						audioSource = source;
						level.StopCoroutine(observe);
						yield break;
					}
				}
			}
		}

		private AudioClip GetTrack() {
			return musicWaveAsset.GetRandomAudioClip(musicWaveAsset.sounds);
		}

        private void Wave_OnWaveBeginEvent() {
			if (musicWaveAsset && audioSource) {
				audioSource.clip = GetTrack();
				audioSource.loop = false;
				audioSource.Play();
            }
		}

		public override void Update(Level level) {
			if (wave.isRunning && musicWaveAsset && audioSource && !audioSource.isPlaying) {
				audioSource.clip = GetTrack();
				audioSource.Play();
			}
		}

        public override void OnUnload(Level level) {
			if (musicWaveAsset) {
				Catalog.ReleaseAsset(musicWaveAsset);
			}
			if (targetClip) {
				Catalog.ReleaseAsset(targetClip);
			}
			this.level = null;
		}
	}
}
