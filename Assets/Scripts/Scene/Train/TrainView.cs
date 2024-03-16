using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Train;

namespace Ryneus
{
    public class TrainView : BaseView
    {

        [SerializeField] private BattleSelectCharacter battleSelectCharacter = null;
        [SerializeField] private TacticsSelectCharacter selectCharacter = null;
        private new System.Action<TrainViewEvent> _commandData = null;


        [SerializeField] private Button commandHelpButton = null;

        public SkillInfo SelectMagic => battleSelectCharacter.ActionData;

        public void Initialize(System.Action<ViewEvent> parentEvent)
        {
            base.Initialize();
            SetEvent(parentEvent);

            battleSelectCharacter.Initialize();
            SetInputHandler(battleSelectCharacter.GetComponent<IInputHandlerEvent>());
            InitializeSelectCharacter();

            battleSelectCharacter.gameObject.SetActive(false);
            commandHelpButton.onClick.AddListener(() => {
                var eventData = new TrainViewEvent(CommandType.CommandHelp);
                _commandData(eventData);
            });
            SetBackCommand(() => OnClickBack());
            new TrainPresenter(this);
            selectCharacter.gameObject.SetActive(false);
        }

        public void CallTrainCommand(TacticsCommandType tacticsCommandType)
        {
            var eventData = new TrainViewEvent(CommandType.TacticsCommand);
            eventData.template = tacticsCommandType;
            _commandData(eventData);
        }


        public void RefreshListData(ListData listData)
        {
        }

        public void StartAnimation()
        {
        }
        
        private void InitializeSelectCharacter()
        {
            battleSelectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallSkillAlchemy());
            battleSelectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
            SetInputHandler(battleSelectCharacter.MagicList.GetComponent<IInputHandlerEvent>());
            battleSelectCharacter.HideActionList();
        }

        private void OnClickBack()
        {
            var eventData = new TrainViewEvent(CommandType.Back);
            _commandData(eventData);
        }

        public void SetHelpWindow()
        {
        }

