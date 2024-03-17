using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class SkillDates : ScriptableObject
    {
        public List<SkillData> Data = new();
    }

    [Serializable]
    public class SkillData
    {   
        public int Id;
        public string Name;
        public MagicIconType IconIndex;
        public int AnimationId;
        public AnimationType AnimationType;
        public int MpCost;
        public int Rank;
        public AttributeType Attribute;
        public ScopeType Scope;
        public SkillType SkillType;
        public TargetType TargetType;
        public string Help;
        public RangeType Range;
        public int RepeatTime;
        public AliveType AliveType;

        public List<FeatureData> FeatureDates;
        public List<TriggerData> TriggerDates;
        public List<TriggerData> ScopeTriggers;
        public bool IsHpDamageFeature()
        {
            return FeatureDates.Find(a => a.FeatureType == FeatureType.HpDamage || a.FeatureType == FeatureType.HpConsumeDamage) != null;
        }
        public bool IsHpHealFeature()
        {
            return FeatureDates.Find(a => a.FeatureType == FeatureType.HpHeal) != null;
        }
        public bool IsStateFeature(StateType stateType)
        {
            return FeatureDates.Find(a => (a.FeatureType == FeatureType.AddState || a.FeatureType == FeatureType.AddStateNextTurn) && a.Param1 == (int)stateType) != null;
        }
        [Serializable]
        public class SkillAttributeInfo
        {   
            public AttributeType AttributeType;
            public string ValueText;
            public int LearningCost;
            public string LearningCount;
        }

        public string TargetTypeText()
        {
            switch (TargetType)
            {
                case TargetType.Friend:
                return DataSystem.GetTextData(600).Text;
                case TargetType.Opponent:
                return DataSystem.GetTextData(601).Text;
                case TargetType.Self:
                return DataSystem.GetTextData(602).Text;
                case TargetType.All:
                return DataSystem.GetTextData(603).Text;
            }
            return "";
        }
        public string ScopeTypeText()
        {
            switch (Scope)
            {
                case ScopeType.All:
                if (TargetType == TargetType.Opponent)
                {
                    return DataSystem.GetTextData(620).Text;
                }
                return DataSystem.GetTextData(621).Text;
                case ScopeType.Line:
                return DataSystem.GetTextData(622).Text;
                case ScopeType.One:
                if (TargetType == TargetType.Opponent){
                    return DataSystem.GetTextData(623).Text;
                }
                return DataSystem.GetTextData(624).Text;
                case ScopeType.Self:
                return "";
                case ScopeType.FrontLine:
                return DataSystem.GetTextData(625).Text;
            }
            return "";
        }
        public string ConvertHelpText(string help)
        {
            var targetText = TargetTypeText();
            var scopeText = ScopeTypeText();
            return help.Replace("\\s",targetText + scopeText);
        }

        [Serializable]
        public class FeatureData
        {   
            public int SkillId;
            public FeatureType FeatureType;
            public int Param1;
            public int Param2;
            public int Param3;
            public int Rate;
            public FeatureData CopyData()
            {
                var feature = new FeatureData
                {
                    SkillId = SkillId,
                    FeatureType = FeatureType,
                    Param1 = Param1,
                    Param2 = Param2,
                    Param3 = Param3,
                    Rate = Rate
                };
                return feature;
            }
        }
        
        [Serializable]
        public class TriggerData
        {   
            public int SkillId;
            public TriggerType TriggerType;
            public TriggerTiming TriggerTiming;
            public int Param1;
            public int Param2;
            public int Param3;

            public bool IsTriggeredSkillInfo(BattlerInfo battlerInfo,List<BattlerInfo> party,List<BattlerInfo> troops)
            {
                bool CanUse = false;
                
                switch (TriggerType)
                {
                    case TriggerType.None:
                    case TriggerType.InBattleUseCountUnder: // 別処理で判定するためここではパス
                    case TriggerType.ExtendStageTurn: // 別処理で判定するためここではパス
                        CanUse = true;
                    break;
                    case TriggerType.HpRateUnder:
                    if (battlerInfo.Hp == 0)
                    {
                        CanUse = Param1 == 0;
                    } else
                    if (((float)battlerInfo.Hp / (float)battlerInfo.MaxHp) < Param1 * 0.01f)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.HpRateUpper:
                    if (((float)battlerInfo.Hp / (float)battlerInfo.MaxHp) > Param1 * 0.01f)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.HpValue:
                    if (battlerInfo.Hp == Param1)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.HpUnder:
                    if (battlerInfo.Hp <= Param1)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.MpUnder:
                    if (battlerInfo.Mp <= Param1)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.MpUpper:
                    if (battlerInfo.Mp >= Param1)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.IsExistDeathMember:
                    var IsExistDeathMember = battlerInfo.IsActor ? party : troops;
                    if (IsExistDeathMember.FindAll(a => !a.IsAlive()).Count >= Param1)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.IsExistAliveMember:
                    var IsExistAliveMember = battlerInfo.IsActor ? party : troops;
                    if (IsExistAliveMember.FindAll(a => a.IsAlive()).Count >= Param1)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.TurnNumUnder:
                    if (battlerInfo.TurnCount < Param1)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.TurnNumPer:
                    if ((battlerInfo.TurnCount % Param1) - Param2 == 0)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.ActionCountPer:
                    var actionCount = 0;
                    foreach (var member in party)
                    {
                        actionCount += member.TurnCount;
                    }
                    foreach (var member in troops)
                    {
                        actionCount += member.TurnCount;
                    }
                    if (Param1 > 0)
                    {
                        if ((actionCount % Param1) - Param2 == 0)
                        {
                            CanUse = true;
                        }
                    } else
                    {
                        if (actionCount - Param2 == 0)
                        {
                            CanUse = true;
                        }
                    }
                    break;
                    case TriggerType.PartyHpRateUnder:
                    var filter = troops.Find(a => ((float)a.Hp / (float)a.MaxHp) < Param1 * 0.01f);
                    if (filter == null)
                    {
                        CanUse = false;
                    }
                    break;
                    case TriggerType.SelfLineFront:
                    if (battlerInfo.LineIndex == LineType.Front)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.SelfLineBack:
                    if (battlerInfo.LineIndex == LineType.Back)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.IsState:
                    if (battlerInfo.IsState((StateType)Param1))
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.IsNotState:
                    if (!battlerInfo.IsState((StateType)Param1))
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.IsAbnormalState:
                    if (battlerInfo.StateInfos.Find(a => a.Master.Abnormal == true) != null)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.IsNotAwaken:
                    if (!battlerInfo.IsAwaken)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.IsAwaken:
                    if (battlerInfo.IsAwaken)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.LessTroopMembers:
                    if ( troops.Count >= party.Count )
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.MoreTroopMembers:
                    if ( troops.Count <= party.Count )
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.LvUpper:
                    if ( Param1 <= battlerInfo.Level )
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.ChainCount:
                    if (battlerInfo.ChainSuccessCount >= Param1)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.Percent:
                    var rand = UnityEngine.Random.Range(0,100);
                    if (rand <= Param1)
                    {
                        CanUse = true;
                    }
                    break;
                }
                return CanUse;
            }
        }
    }

    public enum AnimationType
    {
        None = 0,
        One = 1,
        Line = 2,
        All = 3
    }

    public enum ScopeType{
        None = 0,
        One = 1,
        Line = 2,
        All = 3,
        Self = 4,
        FrontLine = 5,
        WithoutSelfOne = 11,
        WithoutSelfAll = 13,
        RandomOne = 21,
        OneAndNeighbor = 31,
    }

    public enum SkillType{
        None = 0,
        Magic = 1, // 通常スキル
        Passive = 2, // パッシブ
        Demigod = 3, // 神化
        Awaken = 4, // 覚醒
        UseAlcana = 11, // アルカナ使用
        Reborn  = 12 // 転生
    }

    public enum AttributeType
    {
        None = 0,
        Fire = 1,
        Thunder = 2,
        Ice = 3,
        Shine = 4,
        Dark = 5
    }

    public enum TargetType
    {
        None = 0,
        Opponent = 1,
        Friend = 2,
        All = 3,
        Self = 4,
        AttackTarget = 7,
        IsTriggerTarget = 11,
        Party = 101
    }

    public enum RangeType
    {
        None = 0,
        S = 1,
        L = 2
    }

    public enum DamageType
    {
        None = 0,
        HpDamage = 1,
        HpCritical = 2,
        HpHeal = 3,    
        MpHeal = 4,
        State = 5,
        MpDamage = 6,

    }

    public enum AliveType
    {
        DeathOnly = 0,
        AliveOnly = 1,
        All = 2,
    }

    public enum TriggerType
    {
        None = 0,
        HpRateUnder = 1, // Hpが〇%以下
        HpRateUpper = 2, // Hpが〇%以上
        HpValue = 3, // Hpが〇
        HpUnder = 4, // Hpが〇
        PartyHpRateUnder = 6, // 味方にHpが〇%以下がいる
        MpUnder = 11, // Mpが〇以下
        MpUpper = 12, // Mpが〇以上
        IsExistDeathMember = 21, // 戦闘不能が〇以上存在する
        IsExistAliveMember = 22, // 生存者が〇以上存在する
        SelfLineFront = 31, // 自分が前列にいる
        SelfLineBack = 32, // 自分が後列にいる
        IsState = 41, // StateId状態になっている
        IsNotState = 42, // StateId状態になっていない
        IsAbnormalState = 43, // AbnormalのState状態になっている
        IsNotAwaken = 44, // 神化発動前
        IsAwaken = 45, // 神化発動後
        LessTroopMembers = 51, // 味方より敵が多い
        MoreTroopMembers = 52, // 味方より敵が少ない
        TurnNumUnder = 61, // ターン数が〇以内
        TurnNumPer = 63, // ターン数がparam1 x ターン数 + param2
        ActionCountPer = 64, // 全体の行動数がparam1 x 行動数 + param2
        AttackState = 71, // 攻撃成功時〇%で
        Percent = 72, // 〇%で
        InBattleUseCountUnder = 81, // バトル中使用回数が〇以下
        LvUpper = 92, // Lvが〇以上
        ActionMpCost = 93, // 行動Magicの消費Mpが〇
        TargetHpRateUnder = 94, // 攻撃を受けた対象のHpが〇%以下
        PayBattleMp = 101, // Mpを〇消費する
        ChainCount = 102, // 拘束成功回数
        ActionResultDeath = 103, // 攻撃を受けると戦闘不能になる
        DeadWithoutSelf = 104, // 自身以外が戦闘不能
        SelfDead = 105, // 自身が戦闘不能
        AttackedCount = 106, // 攻撃を〇回受ける
        AllEnemyCurseState = 107, // 敵全員が呪い
        ActionResultAddState = 113, // 相手が状態異常を発動する前
        DefeatEnemyByAttack = 114, // 攻撃で敵を撃破する
        DemigodMagicAttribute = 204, // Demigod魔法の属性が〇の味方が神化する
        ExtendStageTurn = 502, // 存在猶予を延長している
        DodgeCountOver = 1001, // 回避を〇回行う
        HpHealCountOver = 1004, // Hp回復魔法を〇回行う
        AwakenDemigodAttribute = 1005, // Demigod魔法の属性が〇の味方が神化する
        ActionResultSelfDeath = 1006, // 自身が戦闘不能になる攻撃を受ける
    }

    public enum TriggerTiming
    {
        None = 0,
        Use = 1,
        After = 2,
        Interrupt = 3,
        StartBattle = 4,
        Before = 5,
        HpDamaged = 11,
        BeforeAndStartBattle = 20,
        AfterAndStartBattle = 24,
        BeforeTacticsTurn = 51,
        CurrentTacticsTurn = 52
    }

    public enum FeatureType
    {
        None = 0,
        HpDamage = 1010,
        HpDrain = 1020,
        HpDefineDamage = 1030,
        HpStateDamage = 1040,
        NoEffectHpDamage = 1050,
        NoEffectHpPerDamage = 1060,
        NoEffectHpAddDamage = 1070,
        HpConsumeDamage = 1080,
        HpHeal = 2010,
        RemainHpOne = 2020,
        RemainHpOneTarget = 2030,
        DamageHpHealParty = 2110,
        AddState = 3010,
        RemoveState = 3020,
        RemoveAbnormalState = 3030,
        AddStateNextTurn = 3040,
        MpDamage = 4010,
        MpHeal = 4020,
        SetAfterAp = 5010,
        ApHeal = 5020,
        NoResetAp = 5030,
        StartDash = 5040,
        ApDamage = 5050,
        ChangeFeatureParam1 = 6010,
        ChangeFeatureParam2 = 6020,
        ChangeFeatureParam3 = 6030,
        ChangeFeatureParam1StageWinCount = 6040,
        ChangeFeatureParam2StageWinCount = 6050,
        ChangeFeatureParam3StageWinCount = 6060,
        ChangeSkillRepeatTime = 6100,
        ChangeSkillScope = 6210,
        PlusSkill = 7010,
        KindHeal = 8010,
        BreakUndead = 8020,
        Numinous = 301,
        TacticsCost = 302,
        EnemyLv = 303,
        AddSp = 304,
        Subordinate = 305,
        Alcana = 306,
        StatusUpCostDown = 307,
        LineZeroErase = 308,
        VictoryGainSp = 309,
        MagicAlchemy = 310,
        DisableTactics = 311,
        RebornCommandLvUp = 401,
        RebornStatusUp = 402,
        RebornAddSkill = 403,
        RebornQuest = 404,
        GainTurn = 501,
        ActorLvUp = 502,
        AlchemyCostZero = 503,
        NoBattleLost = 504,
        ResourceBonus = 505,
        CommandCostZero = 506,
        AlchemyCostBonus = 507,
        CommandLvUp = 508,
        AddSkillOrCurrency = 509,
        HpCursedDamage = 992004, // 制御用
        RemoveStatePassive = 992022, // 制御用
    }

    public enum LearningState{
        None = 0,
        Learned = 1,
        NotLearn = 2,
        Equipment = 3,
        SelectLearn = 4
    }

    public enum MagicIconType
    {
        None = 0,
        Elementarism = 1, // 元素術
        LightMagic = 2, // 光魔術
        Entropy = 3, // 理術
        Mentalism = 4, // 精神術
        Psionics = 5, // 超次元
        Enchanting = 6, // 自己強化
        Craft = 7, // 工作
        Demigod = 10, //半神
        Awaken = 11, // 覚醒
        Other = 99, // その他
        
    }

    public enum HpHealType{
        EffectValue = 0,
        RateValue = 1, //割合回復
    }
}