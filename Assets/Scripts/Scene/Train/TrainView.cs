using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Train;

namespace Ryneus
{
    public class TrainView : BaseView
    {

        [SerializeField] private BattleSelectCharacter battleSelectCharacter = null;
        [SerializeField] private TacticsSelectCharacter selectCharacter = null;
        //private BaseList attributeList = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        private new System.Action<TrainViewEvent> _commandData = null;


        [SerializeField] private Button commandHelpButton = null;

        public SkillInfo SelectMagic => battleSelectCharacter.ActionData;

        private System.Action<TrainViewEvent,TacticsCommandType> _inputKeyEvent;

        public void Initialize(System.Action<ViewEvent> parentEvent)
        {
            base.Initialize();
            SetEvent(parentEvent);

            battleSelectCharacter.Initialize();
            SetInputHandler(battleSelectCharacter.GetComponent<IInputHandlerEvent>());
            InitializeSelectCharacter();

            battleSelectCharacter.gameObject.SetActive(false);
            commandHelpButton.onClick.AddListener(() => 
            {
                var eventData = new TrainViewEvent(CommandType.CommandHelp);
                _commandData(eventData);
            });
            SetBackCommand(() => OnClickBack());
            new TrainPresenter(this);
            selectCharacter.gameObject.SetActive(false);
        }

        public void SetParentInputKeyActive(System.Action<TrainViewEvent,TacticsCommandType> inputKeyEvent)
        {
            _inputKeyEvent = inputKeyEvent;
        }

        public new void SetBusy(bool isBusy)
        {
            base.SetBusy(isBusy);
            if (isBusy)
            {
                selectCharacter.CharacterList.Deactivate();
                selectCharacter.CommandList.Deactivate();
                battleSelectCharacter.MagicList.Deactivate();
                battleSelectCharacter.AttributeList.Deactivate();
            } else
            {
                selectCharacter.CharacterList.Activate();
                selectCharacter.CommandList.Activate();
                battleSelectCharacter.MagicList.Activate();
                battleSelectCharacter.AttributeList.Activate();
            }
        }

        public void CallTrainCommand(TacticsCommandType tacticsCommandType)
        {
            var eventData = new TrainViewEvent(CommandType.TacticsCommand)
            {
                template = tacticsCommandType
            };
            _commandData(eventData);
        }

        public void SetNuminous(int numinous)
        {
            numinousText.SetText(DataSystem.GetReplaceDecimalText(numinous));
        }
        
        public void SetAttributeList(List<ListData> list)
        {
            battleSelectCharacter.SetAttributeList(list);
            battleSelectCharacter.AttributeList.SetSelectedHandler(() => OnSelectAttribute());
        }

