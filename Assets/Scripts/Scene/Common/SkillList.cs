using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillList : MonoBehaviour
{
    [SerializeField] private GameObject skillListPrefab;
    [SerializeField] private GameObject skillListRoot;
    public SkillActionList skillActionList;
    public SkillAttributeList skillAttributeList;

    public SkillInfo ActionData{
        get {
            return skillActionList.Data;
        }
    }
    public void Initialize()
    {
        GameObject prefab = Instantiate(skillListPrefab);
        prefab.transform.SetParent(skillListRoot.transform, false);
        skillActionList = prefab.GetComponentInChildren<SkillActionList>();
        skillAttributeList = prefab.GetComponentInChildren<SkillAttributeList>();
    }

    public void InitializeAttribute(int listCount,System.Action<AttributeType> callEvent,System.Action conditionEvent)
    {
        skillAttributeList.Initialize(listCount,callEvent,conditionEvent);
    }
    
    public void SetInputHandlerAttribute(InputKeyType keyType,System.Action callEvent)
    {
        skillAttributeList.SetInputHandler(keyType,callEvent);
    }

    public void InitializeAction()
    {
        skillActionList.Initialize();
    }

    public void SetSkillInfos(List<SkillInfo> skillInfoData)
    {
        skillActionList.SetSkillInfos(skillInfoData);
    }

    public void SetInputHandlerAction(InputKeyType keyType,System.Action callEvent)
    {
        skillActionList.SetInputHandler(keyType,callEvent);
    }

    public void RefreshAction(int selectIndex = 0)
    {
        skillActionList.Refresh(selectIndex);
    }

    public void RefreshAttribute(List<AttributeType> attributeTypes,AttributeType currentAttibuteType)
    {
        skillAttributeList.Refresh(attributeTypes,currentAttibuteType);
    }

    public void RefreshValues(List<string> attributeValues)
    {
        skillAttributeList.RefreshValues(attributeValues);
    }

    public void RefreshCostInfo()
    {
        skillActionList.RefreshCostInfo();
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
        skillAttributeList.Activate();
    }

    public void DeactivateAttributeList()
    {
        skillAttributeList.Deactivate();
    }
    
    public void ShowAttributeList()
    {
        skillAttributeList.gameObject.SetActive(true);
    }

    public void HideAttributeList()
    {
        skillAttributeList.gameObject.SetActive(false);
    }

    public int SelectedSkillId()
    {
        return skillActionList.SelectedSkillId();
    }
}
