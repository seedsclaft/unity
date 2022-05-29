﻿using System;
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


            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
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
		#endif
	}

		
	public static void LoadStart()
	{
		#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		{
			//	バイナリ形式でデシリアライズ
			BinaryFormatter	TempBinaryFormatter = new BinaryFormatter();

			//	指定したパスのファイルストリームを開く
			FileStream	TempFileStream = File.Open(debugFilePath, FileMode.Open);
			try 
			{
				//	指定したファイルストリームをオブジェクトにデシリアライズ。
				var m_pSavePlayInfo = (SavePlayInfo)TempBinaryFormatter.Deserialize(TempFileStream);
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
}

[Serializable]
public class SavePlayInfo
{
	public	const	int		SAVEDATA_VER = 100;

	public PlayerInfo _playerInfo = null;
    private List<ActorInfo> _actors = new List<ActorInfo>();
    public List<ActorInfo> Actors {get {return _actors;}}
    public PartyInfo _party = null;
	/// <summary>
	/// 初期化
    /// </summary>
    public SavePlayInfo()
    {
		this.InitActors();
		this.InitParty();
	}

    public void InitActors()
    {
        _actors.Clear();
    }

    public void InitParty()
    {
        _party = new PartyInfo();
    }

	public void InitSaveData()
	{
		this.InitPlayer();
	}

	private void InitPlayer()
	{
		for (int i = 0;i < DataSystem.InitActors.Count;i++)
		{
			var actorInfo = DataSystem.Actors.Find(actor => actor.Id == DataSystem.InitActors[i]);
			if (actorInfo != null)
			{
				var actor = new	ActorInfo(actorInfo);
				_actors.Add(actor);
				_party.AddActor(actor.ActorId);
			}
		}
	}

	

}