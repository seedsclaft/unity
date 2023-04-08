using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SkillActionList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<SkillInfo> _skillInfos = new List<SkillInfo>();

    public void Initialize(System.Action<int> callEvent,System.Action cancelEvent,System.Action conditionEvent)
    {
        InitializeListView(rows);
        // スクロールするものはObjectList.CountでSetSelectHandlerを登録する
        for (int i = 0; i < ObjectList.Count;i++)
        {
            SkillAction skillAction = ObjectList[i].GetComponent<SkillAction>();
            skillAction.SetCallHandler((d) => {
                SkillInfo skillInfo = _skillInfos.Find(a => a.Id == d);
                if (skillInfo.Enable == false)
                {
                    return;
                }
                callEvent(d);
            });
            skillAction.SetSelectHandler((data) => UpdateSelectIndex(data));
            //ObjectList[i].SetActive(false);
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent,conditionEvent));
    }

    
    public void SetSkillInfos(List<SkillInfo> skillInfoData)
    {
        _skillInfos.Clear();
        _skillInfos = skillInfoData;
        SetDataCount(skillInfoData.Count);
    }

    public void Refresh()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (i < _skillInfos.Count) 
            {
                SkillAction skillAction = ObjectList[i].GetComponent<SkillAction>();
                skillAction.SetData(_skillInfos[i],i);
            }
            ObjectList[i].SetActive(i < _skillInfos.Count);
        }
        ResetScrollPosition();
        UpdateSelectIndex(0);
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<int> callEvent,System.Action cancelEvent,System.Action conditionEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            SkillInfo skillInfo = _skillInfos.Find(a => a.Id == _skillInfos[Index].Id);
            if (skillInfo.Enable == false)
            {
                return;
            }
            callEvent(_skillInfos[Index].Id);
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
        if (keyType == InputKeyType.Option1)
        {
            if (conditionEvent != null)
            {
                conditionEvent();
            }
        }
    }

    public override void RefreshListItem(GameObject gameObject, int itemIndex)
    {
        base.RefreshListItem(gameObject,itemIndex);
        var skillAction = gameObject.GetComponent<SkillAction>();
        skillAction.SetData(_skillInfos[itemIndex],itemIndex);
        skillAction.UpdateViewItem();
    }
}
