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


    public void Initialize(System.Action<int> callEvent)
    {
        InitializeListView(rows);
        for (int i = 0; i < rows;i++)
        {
            var skillAction = ObjectList[i].GetComponent<SkillAction>();
            skillAction.SetCallHandler((d) => {
                SkillInfo skillInfo = _data.Find(a => a.Id == d);
                if (skillInfo.Enabel == false)
                {
                    return;
                }
                callEvent(d);
            } );
            skillAction.SetSelectHandler((data) => UpdateSelectIndex(data));
            ObjectList[i].SetActive(false);
        }
    }

    public void Refresh(List<SkillInfo> skillInfoData)
    {
        _data.Clear();
        for (var i = 0; i < skillInfoData.Count;i++)
        {
            _data.Add(skillInfoData[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ObjectList[i].SetActive(false);
            if (i < _data.Count) 
            {
                var statusCommand = ObjectList[i].GetComponent<SkillAction>();
                statusCommand.SetData(skillInfoData[i],i);
                ObjectList[i].SetActive(true);
            }
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
