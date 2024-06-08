using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AlcanaSelect;

namespace Ryneus
{
    public class AlcanaSelectView : BaseView
    {
        [SerializeField] private BattleSelectCharacter selectCharacter = null;
        [SerializeField] private List<SkillInfoComponent> skillInfoComponents = null;
        [SerializeField] private Button deleteButton = null;
        [SerializeField] private BattleStartAnim battleStartAnim = null;
        private bool _animationBusy = false;
        private new System.Action<AlcanaSelectViewEvent> _commandData = null;
        public override void Initialize() 
        {
            base.Initialize();
            selectCharacter.Initialize();
            SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
            InitializeSelectCharacter();
            
            SetBackCommand(() => OnClickBack());
            deleteButton.onClick.AddListener(() => CallDeleteAlcana());
            new AlcanaSelectPresenter(this);
            ChangeUIActive(false);
        }
        
        public void StartAnimation()
        {
            battleStartAnim.SetText(DataSystem.GetText(19010));
            battleStartAnim.StartAnim();
            battleStartAnim.gameObject.SetActive(true);
            _animationBusy = true;
        }

        private void InitializeSelectCharacter()
        {
            selectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallChangeAlcana());
            selectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
            selectCharacter.SetInputHandlerAction(InputKeyType.Option1,() => CallDeleteAlcana());
            
            SetInputHandler(selectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
            selectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
            selectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            selectCharacter.HideActionList();
            selectCharacter.HideStatus();
        }

        public int SkillListIndex => selectCharacter.MagicList.Index;

        public void SetInitHelpText()
        {
            HelpWindow.SetHelpText(DataSystem.GetText(20020));
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

        private void CallDeleteAlcana()
        {
            var listData = selectCharacter.ActionData;
            if (listData != null)
            {
                var eventData = new AlcanaSelectViewEvent(CommandType.DeleteAlcana);
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
                    skillInfoComponents[i].UpdateInfo(selectAlcanaInfo[i]);
                }
            }
        }

        public void RefreshMagicList(List<ListData> skillInfos,int selectIndex = 0)
        {
            selectCharacter.ShowActionList();
            selectCharacter.SetSkillInfos(skillInfos);
            selectCharacter.RefreshAction(selectIndex);
        }

        private new void Update() {
            if (_animationBusy == true)
            {
                CheckAnimationBusy();
                return;
            }
            base.Update();
        }    
        
        private void CheckAnimationBusy()
        {
            if (battleStartAnim.IsBusy == false)
            {
                _animationBusy = false;
                var eventData = new AlcanaSelectViewEvent(CommandType.EndAnimation);
                _commandData(eventData);
                ChangeUIActive(true);
            }
        }
    }
}

namespace AlcanaSelect
{
    public enum CommandType
    {
        None = 0,
        EndAnimation,
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