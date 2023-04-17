
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UtageExtensions;

namespace Utage
{
	//NovelTextのTextMeshPro版
	//ルビとか宴のタグも解析して表示する
	[AddComponentMenu("Utage/TextMeshPro/NovelTextTest")]
	public class TextMeshProNovelTextTest : MonoBehaviour
	{
		public string text;
		public TextMeshProNovelText novelText;

		private void OnEnable()
		{
			novelText.SetText(text);
		}
	}
}
