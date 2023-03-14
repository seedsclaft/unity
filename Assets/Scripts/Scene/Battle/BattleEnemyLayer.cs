using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effekseer;

public class BattleEnemyLayer : ListWindow , IInputHandlerEvent
{
    [SerializeField] private List<GameObject> frontEnemyRoots;
    [SerializeField] private List<GameObject> backEnemyRoots;
    [SerializeField] private GameObject battleEnemyPrefab;
    [SerializeField] private List<GameObject> frontDamageRoots;
    [SerializeField] private List<GameObject> backDamageRoots;
    private List<BattleEnemy> _battleEnemies = new List<BattleEnemy>();
    private ScopeType _targetScopeType = ScopeType.None;
    private List<int> _targetIndexList = new List<int>();
    private int _backStartIndex = 0;
    private List<BattlerInfo> _battleInfos = new List<BattlerInfo>();
    private int _selectIndex = -1;

    public void Initialize(List<BattlerInfo> battlerInfos ,System.Action<List<int>> callEvent,System.Action cancelEvent)
    {
        _battleInfos = battlerInfos;
        frontDamageRoots.ForEach(a => a.SetActive(false));
        backDamageRoots.ForEach(a => a.SetActive(false));
        frontEnemyRoots.ForEach(a => a.SetActive(false));
        backEnemyRoots.ForEach(a => a.SetActive(false));

        int frontIndex = 0;
        int backIndex = 0;
        for (int i = 0; i < battlerInfos.Count;i++)
        {
            GameObject prefab = Instantiate(battleEnemyPrefab);
            BattleEnemy battleEnemy = prefab.GetComponent<BattleEnemy>();
            if (battlerInfos[i].LineIndex == 0)
            {
                frontEnemyRoots[i].SetActive(true);
                prefab.transform.SetParent(frontEnemyRoots[i].transform, false);
                battleEnemy.SetDamageRoot(frontDamageRoots[i]);
                _backStartIndex = battlerInfos[i].Index + 1;
            } else
            {
                backEnemyRoots[backIndex].SetActive(true);
                prefab.transform.SetParent(backEnemyRoots[backIndex].transform, false);
                battleEnemy.SetDamageRoot(backDamageRoots[backIndex]);
                backIndex++;
            }
            battleEnemy.SetData(battlerInfos[i],i);
            battleEnemy.SetCallHandler((enemyIndex) => {
                BattlerInfo battlerInfo = battlerInfos.Find(a => a.Index == enemyIndex);
                if (battlerInfo.IsAlive() == false)
                {
                    return;
                }
                if (_targetIndexList.IndexOf(enemyIndex) == -1)
                {
                    return;
                }
                callEvent(MakeTargetIndexs(battlerInfo));
            });
            battleEnemy.SetSelectHandler((data) => UpdateEnemyIndex(data));
            _battleEnemies.Add(battleEnemy);
            frontIndex++;
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent));
        UpdateAllUnSelect();
    }

    public void RefreshTarget(ActionInfo actionInfo)
    {
        UpdateAllUnSelect();
        if (actionInfo == null) {
            _targetScopeType = ScopeType.None;
            return;
        }
        _selectIndex = actionInfo.LastTargetIndex;
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
            UpdateEnemyIndex(actionInfo.LastTargetIndex);
        } else
        if (_targetScopeType == ScopeType.Self)
        {
            UpdateEnemyIndex(actionInfo.SubjectIndex);
        }
    }

    private void UpdateAllSelect(){
        for (int i = 0; i < _battleEnemies.Count;i++)
        {
            if (_battleInfos[i].IsAlive())
            {
                var listItem = _battleEnemies[i].GetComponent<ListItem>();
                listItem.SetSelect();
                var emitter = listItem.GetComponentInChildren<EffekseerEmitter>();
                emitter.Play();
                _battleEnemies[i].BattlerInfoComponent.SetSelectable(true);
            }
        }
    }

    private void UpdateAllUnSelect(){
        for (int i = 0; i < _battleEnemies.Count;i++)
        {
            var listItem = _battleEnemies[i].GetComponent<ListItem>();
            listItem.SetUnSelect();
        }
    }
    
    private void UpdateEnemyIndex(int index){
        if (_targetIndexList.IndexOf(index) == -1)
        {
            return;
        }
        _selectIndex = index;
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
        if (_targetScopeType != ScopeType.One)
        {
            return;
        }
        for (int i = 0; i < _battleEnemies.Count;i++)
        {
            var listItem = _battleEnemies[i].GetComponent<ListItem>();
            if (index == _battleEnemies[i].EnemyIndex){
                if (_battleInfos[i].IsAlive())
                {
                    listItem.SetSelect();
                    var emitter = listItem.GetComponentInChildren<EffekseerEmitter>();
                    emitter.Play();
                }
            } else{
                listItem.SetUnSelect();
            }
            _battleEnemies[i].BattlerInfoComponent.SetSelectable((index == _battleEnemies[i].EnemyIndex));
        }
    }

    private void UpdateLineSelect(int index){
        if (_targetScopeType != ScopeType.Line){
            return;
        }
        bool isFront = index < _backStartIndex;
        for (int i = 0; i < _battleEnemies.Count;i++)
        {
            var listItem = _battleEnemies[i].GetComponent<ListItem>();
            if (isFront)
            {
                if (_battleEnemies[i].EnemyIndex < _backStartIndex){
                    if (_battleInfos[i].IsAlive())
                    {
                        listItem.SetSelect();
                        var emitter = listItem.GetComponentInChildren<EffekseerEmitter>();
                        emitter.Play();
                    }
                } else{
                    listItem.SetUnSelect();
                }
                _battleEnemies[i].BattlerInfoComponent.SetSelectable(_battleEnemies[i].EnemyIndex < _backStartIndex);
            } else
            {
                if (_battleEnemies[i].EnemyIndex >= _backStartIndex){
                    if (_battleInfos[i].IsAlive())
                    {
                        listItem.SetSelect();
                        var emitter = listItem.GetComponentInChildren<EffekseerEmitter>();
                        emitter.Play();
                    }
                } else{
                    listItem.SetUnSelect();
                }
                _battleEnemies[i].BattlerInfoComponent.SetSelectable(_battleEnemies[i].EnemyIndex >= _backStartIndex);
            }
        }
    }

    public BattlerInfoComponent GetBattlerInfoComp(int index)
    {
        return _battleEnemies[index - 100].BattlerInfoComponent;
    }
    
    private void CallInputHandler(InputKeyType keyType, System.Action<List<int>> callEvent,System.Action cancelEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            BattlerInfo battlerInfo = _battleInfos.Find(a => a.Index == _selectIndex);
            if (battlerInfo.IsAlive() == false)
            {
                return;
            }
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
        if (keyType == InputKeyType.Right)
        {
            BattlerInfo current = _battleInfos.Find(a => a.Index == _selectIndex);
            if (current != null)
            {
                BattlerInfo target = _battleInfos.Find(a => a.Index > current.Index && a.IsAlive());
                if (target != null)
                {
                    if (current.LineIndex == target.LineIndex)
                    {
                        SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                        UpdateEnemyIndex(target.Index);
                    }
                }
            }
        }
        if (keyType == InputKeyType.Left)
        {
            BattlerInfo current = _battleInfos.Find(a => a.Index == _selectIndex);
            if (current != null)
            {
                BattlerInfo target = _battleInfos.Find(a => a.Index < current.Index && a.IsAlive());
                if (target != null)
                {
                    if (current.LineIndex == target.LineIndex)
                    {
                        SoundManager.Instance.PlayStaticSe(SEType.Cursor);
                        UpdateEnemyIndex(target.Index);
                    }
                }
            }
        }
        if (keyType == InputKeyType.Up)
        {
            BattlerInfo current = _battleInfos.Find(a => a.Index == _selectIndex);
            if (current != null)
            {
                BattlerInfo target = _battleInfos.Find(a => a.Index > current.Index && a.IsAlive() && a.LineIndex == 1);
                if (target != null)
                {
                    if (current != target)
                    {
                        UpdateEnemyIndex(target.Index);
                    }
                }
            }
        }
        if (keyType == InputKeyType.Down)
        {
            BattlerInfo current = _battleInfos.Find(a => a.Index == _selectIndex);
            if (current != null)
            {
                BattlerInfo target = _battleInfos.Find(a => a.Index < current.Index && a.IsAlive() && a.LineIndex == 0);
                if (target != null)
                {
                    if (current != target)
                    {
                        UpdateEnemyIndex(target.Index);
                    }
                }
            }
        }
    }

    private List<int> MakeTargetIndexs(BattlerInfo battlerInfo)
    {
        List<int> indexList = new List<int>();
        if (_targetScopeType == ScopeType.All)
        {
            for (int i = 0; i < _battleEnemies.Count;i++)
            {
                if (_battleInfos[i].IsAlive())
                {
                    indexList.Add(_battleInfos[i].Index);
                }
            }
        } else
        if (_targetScopeType == ScopeType.Line)
        {
            for (int i = 0; i < _battleEnemies.Count;i++)
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
        if (_targetScopeType == ScopeType.One)
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
}
