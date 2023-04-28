using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class TacticsTrainList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<ActorInfo> _actorInfos = new List<ActorInfo>();
    [SerializeField] private TextMeshProUGUI costValue;
    [SerializeField] private TacticsCommandList tacticsCommandList;
    private System.Action<TacticsComandType> _confirmEvent = null;

    public void Initialize(System.Action<int> callEvent) {
        InitializeListView(rows);
        for (int i = 0; i < rows;i++)
        {
            TacticsTrain tacticsTrain = ObjectList[i].GetComponent<TacticsTrain>();
            tacticsTrain.SetCallHandler(callEvent);
            tacticsTrain.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent));
    }

    public void Refresh(List<ActorInfo> actorInfos)
    {
        ResetInputFrame();
        SetDataCount(actorInfos.Count);
        _actorInfos = actorInfos;
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var tacticsTrain = ObjectList[i].GetComponent<TacticsTrain>();
            if (i < _actorInfos.Count)
            {
                tacticsTrain.SetData(_actorInfos[i],i);
            }
            tacticsTrain.SetSelectHandler((data) => UpdateSelectIndex(data));
            ObjectList[i].SetActive(i < _actorInfos.Count);
        }
        UpdateSelectIndex(-1);
    }

    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        _confirmEvent = callEvent;
        tacticsCommandList.Initialize(callEvent);
        tacticsCommandList.Refresh(confirmCommands);
        tacticsCommandList.UpdateSelectIndex(-1);
    }

    public void Refresh()
    {
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
    
    private void CallInputHandler(InputKeyType keyType, System.Action<int> callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            if (Index == -1)
            {
                TacticsComandType tacticsComandType = TacticsComandType.None;
                if (tacticsCommandList.Index == 1)
                {
                    tacticsComandType = TacticsComandType.Train;
                }
                _confirmEvent(tacticsComandType);
            } else
            {
                callEvent(_actorInfos[Index].ActorId);
            }
        }
        if (keyType == InputKeyType.Cancel)
        {
            _confirmEvent(TacticsComandType.Train);
        }
        if (keyType == InputKeyType.Down)
        {
            if (Index == 0)
            {
                UpdateSelectIndex(-1);
                tacticsCommandList.UpdateSelectIndex(0);
            }
        }
        if (keyType == InputKeyType.Up)
        {
            if (Index == _actorInfos.Count-1)
            {
                UpdateSelectIndex(_actorInfos.Count-1);
                tacticsCommandList.UpdateSelectIndex(-1);
            }
        }
        if (keyType == InputKeyType.Right)
        {
            if (Index == -1)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                tacticsCommandList.UpdateSelectIndex(1);
            }
        }
        if (keyType == InputKeyType.Left)
        {
            if (Index == -1)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                tacticsCommandList.UpdateSelectIndex(0);
            }
        }
    }
}
