using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillList : MonoBehaviour
{
    [SerializeField] private GameObject skillListPrefab;
    [SerializeField] private GameObject skillListRoot;
    public BaseList skillActionList;
    public BaseList attributeList;

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
        GameObject prefab = Instantiate(skillListPrefab);
        prefab.transform.SetParent(skillListRoot.transform, false);
        var lists = prefab.GetComponentsInChildren<BaseList>();
        skillActionList = lists[0];
        attributeList = lists[1];
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
    }

    public void SetInputHandlerAction(InputKeyType keyType,System.Action callEvent)
    {
        skillActionList.SetInputHandler(keyType,callEvent);
    }

    public void RefreshAction(int selectIndex = 0)
    {
        skillActionList.Refresh(selectIndex);
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

    public void ActivateActionList()
    {
        skillActionList.Activate();
    }

    public void DeactivateActionList()
    {
        skillActionList.Deactivate();
    }

    public void ShowActionList()
    {
        skillActionList.gameObject.SetActive(true);
    }

    public void HideActionList()
    {
        skillActionList.gameObject.SetActive(false);
    }

    public void ActivateAttributeList()
    {
        attributeList.Activate();
    }

    public void DeactivateAttributeList()
    {
        attributeList.Deactivate();
    }
    
    public void ShowAttributeList()
    {
        attributeList.gameObject.SetActive(true);
    }

    public void HideAttributeList()
    {
        attributeList.gameObject.SetActive(false);
    }
}
