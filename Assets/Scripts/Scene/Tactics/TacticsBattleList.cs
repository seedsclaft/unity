using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class TacticsBattleList : ListWindow , IInputHandlerEvent
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
            var tacticsBattle = ObjectList[i].GetComponent<TacticsBattle>();
            if (i < _actorInfos.Count)
            {
                tacticsBattle.SetData(_actorInfos[i],i);
            }
            tacticsBattle.SetCallHandler(callEvent);
            tacticsBattle.SetSelectHandler((data) => UpdateSelectIndex(data));
            ObjectList[i].SetActive(i < _actorInfos.Count);
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent));
        UpdateSelectIndex(0);
        tacticsCommandList.UpdateSelectIndex(-1);
        
        commandLv.text = rank.ToString();
        commandDescription.text = DataSystem.System.GetTextData(10).Text.Replace("\\d",DataSystem.System.BattleCount.ToString());
        if (rank > 0)
        {
            commandDescription.text = DataSystem.System.GetTextData(14).Text.Replace("\\d",(rank * 4).ToString());
        }
        Refresh();
    }

    public void InitializeConfirm(List<SystemData.MenuCommandData> confirmCommands ,System.Action<TacticsComandType> callEvent)
    {
        _confirmEvent = callEvent;
        tacticsCommandList.Initialize(callEvent);
        tacticsCommandList.Refresh(confirmCommands);
        tacticsCommandList.SelectIndex(-1);
        SetCancelEvent(() => _confirmEvent(TacticsComandType.Train));
    }

    public void Refresh()
    {
        UpdateAllItems();
        tacticsCommandList.SelectIndex(-1);
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
