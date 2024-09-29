using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CheckTriggerStatus : ICheckTrigger
    {
        public bool CheckTrigger(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {
            var isTrigger = false;
            switch (triggerData.TriggerType)
            {
                case TriggerType.FriendStatusUpper:
                case TriggerType.FriendStatusUnder:
                case TriggerType.OpponentStatusUpper:
                case TriggerType.OpponentStatusUnder:
                    return true;
                case TriggerType.LvUpper:
                    return triggerData.Param1 <= battlerInfo.Level;
            }
            return isTrigger;
        }
        
        public int CheckTargetIndex(SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo,int targetBattlerIndex)
        {
            switch (triggerData.TriggerType)
            {
                case TriggerType.FriendStatusUpper:
                    var friendStatusUpperIndex = SortStatusUpperTargetIndex(battlerInfo,checkTriggerInfo.Friends,(StatusParamType)triggerData.Param1,targetBattlerIndex);
                    return friendStatusUpperIndex;
                case TriggerType.FriendStatusUnder:
                    var friendStatusUnderIndex = SortStatusUnderTargetIndex(battlerInfo,checkTriggerInfo.Friends,(StatusParamType)triggerData.Param1,targetBattlerIndex);
                    return friendStatusUnderIndex;
                case TriggerType.OpponentStatusUpper:
                    var opponentStatusUpperIndex = SortStatusUpperTargetIndex(battlerInfo,checkTriggerInfo.Opponents,(StatusParamType)triggerData.Param1,targetBattlerIndex);
                    return opponentStatusUpperIndex;
                case TriggerType.OpponentStatusUnder:
                    var opponentStatusUnderIndex = SortStatusUnderTargetIndex(battlerInfo,checkTriggerInfo.Opponents,(StatusParamType)triggerData.Param1,targetBattlerIndex);
                    return opponentStatusUnderIndex;
            }
            return -1;
        }
        
        private int SortStatusUpperTargetIndex(BattlerInfo battlerInfo,List<BattlerInfo> targetInfos,StatusParamType statusParamType,int targetBattlerIndex)
        {
            if (targetInfos.Count > 0)
            {
                if (statusParamType == (int)StatusParamType.Hp)
                {
                    targetInfos.Sort((a,b) => a.MaxHp > b.MaxHp ? -1: 1);
                    var hp = targetInfos[0].MaxHp;
                    targetInfos = targetInfos.FindAll(a => a.MaxHp == hp);
                } else
                if (statusParamType == StatusParamType.Mp)
                {
                    targetInfos.Sort((a,b) => a.MaxMp > b.MaxMp ? -1: 1);
                    var mp = targetInfos[0].MaxMp;
                    targetInfos = targetInfos.FindAll(a => a.MaxMp == mp);
                } else
                if (statusParamType == StatusParamType.Atk)
                {
                    targetInfos.Sort((a,b) => a.CurrentAtk() > b.CurrentAtk() ? -1: 1);
                    var atk = targetInfos[0].CurrentAtk();
                    targetInfos = targetInfos.FindAll(a => a.CurrentAtk() == atk);
                } else
                if (statusParamType == StatusParamType.Def)
                {
                    targetInfos.Sort((a,b) => a.CurrentDef() > b.CurrentDef() ? -1: 1);
                    var def = targetInfos[0].CurrentDef();
                    targetInfos = targetInfos.FindAll(a => a.CurrentDef() == def);
                } else
                if (statusParamType == StatusParamType.Spd)
                {
                    targetInfos.Sort((a,b) => a.CurrentSpd() > b.CurrentSpd() ? -1: 1);
                    var spd = targetInfos[0].CurrentSpd();
                    targetInfos = targetInfos.FindAll(a => a.CurrentSpd() == spd);
                }
                return BattleUtility.NearTargetIndex(battlerInfo,targetInfos,targetBattlerIndex);
            }
            return -1;
        }

        private int SortStatusUnderTargetIndex(BattlerInfo battlerInfo,List<BattlerInfo> targetInfos,StatusParamType statusParamType,int targetBattlerIndex)
        {
            if (targetInfos.Count > 0)
            {
                if (statusParamType == (int)StatusParamType.Hp)
                {
                    targetInfos.Sort((a,b) => a.MaxHp > b.MaxHp ? 1: -1);
                    var hp = targetInfos[0].MaxHp;
                    targetInfos = targetInfos.FindAll(a => a.MaxHp == hp);
                } else
                if (statusParamType == StatusParamType.Mp)
                {
                    targetInfos.Sort((a,b) => a.MaxMp > b.MaxMp ? 1: -1);
                    var mp = targetInfos[0].MaxMp;
                    targetInfos = targetInfos.FindAll(a => a.MaxMp == mp);
                } else
                if (statusParamType == StatusParamType.Atk)
                {
                    targetInfos.Sort((a,b) => a.CurrentAtk() > b.CurrentAtk() ? 1: -1);
                    var atk = targetInfos[0].CurrentAtk();
                    targetInfos = targetInfos.FindAll(a => a.CurrentAtk() == atk);
                } else
                if (statusParamType == StatusParamType.Def)
                {
                    targetInfos.Sort((a,b) => a.CurrentDef() > b.CurrentDef() ? 1: -1);
                    var def = targetInfos[0].CurrentDef();
                    targetInfos = targetInfos.FindAll(a => a.CurrentDef() == def);
                } else
                if (statusParamType == StatusParamType.Spd)
                {
                    targetInfos.Sort((a,b) => a.CurrentSpd() > b.CurrentSpd() ? 1: -1);
                    var spd = targetInfos[0].CurrentSpd();
                    targetInfos = targetInfos.FindAll(a => a.CurrentSpd() == spd);
                }
                return BattleUtility.NearTargetIndex(battlerInfo,targetInfos,targetBattlerIndex);
            }
            return -1;
        }

        public void AddTargetIndexList(List<int> targetIndexList,List<int> targetIndexes,BattlerInfo targetBattler,SkillData.TriggerData triggerData,SkillData skillData,CheckTriggerInfo checkTriggerInfo)
        {
            
        }

        public void AddTriggerTargetList(List<int> targetIndexList,SkillData.TriggerData triggerData,BattlerInfo battlerInfo,CheckTriggerInfo checkTriggerInfo)
        {

        }
    }
}
