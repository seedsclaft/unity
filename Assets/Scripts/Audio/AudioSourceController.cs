using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceController :MonoBehaviour
{
    private AudioSource _audioSource;
    public AudioClip Clip => _audioSource.clip;

    private int _reserveTimeSample = -1;
    public int ReserveTimeSample => _reserveTimeSample;

    public void Initialize()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Play(int timeSamples = 0)
    {
        _audioSource.timeSamples = timeSamples;
        _audioSource.Play();
    }

    public void PlayDelay(float reseverTime)
    {
        _audioSource.PlayDelayed(reseverTime);
    }

    public void Pause()
    {
        _audioSource.Pause();
    }

    public void Stop()
    {
        _audioSource.Stop();
    }

    public void ChangeVolume(float volume)
    {
        _audioSource.volume = volume;
    }

    public void SetAudioData(AudioClip clip,bool isLoop = false,bool playOnAwake = false)
    {
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
}