        private void OnSelectAttribute()
        {
            var listData = battleSelectCharacter.AttributeList.ListData;
            if (listData != null)
            {
                var data = (AttributeType)listData.Data;
                var eventData = new TrainViewEvent(CommandType.SelectAttribute)
                {
                    template = data
                };
                _commandData(eventData);
            }
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
            selectCharacter.Initialize(() => CallBattleReplay());
            //selectCharacter.SetCharacterData(actorInfos);
            SetInputHandler(selectCharacter.GetComponent<IInputHandlerEvent>());
            selectCharacter.SetTacticsCommand(confirmCommands);
            
            selectCharacter.SetTacticsCharacter(tacticsActorInfo);
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Decide,() => CallActorTrain());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Right,() => CallFrontBattleIndex());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Left,() => CallBattleBackIndex());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Cancel,() => CallTrainCommandCancel());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Option1,() => CallSkillTriggerCommand());
            selectCharacter.SetInputHandlerCommand(InputKeyType.Decide,() => CallTrainCommand());
            selectCharacter.SetInputHandlerCommand(InputKeyType.Cancel,() => CallTrainCommandCancel());
            SetInputHandler(selectCharacter.CharacterList.GetComponent<IInputHandlerEvent>());
            SetInputHandler(selectCharacter.CommandList.GetComponent<IInputHandlerEvent>());
            selectCharacter.CharacterList.SetSelectedHandler(() => 
            {
                var listData = selectCharacter.CharacterData;
                if (listData != null)
                {
                    var data = (TacticsActorInfo)listData.Data;
                    var eventData = new TrainViewEvent(CommandType.ChangeSelectTacticsActor)
                    {
                        template = data.ActorInfo.ActorId
                    };
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
                var eventData = new TrainViewEvent(CommandType.SelectTacticsActor)
                {
                    template = data
                };
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
                var eventData = new TrainViewEvent(CommandType.TacticsCommandClose)
                {
                    template = commandType
                };
                _commandData(eventData);
            }
        }

        private void CallTrainCommandCancel()
        {
            var eventData = new TrainViewEvent(CommandType.TacticsCommandClose)
            {
                template = ConfirmCommandType.No
            };
            _commandData(eventData);
        }

        private void CallBattleReplay()
        {
            var eventData = new TrainViewEvent(CommandType.BattleReplay);
            _commandData(eventData);
        }

        public void ShowSelectCharacter(List<ListData> tacticsActorInfo,TacticsCommandData tacticsCommandData)
        {
            selectCharacter.SetTacticsCharacter(tacticsActorInfo);
            selectCharacter.SetTacticsCommandData(tacticsCommandData);
            //selectCharacter.UpdateSmoothSelect();
            selectCharacter.gameObject.SetActive(true);
        }

        public void HideSelectCharacter()
        {
            //attributeList.gameObject.SetActive(false);
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

        public void ShowBattleReplay(bool isActive)
        {
            selectCharacter.ShowBattleReplay(isActive);
        }
        
        public void HideConfirmCommand()
        {
            selectCharacter.HideCommandList();
        }

        public void ShowCharacterDetail(ActorInfo actorInfo,List<ActorInfo> party,bool tabSelect = false)
        {
            battleSelectCharacter.AttributeList.gameObject.SetActive(false);
            battleSelectCharacter.gameObject.SetActive(true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.SkillTrigger,false);
            if (tabSelect)
            {
                battleSelectCharacter.SelectCharacterTab((int)SelectCharacterTabType.Detail);
            }
            battleSelectCharacter.SetActorInfo(actorInfo,party);
            battleSelectCharacter.SetSkillInfos(actorInfo.SkillActionList());
            SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
        }

        public void ShowLeaningList(List<ListData> learnMagicList)
        {
            battleSelectCharacter.AttributeList.gameObject.SetActive(true);
            battleSelectCharacter.gameObject.SetActive(true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.SkillTrigger,false);
            battleSelectCharacter.SelectCharacterTab((int)SelectCharacterTabType.Magic);
            battleSelectCharacter.SetSkillInfos(learnMagicList);
            battleSelectCharacter.ShowActionList();
            //battleSelectCharacter.HideStatus();
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
                var eventData = new TrainViewEvent(CommandType.SelectFrontBattleIndex)
                {
                    template = data.ActorInfo.ActorId
                };
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
                var eventData = new TrainViewEvent(CommandType.SelectBackBattleIndex)
                {
                    template = data.ActorInfo.ActorId
                };
                _commandData(eventData);
            }
        }

        private void CallSkillTriggerCommand()
        {
            var listData = selectCharacter.CharacterData;
            if (listData != null)
            {
                var data = (TacticsActorInfo)listData.Data;
                if (data.TacticsCommandType != TacticsCommandType.Paradigm)
                {
                    return;
                }
                var eventData = new TrainViewEvent(CommandType.SkillTrigger)
                {
                    template = data.ActorInfo.ActorId
                };
                _commandData(eventData);
            }
        }

        private void CallSkillAlchemy()
        {
            var listData = battleSelectCharacter.ActionData;
            if (listData != null && listData.Enable)
            {
                var eventData = new TrainViewEvent(CommandType.SkillAlchemy);
                var data = listData;
                eventData.template = listData;
                _commandData(eventData);
            }
        }

        public void ActivateTacticsCommand()
        {
            if (selectCharacter.CharacterList.gameObject.activeSelf)
                selectCharacter.CharacterList.Activate();
        }

        public void DeactivateTacticsCommand()
        {
            if (selectCharacter.CharacterList.gameObject.activeSelf)
                selectCharacter.CharacterList.Deactivate();
        }

        public void UpdateInputKeyActive(TrainViewEvent viewEvent,TacticsCommandType currentTacticsCommandType)
        {
            _inputKeyEvent?.Invoke(viewEvent,currentTacticsCommandType);
            switch (viewEvent.commandType)
            {
                case CommandType.TacticsCommand:
                    var tacticsCommandType = (TacticsCommandType)viewEvent.template;
                    switch (tacticsCommandType)
                    {
                        case TacticsCommandType.Paradigm:
                            break;
                        case TacticsCommandType.Train:
                        case TacticsCommandType.Alchemy:
                            ActivateTacticsCommand();
                            break;
                        case TacticsCommandType.Status:
                            break;
                    }
                    break;
            }
        }
    }
}