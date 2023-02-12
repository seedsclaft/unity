using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnemyLayer : MonoBehaviour
{
    [SerializeField] private List<GameObject> frontEnemyRoots;
    [SerializeField] private List<GameObject> backEnemyRoots;
    [SerializeField] private GameObject battleEnemyPrefab;
    private List<BattleEnemy> battleEnemies = new List<BattleEnemy>();
    private ScopeType _targetScopeType = ScopeType.None;
    private int _backStartIndex = 0;

    public void Initialize(List<BattlerInfo> battlerInfos ,System.Action<List<int>> callEvent)
    {
        for (int i = 0; i < frontEnemyRoots.Count;i++)
        {
            frontEnemyRoots[i].SetActive(false);
        }

        for (int i = 0; i < battlerInfos.Count;i++)
        {
            GameObject prefab = Instantiate(battleEnemyPrefab);
            frontEnemyRoots[i].SetActive(true);
            prefab.transform.SetParent(frontEnemyRoots[i].transform, false);
            var battleEnemy = prefab.GetComponent<BattleEnemy>();
            battleEnemy.SetData(battlerInfos[i],i);
            battleEnemy.SetCallHandler((enemyIndex) => {
                List<int> indexList = new List<int>();
                if (_targetScopeType == ScopeType.All)
                {
                    for (int i = 0; i < battleEnemies.Count;i++)
                    {
                        indexList.Add(i);
                    }
                } else
                if (_targetScopeType == ScopeType.Line)
                {
                    for (int i = 0; i < battleEnemies.Count;i++)
                    {
                        if (enemyIndex < _backStartIndex)
                        {
                            if (i < _backStartIndex)
                            {
                                indexList.Add(i);
                            }
                        } else{
                            if (i >= _backStartIndex)
                            {
                                indexList.Add(i);
                            }
                        }
                    }
                } else
                if (_targetScopeType == ScopeType.One)
                {
                    indexList.Add(enemyIndex);
                } else
                if (_targetScopeType == ScopeType.Self)
                {
                    indexList.Add(enemyIndex);
                }
                callEvent(indexList);
            });
            battleEnemy.SetSelectHandler((data) => UpdateSelectIndex(data));
            battleEnemies.Add(battleEnemy);
            _backStartIndex++; 
        }
        UpdateAllUnSelect();
    }

    public void RefreshTarget(ActionInfo actionInfo)
    {
        UpdateAllUnSelect();
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
        for (int i = 0; i < battleEnemies.Count;i++)
        {
            var listItem = battleEnemies[i].GetComponent<ListItem>();
            listItem.SetSelect();
        }
    }

    private void UpdateAllUnSelect(){
        for (int i = 0; i < battleEnemies.Count;i++)
        {
            var listItem = battleEnemies[i].GetComponent<ListItem>();
            listItem.SetUnSelect();
        }
    }
    
    private void UpdateSelectIndex(int index){
        if (_targetScopeType == ScopeType.Line)
        {
            UpdateLineSelect(index);
            return;
        }
        if (_targetScopeType != ScopeType.One)
        {
            return;
        }
        for (int i = 0; i < battleEnemies.Count;i++)
        {
            var listItem = battleEnemies[i].GetComponent<ListItem>();
            if (index == i){
                listItem.SetSelect();
            } else{
                listItem.SetUnSelect();
            }
        }
    }

    private void UpdateLineSelect(int index){
        if (_targetScopeType != ScopeType.Line){
            return;
        }
        bool isFront = index < _backStartIndex;
        for (int i = 0; i < battleEnemies.Count;i++)
        {
            var listItem = battleEnemies[i].GetComponent<ListItem>();
            if (isFront)
            {
                if (i < _backStartIndex){
                    listItem.SetSelect();
                } else{
                    listItem.SetUnSelect();
                }
            } else
            {
                if (i >= _backStartIndex){
                    listItem.SetSelect();
                } else{
                    listItem.SetUnSelect();
                }
            }
        }
    }
}
