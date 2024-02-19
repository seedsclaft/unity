using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugBattleData : MonoBehaviour
{
    public TMP_InputField consoleInputField = null;
    [SerializeField] private List<int> inBattleActorIds = null;
    [SerializeField] private int actorLv = 0;
    [SerializeField] private int troopId = 0;
    [SerializeField] private int troopLv = 0;
    [SerializeField] private bool testBattle = false;
    public bool TestBattle => testBattle;
    [SerializeField] private string advName = "";
    public string AdvName => advName;

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
        GameSystem.CurrentStageData = new SaveStageInfo();
        GameSystem.CurrentStageData.Initialize();
        GameSystem.CurrentStageData.MakeStageData(1);
        GameSystem.CurrentStageData.Party.AlcanaInfo.InitData(true);
        if (testBattle)
        {
            var TestBattleData = Resources.Load<TestBattleData>("Data/TestBattle").TestBattleDates;
            var ActorIndex = 1;
            foreach (var TestBattle in TestBattleData)
            {
                if (TestBattle.IsActor)
                {
                    var actorData = DataSystem.FindActor(TestBattle.BattlerId);
                    GameSystem.CurrentStageData.AddTestActor(actorData,TestBattle.Level);
                    var actorInfo = GameSystem.CurrentStageData.Party.ActorInfos.Find(a => a.ActorId == actorData.Id);
                    actorInfo.SetBattleIndex(ActorIndex);
                    actorInfo.SetLineIndex(TestBattle.IsFront ? LineType.Front : LineType.Back);
                    ActorIndex++;
                } else
                {
                    // アルカナ
                    var alcana = GameSystem.CurrentStageData.Party.AlcanaInfo.OwnAlcanaList.Find(a => a.Id == TestBattle.Level);
                    if (alcana != null)
                    {
                        alcana.SetEnable(true);
                    }
                }
            }
            GameSystem.CurrentStageData.CurrentStage.TestTroops(troopId,troopLv);
        } else
        {
            foreach (var actor in DataSystem.Actors)
            { 
                if (inBattleActorIds.Contains(actor.Id))
                {
                    GameSystem.CurrentStageData.AddTestActor(actor,actorLv);
                }
            }
            var idx = 1;
            foreach (var actorInfo in GameSystem.CurrentStageData.Party.ActorInfos)
            {
                actorInfo.SetBattleIndex(idx);
                idx++;
            }
            GameSystem.CurrentStageData.CurrentStage.TestTroops(troopId,troopLv);
        }
    }
#if UNITY_EDITOR
    private BattleModel _model;
    private BattlePresenter _presenter;
    private BattleView _view;

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
            ActionInfo actionInfo = _model.MakeActionInfo(battlerInfo,skillInfo,false,false);
            _presenter.CommandSelectTargetIndexes(_model.MakeAutoSelectIndex(actionInfo));
        }
    }

    private void Update() {
    }
#endif
}