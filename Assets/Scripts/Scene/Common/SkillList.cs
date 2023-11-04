using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillList : MonoBehaviour
{
    [SerializeField] private SkillInfoComponent displaySelectCrad;
    [SerializeField] public BaseList skillActionList;
    [SerializeField] public BaseList attributeList;

    public SkillInfo ActionData{
        get {
            return (SkillInfo)skillActionList.ListData.Data;
        }
    }
    public SkillData.SkillAttributeInfo AttributeInfo{
        get {
            return (SkillData.SkillAttributeInfo)attributeList.ListData.Data;
        }
    }

    public void Initialize()
    {
    }

    public void InitializeAttribute(int listCount,System.Action callEvent,System.Action conditionEvent)
    {
        attributeList.Initialize(listCount);
        attributeList.SetInputHandler(InputKeyType.Decide,() => callEvent());
        attributeList.SetInputHandler(InputKeyType.SideLeft1,() => 
        {
            var index = attributeList.Index - 1;
            if (index < 0)
            {
                index = attributeList.ObjectList.Count-1;
            }
            attributeList.UpdateSelectIndex(index);
            callEvent();
        });
        attributeList.SetInputHandler(InputKeyType.SideRight1,() => 
        {
            var index = attributeList.Index + 1;
            if (index > attributeList.ObjectList.Count-1)
            {
                index = 0;
            }
            attributeList.UpdateSelectIndex(index);
            callEvent();
        });
        //skillAttributeList.Initialize(listCount,callEvent,conditionEvent);
    }

    public void InitializeAction()
    {
    }

    public void SetSkillInfos(List<ListData> skillInfoData)
    {
        skillActionList.Initialize(skillInfoData.Count);
        skillActionList.SetData(skillInfoData);
        skillActionList.SetSelectedHandler(() => DisplaySelectCard());
        if (displaySelectCrad == null)
        {
            displaySelectCrad.gameObject.SetActive(false);
        }
    }

    private void DisplaySelectCard()
    {
        if (displaySelectCrad == null)
        {
            return;
        }
        var listData = skillActionList.ListData;
        if (listData != null)
        {
            var skillInfo = (SkillInfo)listData.Data;
            if (skillInfo != null)
            {
                displaySelectCrad.gameObject.SetActive(true);
                displaySelectCrad.UpdateSkillData(skillInfo.Id);
            }
        }
    }

    public void SetInputHandlerAction(InputKeyType keyType,System.Action callEvent)
    {
        skillActionList.SetInputHandler(keyType,callEvent);
    }

    public void RefreshAction(int selectIndex = 0)
    {
        skillActionList.Refresh(selectIndex);
        skillActionList.Activate();
    }

    public void RefreshAttribute(List<ListData> listData)
    {
        attributeList.SetData(listData);
        attributeList.UpdateSelectIndex(listData.FindIndex(a => a.Selected));
    }


    public void RefreshCostInfo()
    {
        skillActionList.UpdateAllItems();
    }

    public void ShowActionList()
    {
        skillActionList.gameObject.SetActive(true);
        skillActionList.Activate();
    }

    public void HideActionList()
    {
        skillActionList.gameObject.SetActive(false);
        skillActionList.Deactivate();
    }
    
    public void ShowAttributeList()
    {
        //attributeList.gameObject.SetActive(true);
        attributeList.Activate();
    }

    public void HideAttributeList()
    {
        attributeList.gameObject.SetActive(false);
        attributeList.Deactivate();
    }
}
