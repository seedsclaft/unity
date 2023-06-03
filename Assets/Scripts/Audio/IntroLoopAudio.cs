using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// イントロ付きループ BGM を制御するクラスです。
/// </summary>
/// <remarks>
/// WebGL では PlayScheduled で再生するとループしないのでその対応を入れている。
/// WebGL では２つの AudioSource を交互に再生。
/// </remarks>
public class IntroLoopAudio : MonoBehaviour
{

  private float _audioClipLoopLength = -1;

  /// <summary>BGM のイントロ部分の AudioSource。</summary>
  private AudioSource _introAudioSource;

  /// <summary>BGM のループ部分の AudioSource。</summary>
  private AudioSource[] _loopAudioSources = new AudioSource[2];

  /// <summary>一時停止中かどうか。</summary>
  private bool _isPause;

  /// <summary>現在の再生するループ部分のインデックス。</summary>
  private int _nowPlayIndex = 0;

  /// <summary>ループ部分に使用する AudioSource の数。</summary>
  private int _loopSourceCount = 0;

  /// <summary>再生中であるかどうか。一時停止、非アクティブの場合は false を返す。</summary>
  private bool IsPlaying
    => (_introAudioSource.isPlaying || _introAudioSource.time > 0)
      || (_loopAudioSources[0].isPlaying || _loopAudioSources[0].time > 0)
      || (_loopAudioSources[1] != null && (_loopAudioSources[1].isPlaying || _loopAudioSources[1].time > 0));

  /// <summary>現在アクティブで再生しているループ側の AudioSource。</summary>
  private AudioSource LoopAudioSourceActive
    => _loopAudioSources[1] != null && _loopAudioSources[1].time > 0 ? _loopAudioSources[1] : _loopAudioSources[0];

  /// <summary>現在の再生時間 (s)。</summary>
  public float time
    => _introAudioSource == null ? 0
      : _introAudioSource.time > 0 ? _introAudioSource.time
      : LoopAudioSourceActive.time > 0 ? _introAudioSource.clip.length + LoopAudioSourceActive.time
      : 0;


  void Awake()
  {
    #if UNITY_WEBGL
      _loopSourceCount = 2;   
    #else 
      _loopSourceCount = 1; // WebGL でなければ 1
    #endif

    // AudioSource を自身に追加
    _introAudioSource = gameObject.AddComponent<AudioSource>();
    _loopAudioSources[0] = gameObject.AddComponent<AudioSource>();
    if (_loopSourceCount >= 2)
    {
      _loopAudioSources[1] = gameObject.AddComponent<AudioSource>();
    }
  }

  public void SetClip(List<AudioClip> clip,bool isLoop){
    _audioClipLoopLength = -1;
    if (clip.Count == 2){
      _introAudioSource.clip = clip[0];
      _introAudioSource.loop = clip[1] == null ? isLoop : false;
      _introAudioSource.playOnAwake = false;

      _loopAudioSources[0].clip = clip[1];
      _loopAudioSources[0].loop = clip[1] == null ? false :_loopSourceCount == 1;
      _loopAudioSources[0].playOnAwake = false;
      if (_loopAudioSources[1] != null)
      {
        _loopAudioSources[1].clip = clip[1];
        _loopAudioSources[1].loop = false;
        _loopAudioSources[1].playOnAwake = false;
        if (clip[1] != null)
        {
          _audioClipLoopLength = clip[1].length;
        }
      }
    } else{
      _introAudioSource.clip = clip[0];
      _introAudioSource.loop = isLoop;
      _introAudioSource.playOnAwake = false;
      _loopAudioSources[0].Stop();
      _loopAudioSources[0].clip = null;
      if (_loopAudioSources[1] != null) _loopAudioSources[1].Stop();
      if (_loopAudioSources[1] != null) _loopAudioSources[1].clip = null;
      /*
      _loopAudioSources[0].clip = clip[0];
      _loopAudioSources[0].loop = isLoop;
      _loopAudioSources[0].playOnAwake = false;
      if (_loopAudioSources[1] != null)
      {
        _loopAudioSources[1].clip = clip[0];
        _loopAudioSources[1].loop = false;
        _loopAudioSources[1].playOnAwake = false;
      }
      */
    }
  }

