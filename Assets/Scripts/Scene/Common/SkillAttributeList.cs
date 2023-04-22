using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SkillAttributeList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<AttributeType> _attributeTypesData = new List<AttributeType>();

    public void Initialize(System.Action<AttributeType> callEvent)
    {
        InitializeListView(cols);
        for (int i = 0; i < cols;i++)
        {
            var skillAttribute = ObjectList[i].GetComponent<SkillAttribute>();
            skillAttribute.SetCallHandler((a) =>
                {
                    callEvent(a);
                    UpdateSelectIndex((int)a-1);
                }
            );
            skillAttribute.SetSelectHandler((data) => 
            {
                //UpdateSelectIndex(data-1);
            });
        }
        SetInputHandler((a) => CallInputHandler(a,callEvent));
    }

    public void Refresh(List<AttributeType> attributeTypes)
    {
        _attributeTypesData = attributeTypes;
        for (int i = 0; i < attributeTypes.Count;i++)
        {
            var skillAttribute = ObjectList[i].GetComponent<SkillAttribute>();
            skillAttribute.SetData(attributeTypes[i],"",(int)attributeTypes[i] - 1);
        }
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public void RefreshValues(List<string> attributeValues)
    {
        for (int i = 0; i < _attributeTypesData.Count;i++)
        {
            var skillAttribute = ObjectList[i].GetComponent<SkillAttribute>();
            skillAttribute.SetData(_attributeTypesData[i],attributeValues[i],(int)_attributeTypesData[i] - 1);
        }
        UpdateAllItems();
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<AttributeType> callEvent)
    {
        if (keyType == InputKeyType.SideLeft1)
        {
            int index = Index - 1;
            if (index < 0) index = _attributeTypesData.Count-1;
            callEvent(_attributeTypesData[index]);
            UpdateSelectIndex(index);
        }
        if (keyType == InputKeyType.SideRight1)
        {
            int index = Index + 1;
            if (index > _attributeTypesData.Count-1) index = 0;
            callEvent(_attributeTypesData[index]);
            UpdateSelectIndex(index);
        }
    }

    private new void UpdateSelectIndex(int index){
        base.UpdateSelectIndex(index);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (ObjectList[i] == null) continue;
            SkillAttribute skillAttribute = ObjectList[i].GetComponent<SkillAttribute>();
            if (skillAttribute == null) continue;
            if (index == i){
                skillAttribute.SetSelect();
            } else{
                skillAttribute.SetUnSelect();
            }
        }
    }
}
