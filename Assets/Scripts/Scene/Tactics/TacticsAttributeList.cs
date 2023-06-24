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

    public void Initialize(List<AttributeType> attributes,System.Action<AttributeType> callEvent,System.Action cancelEvent)
    {
        InitializeListView(attributes.Count);
        _attributeTypesData = attributes;
        for (int i = 0; i < attributes.Count;i++)
        {
            var skillAttribute = ObjectList[i].GetComponent<SkillAttribute>();
            skillAttribute.SetCallHandler(callEvent);
            skillAttribute.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent));
        UpdateSelectIndex(-1);
    }

    public void Refresh(List<string> attributeValues,List<int> learningCosts,int currensy)
    {
        for (int i = 0; i < _attributeTypesData.Count;i++)
        {
            SkillAttribute skillAttribute = ObjectList[i].GetComponent<SkillAttribute>();
            skillAttribute.SetData(_attributeTypesData[i],attributeValues[i],(int)_attributeTypesData[i] - 1,learningCosts[i]);
            
            ListItem listItem = ObjectList[i].GetComponent<ListItem>();
            listItem.Disable.SetActive(learningCosts[i] > currensy);
        }
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<AttributeType> callEvent,System.Action cancelEvent)
    {
        if (keyType == InputKeyType.Decide && Index > -1)
        {
            callEvent(_attributeTypesData[Index]);
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
    }
}
