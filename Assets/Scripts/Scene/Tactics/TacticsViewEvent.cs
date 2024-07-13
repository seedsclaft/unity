using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class TacticsViewEvent
    {
        public Tactics.CommandType commandType;
        public object template;

        public TacticsViewEvent(Tactics.CommandType type)
        {
            commandType = type;
        }
    }
}

namespace Tactics
{
    public enum CommandType
    {
        None = 0,
        SelectTacticsCommand, // // 戦略コマンドを選択した
        SelectSymbol,
        PopupSkillInfo,
        SelectRecord,
        CancelSymbolRecord,
        CancelSelectSymbol,
        //SelectActorResource,
        Parallel,
        DecideRecord,
        CallEnemyInfo,
        Back,
        SelectSideMenu,
        StageHelp,
        CancelRecordList, // レコードリストを非表示にする
        ScorePrize,
        AlcanaCheck,
        SelectAlcanaList,
        HideAlcanaList,
        EndShopSelect,
        NormalWorld,
        AnotherWorld,
    }
}
