using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StageSymbol;
using DG.Tweening;

public class StageSymbolView : BaseView
{
    [SerializeField] private BaseList symbolRecordList = null;
    [SerializeField] private TacticsSymbolList tacticsSymbolList = null;
    [SerializeField] private StageInfoComponent stageInfoComponent = null;
    [SerializeField] private CanvasGroup canvasGroup = null;
    [SerializeField] private CanvasGroup cloudCanvasGroup = null;
    private new System.Action<StageSymbolViewEvent> _commandData = null;
    public int SymbolListIndex => symbolRecordList.Index;
    private System.Action _backEvent;

    public override void Initialize() 
    {
        base.Initialize();
        symbolRecordList.Initialize();
        tacticsSymbolList.Initialize();
        symbolRecordList.SetSelectedHandler(() => {
            //var eventData = new TacticsViewEvent(CommandType.SelectRecord);
            //_commandData(eventData);
        });
        new StageSymbolPresenter(this);
    }

    public void StartAnimation()
    {
        var duration = 0.4f;
        var uiView = canvasGroup.GetComponent<RectTransform>();
        AnimationUtility.LocalMoveToTransform(uiView.gameObject,
            new Vector3(uiView.localPosition.x,uiView.localPosition.y + 24,0),
            new Vector3(uiView.localPosition.x,uiView.localPosition.y,0),
            duration);

        AnimationUtility.AlphaToTransform(canvasGroup,
            0.2f,
            1,
            duration);

        cloudCanvasGroup.alpha = 0.8f;
        DOTween.Sequence()
            .SetDelay(duration-0.3f).OnComplete(() => {
                cloudCanvasGroup.DOFade(1,0.1f);
            });
    }

    public void SetStage(StageInfo stageInfo)
    {
        stageInfoComponent.UpdateInfo(stageInfo);
    }

    public void SetSymbolRecords(List<ListData> symbolInfos)
    {
        symbolRecordList.SetData(symbolInfos);
        var SymbolRecordDates = symbolRecordList.GetComponentsInChildren<SymbolRecordData>();
        foreach (var SymbolRecordData in SymbolRecordDates)
        {
            SymbolRecordData.SetSymbolItemCallHandler((a) => OnClickSymbol(a));
        }
    }

    public void SetSymbols(List<ListData> symbolInfos)
    {
        tacticsSymbolList.SetData(symbolInfos);
        tacticsSymbolList.SetInfoHandler((a) => OnClickEnemyInfo());
    }

    public void ShowSymbolList()
    {
        tacticsSymbolList.gameObject.SetActive(true);
    }

    public void HideSymbolList()
    {
        tacticsSymbolList.gameObject.SetActive(false);
    }

    public void SetEvent(System.Action<StageSymbolViewEvent> commandData)
    {
        _commandData = commandData;
    }
    
    private void OnClickEnemyInfo()
    {
        var listData = tacticsSymbolList.ListData;
        if (listData != null)
        {
            var data = (SymbolInfo)listData.Data;
            var eventData = new StageSymbolViewEvent(CommandType.CallEnemyInfo);
            eventData.template = data;
            _commandData(eventData);
        }
    }

    public void SetBackEvent(System.Action backEvent)
    {
        _backEvent = backEvent;
        SetBackCommand(() => 
        {    
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            var eventData = new StageSymbolViewEvent(CommandType.Back);
            _commandData(eventData);
            //if (backEvent != null) backEvent();
        });
        ChangeBackCommandActive(true);
    }

    public void CommandBackEvent()
    {
        if (_backEvent != null) _backEvent();
    }
    
    private void OnClickSymbol(SymbolInfo symbolInfo)
    {
        var eventData = new StageSymbolViewEvent(CommandType.SelectRecord);
        eventData.template = symbolInfo;
        _commandData(eventData);
    }
}

namespace StageSymbol
{
    public enum CommandType
    {
        None = 0,
        SelectRecord = 1,
        CancelRecord,
        Back,
        CallEnemyInfo
    }
}
public class StageSymbolViewEvent
{
    public StageSymbol.CommandType commandType;
    public object template;

    public StageSymbolViewEvent(StageSymbol.CommandType type)
    {
        commandType = type;
    }
}


public class StageSymbolViewInfo{
    public SlotInfo SlotInfo;
    public System.Action EndEvent;
}