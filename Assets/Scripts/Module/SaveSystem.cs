﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem : MonoBehaviour
{

	private static readonly string debugFilePath = Application.persistentDataPath;
        public static void SaveStart(SavePlayInfo pSourceSavePlayInfo = null,int fileId = 0)
        {
			#if UNITY_WEBGL
			
			//	バイナリ形式でシリアル化
	        BinaryFormatter TempBinaryFormatter = new BinaryFormatter ();
    	    MemoryStream    memoryStream    = new MemoryStream ();
        	TempBinaryFormatter.Serialize (memoryStream , pSourceSavePlayInfo);
        	var saveData = Convert.ToBase64String (memoryStream   .GetBuffer ());
			if (fileId != 0)
			{
				PlayerPrefs.SetString("PlayerData" + fileId.ToString(), AESManager.Encrypt(saveData));
			} else
			{
				PlayerPrefs.SetString("PlayerData", AESManager.Encrypt(saveData));
			}

			#else
            //	保存情報
            if( pSourceSavePlayInfo == null )
            {
                pSourceSavePlayInfo = new SavePlayInfo();
				pSourceSavePlayInfo.InitSaveData();
            }
            //pSourceSavePlayInfo.SavePointSet(savePointType);


            //#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            {
                //	バイナリ形式でシリアル化
                BinaryFormatter	TempBinaryFormatter = new BinaryFormatter();

			    //	指定したパスにファイルを作成
                FileStream TempFileStream = File.Create(debugFilePath + "/Autosave" + fileId.ToString() + ".dat");

                //	Closeが確実に呼ばれるように例外処理を用いる
                try
                {
                    //	指定したオブジェクトを上で作成したストリームにシリアル化する
                    TempBinaryFormatter.Serialize(TempFileStream, pSourceSavePlayInfo);
                }
                finally
                {
                    //	ファイル操作には明示的な破棄が必要です。Closeを忘れないように。
                    if( TempFileStream != null )
                    {
                        TempFileStream.Close();
                    }
                }
            }
			#endif
		//#endif
	}

		
	public static void LoadStart(int fileId = 0)
	{
		#if UNITY_WEBGL
				
		try
			{
				//	バイナリ形式でデシリアライズ
				BinaryFormatter	TempBinaryFormatter = new BinaryFormatter();
				string saveData;
				if (fileId != 0)
				{
					saveData = PlayerPrefs.GetString("PlayerData"+fileId.ToString());
				} else
				{
					saveData = PlayerPrefs.GetString("PlayerData");
				}
				saveData = AESManager.Decrypt(saveData);
        		MemoryStream    memoryStream    = new MemoryStream (Convert.FromBase64String (saveData));
        		GameSystem.CurrentData = (SavePlayInfo)TempBinaryFormatter.Deserialize (memoryStream);
			}
			// Jsonへの展開失敗　改ざんの可能性あり
			catch(Exception e)
			{
				// 例外が発生するのでここで処理
				Debug.LogException(e);
				Debug.Log("改ざんされたため　冒険の書は消えてしまいました");
				GameSystem.CurrentData  = new SavePlayInfo();
			}
		#else
		//#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		{
			//	バイナリ形式でデシリアライズ
			BinaryFormatter	TempBinaryFormatter = new BinaryFormatter();

			//	指定したパスのファイルストリームを開く
			FileStream	TempFileStream = File.Open(debugFilePath + "/Autosave" + fileId.ToString() + ".dat", FileMode.Open);
			try 
			{
				//	指定したファイルストリームをオブジェクトにデシリアライズ。
				GameSystem.CurrentData = (SavePlayInfo)TempBinaryFormatter.Deserialize(TempFileStream);
			}
			finally 
			{
				//	ファイル操作には明示的な破棄が必要です。Closeを忘れないように。
				if( TempFileStream != null )
				{
					TempFileStream.Close();
				}
			}
		}
		#endif
		//#endif
    }
	public static bool ExistsLoadFile(int fileId = 0)
	{
#if UNITY_WEBGL
		return PlayerPrefs.GetString("PlayerData") != "";
#else
		return File.Exists(debugFilePath + "/Autosave" + fileId.ToString() + ".dat");
#endif
	}

	public static void SaveConfigStart(SaveConfigInfo pSourceSavePlayInfo = null)
    {
#if UNITY_WEBGL
		
		//	バイナリ形式でシリアル化
		BinaryFormatter TempBinaryFormatter = new BinaryFormatter ();
		MemoryStream    memoryStream    = new MemoryStream ();
		TempBinaryFormatter.Serialize (memoryStream , pSourceSavePlayInfo);
		var saveData = Convert.ToBase64String (memoryStream   .GetBuffer ());
		PlayerPrefs.SetString("ConfigData", AESManager.Encrypt(saveData));

#else
		//	保存情報
		if( pSourceSavePlayInfo == null )
		{
			pSourceSavePlayInfo = new SaveConfigInfo();
		}


		//#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		{
			//	バイナリ形式でシリアル化
			BinaryFormatter	TempBinaryFormatter = new BinaryFormatter();

			//	指定したパスにファイルを作成
			FileStream TempFileStream = File.Create(debugFilePath + "/Autoconfig.dat");

			//	Closeが確実に呼ばれるように例外処理を用いる
			try
			{
				//	指定したオブジェクトを上で作成したストリームにシリアル化する
				TempBinaryFormatter.Serialize(TempFileStream, pSourceSavePlayInfo);
			}
			finally
			{
				//	ファイル操作には明示的な破棄が必要です。Closeを忘れないように。
				if( TempFileStream != null )
				{
					TempFileStream.Close();
				}
			}
		}
