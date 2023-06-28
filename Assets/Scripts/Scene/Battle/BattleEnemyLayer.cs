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
    [SerializeField] private List<GameObject> frontStatusRoots;
    [SerializeField] private List<GameObject> backStatusRoots;
    [SerializeField] private List<EffekseerEffectAsset> cursorEffects;
    
    private List<BattleEnemy> _battleEnemies = new List<BattleEnemy>();
    private ScopeType _targetScopeType = ScopeType.None;
    private List<int> _targetIndexList = new List<int>();
    private int _backStartIndex = 0;
    private List<BattlerInfo> _battleInfos = new List<BattlerInfo>();
    private int _selectIndex = -1;
    private AttributeType _attributeType = AttributeType.None;

    public void Initialize(List<BattlerInfo> battlerInfos ,System.Action<List<int>> callEvent,System.Action cancelEvent,System.Action selectPartyEvent)
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
            if (battlerInfos[i].LineIndex == LineType.Front)
            {
                frontEnemyRoots[i].SetActive(true);
                prefab.transform.SetParent(frontEnemyRoots[i].transform, false);
                battleEnemy.SetDamageRoot(frontDamageRoots[i]);
                battleEnemy.SetStatusRoot(frontStatusRoots[i]);
                _backStartIndex = battlerInfos[i].Index + 1;
            } else
            {
                backEnemyRoots[backIndex].SetActive(true);
                prefab.transform.SetParent(backEnemyRoots[backIndex].transform, false);
                battleEnemy.SetDamageRoot(backDamageRoots[backIndex]);
                battleEnemy.SetStatusRoot(backStatusRoots[backIndex]);
                backIndex++;
            }
            battleEnemy.SetData(battlerInfos[i],i,battlerInfos[i].LineIndex == LineType.Front);
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
                if (_selectIndex != enemyIndex)
                {
                    UpdateEnemyIndex(enemyIndex);
                    return;
                }
                callEvent(MakeTargetIndexs(battlerInfo));
            });
            battleEnemy.SetSelectHandler((data) => UpdateEnemyIndex(data));
            _battleEnemies.Add(battleEnemy);
            frontIndex++;
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent,selectPartyEvent));
        UpdateAllUnSelect();
    }

    public void RefreshTarget(int selectIndex,List<int> targetIndexList,ScopeType scopeType,AttributeType attributeType = AttributeType.None)
    {
        UpdateAllUnSelect();
        if (selectIndex == -1) {
            UpdateSelectIndex(-1);
            _targetScopeType = ScopeType.None;
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
        if (_targetScopeType == ScopeType.Line)
        {
            UpdateLineSelect(_selectIndex);
        } else
        if (_targetScopeType == ScopeType.One || _targetScopeType == ScopeType.WithoutSelfOne)
        {
            UpdateEnemyIndex(_selectIndex);
        } else
        if (_targetScopeType == ScopeType.Self)
        {
            UpdateEnemyIndex(_selectIndex);
        }
    }

    private void UpdateAllSelect(){
        for (int i = 0; i < _battleEnemies.Count;i++)
        {
            if (_battleInfos[i].IsAlive())
            {
                BattleEnemy listItem = _battleEnemies[i].GetComponent<BattleEnemy>();
                listItem.SetSelect(cursorEffects[(int)_attributeType]);
                _battleEnemies[i].EnableClick();
            }
        }
    }

    private void UpdateAllUnSelect(){
        for (int i = 0; i < _battleEnemies.Count;i++)
        {
            BattleEnemy listItem = _battleEnemies[i].GetComponent<BattleEnemy>();
            listItem.SetUnSelect();
            _battleEnemies[i].DisableClick();
        }
    }
    
    private void UpdateEnemyIndex(int index){
        if (_targetIndexList.IndexOf(index) == -1)
        {
            return;
        }
        _selectIndex = index;
        if (_targetScopeType == ScopeType.All || _targetScopeType == ScopeType.WithoutSelfAll)
        {
            UpdateAllSelect();
            return;
        }
        if (_targetScopeType == ScopeType.Line)
        {
            UpdateLineSelect(index);
            return;
        }
        if (_targetScopeType != ScopeType.One && _targetScopeType != ScopeType.WithoutSelfOne)
        {
            return;
        }
        for (int i = 0; i < _battleEnemies.Count;i++)
        {
            BattleEnemy listItem = _battleEnemies[i].GetComponent<BattleEnemy>();
            if (index == _battleEnemies[i].EnemyIndex){
                if (_battleInfos[i].IsAlive())
                {
                    listItem.SetSelect(cursorEffects[(int)_attributeType]);
                    _battleEnemies[i].EnableClick();
                }
            } else{
                listItem.SetUnSelect();
                _battleEnemies[i].DisableClick();
            }
        }
    }

    private void UpdateLineSelect(int index){
        if (_targetScopeType != ScopeType.Line){
            return;
        }
        bool isFront = index < _backStartIndex;
        for (int i = 0; i < _battleEnemies.Count;i++)
        {
            BattleEnemy listItem = _battleEnemies[i].GetComponent<BattleEnemy>();
            if (isFront)
            {
                if (_battleEnemies[i].EnemyIndex < _backStartIndex){
                    if (_battleInfos[i].IsAlive())
                    {
                        listItem.SetSelect(cursorEffects[(int)_attributeType]);
                        _battleEnemies[i].EnableClick();
                    }
                } else{
                    listItem.SetUnSelect();
                    _battleEnemies[i].DisableClick();
                }
            } else
            {
                if (_battleEnemies[i].EnemyIndex >= _backStartIndex){
                    if (_battleInfos[i].IsAlive())
                    {
                        listItem.SetSelect(cursorEffects[(int)_attributeType]);
                        _battleEnemies[i].EnableClick();
                    }
                } else{
                    listItem.SetUnSelect();
                    _battleEnemies[i].DisableClick();
                }
            }
        }
    }

    public BattlerInfoComponent GetBattlerInfoComp(int index)
    {
        return _battleEnemies[index - 100].BattlerInfoComponent;
    }
    
    private void CallInputHandler(InputKeyType keyType, System.Action<List<int>> callEvent,System.Action cancelEvent,System.Action selectPartyEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            BattlerInfo battlerInfo = _battleInfos.Find(a => a.Index == _selectIndex);
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
                        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
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
                        Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
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
                BattlerInfo target = _battleInfos.Find(a => a.Index > current.Index && a.IsAlive() && a.LineIndex == LineType.Back);
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
                BattlerInfo target = _battleInfos.Find(a => a.Index < current.Index && a.IsAlive() && a.LineIndex == LineType.Front);
                if (target != null)
                {
                    if (current != target)
                    {
                        UpdateEnemyIndex(target.Index);
                    }
                }
            }
        }
        if (keyType == InputKeyType.SideRight1)
        {
            selectPartyEvent();
        }
    }

    private List<int> MakeTargetIndexs(BattlerInfo battlerInfo)
    {
        List<int> indexList = new List<int>();
        if (_targetScopeType == ScopeType.All || _targetScopeType == ScopeType.WithoutSelfAll)
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
}
