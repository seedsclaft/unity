using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem : MonoBehaviour
{

	private static readonly string debugFilePath = Application.persistentDataPath + "/Autosave.dat";
    public static void SaveStart(SavePlayInfo pSourceSavePlayInfo = null)
        {
            //	保存情報
            if( pSourceSavePlayInfo == null )
            {
                //GameInstance.Get.SavePlayInfo.CardImportFromWork();	//	カード情報をワークからインポート
                pSourceSavePlayInfo = new SavePlayInfo();
				pSourceSavePlayInfo.InitSaveData();
            }
            //pSourceSavePlayInfo.SavePointSet(savePointType);


            //#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            {
                //	バイナリ形式でシリアル化
                BinaryFormatter	TempBinaryFormatter = new BinaryFormatter();

			    //	指定したパスにファイルを作成
                FileStream TempFileStream = File.Create(debugFilePath);

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
		//#endif
	}

		
	public static void LoadStart()
	{
		//#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		{
			//	バイナリ形式でデシリアライズ
			BinaryFormatter	TempBinaryFormatter = new BinaryFormatter();

			//	指定したパスのファイルストリームを開く
			FileStream	TempFileStream = File.Open(debugFilePath, FileMode.Open);
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
		//#endif
    }
}

[Serializable]
public class SavePlayInfo
{
	public	const	int		SAVEDATA_VER = 100;

	public PlayerInfo _playerInfo = null;
    private List<ActorInfo> _actors = new List<ActorInfo>();
    public List<ActorInfo> Actors {get {return _actors;}}
	
    private List<StageInfo> _stages = new List<StageInfo>();
    public List<StageInfo> Stages {get {return _stages;}}
    private PartyInfo _party = null;
	public PartyInfo Party { get {return _party;}}
    private StageInfo _currentStage = null;
	public StageInfo CurrentStage { get {return _currentStage;}}
    private AlcanaInfo _currentAlcana = null;
	public AlcanaInfo CurrentAlcana { get {return _currentAlcana;}}

	private List<TroopsData.TroopData> _troopDatas = new List<TroopsData.TroopData>();
	public List<TroopsData.TroopData> TroopDatas { get {return _troopDatas;}}
    public SavePlayInfo()
    {
		this.InitActors();
		this.InitStages();
		this.InitParty();
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
			stageId = _party.StageId;;
		}
		StagesData.StageData stageData = DataSystem.Stages.Find(a => a.Id == stageId);
		_currentStage = new StageInfo(stageData);
		_currentStage.AddSelectActorId(actorId);
		_currentAlcana = new AlcanaInfo();
		_currentAlcana.InitData();
	}


	public void AddActor(int actorId)
	{
		ActorsData.ActorData actorData = DataSystem.Actors.Find(actor => actor.Id == actorId);
		if (actorData != null)
		{
			//ActorInfo actorInfo = new ActorInfo(actorData);
			//actorInfo.InitSkillInfo(actorData.LearningSkills);
			//_actors.Add(actorInfo);
			_party.AddActor(actorId);
		}
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

	private void InitPlayer()
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
			StageInfo stageInfo = new StageInfo(DataSystem.Stages[i]);
			_stages.Add(stageInfo);
		}
	}
}