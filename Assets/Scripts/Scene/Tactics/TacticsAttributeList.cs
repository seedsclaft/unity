using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class TacticsAttributeList : ListWindow , IInputHandlerEvent
{
    private List<AttributeType> _attributeTypesData = new List<AttributeType>();

    public AttributeType Data{
        get {
            if (Index < 0)
            {
                return AttributeType.None;
            }
            return _attributeTypesData[Index];
        }
    }

    public void Initialize(List<AttributeType> attributes)
    {
        InitializeListView(attributes.Count);
        _attributeTypesData = attributes;
        for (int i = 0; i < attributes.Count;i++)
        {
            var skillAttribute = ObjectList[i].GetComponent<SkillAttribute>();
            skillAttribute.SetCallHandler((a) => CallListInputHandler(InputKeyType.Decide));
            skillAttribute.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputCallHandler((a) => CallSelectHandler(a));
        UpdateSelectIndex(-1);
    }

    public void Refresh(List<SkillData.SkillAttributeInfo> attributeInfos,int currensy)
    {
        for (int i = 0; i < _attributeTypesData.Count;i++)
        {
            SkillAttribute skillAttribute = ObjectList[i].GetComponent<SkillAttribute>();
            skillAttribute.SetData(attributeInfos[i],(int)_attributeTypesData[i] - 1);
            
            ListItem listItem = ObjectList[i].GetComponent<ListItem>();
            listItem.Disable.SetActive(attributeInfos[i].LearningCost > currensy);
        }
        UpdateAllItems();
    }

    public void SelectEnableIndex()
    {
        if (Index == -1 || ObjectList[Index].GetComponent<ListItem>().Disable.activeSelf)
        {
            for (int i = 0; i < _attributeTypesData.Count;i++)
            {
                var skillAttribute = ObjectList[i].GetComponent<ListItem>();
                if (!skillAttribute.Disable.activeSelf)
                {
                    UpdateSelectIndex(i);
                    break;
                }
            }
        }
    }
}
