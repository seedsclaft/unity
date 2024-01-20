using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effekseer;

public class BattleBattlerList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private List<GameObject> damageRoots;
    
    private List<BattleBattler> _battleBattler = new ();
    private ScopeType _targetScopeType = ScopeType.None;
    private List<int> _targetIndexList = new ();
    private int _backStartIndex = 0;
    private List<BattlerInfo> _battleInfos = new ();
    private int _selectIndex = -1;
    public int SelectedIndex => _selectIndex;
    private AttributeType _attributeType = AttributeType.None;

    public void Initialize(List<BattlerInfo> battlerInfos ,System.Action<List<int>> callEvent)
    {
        _battleInfos = battlerInfos;
        InitializeListView(battlerInfos.Count);
        damageRoots.ForEach(a => a.SetActive(false));

        for (int i = 0; i < ObjectList.Count;i++)
        {
            var battleBattler = ObjectList[i].GetComponent<BattleBattler>();
            battleBattler.SetDamageRoot(damageRoots[i]);
            battleBattler.SetData(battlerInfos[i],battlerInfos[i].Index,battlerInfos[i].LineIndex == LineType.Front);
            battleBattler.SetCallHandler((enemyIndex) => {
                var battlerInfo = battlerInfos.Find(a => a.Index == enemyIndex);
                if (battlerInfo.IsAlive() == false)
                {
                    return;
                }
                if (_targetIndexList.IndexOf(enemyIndex) == -1)
                {
                    return;
                }
                if (_selectIndex != enemyIndex)
                {
                    UpdateBattlerIndex(enemyIndex);
                    return;
                }
                callEvent(MakeTargetIndexes(battlerInfo));
            });
            battleBattler.SetSelectHandler((data) => UpdateBattlerIndex(data));
            battleBattler.SetPressHandler((enemyIndex) => {
                _selectIndex = enemyIndex;
                CallListInputHandler(InputKeyType.Option1);
            });
            _battleBattler.Add(battleBattler);
        }
        SetInputCallHandler((a) => CallInputHandler(a,callEvent));
        UpdateAllUnSelect();
        UpdateAllItems();
    }

    public void RefreshTarget(int selectIndex,List<int> targetIndexList,ScopeType scopeType,AttributeType attributeType = AttributeType.None)
    {
        UpdateAllUnSelect();
        if (selectIndex == -1) {
            ClearSelect();
            return;
        }
        _selectIndex = selectIndex;
        _targetIndexList = targetIndexList;
        _targetScopeType = scopeType;
        _attributeType = attributeType;
        if (_targetScopeType == ScopeType.All || _targetScopeType == ScopeType.WithoutSelfAll)
        {
            UpdateAllSelect();
        } else
        if (_targetScopeType == ScopeType.Line || _targetScopeType == ScopeType.FrontLine)
        {
            UpdateLineSelect(_selectIndex);
        } else
        if (_targetScopeType == ScopeType.One || _targetScopeType == ScopeType.WithoutSelfOne)
        {
            UpdateBattlerIndex(_selectIndex);
        } else
        if (_targetScopeType == ScopeType.Self)
        {
            UpdateBattlerIndex(_selectIndex);
        }
    }

    private void UpdateAllSelect(){
        for (int i = 0; i < _battleBattler.Count;i++)
        {
            if (_battleInfos[i].IsAlive())
            {
                _battleBattler[i].SetSelect();
            }
        }
    }

    private void UpdateAllUnSelect(){
        foreach (var battleBattler in _battleBattler)
        {
            battleBattler.SetUnSelect();
        }
    }
    
    private void UpdateBattlerIndex(int index){
        if (_targetIndexList.IndexOf(index) == -1)
        {
            return;
        }
        _selectIndex = index;
        UpdateSelectIndex(_selectIndex);
        if (_targetScopeType == ScopeType.All || _targetScopeType == ScopeType.WithoutSelfAll)
        {
            UpdateAllSelect();
            return;
        }
        if (_targetScopeType == ScopeType.Line || _targetScopeType == ScopeType.FrontLine)
        {
            UpdateLineSelect(index);
            return;
        }
        if (_targetScopeType != ScopeType.One && _targetScopeType != ScopeType.WithoutSelfOne)
        {
            return;
        }
        for (int i = 0; i < _battleBattler.Count;i++)
        {
            if (index == _battleBattler[i].Index){
                if (_battleInfos[i].IsAlive())
                {
                    _battleBattler[i].SetSelect();
                }
            } else{
                _battleBattler[i].SetUnSelect();
            }
        }
    }

    private void UpdateLineSelect(int index){
        if (_targetScopeType != ScopeType.Line && _targetScopeType != ScopeType.FrontLine){
            return;
        }
        bool isFront = index < _backStartIndex;
        for (int i = 0; i < _battleBattler.Count;i++)
        {
            if (isFront)
            {
                if (_battleBattler[i].Index < _backStartIndex){
                    if (_battleInfos[i].IsAlive())
                    {
                        _battleBattler[i].SetSelect();
                    }
                } else{
                    _battleBattler[i].SetUnSelect();
                }
            } else
            {
                if (_battleBattler[i].Index >= _backStartIndex){
                    if (_battleInfos[i].IsAlive())
                    {
                        _battleBattler[i].SetSelect();
                    }
                } else{
                    _battleBattler[i].SetUnSelect();
                }
            }
        }
    }

    public BattlerInfoComponent GetBattlerInfoComp(int index)
    {
        return _battleBattler[index].BattlerInfoComponent;
    }
    
    private void CallInputHandler(InputKeyType keyType, System.Action<List<int>> callEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            var battlerInfo = _battleInfos.Find(a => a.Index == _selectIndex);
            if (battlerInfo == null)
            {
                return;
            }
            if (battlerInfo.IsAlive() == false)
            {
                return;
            }
            if (_targetIndexList.IndexOf(_selectIndex) == -1)
            {
                return;
            }
            callEvent(MakeTargetIndexes(battlerInfo));
        }
        if (keyType == InputKeyType.Right)
        {
            var current = _battleInfos.Find(a => a.Index == _selectIndex);
            if (current != null)
            {
                var target = _battleInfos.Find(a => a.Index > current.Index && a.IsAlive());
                if (target != null)
                {
                    if (current.LineIndex == target.LineIndex)
                    {
                        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                        UpdateBattlerIndex(target.Index);
                    }
                }
            }
        }
        if (keyType == InputKeyType.Left)
        {
            var current = _battleInfos.Find(a => a.Index == _selectIndex);
            if (current != null)
            {
                var targets = _battleInfos.FindAll(a => a.Index < current.Index && a.IsAlive() && current.LineIndex == a.LineIndex);
                if (targets.Count > 0)
                {
                    Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                    UpdateBattlerIndex(targets[targets.Count-1].Index);
                }
            }
        }
        if (keyType == InputKeyType.Up)
        {
            var current = _battleInfos.Find(a => a.Index == _selectIndex);
            if (current != null)
            {
                var target = _battleInfos.Find(a => a.Index > current.Index && a.IsAlive() && a.LineIndex == LineType.Back);
                if (target != null)
                {
                    if (current != target)
                    {
                        UpdateBattlerIndex(target.Index);
                    }
                }
            }
        }
        if (keyType == InputKeyType.Down)
        {
            var current = _battleInfos.Find(a => a.Index == _selectIndex);
            if (current != null)
            {
                var target = _battleInfos.Find(a => a.Index < current.Index && a.IsAlive() && a.LineIndex == LineType.Front);
                if (target != null)
                {
                    if (current != target)
                    {
                        UpdateBattlerIndex(target.Index);
                    }
                }
            }
        }
    }

    private List<int> MakeTargetIndexes(BattlerInfo battlerInfo)
    {
        var indexList = new List<int>();
        if (_targetScopeType == ScopeType.All || _targetScopeType == ScopeType.WithoutSelfAll)
        {
            for (int i = 0; i < _battleBattler.Count;i++)
            {
                if (_battleInfos[i].IsAlive())
                {
                    indexList.Add(_battleInfos[i].Index);
                }
            }
        } else
        if (_targetScopeType == ScopeType.Line || _targetScopeType == ScopeType.FrontLine)
        {
            for (int i = 0; i < _battleBattler.Count;i++)
            {
                if (battlerInfo.LineIndex == _battleInfos[i].LineIndex)
                {
                    if (_battleInfos[i].IsAlive())
                    {
                        indexList.Add(_battleInfos[i].Index);
                    }
                }
            }
        } else
        if (_targetScopeType == ScopeType.One || _targetScopeType == ScopeType.WithoutSelfOne)
        {
            if (battlerInfo.IsAlive())
            {
                indexList.Add(battlerInfo.Index);
            }
        } else
        if (_targetScopeType == ScopeType.Self)
        {
            if (battlerInfo.IsAlive())
            {
                indexList.Add(battlerInfo.Index);
            }
        }
        return indexList;
    }

    public void ClearSelect()
    {
        UpdateAllUnSelect();
        UpdateSelectIndex(-1);
        _targetScopeType = ScopeType.None;
        _targetIndexList = new ();
    }
}
