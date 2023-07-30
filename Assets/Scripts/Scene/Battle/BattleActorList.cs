using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Effekseer;

public class BattleActorList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private List<GameObject> damageRoots;
    private List<BattlerInfo> _battleInfos = new List<BattlerInfo>();
    private List<BattleActor> _battleActors = new List<BattleActor>();
    private ScopeType _targetScopeType = ScopeType.None;
    private List<int> _targetIndexList = new List<int>();
    private int _selectIndex = -1;

    public void Initialize(int battleActorsCount,System.Action<List<int>> callEvent,System.Action cancelEvent,System.Action enemySelectEvent)
    {
        damageRoots.ForEach(a => a.SetActive(false));
        InitializeListView(battleActorsCount);
        for (int i = 0; i < battleActorsCount;i++)
        {
            var battleActor = ObjectList[i].GetComponent<BattleActor>();
            battleActor.SetCallHandler((actorIndex) => {
                BattlerInfo battlerInfo = _battleInfos.Find(a => a.Index == actorIndex);
                if (_targetIndexList.IndexOf(actorIndex) == -1)
                {
                    return;
                }
                if (_selectIndex != actorIndex)
                {
                    UpdateTargetIndex(actorIndex);
                    return;
                }
                callEvent(MakeTargetIndexs(battlerInfo));
            });
            battleActor.SetSelectHandler((data) => 
                {        
                    if (IsInputEnable() == false) return;
                    UpdateTargetIndex(data);
                }
            );
            battleActor.SetDamageRoot(damageRoots[i]);
            _battleActors.Add(battleActor);
            ObjectList[i].SetActive(false);
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent,enemySelectEvent));
    }

    public void Refresh(List<BattlerInfo> battlerInfos)
    {
        _battleInfos.Clear();
        _battleInfos = battlerInfos;
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
    
    public void RefreshTarget(int selectIndex,List<int> targetIndexList,ScopeType scopeType)
    {
        UpdateAllUnSelect();
        if (selectIndex == -1) {
            UpdateSelectIndex(-1);
            _targetScopeType = ScopeType.None;
            _targetIndexList = new ();
            return;
        }
        _selectIndex = selectIndex;
        _targetIndexList = targetIndexList;
        _targetScopeType = scopeType;
        if (_targetScopeType == ScopeType.All)
        {
            UpdateAllSelect();
        } else
        if (_targetScopeType == ScopeType.WithoutSelfAll)
        {
            UpdateTargetIndex(_selectIndex);
        } else
        if (_targetScopeType == ScopeType.Line)
        {
            UpdateTargetIndex(_selectIndex);
        } else
        if (_targetScopeType == ScopeType.One || _targetScopeType == ScopeType.WithoutSelfOne)
        {
            UpdateTargetIndex(_selectIndex);
        } else
        if (_targetScopeType == ScopeType.Self)
        {
            UpdateTargetIndex(_selectIndex);
        }
    }
    
    private void UpdateAllSelect(){
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var listItem = ObjectList[i].GetComponent<ListItem>();
            listItem.SetSelect();
        }
    }

    private void UpdateAllUnSelect(){
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var listItem = ObjectList[i].GetComponent<ListItem>();
            listItem.SetUnSelect();
        }
    }
    
    private void UpdateTargetIndex(int index){
        if (_targetIndexList.IndexOf(index) == -1)
        {
            return;
        }
        _selectIndex = index;
        UpdateSelectIndex(index);
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
            BattlerInfo battlerInfo = _battleInfos.Find(a => a.Index == _selectIndex);
            if (_targetIndexList.IndexOf(_selectIndex) == -1)
            {
                return;
            }
            callEvent(MakeTargetIndexs(battlerInfo));
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
        if (keyType == InputKeyType.SideLeft1)
        {
            enemySelectEvent();
        }
        if (keyType == InputKeyType.Down)
        {
            BattlerInfo current = _battleInfos.Find(a => a.Index == _selectIndex);
            if (current != null)
            {
                List<BattlerInfo> targets = _battleInfos.FindAll(a => a.Index > current.Index);
                for (int i = 0;i < targets.Count;i++)
                {
                    if (_targetIndexList.Contains(targets[i].Index))
                    {
                        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                        UpdateTargetIndex(targets[i].Index);
                        break;
                    }
                }
            }
        }
        if (keyType == InputKeyType.Up)
        {
            BattlerInfo current = _battleInfos.Find(a => a.Index == _selectIndex);
            if (current != null)
            {
                List<BattlerInfo> targets = _battleInfos.FindAll(a => a.Index < current.Index);
                for (int i = 0;i < targets.Count;i++)
                {
                    if (_targetIndexList.Contains(targets[i].Index))
                    {
                        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                        UpdateTargetIndex(targets[i].Index);
                        break;
                    }
                }
            }
        }
    }

    private List<int> MakeTargetIndexs(BattlerInfo battlerInfo)
    {
        List<int> indexList = new List<int>();
        if (_targetScopeType == ScopeType.All || _targetScopeType == ScopeType.WithoutSelfAll)
        {
            for (int i = 0; i < _battleInfos.Count;i++)
            {
                if (_battleInfos[i].IsAlive())
                {
                }
                indexList.Add(_battleInfos[i].Index);
            }
        } else
        if (_targetScopeType == ScopeType.Line)
        {
            for (int i = 0; i < _battleInfos.Count;i++)
            {
                if (battlerInfo.LineIndex == _battleInfos[i].LineIndex)
                {
                    if (_battleInfos[i].IsAlive())
                    {
                    }
                    indexList.Add(_battleInfos[i].Index);
                }
            }
        } else
        if (_targetScopeType == ScopeType.One || _targetScopeType == ScopeType.WithoutSelfOne)
        {
            if (battlerInfo.IsAlive())
            {
            }
            indexList.Add(battlerInfo.Index);
        } else
        if (_targetScopeType == ScopeType.Self)
        {
            if (battlerInfo.IsAlive())
            {
            }
            indexList.Add(battlerInfo.Index);
        }
        return indexList;
    }

    private new void InputHandler(InputKeyType keyType)
    {
    }
}
