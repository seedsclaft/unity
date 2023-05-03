// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

using UnityEngine;

namespace Utage
{
	//宴のAddressableAssetsSystem対応のための
	//ファイルマネージャーへのカスタム処理
	public class UtageForAddressableCustomFileManager : MonoBehaviour
	{
		//Addressableに設定したアドレスを取得する際の、ルートディレクトリとなるパスを指定
		public string RootAddress { get { return rootAddress; } }
		[SerializeField]
		string rootAddress = "";

		//ロードを上書きするコールバックを登録
		void Awake()
		{
			AssetFileManager.GetCustomLoadManager().OnFindAsset += FindAsset;
		}

		void FindAsset(AssetFileManager mangager, AssetFileInfo fileInfo, IAssetFileSettingData settingData, ref AssetFileBase asset)
		{
			asset = new UtageForAddressableCustomFile(mangager, fileInfo, settingData);
		}
	}
}
