using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class TacticsResourceList : ListWindow , IInputHandlerEvent
{
    private List<ActorInfo> _actorInfos = new List<ActorInfo>();

    [SerializeField] private TacticsCommandList tacticsCommandList;
    [SerializeField] private TextMeshProUGUI commandLv;
    [SerializeField] private TextMeshProUGUI commandDescription;
    private System.Action<TacticsComandType> _confirmEvent = null;



    public void Initialize(List<ActorInfo> actorInfos,System.Action<int> callEvent,int rank)
    {
        InitializeListView(actorInfos.Count);
        _actorInfos = actorInfos;
        for (int i = 0; i < actorInfos.Count;i++)
        {
            var tacticsResource = ObjectList[i].GetComponent<TacticsResource>();
            if (i < _actorInfos.Count)
            {
                tacticsResource.SetData(_actorInfos[i],i);
            }
            tacticsResource.SetCallHandler(callEvent);
            tacticsResource.SetSelectHandler((data) => UpdateSelectIndex(data));
            ObjectList[i].SetActive(i < _actorInfos.Count);
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent));
        commandLv.text = rank.ToString();
        commandDescription.text = DataSystem.System.GetReplaceText(10,DataSystem.System.ResourceCount.ToString());
        if (rank > 0)
        {
            commandDescription.text = DataSystem.System.GetReplaceText(15,(rank * 10).ToString());
        }
        UpdateSelectIndex(0);
        Refresh();
    }

    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        _confirmEvent = callEvent;
        tacticsCommandList.Initialize(callEvent);
        tacticsCommandList.Refresh(confirmCommands);
        tacticsCommandList.UpdateSelectIndex(-1);
        SetCancelEvent(() => _confirmEvent(TacticsComandType.Train));
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
                TacticsComandType tacticsComandType = TacticsComandType.Train;
                if (tacticsCommandList.Index == 1)
                {
                    tacticsComandType = TacticsComandType.None;
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
                tacticsCommandList.UpdateSelectIndex(1);
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
