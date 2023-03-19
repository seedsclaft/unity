using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Effekseer;

public class BattleActorList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    [SerializeField] private List<GameObject> damageRoots;
    private List<BattlerInfo> _battleInfos = new List<BattlerInfo>();
    private List<BattleActor> _battleActors = new List<BattleActor>();
    private ScopeType _targetScopeType = ScopeType.None;
    private List<int> _targetIndexList = new List<int>();
    public int selectIndex{
        get {return Index;}
    }

    public void Initialize(System.Action<List<int>> callEvent,System.Action cancelEvent,System.Action enemySelectEvent)
    {
        damageRoots.ForEach(a => a.SetActive(false));
        InitializeListView(rows);
        for (int i = 0; i < rows;i++)
        {
            var battleActor = ObjectList[i].GetComponent<BattleActor>();
            battleActor.SetCallHandler((actorIndex) => {
                BattlerInfo battlerInfo = _battleInfos.Find(a => a.Index == actorIndex);
                if (_targetIndexList.IndexOf(actorIndex) == -1)
                {
                    return;
                }
                callEvent(MakeTargetIndexs(actorIndex));
            });
            battleActor.SetSelectHandler((data) => UpdateSelectIndex(data));
            battleActor.SetDamageRoot(damageRoots[i]);
            _battleActors.Add(battleActor);
            ObjectList[i].SetActive(false);
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent,enemySelectEvent));
    }

    public void Refresh(List<BattlerInfo> battlerInfos)
    {
        _battleInfos.Clear();
        for (var i = 0; i < battlerInfos.Count;i++)
        {
            _battleInfos.Add(battlerInfos[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ObjectList[i].SetActive(false);
            if (i < _battleInfos.Count) 
            {
                var battleActor = ObjectList[i].GetComponent<BattleActor>();
                battleActor.SetData(battlerInfos[i],i);
                ObjectList[i].SetActive(true);
            }
        }
        UpdateAllItems();
    }
    
    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
    
    public void RefreshTarget(ActionInfo actionInfo)
    {
        UpdateAllUnSelect();
        if (actionInfo == null) {
            _targetScopeType = ScopeType.None;
            return;
        }
        _targetIndexList = actionInfo.TargetIndexList;
        _targetScopeType = actionInfo.ScopeType;
        if (_targetScopeType == ScopeType.All)
        {
            UpdateAllSelect();
        } else
        if (_targetScopeType == ScopeType.Line)
        {
            UpdateLineSelect(actionInfo.LastTargetIndex);
        } else
        if (_targetScopeType == ScopeType.One)
        {
            UpdateSelectIndex(actionInfo.LastTargetIndex);
        } else
        if (_targetScopeType == ScopeType.Self)
        {
            UpdateSelectIndex(actionInfo.SubjectIndex);
        }
    }
    
    private void UpdateAllSelect(){
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var listItem = ObjectList[i].GetComponent<ListItem>();
            listItem.SetSelect();
            _battleActors[i].BattlerInfoComponent.SetSelectable(true);
        }
    }

    private void UpdateAllUnSelect(){
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var listItem = ObjectList[i].GetComponent<ListItem>();
            listItem.SetUnSelect();
        }
    }
    
    private new void UpdateSelectIndex(int index){
        if (_targetIndexList.IndexOf(index) == -1)
        {
            return;
        }
        if (_targetScopeType == ScopeType.All)
        {
            UpdateAllSelect();
            return;
        }
        if (_targetScopeType == ScopeType.Line)
        {
            UpdateLineSelect(index);
            return;
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var listItem = ObjectList[i].GetComponent<ListItem>();
            if (index == i){
                listItem.SetSelect();
            } else{
                listItem.SetUnSelect();
            }
            _battleActors[i].BattlerInfoComponent.SetSelectable(index == i);
        }
    }

    private void UpdateLineSelect(int index){
    }
    
    public BattlerInfoComponent GetBattlerInfoComp(int index)
    {
        return _battleActors[index].BattlerInfoComponent;
    }
    
    private void CallInputHandler(InputKeyType keyType, System.Action<List<int>> callEvent,System.Action cancelEvent,System.Action enemySelectEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            int actorIndex = _battleInfos[Index].Index;
            BattlerInfo battlerInfo = _battleInfos.Find(a => a.Index == _battleInfos[Index].Index);
            if (_targetIndexList.IndexOf(actorIndex) == -1)
            {
                return;
            }
            callEvent(MakeTargetIndexs(actorIndex));
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
        if (keyType == InputKeyType.SideLeft1)
        {
            enemySelectEvent();
        }
    }

    private List<int> MakeTargetIndexs(int actorIndex)
    {
        List<int> indexList = new List<int>();
        if (_targetScopeType == ScopeType.All)
        {
            for (int i = 0; i < ObjectList.Count;i++)
            {
                indexList.Add(i);
            }
        } else
        if (_targetScopeType == ScopeType.Line)
        {
            indexList.Add(actorIndex);
        } else
        if (_targetScopeType == ScopeType.One)
        {
            indexList.Add(actorIndex);
        } else
        if (_targetScopeType == ScopeType.Self)
        {
            indexList.Add(actorIndex);
        }
        return indexList;
    }
}
