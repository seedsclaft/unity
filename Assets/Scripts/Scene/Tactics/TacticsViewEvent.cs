using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    namespace Tactics
    {
        public enum CommandType
        {
            None = 0,
            CallSymbolList,
            CallStatus,
            CallSymbol,
            OnClickSymbol,
            OnCancelSymbol,
            SelectSymbol,
            PopupSkillInfo,
            SelectRecord,
            CancelSymbolRecord,
            CancelSelectSymbol,
            //SelectActorResource,
            Parallel,
            DecideRecord,
            CallEnemyInfo,
            CallAddActorInfo,
            Back,
            SelectSideMenu,
            StageHelp,
            CancelRecordList, // レコードリストを非表示にする
            ScorePrize,
            AlcanaCheck,
            SelectAlcanaList,
            HideAlcanaList,
            EndShopSelect,
            SelectCharaLayer,
        }
    }

}
