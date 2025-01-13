using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        private float _bgmVolume = 1.0f;
        public float BgmVolume => _bgmVolume;
        public void SetBgmVolume(float volume) => _bgmVolume = volume;
        public float SeVolume = 1.0f;
        public bool BGMMute = false;
        public bool SeMute = false;
        private List<AudioSource> _se;
        private List<AudioSource> _playingSe = new ();
        private int _seAudioSourceNum = 16;
        private List<AudioSource> _staticSe;
        private List<SEData> _seMaster;
        
        [SerializeField] private SoundIntroLoop _bgmMain;
        [SerializeField] private SoundIntroLoop _bgmSub;
        [SerializeField] private SoundIntroLoop _bgsTrack;
        
        private bool _crossFadeMode = false;
        public bool CrossFadeMode => _crossFadeMode;
        private bool _mainTrack = true;

        private string _lastPlayAudio = "";
        private float _lastBgmVolume = 0f;

        public void Initialize()
        {
            // 全体シーンで使うサウンドを初期ロード
            LoadDefaultSound();
            // SeAudioSourceを生成
            _se = new List<AudioSource>();
            for (int i = 0;i < _seAudioSourceNum;i++)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                _se.Add(audioSource);
            }
        }

        void LoadDefaultSound()
        {
            _staticSe = new List<AudioSource>();
            _seMaster = DataSystem.Data.SE.FindAll(a => a != null);
            for (int i = 0;i < _seMaster.Count;i++)
            {
                var audioSource = gameObject.AddComponent<AudioSource>();
                _staticSe.Add(audioSource);
                SetSeAudio(audioSource,_seMaster[i].FileName,_seMaster[i].Volume,_seMaster[i].Pitch);
            }
        }


        private void SetSeAudio(AudioSource audioSource,string sePath,float volume,float pitch)
        {
            var handle = ResourceSystem.LoadSeAudio(sePath);
            
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
            var playingTrack = _bgmMain;
            if (BGMMute)
            {
                playingTrack.ChangeVolume(0);
            } else
            {
                var volume = _bgmVolume * _lastBgmVolume;
                playingTrack.ChangeVolume(volume);
            }
        }

        public void UpdateSeVolume()
        {
            foreach (var staticSe in _staticSe)
            {
                float baseVolume = _seMaster.Find(a => a.FileName == staticSe.clip.name).Volume;
                staticSe.rolloffMode = AudioRolloffMode.Linear;
                staticSe.volume = SeVolume * baseVolume;
            }
        }

        public void UpdateSeMute()
        {
            if (SeMute)
            {
            } else
            {
                UpdateSeMute();
            }
        }

        public void PlayBgm(List<AudioClip> clip, float volume = 1.0f, bool loop = true,float timeStamp = 0)
        {
            if (clip[0].name == _lastPlayAudio) return;
            _lastBgmVolume = volume;
            _lastPlayAudio = clip[0].name;
            // これから再生するTrackを停止して再生
            var playTrack = _mainTrack ? _bgmSub : _bgmMain;
            playTrack.Stop();
            playTrack.SetClip(clip,loop);
            playTrack.Play(timeStamp);
            playTrack.FadeVolume(volume * _bgmVolume,1);
            
            // 再生中の方をフェードアウト
            var playingTrack = _mainTrack ? _bgmMain : _bgmSub;
            playingTrack.FadeVolume(0,1);
            UpdateBgmVolume();
            _crossFadeMode = false;
            _mainTrack = !_mainTrack;
        }

        public void PlayBgs(AudioClip clip, float volume = 1.0f, bool loop = true)
        {
            _bgsTrack.Stop();
            _bgsTrack.SetClip(new List<AudioClip>(){clip},loop);
            
            UpdateBgmVolume();
            _bgsTrack.Play();
            _bgsTrack.FadeVolume(volume * _bgmVolume,1);
        }

        public void PlayCrossFadeBgm(List<AudioClip> clip, float volume = 1.0f)
        {
            if (clip.Count < 2) return;
            if (clip[0].name == _lastPlayAudio) return;
            _bgmMain.Stop();
            _bgmMain.SetSoloClip(clip[0]);
            _lastBgmVolume = volume;
            _lastPlayAudio = clip[0].name;

            UpdateBgmVolume();
            _bgmMain.Play();
            _bgmSub.Stop();
            _bgmSub.SetSoloClip(clip[1]);
            _mainTrack = true;
            _crossFadeMode = true;
        }

        public void ChangeCrossFade(float volume = 1.0f)
        {
            if (_crossFadeMode == false) return;
            var playingTrack = _bgmMain;
            var resumeTrack = _bgmSub;

            var playingPer = playingTrack.PlayingPer();
            var timeStamp = resumeTrack.TimeStampPer(playingPer);
            _lastBgmVolume = volume;

            _mainTrack = !_mainTrack;
            playingTrack.FadeVolume(0,1);
            UpdateBgmVolume();
            resumeTrack.Play(timeStamp);
            var playVolume = _bgmVolume * _lastBgmVolume;
            if (BGMMute)
            {
                playVolume = 0;
            }
            resumeTrack.FadeVolume(playVolume,1);
        }

        public void StopBgm()
        {
            _bgmSub.Stop();
            _bgmMain.Stop();
            _lastPlayAudio = null;
        }

        public void StopBgs()
        {
            _bgsTrack.Stop();
        }

        public float CurrentTimeStamp()
        {
            var playingTrack = _mainTrack ? _bgmMain : _bgmSub;
            var playingPer = playingTrack.PlayingPer();
            return playingTrack.TimeStampPer(playingPer);
        }

        public void FadeOutBgm()
        {
            var playingTrack = _mainTrack ? _bgmMain : _bgmSub;
            playingTrack.FadeVolume(0,1);
            _lastPlayAudio = null;
        }

        public async void PlaySe(AudioClip clip, float volume,float pitch = 1f,int delayFrame = 0)
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
                _se[audioSourceIndex].volume = volume * SeVolume;
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
            //Debug.Log(sEType);
            if (SeMute) return;
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

    public enum AudioTrackType
    {
        Main = 0,
        Sub = 1
    }
}