using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem : MonoBehaviour
{

	private static readonly string debugFilePath = Application.persistentDataPath;
	public static void SaveStart(SaveInfo pSourceSavePlayInfo = null,int fileId = 0)
	{
		#if UNITY_WEBGL
			//	バイナリ形式でシリアル化
			BinaryFormatter TempBinaryFormatter = new BinaryFormatter ();
			MemoryStream memoryStream = new MemoryStream ();
			TempBinaryFormatter.Serialize (memoryStream , pSourceSavePlayInfo);
			var saveData = Convert.ToBase64String (memoryStream   .GetBuffer ());
			if (fileId != 0)
			{
				PlayerPrefs.SetString("PlayerData" + fileId.ToString(), saveData);
			} else
			{
				PlayerPrefs.SetString("PlayerData", saveData);
			}
		#else
			//	保存情報
			if( pSourceSavePlayInfo == null )
			{
				pSourceSavePlayInfo = new SaveInfo();
				pSourceSavePlayInfo.InitSaveData();
			}


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
		/*
		#if UNITY_SWITCH
		{
			SwitchSaveDataMain	pSwitchSaveDataMain = GameInstance.Get.m_pSwitchMain.m_pSwitchSaveDataMain;

			MemoryStream	TempMemoryStream    = new MemoryStream(); 
			BinaryFormatter	TempBinaryFormatter = new BinaryFormatter();

			//	バイナリ形式でシリアル化
			TempBinaryFormatter.Serialize( TempMemoryStream, pSourceSavePlayInfo );

			//	セーブ開始
			pSwitchSaveDataMain.StartAutoSave( TempMemoryStream.ToArray() );
		}
		#endif
		*/
	}

		
	public static bool LoadStart(int fileId = 0)
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
                var bytes = Convert.FromBase64String(saveData);
        		MemoryStream    memoryStream    = new MemoryStream (bytes);
				GameSystem.CurrentData = (SavePlayInfo)TempBinaryFormatter.Deserialize (memoryStream);
				return true;
			}
			// Jsonへの展開失敗　改ざんの可能性あり
			catch(Exception e)
			{
				// 例外が発生するのでここで処理
				Debug.LogException(e);
				Debug.Log("改ざんされたため　冒険の書は消えてしまいました");
				GameSystem.CurrentData  = new SavePlayInfo();
				return false;
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
				GameSystem.CurrentData = (SaveInfo)TempBinaryFormatter.Deserialize(TempFileStream);
				return true;
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
		PlayerPrefs.SetString("ConfigData", saveData);

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