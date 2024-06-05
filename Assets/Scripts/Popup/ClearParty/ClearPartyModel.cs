using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public class ClearPartyModel : BaseModel
    {
        public async UniTask<List<ListData>> ClearParty()
        {
            var list = new List<SaveBattleInfo>();
            var stageId = string.Format(CurrentStage.Id.ToString(),"0:00");
            var turn = string.Format(CurrentStage.CurrentTurn.ToString(),"0:00");
            var seek = string.Format(CurrentStage.CurrentSeekIndex.ToString(),"0:00");
            if (SaveSystem.ExistReplay(stageId+turn+seek))
            {
                var saveRecord = await SaveSystem.LoadReplay(stageId+turn+seek);
                if (saveRecord != null)
                {
                    list.Add(saveRecord);
                }  
            }
            return MakeListData(list);
        }
        
        public void SetInReplay()
        {
            PartyInfo.SetInReplay(true);
        }

    }
}