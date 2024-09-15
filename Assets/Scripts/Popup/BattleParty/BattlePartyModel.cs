using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class BattlePartyModel : BaseModel
    {

        private ActorInfo _currentActor = null;
        public ActorInfo CurrentActor => _currentActor;
        public void SetCurrentActorInfo(ActorInfo actorInfo) => _currentActor = actorInfo; 
        
        public BattlePartyModel()
        {
            _currentActor = StageMembers()[0];
        }

        public List<ListData> SelectActorLearningMagicList(int selectedSkillId = -1)
        {
            return ActorLearningMagicList(CurrentActor,-1,selectedSkillId);
        }
        
        public void SaveTempBattleMembers()
        {
            TempInfo.CashBattleActors(BattleMembers());
        }

        public void SetPartyBattlerIdList()
        {
            var idList = new List<int>();
            foreach (var battleMember in BattleMembers())
            {
                idList.Add(battleMember.ActorId);
            }
            PartyInfo.SetLastBattlerIdList(idList);
        }

        public List<BattlerInfo> EnemyInfos()
        {
            return CurrentTroopInfo().BattlerInfos;
        }

        public void SetInBattle()
        {
            var actorInfo = _currentActor;
            var battleIndex = StageMembers().FindAll(a => a.BattleIndex >= 0).Count + 1;
            if (actorInfo.BattleIndex >= 0)
            {
                RemoveBattleActor(actorInfo);
                return;
            }
            if (battleIndex > 5) 
            {
                return;
            }
            actorInfo.SetBattleIndex(battleIndex);
        }
        
        private void RemoveBattleActor(ActorInfo actorInfo)
        {
            actorInfo.SetBattleIndex(-1);
            var battleMembers = BattleMembers();
            for (int i = 0;i < battleMembers.Count;i++)
            {
                battleMembers[i].SetBattleIndex(i + 1);
            }
        }
    }
}