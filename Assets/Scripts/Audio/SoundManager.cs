using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEditor;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        public float _bgmVolume = 1.0f;
        public float _seVolume = 1.0f;
        public bool _bgmMute = false;
        public bool _seMute = false;
        private List<AudioSource> _se;
        private List<AudioSource> _playingSe = new ();
        private int _seAudioSourceNum = 16;
        private List<AudioSource> _staticSe;
        private List<SEData> _seMaster;
        
        private IntroLoopAudio _bgm;
        private string _lastPlayAudio = "";
        private float _lastBgmVolume = 0f;

        void Awake()
        {
            _bgm = gameObject.AddComponent<IntroLoopAudio>();

            // 全体シーンで使うサウンドを初期ロード
            LoadDefaultSound();
            // SeAudioSourceを生成
            _se = new List<AudioSource>();
            for (int i = 0;i < _seAudioSourceNum;i++)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                _se.Add(audioSource);
            }
        }

        void LoadDefaultSound()
        {
            _staticSe = new List<AudioSource>();
            _seMaster = DataSystem.Data.SE.FindAll(a => a != null);
            for (int i = 0;i < _seMaster.Count;i++)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                _staticSe.Add(audioSource);
                SetSeAudio(audioSource,_seMaster[i].FileName,_seMaster[i].Volume,_seMaster[i].Pitch);
            }
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
            foreach (var staticSe in _staticSe)
            {
                float baseVolume = _seMaster.Find(a => a.FileName == staticSe.clip.name).Volume;
                staticSe.rolloffMode = AudioRolloffMode.Linear;
                staticSe.volume = _seVolume * baseVolume;
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


        public async void PlaySe(AudioClip clip, float volume,float pitch,int delayFrame = 0)
        {
            int audioSourceIndex = -1;
            for (int i = 0;i < _seAudioSourceNum;i++)
            {
                if (_se[i].isPlaying == false && _playingSe.Contains(_se[i]) == false)
                {
                    audioSourceIndex = i;
                    break;
                }
            }
            if (audioSourceIndex > -1)
            {
                _se[audioSourceIndex].clip = clip;
                _se[audioSourceIndex].volume = volume * _seVolume;
                _se[audioSourceIndex].pitch = pitch;
                _playingSe.Add(_se[audioSourceIndex]);
                if (delayFrame > 0)
                {
                    await UniTask.DelayFrame(delayFrame);
                }
                _se[audioSourceIndex].Play();
                _playingSe.Remove(_se[audioSourceIndex]);
            }
        }

        public void PlayStaticSe(SEType sEType, float volume = 1.0f)
        {
            if (_seMute) return;
            var seIndex = DataSystem.Data.SE.FindIndex(a => a.Id == (int)sEType);
            if (seIndex > -1)
            {
                _staticSe[seIndex].Play();
            }
        }

        public void AllPause()
        {

            foreach (var data in _staticSe)
            {
                if (data.isPlaying)
                {
                    data.Pause();
                }
            }
        }

        public void AllUnPause()
        {

            foreach (var data in _staticSe)
            {
                if (data.isPlaying)
                {
                    data.UnPause();
                }
            }
        }
    }
}