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
                return DataSystem.GetText(600);
                case TargetType.Opponent:
                return DataSystem.GetText(601);
                case TargetType.Self:
                return DataSystem.GetText(602);
                case TargetType.All:
                return DataSystem.GetText(603);
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
                    return DataSystem.GetText(620);
                }
                return DataSystem.GetText(621);
                case ScopeType.Line:
                return DataSystem.GetText(622);
                case ScopeType.One:
                if (TargetType == TargetType.Opponent){
                    return DataSystem.GetText(623);
                }
                return DataSystem.GetText(624);
                case ScopeType.Self:
                return "";
                case ScopeType.FrontLine:
                return DataSystem.GetText(625);
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
                    case TriggerType.LessHpFriend: // 別処理で判定するためここではパス
                    case TriggerType.MostHpFriend: // 別処理で判定するためここではパス
                    case TriggerType.LessHpTarget: // 別処理で判定するためここではパス
                    case TriggerType.MostHpTarget: // 別処理で判定するためここではパス
                        CanUse = true;
                    break;
                    case TriggerType.SelfHpRateUnder:
                    if (battlerInfo.HpRate <= Param1 * 0.01f)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.SelfHpRateUpper:
                    if (battlerInfo.HpRate >= Param1 * 0.01f)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.FriendHpRateUnder:
                    if (party.Find(a => a.HpRate <= Param1 * 0.01f) != null)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.FriendHpRateUpper:
                    if (party.Find(a => a.HpRate >= Param1 * 0.01f) != null)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.OpponentHpRateUnder:
                    if (troops.Find(a => a.HpRate <= Param1 * 0.01f) != null)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.OpponentHpRateUpper:
                    if (troops.Find(a => a.HpRate >= Param1 * 0.01f) != null)
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
                    case TriggerType.SelfMpUnder:
                    if (battlerInfo.MpRate <= Param1 * 0.01f)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.SelfMpUpper:
                    if (battlerInfo.MpRate >= Param1 * 0.01f)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.FriendMpUnder:
                    if (party.Find(a => a.MpRate >= Param1 * 0.01f) != null)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.FriendMpUpper:
                    if (troops.Find(a => a.MpRate <= Param1 * 0.01f) != null)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.OpponentMpUnder:
                    if (troops.Find(a => a.MpRate >= Param1 * 0.01f) != null)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.OpponentMpUpper:
                    if (troops.Find(a => a.MpRate <= Param1 * 0.01f) != null)
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
                    if (troops.Count >= party.Count)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.MoreTroopMembers:
                    if (troops.Count <= party.Count)
                    {
                        CanUse = true;
                    }
                    break;
                    case TriggerType.LvUpper:
                    if (Param1 <= battlerInfo.Level)
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
        Active = 1, // 通常スキル
        Passive = 2, // パッシブ
        Messiah = 3, // 神化
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
        Counter = 6,
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
        SelfHpRateUnder = 1010, // 自分のHpが〇%未満
        SelfHpRateUpper = 1020, // 自分のHpが〇%以上
        FriendHpRateUnder = 1030, // Hp〇%未満の味方
        FriendHpRateUpper = 1040, // Hp〇%以上の味方
        OpponentHpRateUnder = 1050, // Hp〇%未満の敵
        OpponentHpRateUpper = 1060, // Hp〇%以上の敵
        LessHpFriend = 1130, // 最もHpが少ない味方
        MostHpFriend = 1140, // 最もHpが多い味方
        LessHpTarget = 1150, // 最もHpが少ない敵
        MostHpTarget = 1160, // 最もHpが多い敵
        HpValue = 991030, // Hpが〇
        HpUnder = 991040, // Hpが〇以下
        PartyHpRateUnder = 2010, // 味方にHpが〇%以下がいる
        SelfMpUnder = 3010, // 自身のMpが〇以下
        SelfMpUpper = 3020, // 自身のMpが〇以上
        FriendMpUnder = 3030, // Mpが〇以下の味方
        FriendMpUpper = 3040, // Mpが〇以上の味方
        OpponentMpUnder = 3050, // Mpが〇以下の敵
        OpponentMpUpper = 3060, // Mpが〇以上の敵
        IsExistDeathMember = 4010, // 戦闘不能が〇以上存在する
        IsExistAliveMember = 4020, // 生存者が〇以上存在する
        SelfLineFront = 5010, // 自分が前列にいる
        SelfLineBack = 5020, // 自分が後列にいる
        IsState = 6010, // StateId状態になっている
        IsNotState = 6020, // StateId状態になっていない
        IsAbnormalState = 6030, // AbnormalのState状態になっている
        IsNotAwaken = 7010, // 神化発動前
        IsAwaken = 7020, // 神化発動後
        LessTroopMembers = 8010, // 味方より敵が多い
        MoreTroopMembers = 8020, // 味方より敵が少ない
        TurnNumUnder = 9010, // ターン数が〇以内
        TurnNumPer = 9030, // ターン数がparam1 x ターン数 + param2
        ActionCountPer = 9040, // 全体の行動数がparam1 x 行動数 + param2
        AttackState = 10010, // 攻撃成功時〇%で
        Percent = 10020, // 〇%で
        InBattleUseCountUnder = 11010, // バトル中使用回数が〇以下
        LvUpper = 12020, // Lvが〇以上
        ActionMpCost = 12030, // 行動Magicの消費Mpが〇
        TargetHpRateUnder = 12040, // 攻撃を受けた対象のHpが〇%以下
        OneAttackOverDamage = 12050, // 1回の攻撃で〇ダメージ以上受ける
        PayBattleMp = 20010, // Mpを〇消費する
        ChainCount = 20020, // 拘束成功回数
        ActionResultDeath = 20030, // 攻撃を受けると戦闘不能になる
        DeadWithoutSelf = 20040, // 自身以外が戦闘不能
        SelfDead = 20050, // 自身が戦闘不能
        AttackedCount = 20060, // 攻撃を〇回受ける
        AllEnemyCurseState = 20070, // 敵全員が呪い
        BeCriticalCount = 20080, // クリティカル攻撃を〇回受ける
        DodgeCountOver = 20090, // 回避を〇回行う
        HpHealCountOver = 20100, // Hp回復魔法を〇回行う
        DemigodMemberCount = 20110, // 味方の神化状態が〇以上
        AwakenDemigodAttribute = 20120, // Demigod魔法の属性が〇の味方が神化する
        ActionResultAddState = 20130, // 相手が状態異常を発動する前
        DefeatEnemyByAttack = 20140, // 攻撃で敵を撃破する
        DemigodMagicAttribute = 20150, // Demigod魔法の属性が〇の味方が神化する
        ActionResultSelfDeath = 20160, // 自身が戦闘不能になる攻撃を受ける
        ExtendStageTurn = 30010, // 存在猶予を延長している
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
        DamageMpHealParty = 2120,
        AddState = 3010,
        RemoveState = 3020,
        RemoveAbnormalState = 3030,
        AddStateNextTurn = 3040,
        RemoveBuffState = 3050,
        RemoveDeBuffState = 3060,
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
        ChangeFeatureRate = 6070,
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