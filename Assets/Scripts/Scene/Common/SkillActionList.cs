using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class SkillActionList : ListWindow , IInputHandlerEvent
{
    private List<SkillInfo> _skillInfos = new List<SkillInfo>();

    public SkillInfo Data{
        get {
            if (Index < 0)
            {
                return null;
            }
            return _skillInfos[Index];
        }
    }
    public void Initialize()
    { 
        InitializeListView(0);
        // スクロールするものはObjectList.CountでSetSelectHandlerを登録する
        for (int i = 0; i < ObjectList.Count;i++)
        {
            SkillAction skillAction = ObjectList[i].GetComponent<SkillAction>();
            skillAction.SetCallHandler((d) => {
                CallListInputHandler(InputKeyType.Decide);
            });
            skillAction.SetSelectHandler((data) => UpdateSelectIndex(data));
            //ObjectList[i].SetActive(false);
        }
        SetInputCallHandler((a) => CallSelectHandler(a));
    }

    public void SetSkillInfos(List<SkillInfo> skillInfoData)
    {
        if (skillInfoData.Count > ObjectList.Count)
        {
            var objectListCount = ObjectList.Count;
            AddCreateList(skillInfoData.Count-ObjectList.Count);
            for (int i = 0; i < ObjectList.Count;i++)
            {
                if (i >= objectListCount)
                {
                    SkillAction skillAction = ObjectList[i].GetComponent<SkillAction>();
                    skillAction.SetCallHandler((d) => {
                        CallListInputHandler(InputKeyType.Decide);
                    });
                    skillAction.SetSelectHandler((data) => UpdateSelectIndex(data));
                }
            }
        }
        //SetInputCallHandler((a) => CallSelectHandler(a));
        _skillInfos = skillInfoData;
        SetDataCount(skillInfoData.Count);
        SetItemCount(skillInfoData.Count);
    }

    public async void Refresh(int selectIndex = 0)
    {
        UpdateSelectIndex(-1);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (i < _skillInfos.Count) 
            {
                SkillAction skillAction = ObjectList[i].GetComponent<SkillAction>();
                skillAction.SetData(_skillInfos[i],i);
            }
            ObjectList[i].SetActive(i < _skillInfos.Count);
        }
        UpdateAllItems();
        //ResetScrollPosition();
        
        await UniTask.DelayFrame(1);
        UpdateSelectIndex(selectIndex);
        if (selectIndex > 0)
        {
            UpdateScrollRect(selectIndex);
        } else
        {        
            ResetScrollRect();
        }
    }

    public void RefreshCostInfo()
    {
        UpdateAllItems();
    }

    public int SelectedSkillId()
    {
        int skillId = -1;
        if (Index >= 0 && _skillInfos.Count > Index && _skillInfos[Index] != null)
        {
            skillId = _skillInfos[Index].Id;
        }
        return skillId;
    }
}
