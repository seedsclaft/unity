using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class PartyInfo 
    {
        public PartyInfo()
        {
        }

        // 所持アクターリスト
        private List<ActorInfo> _actorInfos = new();
        public List<ActorInfo> ActorInfos => _actorInfos;
        public void SetActorInfos(List<ActorInfo> actorInfos) => _actorInfos = actorInfos;
    
        // 現在のステージ場所
        private int _stageId = -1;
        public int StageId => _stageId;
        public void SetStageId(int stageId)
        {
            _stageId = stageId;
        }
        private int _seek = -1;
        public int Seek => _seek;
        public void SetSeek(int seek)
        {
            _seek = seek;
        }
        private int _seekIndex = -1;
        public int SeekIndex => _seekIndex;
        public void SetSeekIndex(int seekIndex)
        {
            _seekIndex = seekIndex;
        }

        // クリア情報
        private List<GetItemInfo> _getItemInfos = new ();
        public List<GetItemInfo> GetItemInfos => _getItemInfos;
        public void AddGetItemInfo(GetItemInfo getItemInfo)
        {
            _getItemInfos.Add(getItemInfo);
        }
    }
}