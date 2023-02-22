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
    private bool _animationBusy = false;
    public bool AnimationBusy {
        get {return _animationBusy;}
    }
    private ScopeType _targetScopeType = ScopeType.None;
    private List<int> _targetIndexList = new List<int>();
    public int selectIndex{
        get {return Index;}
    }


    public void Initialize(System.Action<List<int>> callEvent)
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
                callEvent(indexList);
            });
            battleActor.SetSelectHandler((data) => UpdateSelectIndex(data));
            battleActor.SetDamageRoot(damageRoots[i]);
            ObjectList[i].SetActive(false);
        }
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

    public void StartAnimation(List<int> indexList, EffekseerEffectAsset effectAsset)
    {
        for (int i = 0; i < indexList.Count;i++)
        {
            BattleActor battleActor = ObjectList[indexList[i]].GetComponent<BattleActor>();
            battleActor.StartAnimation(effectAsset);
        }
        _animationBusy = true;
    }

    public void StartAnimation(int targetIndex, EffekseerEffectAsset effectAsset)
    {
        BattleActor battleActor = ObjectList[targetIndex].GetComponent<BattleActor>();
        battleActor.StartAnimation(effectAsset);
        _animationBusy = true;
    }

    public void StartSkillDamage(int targetIndex,int damageTiming, System.Action<int> callEvent)
    {
        BattleActor battleActor = ObjectList[targetIndex].GetComponent<BattleActor>();
        battleActor.SetStartSkillDamage(damageTiming,callEvent);
        _animationBusy = true;
    }

    public void ClearDamagePopup()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var battleActor = ObjectList[i].GetComponent<BattleActor>();
            battleActor.ClearDamagePopup();
        }
    }
    
    public void StartDamage(int targetIndex , DamageType damageType , int value)
    {        
        BattleActor battleActor = ObjectList[targetIndex].GetComponent<BattleActor>();
        battleActor.StartDamage(damageType,value);
    }

    public void StartHeal(int targetIndex , DamageType damageType , int value)
    {        
        BattleActor battleActor = ObjectList[targetIndex].GetComponent<BattleActor>();
        battleActor.StartHeal(damageType,value);
    }
    
    public void StartStatePopup(int targetIndex , DamageType damageType ,string stateName)
    {        
        BattleActor battleActor = ObjectList[targetIndex].GetComponent<BattleActor>();
        battleActor.StartStatePopup(damageType,stateName);
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
        }
    }

    private void UpdateLineSelect(int index){
    }
    
    public void RefreshStatus()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            BattleActor battleActor = ObjectList[i].GetComponent<BattleActor>();
            battleActor.RefreshStatus();
        }
    }
    
    private void Update() {
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
        for (int i = 0; i < ObjectList.Count;i++)
        {
            BattleActor battleActor = ObjectList[i].GetComponent<BattleActor>();
            if (battleActor.IsBusy)
            {
                isBusy = true;
                break;
            }
        }
        return isBusy;
    }
}
