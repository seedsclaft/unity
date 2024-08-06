using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    // セーブデータに保存しないデータ類を管理
    public class TempInfo
    {
        private List<ActorInfo> _tempActorInfos = new ();
        // バトル前のアクターデータを設定
        public List<ActorInfo> TempActorInfos => _tempActorInfos;
        private Dictionary<int,List<RankingInfo>> _tempRankingData = new ();
        public Dictionary<int,List<RankingInfo>> TempRankingData => _tempRankingData;
        private bool _tempInputType = false;
        public bool TempInputType => _tempInputType;
        public void CashBattleActors(List<ActorInfo> actorInfos)
        {
            ClearBattleActors();
            foreach (var actorInfo in actorInfos)
            {
                var tempInfo = new ActorInfo(actorInfo.Master);
                tempInfo.CopyData(actorInfo);
                _tempActorInfos.Add(tempInfo);
            }
        }

        public void ClearBattleActors()
        {
            _tempActorInfos.Clear();
        }
        
        public void SetRankingInfo(int stageId,List<RankingInfo> rankingInfos)
        {
            _tempRankingData[stageId] = rankingInfos;
        }
        
        public void ClearRankingInfo()
        {
            _tempRankingData.Clear();
        }
        
        public void SetInputType(bool inputType)
        {
            _tempInputType = inputType;
        }    

        // リプレイデータ
        private SaveBattleInfo _clearPartyReplayData;
        public SaveBattleInfo ClearPartyReplayData => _clearPartyReplayData;
        public void SetSaveBattleInfo(SaveBattleInfo clearPartyReplayData)
        {
            _clearPartyReplayData = clearPartyReplayData;
        }

        private bool _inReplay = false;
        public bool InReplay => _inReplay;
        public void SetInReplay(bool inReplay)
        {
            _inReplay = inReplay;
        }
    }
}