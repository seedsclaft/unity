
namespace Utage
{
	//ルビの情報
	internal class TextMeshProRubyInfo
	{
		//ルビの文字列
		internal string Ruby { get; set; }

		//ルビがつく本文の先頭文字インデックス
		internal int BeginIndex { get; private set; }
		//ルビがつく本文の末尾文字インデックス
		internal int EndIndex { get; private set; }

		//傍点かどうか
		internal bool IsEmphasis { get; private set; }

		//再度ルビを作成する場合の文字間
		internal float RemakeCspace { get; set; }

		internal TextMeshProRubyInfo(string ruby, int index, bool isEmphasis)
		{
			this.Ruby = ruby;
			this.BeginIndex = index;
			this.IsEmphasis = isEmphasis;
		}
		internal TextMeshProRubyInfo(string ruby, int index, int endIndex)
		{
			this.Ruby = ruby;
			this.BeginIndex = index;
			this.EndIndex = endIndex;
		}

		internal void SetEndIndex(int index)
		{
			EndIndex = index;
		}
	}
}
