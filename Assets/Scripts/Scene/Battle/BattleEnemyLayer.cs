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
    private List<BattleEnemy> battleEnemies = new List<BattleEnemy>();
    private ScopeType _targetScopeType = ScopeType.None;
    private List<int> _targetIndexList = new List<int>();
    private int _backStartIndex = 0;
    private bool _animationBusy = false;
    public bool AnimationBusy {
        get {return _animationBusy;}
    }
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
                List<int> indexList = new List<int>();
                if (_targetScopeType == ScopeType.All)
                {
                    for (int i = 0; i < battleEnemies.Count;i++)
                    {
                        if (battlerInfos[i].IsAlive())
                        {
                            indexList.Add(battlerInfos[i].Index);
                        }
                    }
                } else
                if (_targetScopeType == ScopeType.Line)
                {
                    for (int i = 0; i < battleEnemies.Count;i++)
                    {
                        if (battlerInfo.LineIndex == battlerInfos[i].LineIndex)
                        {
                            if (battlerInfos[i].IsAlive())
                            {
                                indexList.Add(battlerInfos[i].Index);
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
                callEvent(indexList);
            });
            battleEnemy.SetSelectHandler((data) => UpdateEnemyIndex(data));
            battleEnemies.Add(battleEnemy);
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
        for (int i = 0; i < battleEnemies.Count;i++)
        {
            var listItem = battleEnemies[i].GetComponent<ListItem>();
            if (index == battleEnemies[i].EnemyIndex){
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
                if (battleEnemies[i].EnemyIndex < _backStartIndex){
                    listItem.SetSelect();
                } else{
                    listItem.SetUnSelect();
                }
            } else
            {
                if (battleEnemies[i].EnemyIndex >= _backStartIndex){
                    listItem.SetSelect();
                } else{
                    listItem.SetUnSelect();
                }
            }
        }
    }

    public void StartAnimation(List<int> indexList, EffekseerEffectAsset effectAsset,int animationPosition)
    {
        for (int i = 0; i < indexList.Count;i++)
        {
            battleEnemies[indexList[i]].StartAnimation(effectAsset,animationPosition);
        }
        _animationBusy = true;
    }

    public void StartAnimation(int targetIndex, EffekseerEffectAsset effectAsset,int animationPosition)
    {
        battleEnemies[targetIndex].StartAnimation(effectAsset,animationPosition);
        _animationBusy = true;
    }

    public void StartSkillDamage(int targetIndex,int damageTiming, System.Action<int> callEvent)
    {
        battleEnemies[targetIndex].SetStartSkillDamage(damageTiming,callEvent);
        _animationBusy = true;
    }

    public void ClearDamagePopup()
    {
        battleEnemies.ForEach(a => a.ClearDamagePopup());
    }

    public void StartDamage(int targetIndex,DamageType damageType,int value)
    {        
        battleEnemies[targetIndex].StartDamage(damageType,value);
    }

    public void StartBlink(int targetIndex)
    {
        battleEnemies[targetIndex].StartBlink();
    }

    public void StartHeal(int targetIndex,DamageType damageType,int value)
    {        
        battleEnemies[targetIndex].StartHeal(damageType,value);
    }

    public void StartStatePopup(int targetIndex , DamageType damageType ,string stateName)
    {        
        battleEnemies[targetIndex].StartStatePopup(damageType,stateName);
    }

    public void StartDeathAnimation(int targetIndex)
    {
        battleEnemies[targetIndex].StartDeathAnimation();
    }

    public void RefreshStatus()
    {
        for (int i = 0; i < battleEnemies.Count;i++)
        {
            battleEnemies[i].RefreshStatus();
        }
    }

    private new void Update() {
        base.Update();
        if (_animationBusy == true)
        {
            if (CheckAnimationBusy() == false)
            {
                _animationBusy = false;
            }
        }
    }

    private bool CheckAnimationBusy()
    {
        bool isBusy = false;
        for (int i = 0; i < battleEnemies.Count;i++)
        {
            if (battleEnemies[i].IsBusy)
            {
                isBusy = true;
                break;
            }
        }
        return isBusy;
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
            List<int> indexList = new List<int>();
            if (_targetScopeType == ScopeType.All)
            {
                for (int i = 0; i < battleEnemies.Count;i++)
                {
                    if (_battleInfos[i].IsAlive())
                    {
                        indexList.Add(_battleInfos[i].Index);
                    }
                }
            } else
            if (_targetScopeType == ScopeType.Line)
            {
                for (int i = 0; i < battleEnemies.Count;i++)
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
            callEvent(indexList);
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

}
