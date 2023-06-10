using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEditor;

namespace Ryneus{

    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        public float _bgmVolume = 1.0f;
        public float _seVolume = 1.0f;
        public bool _bgmMute = false;
        public bool _seMute = false;
        private List<AudioSource> _seData;
        
        private IntroLoopAudio _bgm;
        private string _lastPlayAudio = "";
        private float _lastBgmVolume = 0f;

        void Awake()
        {
            _bgm = gameObject.AddComponent<IntroLoopAudio>();

            // 全体シーンで使うサウンドを初期ロード
            LoadDefaultSound();
        }

        void LoadDefaultSound()
        {
            _seData = new List<AudioSource>();
            List<SEData> sEDatas = DataSystem.Data.SE.FindAll(a => a != null);
            for (int i = 0;i < sEDatas.Count;i++)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                _seData.Add(audioSource);
                SetSeAudio(audioSource,sEDatas[i].FileName,sEDatas[i].Volume,sEDatas[i].Pitch);
            }
            /*
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            _seData.Add(audioSource);
            */
        }


        private void SetSeAudio(AudioSource audioSource,string sePath,float volume,float pitch)
        {
            var handle = Resources.Load<AudioClip>("Audios/SE/" + sePath + "");
            
            if (audioSource != null)
            {
                audioSource.clip = handle;
                audioSource.pitch = pitch;
                audioSource.volume = volume;
            }
                /*
            Addressables.LoadAssetAsync<AudioClip>(
                "Assets/Audios/SE/" + sePath + ".ogg"
            ).Completed += op => {
                if (audioSource != null)
                {
                    audioSource.clip = op.Result;
                    audioSource.pitch = pitch;
                    audioSource.volume = volume;
                }
            };
            */
        }

        void LateUpdate()
        {
            //UpdateVolume(_seData, _seVolume);
        }

        public void UpdateBgmVolume()
        {
            var volume = _bgmVolume * _lastBgmVolume;
            _bgm.ChangeVolume(volume);
        }

        public void UpdateBgmMute()
        {
            if (_bgmMute)
            {
                _bgm.ChangeVolume(0);
            } else
            {
                UpdateBgmVolume();
            }
        }

        public void UpdateSeVolume()
        {
            var volume = _seVolume;
            foreach (var seData in _seData)
            {
                seData.volume = volume;
            }
        }

        public void UpdateSeMute()
        {
            if (_seMute)
            {
            } else
            {
                UpdateSeMute();
            }
        }

        public void PlayBgm(List<AudioClip> clip, float volume = 1.0f, bool loop = true)
        {
            if (clip[0].name == _lastPlayAudio) return;
            _bgm.Stop();
            _bgm.SetClip(clip,loop);
            _lastBgmVolume = volume;
            UpdateBgmMute();
            _bgm.Play();
            _lastPlayAudio = clip[0].name;
        }

        public void StopBgm()
        {
            _bgm.Stop();
            _lastPlayAudio = null;
        }


        public void PlaySe(string keyName, float volume = 1.0f)
        {
            int seIndex = DataSystem.Data.SE.FindIndex(a => a.Key == keyName);
            if (seIndex > 0)
            {
                _seData[seIndex].Play();
            }
        }

        public void PlayStaticSe(SEType sEType, float volume = 1.0f)
        {
            if (_seMute) return;
            var seIndex = DataSystem.Data.SE.FindIndex(a => a.Id == (int)sEType);
            if (seIndex > -1)
            {
                _seData[seIndex].Play();
            }
        }



        public void AllPause()
        {

            foreach (var data in _seData)
            {
                if (data.isPlaying)
                {
                    data.Pause();
                }
            }
        }

        public void AllUnPause()
        {

            foreach (var data in _seData)
            {
                if (data.isPlaying)
                {
                    data.UnPause();
                }
            }
        }
    }


}