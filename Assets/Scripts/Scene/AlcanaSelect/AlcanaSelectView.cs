using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlcanaSelect;

public class AlcanaSelectView : BaseView
{
    [SerializeField] private BattleSelectCharacter selectCharacter = null;
    [SerializeField] private List<SkillInfoComponent> skillInfoComponents = null;
    
    private new System.Action<AlcanaSelectViewEvent> _commandData = null;
    public override void Initialize() 
    {
        base.Initialize();
        selectCharacter.Initialize();
        SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
        InitializeSelectCharacter();
        
        SetBackCommand(() => OnClickBack());
        new AlcanaSelectPresenter(this);
    }
    
    private void InitializeSelectCharacter()
    {
        selectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallChangeAlcana());
        selectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
        
        SetInputHandler(selectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
        selectCharacter.HideActionList();
        selectCharacter.HideStatus();
    }

    public void SetInitHelpText()
    {
        HelpWindow.SetHelpText(DataSystem.System.GetTextData(20020).Text);
        //HelpWindow.SetInputInfo("MAINMENU");
    }

    public void SetEvent(System.Action<AlcanaSelectViewEvent> commandData)
    {
        _commandData = commandData;
    }


    private void CallChangeAlcana()
    {
        var listData = selectCharacter.ActionData;
        if (listData != null)
        {
            var eventData = new AlcanaSelectViewEvent(CommandType.ChangeAlcana);
            eventData.template = listData;
            _commandData(eventData);
        }
    }

    private void OnClickBack()
    {
        var eventData = new AlcanaSelectViewEvent(CommandType.Back);
        _commandData(eventData);
    }

    public void CommandRefresh(List<SkillInfo> selectAlcanaInfo)
    {
        for (var i = 0;i < skillInfoComponents.Count;i++)
        {
            skillInfoComponents[i].gameObject.SetActive(selectAlcanaInfo.Count > i);
            if (selectAlcanaInfo.Count > i)
            {
                skillInfoComponents[i].UpdateSkillInfo(selectAlcanaInfo[i]);
            }
        }
    }

    public void RefreshMagicList(List<ListData> skillInfos)
    {
        selectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
        selectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
        selectCharacter.ShowActionList();
        selectCharacter.SetSkillInfos(skillInfos);
        selectCharacter.RefreshAction();
    }
}

namespace AlcanaSelect
{
    public enum CommandType
    {
        None = 0,
        ChangeAlcana,
        DeleteAlcana,
        Back
    }
}

public class AlcanaSelectViewEvent
{
    public AlcanaSelect.CommandType commandType;
    public object template;

    public AlcanaSelectViewEvent(AlcanaSelect.CommandType type)
    {
        commandType = type;
    }
}