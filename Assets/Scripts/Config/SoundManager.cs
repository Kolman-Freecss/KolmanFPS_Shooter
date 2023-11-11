using System;
using System.Collections.Generic;
using Entities.Utils;
using UnityEngine;

namespace Config
{
    public class SoundManager : MonoBehaviour
    {
        [Serializable]
        public enum BackgroundMusic
        {
            Intro,
            InGame
        }

        #region Member Variables

        public static SoundManager Instance { get; private set; }

        [Range(0, 100)] public float EffectsAudioVolume = 50f;
        [Range(0, 100)] public float MusicAudioVolume = 40f;

        public List<SerializableDictionaryEntry<BackgroundMusic, AudioClip>> BackgroundMusicClips;
        public AudioClip ButtonClickSound;

        #endregion

        #region InitData

        private void Awake()
        {
            ManageSingleton();
        }

        private void Start()
        {
            if (BackgroundMusicClips == null) BackgroundMusicClips = new List<SerializableDictionaryEntry<BackgroundMusic, AudioClip>>();
            SetEffectsVolume(EffectsAudioVolume);
            SetMusicVolume(MusicAudioVolume);
            StartBackgroundMusic(BackgroundMusic.Intro);
        }

        void ManageSingleton()
        {
            if (Instance != null)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        #endregion

        #region Logic

        public void StartBackgroundMusic(BackgroundMusic backgroundMusic)
        {
            AudioClip clip = BackgroundMusicClips.Find(x => x.Key == backgroundMusic).Value;
            if (clip != null)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource.isPlaying) audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning($"No clip found for {backgroundMusic}");
            }
        }

        public void SetEffectsVolume(float volume)
        {
            EffectsAudioVolume = volume;
        }

        public void SetMusicVolume(float volume)
        {
            GetComponent<AudioSource>().volume = volume / 100;
        }

        public void PlayButtonClickSound(Vector3 position)
        {
            AudioSource.PlayClipAtPoint(ButtonClickSound, position, EffectsAudioVolume / 100);
        }

        #endregion
    }
}