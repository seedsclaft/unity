using System;
using System.Collections.Generic;
using UnityEngine;

public class StageDates : ScriptableObject
{
    [SerializeField] public List<StageData> Data = new();
}

[Serializable]
public class StageData
{   
    public int Id;
    public string Name;
    public string AchieveText;
    public bool Selectable;
    public string Help;
    public int Turns;
    public List<int> InitMembers;
    public int RandomTroopCount;
    public int BossId;
    public int BGMId;
    public int BossBGMId;
    public bool Reborn;
    public bool Alcana;
    public int SaveLimit;
    public int ContinueLimit;
    public int RankingStage;
    public bool SlotSave;
    public int SubordinateValue;
    public bool UseSlot;
    public List<StageEventData> StageEvents;
    public List<StageTutorialData> Tutorials;
}



[Serializable]
public class StageEventData
{
    public string EventKey;
    public int Turns;
    public EventTiming Timing;
    public StageEventType Type;
    public int Param;
    public bool ReadFlag;
}

[Serializable]
public class StageTutorialData
{
    public int Turns;
    public EventTiming Timing;
    public TutorialType Type;
    public int X;
    public int Y;
    public int Width;
    public int Height;
}

public enum EventTiming{
    None = 0,
    StartTactics = 1,
    StartStrategy = 2,
    StartBattle = 3,
    TurnedBattle = 4,
    AfterDemigod = 5,
    BeforeTactics = 11,
}

public enum StageEventType{
    None = 0,
    CommandDisable = 1, // コマンドを制限する
    TutorialBattle = 2, // バトルをチュートリアルで固定する
    NeedAllTactics = 3, // 全員コマンドを選ばないと進まない
    IsSubordinate = 4, // 隷従属度フラグを管理
    IsAlcana = 5, // アルカナフラグを管理
    SelectAddActor = 6, // 仲間を選んで加入する
    SaveCommand = 7, // セーブを行う,
    SetDefineBossIndex = 8, // ボスの選択番号を設定する
    NeedUseSp = 9, // SPを消費しないと進まない
    AdvStart = 11, // ADV再生
    SelectActorAdvStart = 12, // IDにActorIDを加算してADV再生
    RouteSelectEvent = 13, // ルート分岐イベント
    SetRouteSelectParam = 14, // ルート分岐パラメータを保存
    RouteSelectMoveEvent = 15, // ルート分岐ステージイベント
    ClearStage = 21, // ステージをクリアする
    ChangeRouteSelectStage = 31, // ルート分岐でステージに移動
    RouteSelectBattle = 32, // ルート分岐敵グループを生成
    SetDisplayTurns = 33, // 表示残りターンをマスターから取得
    RebornSkillEffect = 41, // 継承スキル演出再生
    MoveStage = 51, // ステージ移動
    SetDefineBoss = 61, // 中ボスを設定する
    SetLastBoss = 62, // 上位者ボスを設定する
    SurvivalMode = 201, // サバイバルモードにする
}

public enum TutorialType{
    None = 0,
    TacticsCommandTrain = 1, // TacticsでTrain選択
    TacticsCommandAlchemy = 2, // TacticsでAlchemy選択
    TacticsCommandRecover = 3, // TacticsでRecover選択
    TacticsCommandBattle = 4, // TacticsでBattle選択
    TacticsCommandResource = 5, // TacticsでResource選択
    TacticsSelectTacticsActor = 11, // TacticsTrainでアクターを選択
    TacticsSelectTacticsDecide = 12, // TacticsTrainで決定を選択
    TacticsSelectEnemy = 21, // TacticsBattleで敵を選択
    TacticsSelectAlchemyMagic = 22, // TacticsAlchemyで魔法を選択

}

public enum EndingType{
    A,
    B,
    C,
}
