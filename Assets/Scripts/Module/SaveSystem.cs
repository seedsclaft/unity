using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Cysharp.Threading.Tasks;

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
		private static readonly string _replayDataKey = "ReplayData_";
		private static string ReplayDataKey(string stageKey)
		{
			return _replayDataKey + stageKey;
		}

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
				}
			}
			return default;
		}

		private static async UniTask<SaveBattleInfo> LoadFileAsync(string key)
		{
			if (_useEasySave)
			{
				try
				{
					var data = ES3.Load<string>(key,key);
					var bytes = Convert.FromBase64String(data);
					var	TempBinaryFormatter = new BinaryFormatter();
					var memoryStream = new MemoryStream(bytes);
					var saveData = (SaveBattleInfo)TempBinaryFormatter.Deserialize(memoryStream);
					await UniTask.WaitUntil(() => saveData != null);
					return saveData;
				} catch(Exception e)
				{
					Debug.LogException(e);
					return null;
				} finally 
				{
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
			var playerInfo = LoadFile<SaveInfo>(_playerDataKey,(a) => 
			{
				GameSystem.CurrentData = a;
			});
			return playerInfo != null;
		}

		public static void SaveReplay(string stageKey,SaveBattleInfo userSaveInfo = null)
		{
			SaveFile(ReplayDataKey(stageKey),userSaveInfo);
		}
			
		public static async UniTask<SaveBattleInfo> LoadReplay(string stageKey)
		{
			var playerInfo = await LoadFileAsync(ReplayDataKey(stageKey));
			return playerInfo;
		}

		public static bool ExistReplay(string stageKey)
		{
			return ES3.FileExists(ReplayDataKey(stageKey));
		}

		private static bool ExistsLoadFile(string key)
		{
			if (_useEasySave)
			{
				return ES3.FileExists(key);
			}
			return false;
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
			var playerInfo = LoadFile<SaveStageInfo>(PlayerStageDataKey(fileId),(a) => 
			{
				GameSystem.CurrentStageData = a;
				//GameSystem.CurrentStageData.Party.InitScorePrizeInfos();
			});
			return playerInfo != null;
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
			var playerInfo = LoadFile<SaveConfigInfo>(_optionDataKey,(a) => 
			{
				GameSystem.ConfigData = a;
			});
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
			}
		}

		public static void DeleteStageData(int fileId = 0)
		{
			if (_useEasySave)
			{
				ES3.DeleteFile(PlayerStageDataKey(fileId));
			}
		}
	}
}