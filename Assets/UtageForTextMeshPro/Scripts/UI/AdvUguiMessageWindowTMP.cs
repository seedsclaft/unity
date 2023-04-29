// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;
using Utage;

namespace Utage
{

	/// <summary>
	/// メッセージウィドウ処理のサンプル
	/// </summary>
	[AddComponentMenu("Utage/ADV/UiMessageWindowTMP")]
	public class AdvUguiMessageWindowTMP : AdvUguiMessageWindow
	{
		/// <summary>本文テキスト</summary>
		public TextMeshProNovelText TextPro { get { return textPro; } }
		[SerializeField]
		TextMeshProNovelText textPro = null;

		/// <summary>名前テキスト</summary>
		public TextMeshProNovelText NameTextPro { get { return nameTextPro; } }
		[SerializeField]
		TextMeshProNovelText nameTextPro = null;

		//ゲーム起動時の初期化
		public override void OnInit(AdvMessageWindowManager windowManager)
		{
			if (TextPro)
			{
				defaultTextColor = TextPro.Color;
			}
			if (NameTextPro)
			{
				defaultNameTextColor = NameTextPro.Color;
			}
			Clear();
		}

		protected override void Clear()
		{
			if (TextPro) TextPro.Clear();
			if (NameTextPro) NameTextPro.Clear();
			if (iconWaitInput) iconWaitInput.SetActive(false);
			if (iconBrPage) iconBrPage.SetActive(false);
			rootChildren.SetActive(false);
		}

		//テキストに変更があった場合
		public override void OnTextChanged(AdvMessageWindow window)
		{
			if (TextPro)
			{
				TextPro.SetNovelTextData(window.Text, window.TextLength);
			}

			NameTextPro.gameObject.SetActive(window.NameText != "");
			if (NameTextPro)
			{
				NameTextPro.SetText(window.NameText);
			}

			switch (readColorMode)
			{
				case ReadColorMode.Change:
					if (TextPro)
					{
						TextPro.Color = Engine.Page.CheckReadPage() ? readColor : defaultTextColor;
					}
					if (NameTextPro)
					{
						NameTextPro.Color = Engine.Page.CheckReadPage() ? readColor : defaultNameTextColor;
					}
					break;
				case ReadColorMode.None:
				default:
					break;
			}

			LinkIcon();
		}


		//現在のメッセージウィンドウの場合のみの更新
		protected override void UpdateCurrent()
		{
			if (!IsCurrent) return;

			if (Engine.UiManager.Status == AdvUiManager.UiStatus.Default)
			{
				if (Engine.UiManager.IsShowingMessageWindow)
				{
					//テキストの文字送り
					if (TextPro)
					{
						TextPro.MaxVisibleCharacters = Engine.Page.CurrentTextLength;
					}
				}
				LinkIcon();
			}
		}


		//アイコンの場所をテキストの末端にあわせる
		protected override void LinkIconSub(GameObject icon, bool isActive)
		{
			if (icon == null) return;

			if (!Engine.UiManager.IsShowingMessageWindow)
			{
				icon.SetActive(false);
			}
			else
			{
				icon.SetActive(isActive);
				if (isActive)
				{
					UnityEngine.Profiling.Profiler.BeginSample("TextPro.CurrentEndPosition");
					if (isLinkPositionIconBrPage) icon.transform.localPosition = TextPro.CurrentEndPosition;
					UnityEngine.Profiling.Profiler.EndSample();
				}
			}
		}

		//表示文字数チェック開始（今設定されているテキストを返す）
		public override string StartCheckCaracterCount()
		{
			if (TextPro == null)
			{
				return "";
			}
			return TextPro.GetText();
		}

		//指定テキストに対する表示文字数チェック
		public override bool TryCheckCaracterCount(string text, out int count, out string errorString)
		{
			if (TextPro == null)
			{
				errorString = "TextMeshPro has not been added.";
				count = 0;
				return true;
			}
			return TextPro.TryCheckCaracterCount(text, out count, out errorString);
		}

		//Startで設定されていたテキストに戻す
		public override void EndCheckCaracterCount(string text)
		{
			if (TextPro == null)
			{
				return;
			}
			TextPro.SetTextDirect(text);
		}
	}
}
