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
		private static string _gameKey = "norm";

#if !UNITY_WEBGL
		private static FileStream TempFileStream = null;
#endif
		private static readonly string debugFilePath = Application.persistentDataPath;
		private static string SaveFilePath(string saveKey,int fileId = 0)
		{
			return debugFilePath + "/" + saveKey + fileId.ToString() + ".dat";
		}
		
		private static readonly string _playerDataKey = _gameKey + "PlayerData";
		private static readonly string _playerStageDataKey = _gameKey + "PlayerStageData";
		private static string PlayerStageDataKey(int fileId)
		{
			return _playerStageDataKey + fileId.ToString();
		}
		private static readonly string _optionDataKey = _gameKey + "OptionData";
		private static readonly string _replayDataKey = _gameKey + "ReplayData_";
		private static string ReplayDataKey(string stageKey)
		{
			return _replayDataKey + stageKey;
		}

		private static void SaveFile<T>(string key,T data)
		{
			var TempBinaryFormatter = new BinaryFormatter();
			var memoryStream = new MemoryStream();
			TempBinaryFormatter.Serialize (memoryStream,data);
			var saveData = Convert.ToBase64String (memoryStream.GetBuffer());
			ES3.Save(key,saveData,key);
		}

		private static T LoadFile<T>(string key,Action<T> successAction)
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
			return default;
		}

		private static async UniTask<SaveBattleInfo> LoadFileAsync(string key)
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
			return ES3.FileExists(key);
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

		public static void SaveConfigStart(SaveConfigInfo userSaveInfo)
		{
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

		public static void DeleteAllData(int fileId = 0)
		{
			DeletePlayerData();
			DeleteStageData(fileId);
			DeleteConfigData();
	}

		public static void DeletePlayerData()
		{
			ES3.DeleteFile(_playerDataKey);
		}

		public static void DeleteStageData(int fileId = 0)
		{
			ES3.DeleteFile(PlayerStageDataKey(fileId));
		}

		public static void DeleteConfigData()
		{
			ES3.DeleteFile(_optionDataKey);
		}
	}
}