  void Update()
  {
    // WebGL のためのループ切り替え処理
    #if UNITY_WEBGL
    if (_audioClipLoopLength != -1)
    {
      // 終了する１秒前から次の再生のスケジュールを登録する
      if (_nowPlayIndex == 0 && _loopAudioSources[0].time >= _audioClipLoopLength - 1)
      {
        _loopAudioSources[1].PlayScheduled(AudioSettings.dspTime + (_audioClipLoopLength - _loopAudioSources[0].time));
        _nowPlayIndex = 1;
      }
      else if (_nowPlayIndex == 1 && _loopAudioSources[1].time >= _audioClipLoopLength - 1)
      {
        _loopAudioSources[0].PlayScheduled(AudioSettings.dspTime + (_audioClipLoopLength - _loopAudioSources[1].time));
        _nowPlayIndex = 0;
      }
    }
    #endif
  }

  public void Play()
  {
    // クリップが設定されていない場合は何もしない
    if (_introAudioSource == null || _loopAudioSources == null) return;

    // Pause 中は isPlaying は false
    // 標準機能だけでは一時停止中か判別不可能
    if (_isPause)
    {
      _introAudioSource.UnPause();
      if (_introAudioSource.isPlaying)
      {
        // イントロ中ならループ開始時間を残り時間で再設定
        _loopAudioSources[0].Stop();
        _loopAudioSources[0].PlayScheduled(AudioSettings.dspTime + _introAudioSource.clip.length - _introAudioSource.time);
      }
      else
      {
        #if UNITY_WEBGL
          // WebGL の場合は切り替え処理を実行
          if (_loopAudioSources[0].time > 0)
          {
            _loopAudioSources[0].UnPause();
            if (_loopAudioSources[0].time >= _audioClipLoopLength - 1)
            {
              _loopAudioSources[1].Stop();
              _loopAudioSources[1].PlayScheduled(AudioSettings.dspTime + (_audioClipLoopLength - _loopAudioSources[0].time));
              _nowPlayIndex = 1;
            }
          }
          else
          {
            _loopAudioSources[1].UnPause();
            if (_loopAudioSources[1].time >= _audioClipLoopLength - 1)
            {
              _loopAudioSources[0].Stop();
              _loopAudioSources[0].PlayScheduled(AudioSettings.dspTime + (_audioClipLoopLength - _loopAudioSources[0].time));
              _nowPlayIndex = 0;
            }
          }
        #else
          // WebGL 以外は UnPause するだけ
          _loopAudioSources[0].UnPause();
        #endif
      }
    }
    else if (IsPlaying == false)
    {
      Stop();
      if (_introAudioSource.clip != null){
        _introAudioSource.Play();
        _loopAudioSources[0].PlayScheduled(AudioSettings.dspTime + _introAudioSource.clip.length);
      } else{
        _loopAudioSources[0].Play();
      }
    }

    _isPause = false;
  }

  /// <summary>BGM を一時停止します。</summary>
  public void Pause()
  {
    if (_introAudioSource == null || _loopAudioSources == null) return;

    _introAudioSource.Pause();
    _loopAudioSources[0].Pause();
    if (_loopAudioSources[1] != null) _loopAudioSources[1].Pause();

    _isPause = true;
  }

  /// <summary>BGM を停止します。</summary>
  public void Stop()
  {
    if (_introAudioSource == null || _loopAudioSources == null) return;

    _introAudioSource.Stop();
    _loopAudioSources[0].Stop();
    if (_loopAudioSources[1] != null) _loopAudioSources[1].Stop();
    _isPause = false;
  }
}