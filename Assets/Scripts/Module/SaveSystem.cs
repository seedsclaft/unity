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
		}

		public static void SavePlayerInfo(SaveInfo userSaveInfo = null)
		{
			//	保存情報
			if( userSaveInfo == null )
			{
				userSaveInfo = new SaveInfo();
			}
	#if UNITY_ANDROID || UNITY_STANDALONE_WIN
			SaveInfo(SaveFilePath(_playerDataKey),userSaveInfo);
	#elif UNITY_WEBGL 
			SaveInfo(_playerDataKey,userSaveInfo);
	#endif
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
	#if UNITY_ANDROID || UNITY_STANDALONE_WIN
			SaveInfo(SaveFilePath(_playerStageDataKey,fileId),userSaveInfo);
	#elif UNITY_WEBGL
			SaveInfo(PlayerStageDataKey(fileId),userSaveInfo);
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
		}

		public static bool ExistsStageFile()
		{
			return ExistsLoadFile(_playerStageDataKey);
		}

		public static void SaveConfigStart(SaveConfigInfo userSaveInfo = null)
		{
			//	保存情報
			if( userSaveInfo == null )
			{
				userSaveInfo = new SaveConfigInfo();
			}
	#if UNITY_ANDROID || UNITY_STANDALONE_WIN
			SaveInfo(SaveFilePath(_optionDataKey),userSaveInfo);
	#elif UNITY_WEBGL
			SaveInfo(_optionDataKey,userSaveInfo);
	#endif
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
		}

		public static bool ExistsConfigFile()
		{
			return ExistsLoadFile(_optionDataKey);
		}

		public static void DeletePlayerData(int fileId = 0)
		{
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
		}
	}
}