#endif
	}


	public static void LoadConfigStart()
	{
		#if UNITY_WEBGL
				
		try
			{
				//	バイナリ形式でデシリアライズ
				BinaryFormatter	TempBinaryFormatter = new BinaryFormatter();
				string saveData = PlayerPrefs.GetString("ConfigData");
				saveData = AESManager.Decrypt(saveData);
        		MemoryStream    memoryStream    = new MemoryStream (Convert.FromBase64String (saveData));
        		GameSystem.ConfigData = (SaveConfigInfo)TempBinaryFormatter.Deserialize (memoryStream);
			}
			// Jsonへの展開失敗　改ざんの可能性あり
			catch(Exception e)
			{
				// 例外が発生するのでここで処理
				Debug.LogException(e);
				Debug.Log("改ざんされたため　冒険の書は消えてしまいました");
				GameSystem.ConfigData  = new SaveConfigInfo();
			}
		#else
		//#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		{
			//	バイナリ形式でデシリアライズ
			BinaryFormatter	TempBinaryFormatter = new BinaryFormatter();

			//	指定したパスのファイルストリームを開く
			FileStream	TempFileStream = File.Open(debugFilePath + "/Autoconfig.dat", FileMode.Open);
			try 
			{
				//	指定したファイルストリームをオブジェクトにデシリアライズ。
				GameSystem.ConfigData = (SaveConfigInfo)TempBinaryFormatter.Deserialize(TempFileStream);
			}
			finally 
			{
				//	ファイル操作には明示的な破棄が必要です。Closeを忘れないように。
				if( TempFileStream != null )
				{
					TempFileStream.Close();
				}
			}
		}
		#endif
    }
	public static bool ExistsConfigFile()
	{
#if UNITY_WEBGL
		return PlayerPrefs.GetString("ConfigData") != "";
#else
		return File.Exists(debugFilePath + "/Autoconfig.dat");
#endif
	}
}

[Serializable]
public class SavePlayInfo
{
	private PlayerInfo _playerInfo = null;
    public PlayerInfo PlayerInfo => _playerInfo;
    private List<ActorInfo> _actors = new ();
    public List<ActorInfo> Actors => _actors;
	
    private List<StageInfo> _stages = new ();
    public List<StageInfo> Stages => _stages;
    private PartyInfo _party = null;
	public PartyInfo Party => _party;
    private StageInfo _currentStage = null;
	public StageInfo CurrentStage => _currentStage;
    private AlcanaInfo _currentAlcana = null;
	public AlcanaInfo CurrentAlcana => _currentAlcana;

	private bool _resumeStage = true;
	public bool ResumeStage => _resumeStage;
	public void SetResumeStage(bool resumeStage)
	{
		_resumeStage = resumeStage;
	}
    public SavePlayInfo()
    {
		this.InitActors();
		this.InitStages();
		this.InitParty();
		_playerInfo = new PlayerInfo();
	}

    public void InitActors()
    {
        _actors.Clear();
    }

