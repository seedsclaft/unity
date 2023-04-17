// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UtageExtensions;

namespace Utage
{
	/// <summary>
	/// バックログ用UI
	/// </summary>
	[AddComponentMenu("Utage/ADV/TMP/UiBacklog")]
	[RequireComponent(typeof(Button))]
	public class AdvUguiBacklogTMP : AdvUguiBacklog
	{
		public TextMeshProNovelText textMeshProLogText;
		public TextMeshProNovelText textMeshProCharacterName;

		TMP_Text TextMeshPro { get { return textMeshProLogText.TextMeshPro;} }

		public override void Init(Utage.AdvBacklog data)
		{
			this.data = data;
			int countVoice = data.CountVoice;
			var textString = data.Text;

			if (isMultiTextInPage)
			{
				float defaultHeight = this.TextMeshPro.rectTransform.rect.height;
				RectTransform r = (RectTransform)this.transform; 
				this.textMeshProLogText.SetText(textString);
				this.textMeshProLogText.ForceUpdate();
				float height = this.TextMeshPro.preferredHeight;
				this.TextMeshPro.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

				float baseH = r.rect.height;
				float scale = this.textMeshProLogText.transform.lossyScale.y / r.lossyScale.y;
				baseH += (height - defaultHeight) * scale;
				r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, baseH);
			}
			else
			{
				this.textMeshProLogText.SetText(textString);
			}

			textMeshProCharacterName.SetText(data.MainCharacterNameText);

			if (countVoice <= 0)
			{
				soundIcon.SetActive(false);
				Button.interactable = false;
			}
			else
			{
				if (countVoice >= 2 && isMultiTextInPage)
				{
					AdvUguiBacklogTMPEventTrigger trigger = textMeshProLogText.gameObject.GetComponentCreateIfMissing<AdvUguiBacklogTMPEventTrigger>();
					trigger.InitAsBackLog(this);
				}
				else
				{
					Button.onClick.AddListener(() => OnClicked(data.MainVoiceFileName));
				}
			}
		}

		public void OnClicked(AdvBacklog.AdvBacklogDataInPage dataInPage )
		{
			OnClicked(dataInPage.VoiceFileName);
		}
	}
}

