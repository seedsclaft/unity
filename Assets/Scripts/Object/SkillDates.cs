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
        public int CountTurn;
        public RankType Rank;
        public AttributeType Attribute;
        public ScopeType Scope;
        public SkillType SkillType;
        public TargetType TargetType;
        public string Help;
        public RangeType Range;
        public int RepeatTime;
        public AliveType AliveType;
        public int TimingOnlyCount; // 同じタイミングで発動できる回数
        //public int TurnCount;

        public List<FeatureData> FeatureDates;
        public List<TriggerData> TriggerDates;
        public List<TriggerData> ScopeTriggers;
        public bool IsHpDamageFeature()
        {
            return FeatureDates.Find(a => a.FeatureType == FeatureType.HpDamage || a.FeatureType == FeatureType.HpConsumeDamage || a.FeatureType == FeatureType.RevengeHpDamage || a.FeatureType == FeatureType.HpStateDamage) != null;
        }

        public bool IsHpHealFeature()
        {
            return FeatureDates.Find(a => a.FeatureType == FeatureType.HpHeal || a.FeatureType == FeatureType.HpDivide) != null;
        }

        public bool IsAbsoluteHit()
        {
            return FeatureDates.Find(a => a.FeatureType == FeatureType.AbsoluteHit) != null;
        }

        public bool IsStateFeature(StateType stateType)
        {
            return FeatureDates.Find(a => (a.FeatureType == FeatureType.AddState || a.FeatureType == FeatureType.AddStateNextTurn) && a.Param1 == (int)stateType) != null;
        }

        public bool IsAddBuffFeature()
        {
            return FeatureDates.Find(a => (a.FeatureType == FeatureType.AddState || a.FeatureType == FeatureType.AddStateNextTurn) && DataSystem.FindState(a.Param1).Buff) != null;
        }
        public bool IsAddAbnormalFeature()
        {
            return FeatureDates.Find(a => (a.FeatureType == FeatureType.AddState || a.FeatureType == FeatureType.AddStateNextTurn) && DataSystem.FindState(a.Param1).Abnormal) != null;
        }

        public bool IsRevengeHpDamageFeature()
        {
            return FeatureDates.Find(a => a.FeatureType == FeatureType.RevengeHpDamage) != null;
        }

        public bool IsDisplayBattleSkill()
        {
            if (IsDisplayStartBattle())
            {
                return false;
            }
            return Id >= 100 || Id == 31 || Id == 33;
        }

        public bool IsDisplayStartBattle()
        {
            return TriggerDates.Find(a => a.TriggerTiming == TriggerTiming.StartBattle) != null;
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
                case TargetType.Counter:
                return DataSystem.GetText(606);
                case TargetType.IsTriggerTarget:
                return DataSystem.GetText(611);
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
                case ScopeType.OneAndNeighbor:
                return DataSystem.GetText(651);
                case ScopeType.Neighbor:
                return DataSystem.GetText(652);
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
        }
    }

    public enum AnimationType
    {
        None = 0,
        One = 1,
        Line = 2,
        All = 3
    }

    public enum ScopeType
    {
        None = 0,
        One = 1,
        Line = 2,
        All = 3,
        Self = 4,
        FrontLine = 5,
        WithoutSelfOne = 11,
        WithoutSelfLine = 12,
        WithoutSelfAll = 13,
        RandomOne = 21,
        OneAndNeighbor = 31,
        Neighbor = 32,
    }

    public enum SkillType
    {
        None = 0,
        Active = 1, // アクティブ
        Passive = 2, // パッシブ
        Messiah = 3, // 神化
        Awaken = 4, // 覚醒
        Relic = 5, // レリック
        Enhance = 6, // 強化
        UseAlcana = 11, // アルカナ使用
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

    public enum RankType
    {
        None = 0,
        ActiveRank1 = 1,
        ActiveRank2 = 2,
        PassiveRank1 = 10,
        PassiveRank2 = 20,
        EnhanceRank1 = 100,
        EnhanceRank2 = 110,
        RelicRank1 = 200,
        RelicRank2 = 210,
        Uniq = 1000,
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
        FriendLineFront = 5030, // 前列の味方
        FriendLineBack = 5040, // 後列の味方
        OpponentLineFront = 5050, // 前列の敵
        OpponentLineBack = 5060, // 後列の敵
        FriendLineMoreTarget = 5070, // 人数が多い列の味方
        OpponentLineMoreTarget = 5080, // 人数が多い列の敵
        FriendLineLessTarget = 5090, // 人数が少ない列の味方
        OpponentLineLessTarget = 5100, // 人数が少ない列の敵
        FriendMoreTargetCount = 5110, // 〇人以上いる列の味方限定
        OpponentMoreTargetCount = 5120, // 〇人以上いる列の敵限定
        FriendBattleIndex = 5130, // 〇番目の味方限定
        OpponentBattleIndex = 5140, // 〇番目の敵限定
        IsState = 6010, // StateId状態になっている
        IsNotState = 6020, // StateId状態になっていない
        FriendIsState = 6030, // StateId状態になっている味方
        OpponentIsState = 6040, // StateId状態になっている敵
        FriendIsNotState = 6050, // StateId状態になっていない味方
        OpponentIsNotState = 6060, // StateId状態になっていない敵
        IsAbnormalState = 6110, // AbnormalのState状態になっている
        FriendIsAbnormalState = 6120, // AbnormalのState状態になっている味方
        OpponentIsAbnormalState = 6130, // AbnormalのState状態になっている敵
        FriendIsNotAbnormalState = 6140, // AbnormalのState状態になっていない味方
        OpponentIsNotAbnormalState = 6150, // AbnormalのState状態になっていない敵
        IsBuffState = 6210, // バフのState状態になっている
        FriendIsBuffState = 6220, // バフのState状態になっている味方
        OpponentIsBuffState = 6230, // バフのState状態になっている敵
        FriendIsNotBuffState = 6240, // バフのState状態になっていない味方
        OpponentIsNotBuffState = 6250, // バフのState状態になっていない敵
        IsDeBuffState = 6310, // デバフのState状態になっている
        FriendIsDeBuffState = 6320, // デバフのState状態になっている味方
        OpponentIsDeBuffState = 6330, // デバフのState状態になっている敵
        FriendIsNotDeBuffState = 6340, // デバフのState状態になっていない味方
        OpponentIsNotDeBuffState = 6350, // デバフのState状態になっていない敵
        IsNotAwaken = 7010, // 神化発動前
        IsAwaken = 7020, // 神化発動後
        FriendIsNotAwaken = 7030, // 神化発動前の味方
        FriendIsAwaken = 7040, // 神化発動後の味方
        OpponentIsNotAwaken = 7050, // 神化発動前の敵
        OpponentIsAwaken = 7060, // 神化発動後の敵
        LessTroopMembers = 8010, // 味方より敵が多い
        MoreTroopMembers = 8020, // 味方より敵が少ない
        FriendMembersMoreCount = 8030, // 味方の数が〇以上
        FriendMembersLessCount = 8040, // 味方の数が〇以下
        OpponentMembersMoreCount = 8050, // 敵の数が〇以上
        OpponentMembersLessCount = 8060, // 敵の数が〇以下
        TurnNumUnder = 9010, // ターン数が〇以内
        TurnNum = 9020, // ターン数が〇
        TurnNumPer = 9030, // ターン数がparam1 x ターン数 + param2
        ActionInfoTurnNumPer = 9031, // 行動者のターン数がparam1 x ターン数 + param2
        ActionCountPer = 9040, // 全体の行動数がparam1 x 行動数 + param2
        SelfTargetOnly = 9110, // 自身を対象にする
        SelfTargetNotOnly = 9120, // 自身を対象にしない
        AttackState = 10010, // 攻撃成功時〇%で
        Percent = 10020, // 〇%で
        AttackStateNoFreeze = 10110, // 攻撃成功時対象が凍結ではない%で
        InBattleUseCountUnder = 11010, // バトル中使用回数が〇以下
        InTurnUseCountUnder = 11020, // ターン中使用回数が〇以下
        ActionMpCost = 12030, // 行動Magicの消費Mpが〇
        TargetHpRateUnder = 12040, // 攻撃を受けた対象のHpが〇%以下
        TargetDeath = 12041, // 攻撃を受けた対象が戦闘不能になった
        TargetAbnormal = 12042, // 攻撃を受けた対象がAbnormalになった
        TargetBuff = 12043, // 攻撃を受けた対象がバフ状態になった
        OneAttackOverDamage = 12050, // 1回の攻撃で〇ダメージ以上受ける
        FriendAttackedAction = 12060, // 味方が攻撃を受ける
        SelfAbnormalAction = 12063, // 自身が状態異常を受ける
        SelfPassiveAction = 12066, // 自身がパッシブを受ける
        FriendAction = 12070, // 味方が行動を成功する
        FriendAttackAction = 12071, // 味方が攻撃を成功する
        OpponentHealAction = 12082, // 相手が回復を成功する
        OpponentDamageShieldAction = 12092, // 相手が攻撃無効を発動する
        FriendHasKind = 13010, // 〇のKindを持っている
        OpponentHasKind = 13020, // 〇のKindを持っている
        FriendStatusUpper = 14010, // ステータスの高い味方
        FriendStatusUnder = 14110, // ステータスの低い味方
        OpponentStatusUpper = 14210, // ステータスの高い敵
        OpponentStatusUnder = 14310, // ステータスの低い敵
        LvUpper = 14410, // Lvが〇以上
        SelfAttackActionInfo = 15010, // 自身が攻撃タイプの行動をしようとしている
        FriendAttackActionInfo = 15011, // 味方が攻撃タイプの行動をしようとしている
        OpponentBuffActionInfo = 15022, // 相手がバフの行動をしようとしている
        SelfActionInfo = 16010, // 行動したのが自身
        SkillUsedCount = 17010, // 魔法を〇回使用する
        AttackedCount = 17020, // 攻撃を〇回受ける
        BeCriticalCount = 17030, // クリティカル攻撃を〇回受ける
        DodgeCountOver = 17040, // 回避を〇回行う
        HpHealCountOver = 17050, // Hp回復魔法を〇回行う
        ChainCount = 17060, // 拘束成功回数
        IsFriendBattler = 18010, // 自身の味方
        IsOpponentBattler = 18020, // 自身の味方
        AttackedActionIsScope = 19010, // 自身がParam1=Sconeの攻撃を受けるとき
        AttackedActionIsKind = 19020, // 自身がParam1=Kindから攻撃を受けるとき
        ActionResultDeath = 20030, // 攻撃を受けると戦闘不能になる
        DeadWithoutSelf = 20040, // 自身以外が戦闘不能
        SelfDead = 20050, // 自身が戦闘不能
        AllEnemyCurseState = 20070, // 敵全員が呪い
        AllEnemyFreezeState = 20071, // 敵全員が呪い
        DemigodMemberCount = 20110, // 味方の神化状態が〇以上
        AwakenDemigodAttribute = 20120, // Demigod魔法の属性が〇の味方が神化する
        ActionResultAddState = 20130, // 相手が状態異常を発動する前
        DefeatEnemyByAttack = 20140, // 攻撃で敵を撃破する
        DemigodMagicAttribute = 20150, // Demigod魔法の属性が〇の味方が神化する
        ActionResultSelfDeath = 20160, // 自身が戦闘不能になる攻撃を受ける
        InterruptAttackDodge = 23010, // 攻撃を回避した時
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
        BeforeSelfUse = 31,
        BeforeOpponentUse = 32,
        BeforeFriendUse = 33,
        BeforeTacticsTurn = 51,
        CurrentTacticsTurn = 52
    }

    public enum FeatureType
    {
        None = 0,
        HpDamage = 1010,
        HpDrain = 1020,
        HpSlipDamage = 1030,
        HpStateDamage = 1040,
        NoEffectHpDamage = 1050,
        NoEffectHpPerDamage = 1060,
        NoEffectHpAddDamage = 1070,
        HpConsumeDamage = 1080,
        RevengeHpDamage = 1090,
        PenetrateHpDamage = 1100,
        HpParamHpDamage = 1110, // 残りHp比例ダメージ
        HpHeal = 2010,
        RemainHpOne = 2020,
        RemainHpOneTarget = 2030,
        HpDivide = 2040,
        DamageHpHealParty = 2110,
        DamageMpHealParty = 2120,
        AddState = 3010,
        RemoveState = 3020,
        RemoveAbnormalState = 3030,
        AddStateNextTurn = 3040,
        RemoveBuffState = 3050,
        RemoveDeBuffState = 3060,
        ChangeStateParam = 3070,
        CtDamage = 4010,
        CtHeal = 4020,
        CtDrain = 4030,
        ActiveCtHeal = 4040,
        SetAfterAp = 5010,
        SetAfterApHalf = 5011,
        ActionResultSetAp = 5012,
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
        ChangeMagicCountTurn = 6080,
        ChangeSkillRepeatTime = 6100,
        ChangeSkillScope = 6210,
        AddSkillPlusSkill = 6310,
        PlusSkill = 7010,
        PlusSkillTrigger = 7020,
        KindHeal = 8010,
        BreakUndead = 8020,
        ActionAfterGainAp = 10010, // 行動後にAp+
        AbsoluteHit = 11010,
        ReflectLastAbnormal = 12010, // 最後に受けたAbnormalを相手に移す
        RobBuffState = 13010, // 対象のバフを奪い取る
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

    public enum LearningState
    {
        None = 0,
        Learned = 1,
        NotLearn = 2,
        Equipment = 3,
        SelectLearn = 4,
        NotLearnedByAlchemy = 11, // 習得しているがpartyがそのフラグを持っていない
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