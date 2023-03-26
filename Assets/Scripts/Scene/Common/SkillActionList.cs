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


    public void Initialize(System.Action<int> callEvent,System.Action cancelEvent,System.Action conditionEvent)
    {
        InitializeListView(rows);
        for (int i = 0; i < rows + 1;i++)
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
        SetInputHandler((a) => CallInputHandler(a,callEvent,cancelEvent,conditionEvent));
    }

    public void Refresh(List<SkillInfo> skillInfoData)
    {
        _data.Clear();
        _data = skillInfoData;
        SetDataCount(skillInfoData.Count);
        for (int i = 0; i < ObjectList.Count;i++)
        {
            ObjectList[i].SetActive(false);
            if (i < _data.Count) 
            {
                var skillAction = ObjectList[i].GetComponent<SkillAction>();
                skillAction.SetData(skillInfoData[i],i);
                ObjectList[i].SetActive(true);
            }
        }
        ResetScrollPosition();
        UpdateSelectIndex(0);
        UpdateAllItems();
    }

    public override void UpdateHelpWindow(){
        if (_helpWindow != null)
        {
            //_helpWindow.SetHelpText(_data[Index].Help);
        }
    }

    private void CallInputHandler(InputKeyType keyType, System.Action<int> callEvent,System.Action cancelEvent,System.Action conditionEvent)
    {
        if (keyType == InputKeyType.Decide)
        {
            SkillInfo skillInfo = _data.Find(a => a.Id == _data[Index].Id);
            if (skillInfo.Enabel == false)
            {
                return;
            }
            callEvent(_data[Index].Id);
        }
        if (keyType == InputKeyType.Cancel)
        {
            cancelEvent();
        }
        if (keyType == InputKeyType.Option1)
        {
            if (conditionEvent != null)
            {
                conditionEvent();
            }
        }
    }

    public override void RefreshListItem(GameObject gameObject, int itemIndex)
    {
        base.RefreshListItem(gameObject,itemIndex);
        var skillAction = gameObject.GetComponent<SkillAction>();
        skillAction.SetData(_data[itemIndex],itemIndex);
        skillAction.UpdateViewItem();
    }
}
