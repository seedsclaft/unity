// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;

namespace Utage
{
	//宴のAddressableAssetsSystem対応のための
	//個々のファイル処理
	public class UtageForAddressableCustomFile : AssetFileBase
	{
		UtageForAddressableCustomFileManager CustomFileManager { get; set; }

		public UtageForAddressableCustomFile(AssetFileManager manager, AssetFileInfo fileInfo, IAssetFileSettingData settingData)
			: base(manager, fileInfo, settingData)
		{
			CustomFileManager = manager.GetComponent<UtageForAddressableCustomFileManager>();
		}


		//アセットのアドレスを取得
		//宴側が認識しているファイルパス（FileInfo.FileName）を、Addressableに設定したアドレスになるように変換する
		public string GetAdress()
		{
			//FileInfo.FileNameは　
			//   AdvEngineStaterの「Root Resource Dir」
			//   + TexureやSoundなどの各リソースごとのディレクトリ
			//   + 宴のエクセルの各シートで設定したファイルパス
			//というパスが帰ってくる。
			string adress = CustomFileManager.RootAddress + FileInfo.FileName;
			return adress;
		}

		public override bool CheckCacheOrLocal() { return false; }

		//ロード処理
		public override IEnumerator LoadAsync(System.Action onComplete, System.Action onFailed)
		{
			if (Priority == AssetFileLoadPriority.DownloadOnly)
			{
				yield return DownLoadAsync(onComplete, onFailed);
			}
			else
			{
				yield return LoadAsyncNormal(onComplete, onFailed);
			}
		}

		IEnumerator DownLoadAsync(System.Action onComplete, System.Action onFailed)
		{
			string key = GetAdress();
			Debug.Log("DownLoad" + key);
			var op = Addressables.DownloadDependenciesAsync(key);
			yield return op;
			if (op.Status == AsyncOperationStatus.Failed)
			{
				onFailed();
			}
			else
			{
				onComplete();
			}
		}

		IEnumerator LoadAsyncNormal(System.Action onComplete, System.Action onFailed)
		{
			switch (FileType)
			{
				case AssetFileType.Text:        //テキスト
					yield return LoadAsyncNormalSub<TextAsset>((result) =>
					{
						Text = result;
						onComplete();
					},
					onFailed);
					break;
				case AssetFileType.Texture:     //テクスチャ
					yield return LoadAsyncNormalSub<Texture2D>((result) =>
					{
						Texture = result;
						onComplete();
					},
					onFailed);
					break;
				case AssetFileType.Sound:       //サウンド
					yield return LoadAsyncNormalSub<AudioClip>((result) =>
					{
						Sound = result;
						onComplete();
					},
					onFailed);
					break;
				case AssetFileType.UnityObject:     //Unityオブジェクト（プレハブとか）
					yield return LoadAsyncNormalSub<UnityEngine.Object>((result) =>
					{
						this.UnityObject = result;
						onComplete();
					},
					onFailed);
					break;
				default:
					break;
			}
		}

		IEnumerator LoadAsyncNormalSub<TObject>(System.Action<TObject> onComplete, System.Action onFailed)
		{
			string key = GetAdress();
			Debug.Log("Load" + key);
			var op = Addressables.LoadAssetAsync<TObject>(key);
			yield return op;
			if (op.Status == AsyncOperationStatus.Failed)
			{
				onFailed();
			}
			else
			{
				IsLoadEnd = true;
				onComplete(op.Result);
			}
		}


		//アンロード処理
		public override void Unload()
		{
			string key = GetAdress();
			switch (FileType)
			{
				case AssetFileType.Text:        //テキスト
					Addressables.Release(Text);
					Text = null;
					break;
				case AssetFileType.Texture:     //テクスチャ
					Addressables.Release(Texture);
					Texture = null;
					break;
				case AssetFileType.Sound:       //サウンド
					Addressables.Release(Sound);
					Sound = null;
					break;
				case AssetFileType.UnityObject:     //Unityオブジェクト（プレハブとか）
					Addressables.Release(UnityObject);
					UnityObject = null;
					break;
				default:
					break;
			}
			IsLoadEnd = false;
		}
	}
}
