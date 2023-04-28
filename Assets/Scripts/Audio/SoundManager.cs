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
        private List<AudioSource> _seData;
        
        private IntroLoopAudio _bgm;
        private string _lastPlayAudio = "";

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
        }

        void LateUpdate()
        {
            //UpdateVolume(_seData, _seVolume);
        }

        void UpdateVolume(List<AudioSource> audioData, float volume)
        {
            foreach (var data in audioData)
            {
                if (data.isPlaying)
                {
                    data.volume = (volume * 1);
                }
            }
        }

        public void PlayBgm(List<AudioClip> clip, float volume = 1.0f, bool loop = true)
        {
            if (clip[0].name == _lastPlayAudio) return;
            _bgm.Stop();
            _bgm.SetClip(clip,loop);
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