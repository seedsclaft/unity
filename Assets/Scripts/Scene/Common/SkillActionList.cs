using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class SkillActionList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;

    private List<SkillInfo> _skillInfos = new List<SkillInfo>();

    public void Initialize(System.Action<SkillInfo> callEvent,System.Action cancelEvent,System.Action<SkillInfo> learningEvent,System.Action escapeEvent,System.Action optionEvent)
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
                if (skillInfo.LearningState == LearningState.Notlearned || skillInfo.LearningState == LearningState.SelectLearn){
                    if (learningEvent != null)
                    {
                        learningEvent(skillInfo);
                    }
                } else{
                    if (callEvent != null)
                    {
                        callEvent(skillInfo);
                    }
                }
            });
            skillAction.SetSelectHandler((data) => UpdateSelectIndex(data));
            //ObjectList[i].SetActive(false);
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent,learningEvent,escapeEvent,optionEvent));
    }

    public void SetSkillInfos(List<SkillInfo> skillInfoData)
    {
        _skillInfos.Clear();
        _skillInfos = skillInfoData;
        SetDataCount(skillInfoData.Count);
    }

    public void Refresh(int selectIndex = 0)
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
        UpdateSelectIndex(selectIndex);
        UpdateAllItems();
    }

    public void RefreshCostInfo()
    {
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<SkillInfo> callEvent,System.Action cancelEvent,System.Action<SkillInfo> learningEvent,System.Action escapeEvent,System.Action optionEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            if (Index < 0)
            {
                return;
            }
            SkillInfo skillInfo = _skillInfos.Find(a => a.Id == _skillInfos[Index].Id);
            if (skillInfo == null)
            {
                return;
            }
            if (skillInfo.Enable == false)
            {
                return;
            }
            if (skillInfo.LearningState == LearningState.Notlearned || skillInfo.LearningState == LearningState.SelectLearn){
                if (learningEvent != null)
                {
                    learningEvent(skillInfo);
                }
            } else{
                if (callEvent != null)
                {
                    callEvent(skillInfo);
                }
            }
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
        if (keyType == InputKeyType.Option1)
        {
            if (optionEvent != null)
            {
                optionEvent();
            }
        }
        if (keyType == InputKeyType.Option2)
        {
            if (escapeEvent != null)
            {
                escapeEvent();
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
