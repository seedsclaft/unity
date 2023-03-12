using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;
using UnityEditor;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    [SerializeField] SoundData _soundData;
    readonly int SeChannel = 16;
    public float _bgmVolume = 1.0f;
    public float _seVolume = 1.0f;
    AudioData[] _seData;
    
    private IntroLoopAudio _bgm;
    private string _lastPlayAudio = "";

    void Awake()
    {
        _bgm = gameObject.AddComponent<IntroLoopAudio>();

        _seData = new AudioData[SeChannel];
        for (int i = 0; i < _seData.Length; i++)
        {
            _seData[i] = new AudioData(gameObject.AddComponent<AudioSource>());
        }

        // 全体シーンで使うサウンドを初期ロード
        LoadDefaultSound();
    }

    class CrossFadeData
    {
        public AudioData _srcAudioData;
        public AudioData _dstAudioData;
        public float _fadeCompleteTime;
        public float _fadeTime;
    }
    CrossFadeData _crossFadeData;

    public class AudioData
    {
        public string Guid { get; private set; }
        private AudioSource _audioSource;
        private float _initVolume;
        public float _audioLength;

        int _pauseRefCount;
        public AudioData(AudioSource audioSource)
        {
            _audioSource = audioSource;
            _audioSource.playOnAwake = false;
        }
        public void SetInitVolume(float volume)
        {
            _initVolume = volume;
        }
        public float InitVolume()
        {
            return _initVolume;
        }
        public void Pause()
        {
            _pauseRefCount++;
            _audioSource.Pause();
        }

        public void UnPause()
        {
            if (_pauseRefCount > 0)
            {
                _pauseRefCount--;
                if (_pauseRefCount <= 0)
                {
                    _audioSource.UnPause();
                }
            }
        }

        public bool Paused()
        {
            return _pauseRefCount > 0;
        }

        public void SetGuid()
        {
            Guid = System.Guid.NewGuid().ToString();
        }

        public AudioDataHandler ToAudioSourceHandler()
        {
            return new AudioDataHandler(this);
        }

        public void Stop() {
            _audioSource.Stop();
        }
        public void SetClip(AudioClip clip){
            _audioSource.clip = clip;
        }
        public void SetVolume(float value) {
            _audioSource.volume = value;
        }
        public void Play(bool loop){
            if(loop) _audioSource.loop = loop;
            _audioSource.Play();
        }
        public bool IsPlaying(){
            return _audioSource.isPlaying;
        }
        public float AudioTime { get {return _audioSource.time; } }
    }

    public readonly struct AudioDataHandler
    {
        public readonly string _guid;

        public AudioDataHandler(AudioData audioSourceData)
        {
            _guid = audioSourceData.Guid;
        }
    }


    void LoadDefaultSound()
    {
            _soundData = Resources.Load<SoundData>("DebugTestData");
// #if UNITY_EDITOR
    //    _soundData = AssetDatabase.LoadAssetAtPath<SoundData>("Assets/Scripts/Sound/DebugTestData.asset");
// #else
    //    StartCoroutine(LoadAssetBundle());
// #endif
    }

    IEnumerator LoadAssetBundle()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "DebugTestData");
        using (UnityWebRequest req = UnityWebRequestAssetBundle.GetAssetBundle(path))
        {
            yield return req.SendWebRequest();

            if (req.isNetworkError || req.isHttpError)
            {
                Debug.LogError(req.error);
            }
            else
            {
                var bundle = DownloadHandlerAssetBundle.GetContent(req);
                var abReq = bundle.LoadAssetAsync<SoundData>("DebugTestData");
                yield return abReq;
                _soundData = abReq.asset as SoundData;
                bundle.Unload(false);
            }
        }
    }

    void LateUpdate()
    {
        UpdateVolume(_seData, _seVolume);

        if (_crossFadeData != null)
        {
            if (_crossFadeData._fadeTime > _crossFadeData._fadeCompleteTime)
            {
                _crossFadeData._srcAudioData.Stop();
                _crossFadeData._dstAudioData.SetVolume(_bgmVolume * _crossFadeData._dstAudioData.InitVolume());
                _crossFadeData = null;
            }
            else
            {
                _crossFadeData._srcAudioData.SetVolume(_bgmVolume * _crossFadeData._srcAudioData.InitVolume() * (1.0f - (_crossFadeData._fadeTime / _crossFadeData._fadeCompleteTime)));
                _crossFadeData._dstAudioData.SetVolume(_bgmVolume * _crossFadeData._dstAudioData.InitVolume() * (_crossFadeData._fadeTime / _crossFadeData._fadeCompleteTime));
                _crossFadeData._fadeTime += Time.deltaTime;
            }
        }
    }

    public bool Initialized()
    {
        return _soundData != null;
    }

    void UpdateVolume(AudioData[] audioData, float volume)
    {
        foreach (var data in audioData)
        {
            if (data.IsPlaying())
            {
                data.SetVolume(volume * data.InitVolume());
            }
        }
    }

    bool TryGetAudioClip(string name, out AudioClip outData)
    {
        foreach (var clip in _soundData._audioClips)
        {
            if(clip == null) break;
            if (clip.name == name)
            {
                outData = clip;
                return true;
            }
        }
        outData = null;
        return false;
    }

    bool TryGetAudioSourceData(AudioDataHandler handler, out AudioData outData)
    {

        foreach (var data in _seData)
        {
            if (handler._guid == data.Guid)
            {
                outData = data;
                return true;
            }
        }

        outData = null;
        return false;
    }

    public AudioDataHandler Play(SystemSeType type, float volume = 1.0f)
    {
        int index = (int)type; 
        if(_soundData._audioClips.Count() > index && index >= 0)
        {
            if(_soundData._audioClips[index] != null)
            {
                Play(_soundData._audioClips[index],volume);
            }
        }
        
        return default;
    }


    public AudioDataHandler Play(string name, float volume = 1.0f)
    {
        if (TryGetAudioClip(name, out var clip))
        {
            return Play(clip, volume);
        }
        return default;
    }

    public AudioDataHandler Play(AudioClip clip, float volume = 1.0f)
    {
        foreach (var data in _seData)
        {
            if (!data.IsPlaying() && !data.Paused())
            {
                data.SetClip(clip);
                data.SetVolume(_seVolume * volume);
                data.Play(false);

                data._audioLength = clip.length;
                data.SetInitVolume(volume);
                data.SetGuid();
                return data.ToAudioSourceHandler();
            }
        }

        return default;
    }
    /*

    public AudioDataHandler PlayBgm(string name, float volume = 1.0f, bool loop = true, bool introLoop = false)
    {
        if (TryGetAudioClip(name, out var clip))
        {
            return PlayBgm(clip, volume, loop, introLoop);
        }
        return default;
    }
    */

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


    public void PlaySe(List<AudioClip> clip, float volume = 1.0f)
    {
    }


    /// <summary>
    /// 再生されていなければfalseを返します
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public bool Stop(AudioDataHandler handler)
    {
        if (TryGetAudioSourceData(handler, out var data))
        {
            if (!data.IsPlaying()) return false;

            data.Stop();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 再生されていなければ-1を返します
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public float GetPlayTime(AudioDataHandler handler)
    {
        if (TryGetAudioSourceData(handler, out var data))
        {
            if (!data.IsPlaying() && !data.Paused()) return data._audioLength;

            return data.AudioTime;
        }
        return -1;
    }

    public bool Completed(AudioDataHandler handler)
    {
        if (TryGetAudioSourceData(handler, out var data))
        {
            return data.IsPlaying() && !data.Paused();
        }
        return false;
    }

    public bool Playing(AudioDataHandler handler)
    {
        if (TryGetAudioSourceData(handler, out var data))
        {
            return data.IsPlaying();
        }
        return false;
    }

    public bool Paused(AudioDataHandler handler)
    {
        if (TryGetAudioSourceData(handler, out var data))
        {
            return data.Paused();
        }
        return false;
    }

    public bool Pause(AudioDataHandler handler)
    {
        if (TryGetAudioSourceData(handler, out var data))
        {
            if (!data.IsPlaying()) return false;

            data.Pause();
            return true;
        }
        return false;
    }

    public bool UnPause(AudioDataHandler handler)
    {
        if (TryGetAudioSourceData(handler, out var data))
        {
            if (!data.Paused()) return false;

            data.UnPause();
            return true;
        }
        return false;
    }

    public void AllPause()
    {

        foreach (var data in _seData)
        {
            if (data.IsPlaying())
            {
                data.Pause();
            }
        }
    }

    public void AllUnPause()
    {

        foreach (var data in _seData)
        {
            if (data.IsPlaying())
            {
                data.UnPause();
            }
        }
    }
}

public enum SystemSeType
{
    Cursor			= 1,
    Decide			,
    Cancel			,
}