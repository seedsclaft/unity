using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SymbolRecord;
using TMPro;
using DG.Tweening;

public class SymbolRecordView : BaseView
{
    [SerializeField] private TacticsCharaLayer tacticsCharaLayer = null;
    [SerializeField] private TacticsSymbolList tacticsSymbolList = null;
    [SerializeField] private BaseList symbolRecordList = null;
    [SerializeField] private TextMeshProUGUI turnText = null;
    [SerializeField] private TextMeshProUGUI numinousText = null;

    private new System.Action<SymbolRecordViewEvent> _commandData = null;

    public int SymbolListIndex => symbolRecordList.Index;

    public override void Initialize() 
    {
        base.Initialize();
        
        tacticsSymbolList.Initialize();
        symbolRecordList.Initialize();
        symbolRecordList.SetSelectedHandler(() => {
            var eventData = new SymbolRecordViewEvent(CommandType.SelectRecord);
            _commandData(eventData);
        });
        new SymbolRecordPresenter(this);
        SetBackCommand(() => OnClickBack());
    }

    private void OnClickBack()
    {
        var eventData = new SymbolRecordViewEvent(CommandType.Back);
        _commandData(eventData);
    }

    public void SetTacticsCharaLayer(List<ActorInfo> actorInfos)
    {
        tacticsCharaLayer.SetData(actorInfos);
    }

    public void SetSymbols(List<ListData> symbolInfos)
    {
        tacticsSymbolList.SetData(symbolInfos);
    }
    
    public void SetSymbolRecords(List<ListData> symbolInfos)
    {
        symbolRecordList.SetData(symbolInfos);
    }

    public void SetTurns(int turns)
    {
        turnText.text = (turns).ToString();
    }
    
    public void SetNuminous(int numinous)
    {
        numinousText.text = numinous.ToString();
    }

    public void SetHelpWindow(){
        HelpWindow.SetInputInfo("");
        HelpWindow.SetHelpText(DataSystem.GetTextData(14010).Text);
    }

    public void SetEvent(System.Action<SymbolRecordViewEvent> commandData)
    {
        _commandData = commandData;
    }
    



}

namespace SymbolRecord
{
    public enum CommandType
    {
        None = 0,
        SelectRecord = 1,
        Back = 2,
    }
}
public class SymbolRecordViewEvent
{
    public SymbolRecord.CommandType commandType;
    public object template;

    public SymbolRecordViewEvent(SymbolRecord.CommandType type)
    {
        commandType = type;
    }
}
