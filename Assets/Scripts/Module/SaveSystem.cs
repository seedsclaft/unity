using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Ryneus
{
	public class SaveSystem : MonoBehaviour
	{
		private static bool _useEasySave = true;

	#if !UNITY_WEBGL
		private static FileStream TempFileStream = null;
	#endif
		private static readonly string debugFilePath = Application.persistentDataPath;
		private static string SaveFilePath(string saveKey,int fileId = 0)
		{
			return debugFilePath + "/" + saveKey + fileId.ToString() + ".dat";
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
			/*
	#if UNITY_WEBGL 
			//	バイナリ形式でシリアル化
			var TempBinaryFormatter = new BinaryFormatter();
			var memoryStream = new MemoryStream();
			TempBinaryFormatter.Serialize (memoryStream,userSaveInfo);
			var saveData = Convert.ToBase64String (memoryStream.GetBuffer());
			PlayerPrefs.SetString(path, saveData);
	#elif UNITY_STANDALONE_WIN
			var TempBinaryFormatter = new BinaryFormatter();
			var memoryStream = new MemoryStream();
			TempBinaryFormatter.Serialize (memoryStream,userSaveInfo);
			var saveData = Convert.ToBase64String (memoryStream.GetBuffer());
			StreamWriter Stm_Writer = new StreamWriter(path,false);
			Stm_Writer.Write(saveData);
			Stm_Writer.Flush();
			Stm_Writer.Close();
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
	*/
		}

		private static void SaveFile<T>(string key,T data)
		{
			if (_useEasySave)
			{
				var TempBinaryFormatter = new BinaryFormatter();
				var memoryStream = new MemoryStream();
				TempBinaryFormatter.Serialize (memoryStream,data);
				var saveData = Convert.ToBase64String (memoryStream.GetBuffer());
				ES3.Save(key,saveData,key);
			} else
			{
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
				SaveInfo(SaveFilePath(key),data);
#elif UNITY_WEBGL 
				SaveInfo(key,data);
#endif
			}
		}

		private static T LoadFile<T>(string key,Action<T> successAction)
		{
			if (_useEasySave)
			{
				try
				{
					var data = ES3.Load<string>(key,key);
					var bytes = Convert.FromBase64String(data);
					var	TempBinaryFormatter = new BinaryFormatter();
					var memoryStream = new MemoryStream(bytes);
					var saveData = (T)TempBinaryFormatter.Deserialize(memoryStream);
					successAction(saveData);
					return saveData;
				} catch(Exception e)
				{
					Debug.LogException(e);
				} finally 
				{
					//	ファイル操作には明示的な破棄が必要です。Closeを忘れないように。
					if( TempFileStream != null )
					{
						TempFileStream.Close();
					}
				}
			}
			return default;
		}

		public static void SavePlayerInfo(SaveInfo userSaveInfo = null)
		{
			//	保存情報
			if( userSaveInfo == null )
			{
				userSaveInfo = new SaveInfo();
			}
			SaveFile(_playerDataKey,userSaveInfo);
		}

			
		public static bool LoadPlayerInfo()
		{
			var playerInfo = LoadFile<SaveInfo>(_playerDataKey,(a) => {
				GameSystem.CurrentData = a;
			});
			return playerInfo != null;
			/*
	#if UNITY_WEBGL
			try
			{
				//	バイナリ形式でデシリアライズ
				var	TempBinaryFormatter = new BinaryFormatter();
				var saveData = PlayerPrefs.GetString(_playerDataKey);
				var bytes = Convert.FromBase64String(saveData);
				var memoryStream = new MemoryStream(bytes);
				GameSystem.CurrentData = (SaveInfo)TempBinaryFormatter.Deserialize (memoryStream);
				return true;
			}
			// Jsonへの展開失敗　改ざんの可能性あり
			catch(Exception e)
			{
				// 例外が発生するのでここで処理
				Debug.LogException(e);
				GameSystem.CurrentData = new SaveInfo();
				return false;
			}
	#elif UNITY_STANDALONE_WIN
			try
			{
				var	TempBinaryFormatter = new BinaryFormatter();
				StreamReader Stm_Reader = new StreamReader(SaveFilePath(_playerDataKey));
				var saveData = Stm_Reader.ReadToEnd();
				var bytes = Convert.FromBase64String(saveData);
				Stm_Reader.Close();
				var memoryStream = new MemoryStream(bytes);
				GameSystem.CurrentData = (SaveInfo)TempBinaryFormatter.Deserialize (memoryStream);
				return true;
			} catch(Exception e)
			{
				// 例外が発生するのでここで処理
				Debug.LogException(e);
				GameSystem.CurrentData = new SaveInfo();
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
	*/
		}

		private static bool ExistsLoadFile(string key)
		{
			if (_useEasySave)
			{
				return ES3.FileExists(key);
			}
			return false;
			/*
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
	*/
		}

		public static bool ExistsLoadPlayerFile()
		{
			return ExistsLoadFile(_playerDataKey);
		}

		public static void SaveStageInfo(SaveStageInfo userSaveInfo = null,int fileId = 0)
		{
			SaveFile(PlayerStageDataKey(fileId),userSaveInfo);
		}

		public static bool LoadStageInfo(int fileId = 0)
		{
			var playerInfo = LoadFile<SaveStageInfo>(PlayerStageDataKey(fileId),(a) => {
				GameSystem.CurrentStageData = a;
			});
			return playerInfo != null;
			/*
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
				GameSystem.CurrentData = new SaveInfo();
				return false;
			}
	#elif UNITY_STANDALONE_WIN
			try
			{
				StreamReader Stm_Reader = new StreamReader(SaveFilePath(_playerStageDataKey,fileId));
				var saveData = Stm_Reader.ReadToEnd();
				Stm_Reader.Close();
				//	バイナリ形式でデシリアライズ
				var	TempBinaryFormatter = new BinaryFormatter();
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
				GameSystem.CurrentData = new SaveInfo();
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
	*/
		}

		public static bool ExistsStageFile(int fileId = 0)
		{
			return ExistsLoadFile(PlayerStageDataKey(fileId));
		}

		public static void SaveConfigStart(SaveConfigInfo userSaveInfo = null)
		{
			//	保存情報
			if( userSaveInfo == null )
			{
				userSaveInfo = new SaveConfigInfo();
			}
			SaveFile(_optionDataKey,userSaveInfo);
		}


		public static void LoadConfigStart()
		{
			var playerInfo = LoadFile<SaveConfigInfo>(_optionDataKey,(a) => {
				GameSystem.ConfigData = a;
			});
			return;
			/*
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
	#elif UNITY_STANDALONE_WIN
			try
			{
				//	バイナリ形式でデシリアライズ
				StreamReader Stm_Reader = new StreamReader(SaveFilePath(_optionDataKey));
				var saveData = Stm_Reader.ReadToEnd();
				Stm_Reader.Close();
				var	TempBinaryFormatter = new BinaryFormatter();
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
			*/
		}

		public static bool ExistsConfigFile()
		{
			return ExistsLoadFile(_optionDataKey);
		}

		public static void DeletePlayerData(int fileId = 0)
		{
			if (_useEasySave)
			{
				ES3.DeleteFile(_playerDataKey);
				ES3.DeleteFile(PlayerStageDataKey(fileId));
			}
			/*
	#if UNITY_WEBGL
			try
			{
				PlayerPrefs.DeleteKey(_playerDataKey);
				PlayerPrefs.DeleteKey(_playerStageDataKey + fileId);
				GameSystem.CurrentStageData = new SaveStageInfo();
				GameSystem.CurrentData = new SaveInfo();
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}
	#elif UNITY_STANDALONE_WIN
			try
			{
				File.Delete(SaveFilePath(_playerDataKey));
				File.Delete(SaveFilePath(_playerStageDataKey + fileId));
				GameSystem.CurrentStageData = new SaveStageInfo();
				GameSystem.CurrentData = new SaveInfo();
			}
			catch(Exception e)
			{
				Debug.LogException(e);
			}
	#else
			try 
			{
				File.Delete(SaveFilePath(_playerDataKey));
				File.Delete(SaveFilePath(_playerStageDataKey + fileId));
				GameSystem.CurrentStageData = new SaveStageInfo();
				GameSystem.CurrentData = new SaveInfo();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			#endif
			*/
		}
	}
}