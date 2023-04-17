using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UtageExtensions;

namespace Utage
{
	//宴のTextMeshPro対応
	[AddComponentMenu("Utage/TextMeshPro/UtageForTextMeshPro")]
	public class UtageForTextMeshPro : MonoBehaviour
	{
		void Awake()
		{
			//テキスト解析処理をカスタム
			TextData.CreateCustomTextParser = CreateCustomTextParser;
			//ログテキスト作成をカスタム
			TextData.MakeCustomLogText = MakeCustomLogText;
		}

		void OnDestroy()
		{
			TextData.CreateCustomTextParser = null;
			TextData.MakeCustomLogText = null;
		}

		//テキスト解析をカスタム
		TextParser CreateCustomTextParser(string text)
		{
			return new TextMeshProTextParser(text);
		}

		//ログテキスト作成をカスタム
		string MakeCustomLogText(string text)
		{
			return new TextMeshProTextParser(text,true).NoneMetaString;
		}
		
	}
}
