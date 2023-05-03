// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;

namespace Utage
{
	// チャプター0で全共通設定（共通リソースやマクロや、パラメーター設定）を定義し、
	// チャプター1～は個別にロードする場合のサンプルの
	//　AddressableAssetsSystem対応版
	public class UtageForAddressableChapterLoad : MonoBehaviour
	{
		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine { get { return this.engine ?? (this.engine = FindObjectOfType<AdvEngine>()); } }
		[SerializeField]
		protected AdvEngine engine;

		public List<string> chapterKeyList = new List<string>();

		//指定インデックスのチャプターをロード
		public void LoadChpater(int chapterIndex)
		{
			StartCoroutine(LoadChaptersAsync(chapterIndex));
		}

		//ロードしたチャプターで使う未DLのファイルサイズの合計
		public long DownloadSize { get; set; }

		//シナリオ内で使用するファイルのみダウンロード
		public void DownloadResources()
		{
			this.Engine.DataManager.DownloadAllFileUsed();
		}

		IEnumerator LoadChaptersAsync(int chapterIndex)
		{
			string key = chapterKeyList[chapterIndex];
			//もう設定済みならロードしない
			if (!this.Engine.ExitsChapter(key))
			{
				yield return this.Engine.LoadChapterAsync(key);
			}
			//設定データを反映
			this.Engine.GraphicManager.Remake(this.Engine.DataManager.SettingDataManager.LayerSetting);


			//DL前にファイルサイズを取得する場合に備えDLサイズを取得する
			yield return GetDownloadSizeAync();
		}


		IEnumerator GetDownloadSizeAync()
		{
			var fileSet = this.Engine.DataManager.GetAllFileSet();
			DownloadSize = 0;
			List<AsyncOperationHandle<long>> opList = new List<AsyncOperationHandle<long>>();
			foreach (var file in fileSet)
			{
				UtageForAddressableCustomFile assetFile = file as UtageForAddressableCustomFile;
				if (assetFile == null)
				{
					Debug.LogError("Not Support Type");
					continue;
				}
				else
				{
					var op = Addressables.GetDownloadSizeAsync(assetFile.GetAdress());
					opList.Add(op);
				}
			}
			foreach (var op in opList)
			{
				while (!op.IsDone)
				{
					yield return null;
				}
				DownloadSize = op.Result;
			}
		}
	}
}
