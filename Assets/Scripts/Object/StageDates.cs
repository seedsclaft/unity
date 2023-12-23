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
    public int SubordinateValue;
    public List<StageEventData> StageEvents;
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
    AbortStage = 21, // ステージを中断する
    ChangeRouteSelectStage = 31, // ルート分岐でステージに移動
    RouteSelectBattle = 32, // ルート分岐敵グループを生成
    SetDisplayTurns = 33, // 表示残りターンをマスターから取得
    RebornSkillEffect = 41, // 継承スキル演出再生
    MoveStage = 51, // ステージ移動
    SetDefineBoss = 61, // 中ボスを設定する
    SetLastBoss = 62, // 上位者ボスを設定する
}


public enum EndingType{
    A,
    B,
    C,
}
