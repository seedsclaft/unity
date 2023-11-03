using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class TacticsRecoveryList : ListWindow , IInputHandlerEvent
{
    private List<ActorInfo> _actorInfos = new List<ActorInfo>();
    [SerializeField] private BaseList baseCommandList;
    [SerializeField] private TextMeshProUGUI commandLv;
    [SerializeField] private TextMeshProUGUI commandDescription;
    private System.Action _confirmEvent = null;

    public ListData CommandData {
        get {
            if (baseCommandList.Index > -1)
            {
                return baseCommandList.ListData;
            }
            return null;
        }
    }

    public void Initialize(List<ActorInfo> actorInfos,System.Action<int> callEvent,System.Action<int> plusEvent,System.Action<int> minusEvent,int rank)
    {
        InitializeListView(actorInfos.Count);
        _actorInfos = actorInfos;
        for (int i = 0; i < actorInfos.Count;i++)
        {
            var tacticsRecovery = ObjectList[i].GetComponent<TacticsRecovery>();
            if (i < _actorInfos.Count)
            {
                tacticsRecovery.SetData(_actorInfos[i],i);
            }
            tacticsRecovery.SetCallHandler(callEvent);
            tacticsRecovery.SetPlusHandler(plusEvent);
            tacticsRecovery.SetMinusHandler(minusEvent);
            tacticsRecovery.SetSelectHandler((data) => UpdateSelectIndex(data));
            ObjectList[i].SetActive(i < _actorInfos.Count);
        }
        SetInputCallHandler((a) => CallInputHandler(a,callEvent,plusEvent,minusEvent));
        UpdateSelectIndex(0);
        commandLv.text = rank.ToString();
        commandDescription.text = DataSystem.System.GetReplaceText(10,DataSystem.System.RecoveryCount.ToString());
        if (rank > 0)
        {
            commandDescription.text = DataSystem.System.GetReplaceText(13,(rank * 10).ToString());
        }
        Refresh();
    }

    public void InitializeConfirm(List<ListData> confirmCommands ,System.Action callEvent)
    {
        _confirmEvent = callEvent;
        baseCommandList.Initialize(confirmCommands.Count);
        baseCommandList.SetInputHandler(InputKeyType.Decide,callEvent);
        baseCommandList.SetData(confirmCommands);
        baseCommandList.UpdateSelectIndex(-1);
        baseCommandList.Activate();
        SetCancelEvent(() => _confirmEvent());
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

    private void CallInputHandler(InputKeyType keyType, System.Action<int> callEvent,System.Action<int> plusEvent,System.Action<int> minusEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            if (Index == -1)
            {
                _confirmEvent();
            } else
            {
                callEvent(_actorInfos[Index].ActorId);
            }
        }
        if (keyType == InputKeyType.Cancel)
        {
            _confirmEvent();
        }
        if (keyType == InputKeyType.Down)
        {
            if (Index == 0)
            {
                UpdateSelectIndex(-1);
                baseCommandList.UpdateSelectIndex(1);
            }
        }
        if (keyType == InputKeyType.Up)
        {
            if (Index == _actorInfos.Count-1)
            {
                UpdateSelectIndex(_actorInfos.Count-1);
                baseCommandList.UpdateSelectIndex(-1);
            }
        }
        if (keyType == InputKeyType.Right)
        {
            if (Index == -1)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                baseCommandList.UpdateSelectIndex(1);
            } else{
                plusEvent(_actorInfos[Index].ActorId);
            }
        }
        if (keyType == InputKeyType.Left)
        {
            if (Index == -1)
            {
                Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                baseCommandList.UpdateSelectIndex(0);
            } else{
                minusEvent(_actorInfos[Index].ActorId);
            }
        }
    }
}
