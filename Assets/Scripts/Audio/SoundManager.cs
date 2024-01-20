using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        public float BGMVolume = 1.0f;
        public float SeVolume = 1.0f;
        public bool BGMMute = false;
        public bool SeMute = false;
        private List<AudioSource> _se;
        private List<AudioSource> _playingSe = new ();
        private int _seAudioSourceNum = 16;
        private List<AudioSource> _staticSe;
        private List<SEData> _seMaster;
        
        [SerializeField] private List<IntroLoopAudio> _bgmTracks;
        public IntroLoopAudio BgmTrack => _bgmTracks[_mainTrack];
        private int _mainTrack = 0;
        public IntroLoopAudio BgmSubTrack => _bgmTracks[_subTrack];
        private int _subTrack = 1;

        private int _crossFadeTrackNo = 0;

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
            _mainTrack = 0;
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
            var volume = BGMVolume * _lastBgmVolume;
            BgmTrack.ChangeVolume(volume);
        }

        public void UpdateBgmMute()
        {
            if (BGMMute)
            {
                BgmTrack.ChangeVolume(0);
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

        public void PlayBgm(List<AudioClip> clip, float volume = 1.0f, bool loop = true)
        {
            if (clip[0].name == _lastPlayAudio) return;
            BgmTrack.Stop();
            BgmTrack.SetClip(clip,loop);
            _lastBgmVolume = volume;
            UpdateBgmMute();
            BgmTrack.Play();
            _lastPlayAudio = clip[0].name;
        }

        public void PlayCrossFadeBgm(List<AudioClip> clip, float volume = 1.0f)
        {
            if (clip.Count < 2) return;
            if (clip[0].name == _lastPlayAudio) return;
            BgmTrack.Stop();
            BgmTrack.SetSoloClip(clip[0]);
            _lastBgmVolume = volume;
            UpdateBgmMute();
            BgmTrack.Play();
            _lastPlayAudio = clip[0].name;

            BgmSubTrack.Stop();
            BgmSubTrack.SetSoloClip(clip[1]);
            _crossFadeTrackNo = 0;
        }

        public void ChangeCrossFade(float volume = 1.0f)
        {
            var playingTrack = _crossFadeTrackNo == 0 ? BgmTrack : BgmSubTrack;
            var playTrack = _crossFadeTrackNo == 0 ? BgmSubTrack : BgmTrack;

            var playingPer = playingTrack.PlayingPer();
            var timeStamp = playTrack.TimeStampPer(playingPer);
            playTrack.Play(timeStamp);
            playTrack.FadeVolume(1,1);
            _lastBgmVolume = volume;
            UpdateBgmMute();

            playingTrack.FadeVolume(0,1);
            _crossFadeTrackNo = _crossFadeTrackNo == 0 ? 1 : 0 ;
        }

        public void StopBgm()
        {
            BgmTrack.Stop();
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
}