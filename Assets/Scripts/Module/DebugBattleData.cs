using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugBattleData : MonoBehaviour
{
    [SerializeField] private List<int> inBattleActorIds = null;
    [SerializeField] private int troopId = 0;
    [SerializeField] private List<ActorsData.ActorData> actorDatas = null;
    [SerializeField] private string advName = "";
    public string AdvName { get {return advName;}}

    public void MakeBattleActor()
    {
        GameSystem.CurrentData.MakeStageData(1,1);
        GameSystem.CurrentData.CurrentStage.TacticsTroops();
        GameSystem.CurrentData.InitActors();     
        foreach (var actor in actorDatas)
        { 
            if (inBattleActorIds.Contains(actor.Id))
            {
                GameSystem.CurrentData.AddTestActor(actor);
            }
        }
        GameSystem.CurrentData.Actors.ForEach(a => a.InBattle = true);

        GameSystem.CurrentData.CurrentStage.TestTroops(troopId);
    }
}