
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
	[AddComponentMenu("Utage/TextMeshPro/NovelText")]
	public class TextMeshProNovelText : MonoBehaviour
	{
		//ルビのプレハブ
		[SerializeField]
		TextMeshProRuby rubyPrefab = null;

		//ルビが本文よりも大きかった場合に、本文のほうに余白を開けるようにテキストを作り直す
		bool RemakeLargeRuby { get { return remakeLargeRuby; } }
		[SerializeField]
		bool remakeLargeRuby = true;

		//宴のテキスト解析と実際に表示するテキストに違いがないかなどのチェックを行う
		//タグの構文エラーチェックなどの代用となる
		bool DebugLogError { get { return debugLogError; } }
		[SerializeField]
		bool debugLogError = true;

		//文字溢れをランタイムでチェックする
		bool CheckOverFlow { get { return checkOverFlow; } }
		[SerializeField]
		bool checkOverFlow = true;

		//文字溢れをエディタ上でチェックする
		//主にシナリオインポート時のエラーチェックだが、
		//TextMeshProのForceMeshUpdateはエディタ上だと動作が不安定なので、デフォルトではオフ
		bool CheckOverFlowInEditor { get { return checkOverFlowInEditor; } }
		[SerializeField]
		bool checkOverFlowInEditor = false;

		public TMP_Text TextMeshPro { get { return this.GetComponentCache(ref textMeshPro); } }
		TMP_Text textMeshPro;

		public virtual int MaxVisibleCharacters
		{
			get
			{
				return TextMeshPro.maxVisibleCharacters;
			}
			set
			{
				if (TextMeshPro.maxVisibleCharacters != value)
				{
					TextMeshPro.maxVisibleCharacters = value;
					UpdateVisibleIndex();
				}
			}
		}

		public Color Color
		{
			get
			{
				return TextMeshPro.color;
			}
			set
			{
				TextMeshPro.color = value;
				foreach (var obj in RubyObjectList)
				{
					obj.SetColor(value);
				}
			}
		}

		public Vector3 CurrentEndPosition
		{
			get
			{
				if (HasChanged)
				{
					ForceUpdate();
				}
				int index = MaxVisibleCharacters - 1;
				TMP_TextInfo info = TextMeshPro.textInfo;
				int len = info.characterInfo.Length;
				if (index < 0 || len <= index)
				{
					Debug.LogFormat("index={0} len={1} ", index, len);
					return TextMeshPro.rectTransform.anchoredPosition3D;
				}
				else
				{
					var c = info.characterInfo[index];
					var line = info.lineInfo[c.lineNumber];
					Vector3 pos = c.bottomRight;
					pos.y = line.baseline;
					return pos;
				}
			}
		}

		TextData TextData { get; set; }
		//ルビの情報
		List<TextMeshProRubyInfo> RubyInfoList { get { return rubyInfoList; } }
		List<TextMeshProRubyInfo> rubyInfoList = new List<TextMeshProRubyInfo>();

		List<TextMeshProRuby> RubyObjectList { get { return rubyObjectList; } }
		List<TextMeshProRuby> rubyObjectList = new List<TextMeshProRuby>();

		bool HasChanged { get; set; }

		public void SetNovelTextData(TextData textData, int length)
		{
			this.TextData = textData;
			MaxVisibleCharacters = length;
			HasChanged = true;
		}

		public void SetText(string text)
		{
			this.TextData = new TextData(text);
			HasChanged = true;
		}

		public virtual string GetText()
		{
			return TextMeshPro.text;
		}
		public virtual void SetTextDirect(string text)
		{
			TextMeshPro.text = text;
		}


		public void Clear()
		{
			SetText("");
		}

		void Update()
		{
			if (HasChanged)
			{
				ForceUpdate();
			}
		}

		public void ForceUpdate(bool ignoreActiveState = false)
		{
			if (this.TextData == null)
			{
				return;
			}
			ForceUpdateSub(ignoreActiveState,false);

			//大きなルビがあった場合に作り直す
			if (CheckRemakeRuby())
			{
				ForceUpdateSub(ignoreActiveState,true);
			}

			HasChanged = false;

#if UNITY_EDITOR
			if (DebugLogError && Application.isPlaying)
			{
				int len = TextData.Length;
				int count = TextMeshPro.textInfo.characterCount;
				if (len != count)
				{
					Debug.LogErrorFormat(this, "テキストの解析結果の文字数 {0} と TextMeshProで表示する文字数 {1}　が違います\n{2}", len, count, TextMeshPro.text);
				}
				if ( CheckOverFlow && TextMeshPro.isTextOverflowing && len > 0)
				{
					Debug.LogErrorFormat(this,"表示文字がオーバーしています\n{0}", TextMeshPro.text);
				}
			}
#endif
		}

		bool CheckRemakeRuby()
		{
			if (!RemakeLargeRuby) return false;
			foreach ( var item in RubyInfoList )
			{
				if (item.RemakeCspace>0) return true;
			}
			return false;
		}

		protected virtual void ForceUpdateSub(bool ignoreActiveState, bool remakingLargeRuby)
		{
			Profiler.BeginSample("MakeTextMeshProString");
			string textMeshProString = MakeTextMeshProString(remakingLargeRuby);
			Profiler.EndSample();
			ForceUpdateText(textMeshProString, ignoreActiveState);
			MakeRubyTextObjects();
		}

		protected virtual void ForceUpdateText(string textMeshProString,bool ignoreActiveState)
		{
			TextMeshPro.text = textMeshProString;
			TextMeshPro.ForceMeshUpdate(ignoreActiveState);
		}

		// TextMeshPro形式のタグつき文字列を作成
		string MakeTextMeshProString(bool remakingLargeRuby)
		{
			StringBuilder builder = new StringBuilder();
			if (remakingLargeRuby)
			{
				List<TextMeshProRubyInfo> oldRubyList = new List<TextMeshProRubyInfo>();
				oldRubyList.AddRange(RubyInfoList);
				MakeTextMeshProString(builder, remakingLargeRuby, oldRubyList);
			}
			else
			{
				MakeTextMeshProString(builder, remakingLargeRuby, null);
			}
			return builder.ToString();
		}

		// TextMeshPro形式のタグつき文字列を作成
		void MakeTextMeshProString(StringBuilder builder, bool remakingLargeRuby, List<TextMeshProRubyInfo> oldRubyList)
		{
			RubyInfoList.Clear();
			TextMeshProRubyInfo rubyInfo = null;
			int index = 0;
			int countCharacter = 0;
			foreach (IParsedTextData data in TextData.ParsedText.ParsedDataList)
			{
				if (data is CharData)
				{
					CharData c = data as CharData;
					if (TextData.CharList[index] != c)
					{
						Debug.LogError("テキストの解析が失敗しています");
						continue;
					}
					CharData.CustomCharaInfo info = c.CustomInfo;
					bool ignoreChar = (info.IsEmoji) || info.IsDash;
					if (!ignoreChar)
					{
						builder.Append(c.Char);
						++countCharacter;
					}
					index++;
				}
				else if (data is TagData)
				{
					TagData tag = data as TagData;
					if (!tag.IgnoreTagString)
					{
						builder.Append(tag.TagString);
					}
					switch (tag.TagName)
					{
						case "ruby":
							rubyInfo = new TextMeshProRubyInfo(tag.TagArg, countCharacter, false);
							builder.Append("<nobr>");
							if (remakingLargeRuby)
							{
								int rubyInex = RubyInfoList.Count;
								if (rubyInex >= oldRubyList.Count)
								{
									Debug.LogErrorFormat("Ramke Ruby error index={0} oldRubyList.Count={1}", rubyInex, oldRubyList.Count);
									rubyInex = oldRubyList.Count - 1;
								}
								float width = oldRubyList[rubyInex].RemakeCspace;
								builder.Append("<space=").Append(width).Append(">");
								builder.Append("<cspace=").Append(width).Append(">");
							}
							break;
						case "/ruby":
							if (remakingLargeRuby)
							{
								int rubyInex = RubyInfoList.Count;
								if (rubyInex >= oldRubyList.Count)
								{
									Debug.LogErrorFormat("Ramke Ruby error index={0} oldRubyList.Count={1}", rubyInex, oldRubyList.Count);
									rubyInex = oldRubyList.Count - 1;
								}
								float width = oldRubyList[rubyInex].RemakeCspace;
								builder.Append("</cspace>");
								builder.Append("<space=").Append(width).Append(">");
							}
							rubyInfo.SetEndIndex(countCharacter-1);
							RubyInfoList.Add(rubyInfo);
							//通常のルビは改行不可を解除
							builder.Append("</nobr>");
							break;
						case "em":
							rubyInfo = new TextMeshProRubyInfo(tag.TagArg, countCharacter, true);
							break;
						case "/em":
							//傍点の場合、文字づつ作成
							int begin = rubyInfo.BeginIndex;
							int end = countCharacter - 1;
							string str = rubyInfo.Ruby;
							for (int i = begin; i <= end; i++)
							{
								RubyInfoList.Add(new TextMeshProRubyInfo(str, i, i));
							}
							break;
						default:
							break;
					}
				}
			}
		}


		//ルビのテキストオブジェクトを作成
		void MakeRubyTextObjects()
		{
			//現在のルビのオブジェクトを全て消去
			foreach (var obj in RubyObjectList)
			{
				if (Application.isPlaying)
				{
					GameObject.Destroy(obj.gameObject);
				}
				else
				{
					GameObject.DestroyImmediate(obj.gameObject);
				}
			}
			RubyObjectList.Clear();
			TMP_TextInfo textInfo = TextMeshPro.textInfo;
			foreach (var ruby in RubyInfoList)
			{
				AddRubyTextObject(textInfo, ruby);
			}
			UpdateVisibleIndex();
			HasChanged = false;
		}


		//表示文字数に応じて、ルビの表示のオン、オフを更新
		protected void UpdateVisibleIndex()
		{
			if (this.RubyObjectList.Count <= 0) return;

			int index = MaxVisibleCharacters - 1;
			foreach (var obj in RubyObjectList)
			{
				obj.SetVisibleIndex(index);
			}
		}

		//ルビを作成して配置する
		void AddRubyTextObject(TMP_TextInfo textInfo, TextMeshProRubyInfo rubyInfo)
		{
			if (string.IsNullOrEmpty(rubyInfo.Ruby)) return;
			if (rubyPrefab == null)
			{
//				Debug.LogError("ルビ用のプレハブがありません");
				return;
			}

			int len = textInfo.characterInfo.Length;
			if (rubyInfo.BeginIndex >= len || rubyInfo.EndIndex >= len)
			{
				Debug.LogError("RubyIndex Error");
				return;
			}

			//プレハブからルビのオブジェクトを作成する
			GameObject go = this.transform.AddChildPrefab(rubyPrefab.gameObject);
			go.hideFlags = HideFlags.DontSave;
			TextMeshProRuby ruby = go.GetComponent<TextMeshProRuby>();
			ruby.Init(textInfo, rubyInfo, GetPivotOffset());
			if (ruby.RubyWidth > ruby.TextWidth)
			{
				int count = (rubyInfo.EndIndex - rubyInfo.BeginIndex +1) +1;
				float w = (ruby.RubyWidth - ruby.TextWidth)/ count + TextMeshPro.characterSpacing;
				rubyInfo.RemakeCspace = w;
			}
			RubyObjectList.Add(ruby);
		}

		Vector2 GetPivotOffset()
		{
			if (TextMeshPro.transform is RectTransform)
			{
				RectTransform rect = TextMeshPro.transform as RectTransform;
				Rect r = rect.rect;
				Vector2 pivot = rect.pivot;
				float x_offset = (pivot.x - 0.5f) * r.width;
				float y_offset = (pivot.y - 0.5f) * r.height;
				return new Vector2(x_offset, y_offset);
			}
			else
			{
				return Vector2.zero;
			}
		}

		//指定テキストに対する表示文字数チェック
		internal bool TryCheckCaracterCount(string text, out int count, out string errorString)
		{
			SetText(text);

			//文字あふれチェックをしない
			if (!CheckOverFlowInEditor)
			{
				count = TextData.CharList.Count;
				errorString = "";
				return true;
			}

			//強制アップデートして文字あふれチェック
			ForceUpdate(true);
			if (!TextMeshPro.isTextOverflowing)
			{
				count = TextMeshPro.textInfo.characterCount;
				errorString = "";
				return true;
			}

			//文字溢れがある場合
			int overflowIndex = TextMeshPro.firstOverflowCharacterIndex;
			string str = text;
			int len = str.Length;
			count = len - overflowIndex;
			if (len <= overflowIndex || (overflowIndex + count - 1) > len)
			{
				errorString = string.Format("Index over overflowIndex={0} len={1} count ={2}", overflowIndex, len, count);
				Debug.LogError(errorString);
			}
			else
			{
				errorString = string.Format("Index over overflowIndex={0} len={1} count ={2}", overflowIndex, len, count);
				errorString += str.Substring(0, overflowIndex) + TextParser.AddTag(str.Substring(overflowIndex, count - 1), "color", "red");
			}
			return false;
		}
	}
}