        public void SetEvent(System.Action<TrainViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetSelectCharacter(List<ListData> tacticsActorInfo,List<ListData> confirmCommands)
        {
            selectCharacter.Initialize();
            //selectCharacter.SetCharacterData(actorInfos);
            SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
            selectCharacter.SetTacticsCommand(confirmCommands);
            
            selectCharacter.SetTacticsCharacter(tacticsActorInfo);
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Decide,() => CallActorTrain());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Right,() => CallFrontBattleIndex());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Left,() => CallBattleBackIndex());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Cancel,() => CallTrainCommandCancel());
            selectCharacter.SetInputHandlerCommand(InputKeyType.Decide,() => CallTrainCommand());
            selectCharacter.SetInputHandlerCommand(InputKeyType.Cancel,() => CallTrainCommandCancel());
            SetInputHandler(selectCharacter.CharacterList.GetComponent<IInputHandlerEvent>());
            SetInputHandler(selectCharacter.CommandList.GetComponent<IInputHandlerEvent>());
            selectCharacter.CharacterList.SetSelectedHandler(() => {
                var listData = selectCharacter.CharacterData;
                if (listData != null)
                {
                    var data = (TacticsActorInfo)listData.Data;
                    var eventData = new TrainViewEvent(CommandType.ChangeSelectTacticsActor);
                    eventData.template = data.ActorInfo.ActorId;
                    _commandData(eventData);
                }
            });
            HideSelectCharacter();

        }

        public void CommandRefresh()
        {
            selectCharacter.Refresh();
        }


        public void SetEvaluate(int partyEvaluate,int troopEvaluate)
        {
            selectCharacter.SetEvaluate(partyEvaluate,troopEvaluate);
        }

        private void CallActorTrain()
        {
            var listData = selectCharacter.CharacterData;
            if (listData != null)
            {
                var data = (TacticsActorInfo)listData.Data;
                var eventData = new TrainViewEvent(CommandType.SelectTacticsActor);
                eventData.template = data;
                _commandData(eventData);
            }
        }

        private void CallTrainCommand()
        {
            var commandData = selectCharacter.CommandData;
            if (commandData != null)
            {
                var data = (SystemData.CommandData)commandData.Data;
                var commandType = ConfirmCommandType.No;
                if (data.Key == "Yes")
                {
                    commandType = ConfirmCommandType.Yes;
                }
                var eventData = new TrainViewEvent(CommandType.TacticsCommandClose);
                eventData.template = commandType;
                _commandData(eventData);
            }
        }

        private void CallTrainCommandCancel()
        {
            var eventData = new TrainViewEvent(CommandType.TacticsCommandClose);
            eventData.template = ConfirmCommandType.No;
            _commandData(eventData);
        }

        public void ShowSelectCharacter(List<ListData> tacticsActorInfo,TacticsCommandData tacticsCommandData)
        {
            selectCharacter.SetTacticsCharacter(tacticsActorInfo);
            selectCharacter.SetTacticsCommandData(tacticsCommandData);
            selectCharacter.UpdateSmoothSelect();
            selectCharacter.gameObject.SetActive(true);
        }

        public void HideSelectCharacter()
        {
            battleSelectCharacter.gameObject.SetActive(false);
            selectCharacter.gameObject.SetActive(false); 
            SetHelpInputInfo("TACTICS");
        }

        public void ShowSelectCharacterCommand()
        {
            selectCharacter.ShowCharacterList();
        }

        public void HideSelectCharacterCommand()
        {
            selectCharacter.HideCharacterList();
        }

        public void ShowConfirmCommand()
        {
            selectCharacter.ShowCommandList();
        }
        
        public void HideConfirmCommand()
        {
            selectCharacter.HideCommandList();
        }

        public void ShowCharacterDetail(ActorInfo actorInfo,List<ActorInfo> party)
        {
            battleSelectCharacter.gameObject.SetActive(true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,true);
            battleSelectCharacter.SelectCharacterTab(SelectCharacterTabType.Detail);
            battleSelectCharacter.SetActorInfo(actorInfo,party);
            battleSelectCharacter.SetSkillInfos(actorInfo.SkillActionList());
            SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
        }

        public void ShowLeaningList(List<ListData> learnMagicList)
        {
            battleSelectCharacter.gameObject.SetActive(true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
            battleSelectCharacter.SelectCharacterTab(SelectCharacterTabType.Magic);
            battleSelectCharacter.SetSkillInfos(learnMagicList);
            battleSelectCharacter.ShowActionList();
            battleSelectCharacter.HideStatus();
            battleSelectCharacter.MagicList.Activate();
            SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
        }

        private void CallFrontBattleIndex()
        {
            var listData = selectCharacter.CharacterData;
            if (listData != null)
            {
                var data = (TacticsActorInfo)listData.Data;
                if (data.TacticsCommandType != TacticsCommandType.Paradigm)
                {
                    return;
                }
                var eventData = new TrainViewEvent(CommandType.SelectFrontBattleIndex);
                eventData.template = data.ActorInfo.ActorId;
                _commandData(eventData);
            }
        }

        private void CallBattleBackIndex()
        {
            var listData = selectCharacter.CharacterData;
            if (listData != null)
            {
                var data = (TacticsActorInfo)listData.Data;
                if (data.TacticsCommandType != TacticsCommandType.Paradigm)
                {
                    return;
                }
                var eventData = new TrainViewEvent(CommandType.SelectBackBattleIndex);
                eventData.template = data.ActorInfo.ActorId;
                _commandData(eventData);
            }
        }

        private void CallSkillAlchemy()
        {
            var listData = battleSelectCharacter.ActionData;
            if (listData != null && listData.Enable)
            {
                var eventData = new TrainViewEvent(CommandType.SkillAlchemy);
                var data = (SkillInfo)listData;
                eventData.template = listData;
                _commandData(eventData);
            }
        }

        public void ActivateTacticsCommand()
        {
            if (selectCharacter.CharacterList.gameObject.activeSelf) selectCharacter.CharacterList.Activate();
        }

        public void DeactivateTacticsCommand()
        {
            if (selectCharacter.CharacterList.gameObject.activeSelf) selectCharacter.CharacterList.Deactivate();
        }
    }
}