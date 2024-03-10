using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Ryneus
{
    public class AudioSourceController :MonoBehaviour
    {
        private AudioSource _audioSource;
        
        public AudioClip Clip => _audioSource.clip;

        private int _reserveTimeSample = -1;
        public int ReserveTimeSample => _reserveTimeSample;


        private bool _isPlay = false;
        public void Initialize()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        public void Play(int timeSamples = 0)
        {
            if (_audioSource == null) return;
            _audioSource.timeSamples = timeSamples;
            _audioSource.Play();
            _isPlay = true;
        }

        public void PlayDelay(float reserveTime)
        {
            if (_audioSource == null) return;
            _audioSource.PlayDelayed(reserveTime);
        }

        public void Pause()
        {
            if (_audioSource == null) return;
            _audioSource.Pause();
        }

        public void Stop()
        {
            if (_audioSource == null) return;
            _audioSource.Stop();
            _isPlay = false;
        }

        public void ChangeVolume(float volume)
        {
            if (_audioSource == null) return;
            _audioSource.volume = volume;
        }

        public void SetAudioData(AudioClip clip,bool isLoop = false,bool playOnAwake = false)
        {
            if (_audioSource == null) return;
            _audioSource.clip = clip;
            _audioSource.loop = isLoop;
            _audioSource.playOnAwake = playOnAwake;
        }

        public void ResetReserveTimestamp()
        {
            _reserveTimeSample = -1;
        }

        public void SetReserveTimestamp()
        {
            if (_audioSource == null) return;
            _reserveTimeSample = (int)(_audioSource.clip.length * _audioSource.clip.frequency);
        }

        public bool isPlaying()
        {
            return _audioSource != null && _audioSource.isPlaying;
        }

        public int timeSamples()
        {
            return _audioSource.timeSamples;
        }

        public bool IsLoopEnded(float _reservedTime)
        {
            if (timeSamples() >= (ReserveTimeSample - _reservedTime))
            {
                return true;
            }
            if (_isPlay && timeSamples() == 0)
            {
                return true;
            }
            return false;
        }

        public void FadeVolume(float targetVolume,int duration)
        {
            if (_audioSource == null) return;
            _audioSource.DOFade(targetVolume, duration);
        }
    }
}