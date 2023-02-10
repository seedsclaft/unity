using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SkillAttributeList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<AttributeType> _data = new List<AttributeType>();

    public int selectIndex{
        get {return Index;}
    }

    public void Initialize(List<AttributeType> attributeTypes ,System.Action<AttributeType> callEvent)
    {
        InitializeListView(attributeTypes.Count);
        for (var i = 0; i < attributeTypes.Count;i++){
            _data.Add(attributeTypes[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var skillAttribute = ObjectList[i].GetComponent<SkillAttribute>();
            skillAttribute.SetData(attributeTypes[i],"",(int)attributeTypes[i]);
            skillAttribute.SetCallHandler(callEvent);
            //skillAttribute.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        UpdateAllItems();
        UpdateSelectIndex(0);
    }
}