	public void MakeStageData(int actorId,int debugStageId = 0)
	{
		//InitActors();
		_party.InitActors();
		//AddActor(actorId);
		int stageId = 0;
		if (debugStageId != 0)
		{
			stageId = debugStageId;
		} else
		{
			stageId = _party.StageId;
		}
		StagesData.StageData stageData = DataSystem.Stages.Find(a => a.Id == stageId);
		_currentStage = new StageInfo(stageData);
		_currentStage.AddSelectActorId(actorId);
		_currentAlcana = new AlcanaInfo();
		_currentAlcana.InitData();
	}


	public void AddActor(ActorInfo actorInfo)
	{
		_actors.Add(actorInfo);
		_party.AddActor(actorInfo.ActorId);
	}
	
	public void AddTestActor(ActorsData.ActorData actorData)
	{
		if (actorData != null)
		{
			ActorInfo actorInfo = new ActorInfo(actorData);
			actorInfo.InitSkillInfo(actorData.LearningSkills);
			_actors.Add(actorInfo);
			_party.AddActor(actorInfo.ActorId);
		}
	}

    public void InitStages()
    {
        _stages.Clear();
    }

    public void InitParty()
    {
        _party = new PartyInfo();
		_party.ChangeCurrency(DataSystem.System.InitCurrency);
    }

	public void InitSaveData()
	{
		this.InitPlayer();
		this.InitStageInfo();
	}

	public void InitPlayer()
	{
		for (int i = 0;i < DataSystem.InitActors.Count;i++)
		{
			ActorsData.ActorData actorData = DataSystem.Actors.Find(actor => actor.Id == DataSystem.InitActors[i]);
			if (actorData != null)
			{
				ActorInfo actorInfo = new ActorInfo(actorData);
				actorInfo.InitSkillInfo(actorData.LearningSkills);
				_actors.Add(actorInfo);
				_party.AddActor(actorInfo.ActorId);
			}
		}
	}

	private void InitStageInfo()
	{
		for (int i = 0;i < DataSystem.Stages.Count;i++)
		{
			if (DataSystem.Stages[i].Id > 3) continue;
			StageInfo stageInfo = new StageInfo(DataSystem.Stages[i]);
			_stages.Add(stageInfo);
		}
	}

	public void StageClaer()
	{
		StageInfo stageInfo = _stages.Find(a => a.Id == CurrentStage.Id);
		stageInfo.GainClearCount();
	}

	public void StageClaer(int stageId)
	{
		StageInfo stageInfo = _stages.Find(a => a.Id == stageId);
		stageInfo.GainClearCount();
	}

	public void SetPlayerName(string name)
	{
		_playerInfo.SetPlayerName(name);
		_playerInfo.SetPlayerId();
	}

	public void ChangeRouteSelectStage(int stageId)
	{
		StagesData.StageData stageData = DataSystem.Stages.Find(a => a.Id == stageId);
		var current = _currentStage;
		var currentStage = new StageInfo(stageData);
		currentStage.RouteSelectData(current);
		_currentStage = currentStage;
		if (stageId == 11){
			_currentStage.SetEndingType(EndingType.C);
		} else
		if (stageId == 12){
			_currentStage.SetEndingType(EndingType.B);
		} else
		if (stageId == 13){
			_currentStage.SetEndingType(EndingType.A);
		}
	}

	public void ClearStageInfo()
	{
		_currentStage = null;
	}
}

[Serializable]
public class SaveConfigInfo
{
	public float _bgmVolume;
	public bool _bgmMute;
	public float _seVolume;
	public bool _seMute;
	public int _graphicIndex;
	public bool _eventSkipIndex;
	public bool _commandEndCheck;
	public bool _battleWait;
	public bool _battleAnimationSkip;
	public bool _inputType;
	public bool _battleManual;
    public SaveConfigInfo()
    {
		this.InitParameter();
	}

	public void InitParameter()
	{
		_bgmVolume = 1.0f;
		_bgmMute = false;
		_seVolume = 1.0f;
		_seMute = false;
		_graphicIndex = 2;
		_eventSkipIndex = false;
		_commandEndCheck = true;
		_battleWait = true;
		_battleAnimationSkip = false;
		_inputType = false;
		_battleManual = true;
	}

	public void UpdateSoundParameter(float bgmVolume,bool bgmMute,float seVolume,bool seMute)
	{
		_bgmVolume = bgmVolume;
		_bgmMute = bgmMute;
		_seVolume = seVolume;
		_seMute = seMute;
	}
}