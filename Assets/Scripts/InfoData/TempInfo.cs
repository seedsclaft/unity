using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    // セーブデータに保存しないデータ類を管理
    public class TempInfo
    {
        private List<ActorInfo> _tempActorInfos = new ();
        // バトル前のアクターデータを設定
        public List<ActorInfo> TempActorInfos => _tempActorInfos;
        private List<SkillInfo> _tempAlcanaSkillInfos = new ();
        public List<SkillInfo> TempAlcanaSkillInfos => _tempAlcanaSkillInfos;
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

        public void SetAlcanaSkillInfo(List<SkillInfo> skillInfos)
        {
            _tempAlcanaSkillInfos = skillInfos;
        }

        public void ClearAlcana()
        {
            _tempAlcanaSkillInfos.Clear();
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
        
        
        private List<ActorInfo> _tempStatusActorInfos = new ();
        public List<ActorInfo> TempStatusActorInfos => _tempStatusActorInfos;
        public void SetTempStatusActorInfos(List<ActorInfo> tempStatusActorInfos)
        {
            var recordActorInfos = new List<ActorInfo>();
            foreach (var actorInfo in tempStatusActorInfos)
            {
                var recordActorInfo = new ActorInfo(actorInfo.Master);
                recordActorInfo.CopyData(actorInfo);		
                recordActorInfos.Add(recordActorInfo);
            }
            _tempStatusActorInfos = recordActorInfos;
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
        
        private bool _battleResultVictory = false;
        public bool BattleResultVictory => _battleResultVictory;
        public void SetBattleResultVictory(bool isVictory)
        {
            _battleResultVictory = isVictory;
        }

        private int _battleResultScore = 0;
        public int BattleResultScore => _battleResultScore;
        public void SetBattleScore(int score)
        {
            _battleResultScore = score;
        }
    }
}