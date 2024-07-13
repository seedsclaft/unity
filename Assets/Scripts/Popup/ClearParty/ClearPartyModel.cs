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
            var stageKey = CurrentStageKey();
            if (SaveSystem.ExistReplay(stageKey))
            {
                var saveRecord = await SaveSystem.LoadReplay(stageKey);
                if (saveRecord != null)
                {
                    list.Add(saveRecord);
                }  
            }
            FirebaseController.LoadReplayFile(stageKey);
            await UniTask.WaitUntil(() => FirebaseController.IsBusy == false);
            if (FirebaseController.ReplayData.Count > 0)
            {
                foreach (var item in FirebaseController.ReplayData)
                {
                    list.Add(item);
                }
            }
            return MakeListData(list);
        }
        
        public void SetInReplay(SaveBattleInfo saveBattleInfo)
        {
            TempInfo.SetInReplay(true);
            TempInfo.SetSaveBattleInfo(saveBattleInfo);
        }
    }
}