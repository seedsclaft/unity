using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillActionList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;

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
        InitializeListView(rows);
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
        //_skillInfos.Clear();
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
        //ResetScrollPosition();
        ResetScrollRect();
        for (int i = 0;i < selectIndex;i++)
        {
            UpdateScrollRect(InputKeyType.Down);
        }
        UpdateSelectIndex(selectIndex);
        UpdateAllItems();
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
