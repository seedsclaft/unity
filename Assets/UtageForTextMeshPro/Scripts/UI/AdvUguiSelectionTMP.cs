// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;

namespace Utage
{
	/// <summary>
	/// 選択肢用UIのサンプル
	/// </summary>
	[AddComponentMenu("Utage/ADV/TMP/UiSelection")]
	public class AdvUguiSelectionTMP : AdvUguiSelection
	{
		public TextMeshProNovelText textMeshPro;

		public override void Init(AdvSelection data, Action<AdvUguiSelection> ButtonClickedEvent)
		{
			this.data = data;
			this.textMeshPro.SetText(data.Text);

			UnityEngine.UI.Button button = this.GetComponent<UnityEngine.UI.Button> ();
			button.onClick.AddListener( ()=>ButtonClickedEvent(this) );
		}

		/// <summary>
		/// 選択済みの場合色を変える
		/// </summary>
		/// <param name="data">選択肢データ</param>
		public override void OnInitSelected( Color color )
		{
			this.textMeshPro.Color = color;
		}
	}
}
