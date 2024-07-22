using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TrainViewEvent
    {
        public Train.CommandType commandType;
        public object template;

        public TrainViewEvent(Train.CommandType type)
        {
            commandType = type;
        }
    }
}

namespace Train
{
    public enum CommandType
    {
        None = 0,
        SelectTacticsCommand, // 戦略コマンドを選択した
        SelectTacticsActor, // 戦略中でアクターを決定
        DecideTacticsCommand, // 戦略を決定かキャンセル
        ActorLearnMagic, // 魔法を習得する
        SelectFrontBattleIndex, // 陣形を前にする
        SelectBackBattleIndex, // 陣形を後ろにする
        SelectSkillTrigger, // 作戦画面を表示する
        Back,
        CommandHelp,
        ChangeSelectTacticsActor,
        BattleReplay,
        ShowTacticsCharacter,

        SelectAttribute,
    }
}
