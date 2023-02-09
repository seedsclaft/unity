using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class SkillActionList : ListWindow , IInputHandlerEvent
{
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<SkillInfo> _data = new List<SkillInfo>();

    public int selectIndex{
        get {return Index;}
    }

    public void Initialize(List<SkillInfo> skillInfoData ,System.Action<int> callEvent)
    {
        InitializeListView(skillInfoData.Count);
        for (var i = 0; i < skillInfoData.Count;i++){
            _data.Add(skillInfoData[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var statusCommand = ObjectList[i].GetComponent<SkillAction>();
            statusCommand.SetData(skillInfoData[i],i);
            statusCommand.SetCallHandler(callEvent);
            statusCommand.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        UpdateAllItems();
        UpdateSelectIndex(0);
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }
}
