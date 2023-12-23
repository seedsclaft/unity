using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem : MonoBehaviour
{
#if !UNITY_WEBGL
	private static FileStream TempFileStream = null;
#endif
	private static readonly string debugFilePath = Application.persistentDataPath;
	private static string SaveFilePath(string saveKey,int fileId = 0)
	{
		return debugFilePath + "/" + saveKey + fileId + ".dat";
	}
	
	private static readonly string _playerDataKey = "PlayerData";
	private static readonly string _playerStageDataKey = "PlayerStageData";
	private static string PlayerStageDataKey(int fileId)
	{
		return _playerStageDataKey + fileId.ToString();
	}
	private static readonly string _optionDataKey = "OptionData";

	private static void SaveInfo<T>(string path,T userSaveInfo)
	{
#if UNITY_WEBGL
		//	バイナリ形式でシリアル化
		var TempBinaryFormatter = new BinaryFormatter();
		var memoryStream = new MemoryStream();
		TempBinaryFormatter.Serialize (memoryStream,userSaveInfo);
		var saveData = Convert.ToBase64String (memoryStream.GetBuffer());
		PlayerPrefs.SetString(path, saveData);
#else
		try
		{
			//	バイナリ形式でシリアル化
			var	TempBinaryFormatter = new BinaryFormatter();
			//	指定したパスにファイルを作成
			TempFileStream = File.Create(path);
			//	指定したオブジェクトを上で作成したストリームにシリアル化する
			TempBinaryFormatter.Serialize(TempFileStream, userSaveInfo);
		}
		finally
		{
			//	ファイル操作には明示的な破棄が必要です。Closeを忘れないように。
			if( TempFileStream != null )
			{
				TempFileStream.Close();
			}
		}
#endif
	}

	public static void SavePlayerInfo(SaveInfo userSaveInfo = null)
	{
		//	保存情報
		if( userSaveInfo == null )
		{
			userSaveInfo = new SaveInfo();
		}
		SaveInfo(_playerDataKey,userSaveInfo);
	}

		
	public static bool LoadPlayerInfo()
	{
#if UNITY_WEBGL
		try
		{
			//	バイナリ形式でデシリアライズ
			var	TempBinaryFormatter = new BinaryFormatter();
			var saveData = PlayerPrefs.GetString(_playerDataKey);
			var bytes = Convert.FromBase64String(saveData);
			var memoryStream = new MemoryStream(bytes);
			GameSystem.CurrentData = (SavePlayInfo)TempBinaryFormatter.Deserialize (memoryStream);
			return true;
		}
		// Jsonへの展開失敗　改ざんの可能性あり
		catch(Exception e)
		{
			// 例外が発生するのでここで処理
			Debug.LogException(e);
			GameSystem.CurrentData = new SavePlayInfo();
			return false;
		}
#else
		try 
		{
			//	バイナリ形式でデシリアライズ
			var	TempBinaryFormatter = new BinaryFormatter();
			//	指定したパスのファイルストリームを開く
			TempFileStream = File.Open(SaveFilePath(_playerDataKey), FileMode.Open);
			//	指定したファイルストリームをオブジェクトにデシリアライズ。
			GameSystem.CurrentData = (SaveInfo)TempBinaryFormatter.Deserialize(TempFileStream);
			return true;
		}
		catch (Exception e)
		{
			Debug.LogException(e);
			GameSystem.CurrentData = new SaveInfo();
			return false;
		}
		finally 
		{
			//	ファイル操作には明示的な破棄が必要です。Closeを忘れないように。
			if( TempFileStream != null )
			{
				TempFileStream.Close();
			}
		}
#endif
    }

	private static bool ExistsLoadFile(string key)
	{
#if UNITY_WEBGL
		return PlayerPrefs.GetString(key) != "";
#else
		try
		{
			return File.Exists(SaveFilePath(key));
		}
		catch(Exception e)
		{
			Debug.LogException(e);
			return false;
		}
#endif
	}

	public static bool ExistsLoadPlayerFile()
	{
		return ExistsLoadFile(_playerDataKey);
	}

	public static void SaveStageInfo(SaveStageInfo userSaveInfo = null,int fileId = 0)
	{
		//	保存情報
		if( userSaveInfo == null )
		{
			userSaveInfo = new SaveStageInfo();
		}
#if UNITY_WEBGL
		SaveInfo(PlayerStageDataKey(fileId),userSaveInfo);
#else
		SaveInfo(SaveFilePath(_playerStageDataKey,fileId),userSaveInfo);
#endif
	}

		
	public static bool LoadStageInfo(int fileId = 0)
	{
#if UNITY_WEBGL		
		try
		{
			//	バイナリ形式でデシリアライズ
			var	TempBinaryFormatter = new BinaryFormatter();
			var saveData = PlayerPrefs.GetString(PlayerStageDataKey(fileId));
			var bytes = Convert.FromBase64String(saveData);
			var memoryStream = new MemoryStream(bytes);
			GameSystem.CurrentStageData = (SaveStageInfo)TempBinaryFormatter.Deserialize (memoryStream);
			return true;
		}
		// Jsonへの展開失敗　改ざんの可能性あり
		catch(Exception e)
		{
			// 例外が発生するのでここで処理
			Debug.LogException(e);
			Debug.Log("改ざんされたため　冒険の書は消えてしまいました");
			GameSystem.CurrentData = new SaveStageInfo();
			return false;
		}
#else
		try 
		{
			//	バイナリ形式でデシリアライズ
			var	TempBinaryFormatter = new BinaryFormatter();
			//	指定したパスのファイルストリームを開く
			TempFileStream = File.Open(SaveFilePath(_playerStageDataKey,fileId), FileMode.Open);
			//	指定したファイルストリームをオブジェクトにデシリアライズ。
			GameSystem.CurrentStageData = (SaveStageInfo)TempBinaryFormatter.Deserialize(TempFileStream);
			return true;
		}
		catch (Exception e)
		{
			Debug.LogException(e);
			GameSystem.CurrentStageData = new SaveStageInfo();
			return false;
		}
		finally 
		{
			//	ファイル操作には明示的な破棄が必要です。Closeを忘れないように。
			if( TempFileStream != null )
			{
				TempFileStream.Close();
			}
		}
#endif
    }

	public static void SaveConfigStart(SaveConfigInfo userSaveInfo = null)
    {
		//	保存情報
		if( userSaveInfo == null )
		{
			userSaveInfo = new SaveConfigInfo();
		}
		SaveInfo(SaveFilePath(_optionDataKey),userSaveInfo);
	}


	public static void LoadConfigStart()
	{
#if UNITY_WEBGL
		try
		{
			//	バイナリ形式でデシリアライズ
			var	TempBinaryFormatter = new BinaryFormatter();
			var saveData = PlayerPrefs.GetString(_optionDataKey);
			var memoryStream = new MemoryStream(Convert.FromBase64String (saveData));
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
		try 
		{
			//	バイナリ形式でデシリアライズ
			var	TempBinaryFormatter = new BinaryFormatter();
			//	指定したパスのファイルストリームを開く
			TempFileStream = File.Open(SaveFilePath(_optionDataKey), FileMode.Open);
			//	指定したファイルストリームをオブジェクトにデシリアライズ。
			GameSystem.ConfigData = (SaveConfigInfo)TempBinaryFormatter.Deserialize(TempFileStream);
		}
		catch (Exception e)
		{
			Debug.LogException(e);
			GameSystem.ConfigData = new SaveConfigInfo();
		}
		finally 
		{
			//	ファイル操作には明示的な破棄が必要です。Closeを忘れないように。
			if( TempFileStream != null )
			{
				TempFileStream.Close();
			}
		}
		#endif
    }

	public static bool ExistsConfigFile()
	{
		return ExistsLoadFile(_optionDataKey);
	}
}