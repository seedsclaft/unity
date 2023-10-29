// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

namespace Utage
{
	// サウンド管理のインターフェース（SoundManagerSystemInterfaceにはない機能を追加する場合に、互換性をとるためにインターフェースを追加する）
	public interface SoundManagerSystemInterfaceEx
	{
		// 指定のグループのサウンドが鳴っているか
		bool IsPlaying(string groupName);
		// 指定のグループ名、ラベル、ファイルのサウンドが鳴っているかどうか
		bool IsPlaying(string groupName, string label, AssetFile file);
	}
}
