using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugBattleData : MonoBehaviour
{
    public TMP_InputField consoleInputField = null;
    [SerializeField] private List<int> inBattleActorIds = null;
    [SerializeField] private int troopId = 0;
    [SerializeField] private int troopLv = 0;
    [SerializeField] private List<ActorsData.ActorData> actorDatas = null;
    [SerializeField] private string advName = "";
    public string AdvName { get {return advName;}}

    void Start()
    {
#if UNITY_EDITOR
        consoleInputField.onEndEdit.AddListener((a) => CallConsoleCommand(a));
#else
        gameObject.SetActive(false);
#endif
    }

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
        GameSystem.CurrentData.Actors.ForEach(a => a.SetInBattle(true));

        GameSystem.CurrentData.CurrentStage.TestTroops(troopId,troopLv);
    }
#if UNITY_EDITOR
    private BattleModel _model;
    private BattlePresenter _presenter;
    private BattleView _view;
    [SerializeField] private bool saveActorData = false;
    public void SetDebugger(BattleModel model,BattlePresenter presenter,BattleView view)
    {
        _model = model;
        _presenter = presenter;
        _view = view;
    }
    
    void CallConsoleCommand(string inputText)
    {
        if (_presenter == null) return;
        if (consoleInputField.text == "T0")
        {
            _presenter.SetDebug(true);
        }
        if (consoleInputField.text == "T1")
        {
            _presenter.SetDebug(false);
        }
        if (consoleInputField.text.Contains("AC"))
        {
            var replace = consoleInputField.text.Replace("AC","");
            var command = replace.Split(",");
            if (command.Length != 2) return;
            var battlerInfo = _model.GetBattlerInfo(int.Parse( command[0] ));
            if (battlerInfo == null) return;
            var skillInfo = battlerInfo.Skills.Find(a => a.Master.Id == int.Parse( command[1] ));
            if (skillInfo == null) return;
            _model.SetActionBattler(battlerInfo.Index);
            ActionInfo actionInfo = _model.MakeActionInfo(battlerInfo,skillInfo.Id,false,false);
            _presenter.CommandSelectIndex(_model.MakeAutoSelectIndex(actionInfo));
        }
    }

    private void Update() {
        if (saveActorData == true)
        {
            saveActorData = false;
            actorDatas = DataSystem.Actors;
        }
    }
#endif
}