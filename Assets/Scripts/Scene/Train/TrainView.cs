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
        public int CharacterSelectIndex => selectCharacter.CharacterList.Index;
        //private BaseList attributeList = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        private new System.Action<TrainViewEvent> _commandData = null;
        private new System.Action<StatusViewEvent> _statusCommandData = null;

        [SerializeField] private OnOffButton enemyInfoButton = null;
        [SerializeField] private Button commandHelpButton = null;
        [SerializeField] private TrainAnimation trainAnimation = null;
        [SerializeField] private StatusLevelUp statusLevelUp = null;

        public SkillInfo SelectMagic => battleSelectCharacter.ActionData;

        private System.Action<TrainViewEvent,TacticsCommandType> _inputKeyEvent;

        public void Initialize(System.Action<ViewEvent> parentEvent)
        {
            base.Initialize();
            SetEvent(parentEvent);

            battleSelectCharacter.Initialize();
            SetInputHandler(battleSelectCharacter.gameObject);
            InitializeSelectCharacter();

            battleSelectCharacter.gameObject.SetActive(false);
            commandHelpButton.onClick.AddListener(() => 
            {
                var eventData = new TrainViewEvent(CommandType.CommandHelp);
                _commandData(eventData);
            });
            SetBackCommand(() => OnClickBack());
            SetBaseAnimation(trainAnimation);
            enemyInfoButton?.SetCallHandler(() => 
            {
                var eventData = new TrainViewEvent(CommandType.EnemyInfo);
                _commandData(eventData);
                enemyInfoButton.SetActiveCursor(false);
            });
            enemyInfoButton?.SetText(DataSystem.GetText(809));
            statusLevelUp.Initialize();
            statusLevelUp.SetActive(false);
            new TrainPresenter(this);
            selectCharacter.gameObject.SetActive(false);
        }

        public void OpenAnimation()
        {
        }


        public void SetParentInputKeyActive(System.Action<TrainViewEvent,TacticsCommandType> inputKeyEvent)
        {
            _inputKeyEvent = inputKeyEvent;
        }

        public new void SetBusy(bool isBusy)
        {
            base.SetBusy(isBusy);
        }

        public void CallTrainCommand(TacticsCommandType tacticsCommandType)
        {
            var eventData = new TrainViewEvent(CommandType.SelectTacticsCommand)
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
            battleSelectCharacter.HideActionList();
        }

        public void SetStatusButtonEvent(System.Action statusEvent)
        {
            battleSelectCharacter.SetStatusButtonEvent(statusEvent);
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

        public void SetStatusEvent(System.Action<StatusViewEvent> commandData)
        {
            _statusCommandData = commandData;
            statusLevelUp.SetEvent(commandData);
        }

        public void SetLvUpCost(int cost)
        {
            statusLevelUp.SetLvUpCost(cost);
        }
        
        public void SetToLvText(int current)
        {
            statusLevelUp.ToLvText(current);
        }
        
        public void SetLearnMagicButtonActive(bool IsActive)
        {
            statusLevelUp.SetLearnMagicButtonActive(IsActive);
        }

        public void SetSelectCharacter(List<ListData> tacticsActorInfo,List<ListData> confirmCommands)
        {
            selectCharacter.Initialize(() => CallBattleReplay());
            //selectCharacter.SetCharacterData(actorInfos);
            SetInputHandler(selectCharacter.gameObject);
            SetInputHandler(selectCharacter.CharacterList.gameObject);
            
            //selectCharacter.SetTacticsCharacter(tacticsActorInfo);
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
                    template = data.ActorInfo.ActorId
                };
                _commandData(eventData);
            }
        }

        private void CallTrainCommand()
        {
            var commandType = ConfirmCommandType.Yes;
            var eventData = new TrainViewEvent(CommandType.DecideTacticsCommand)
            {
                template = commandType
            };
            _commandData(eventData);
        }

        private void CallTrainCommandCancel()
        {
            var eventData = new TrainViewEvent(CommandType.DecideTacticsCommand)
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

        public void ShowTacticsCharacter()
        {
            var eventData = new TrainViewEvent(CommandType.ShowTacticsCharacter);
            _commandData(eventData);
        }

        public void ShowSelectCharacter(List<ListData> tacticsActorInfo,TacticsCommandData tacticsCommandData)
        {
            selectCharacter.SetTacticsCharacter(tacticsActorInfo);
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Decide,() => CallActorTrain());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Left,() => CallFrontBattleIndex());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Right,() => CallBattleBackIndex());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Cancel,() => CallTrainCommandCancel());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Option1,() => CallSkillTriggerCommand());
            selectCharacter.SetInputHandlerCharacter(InputKeyType.Start,() => CallTrainCommand());
            selectCharacter.SetInputHandlerCommand(() => CallTrainCommand());
            selectCharacter.CharacterList.SetSelectedHandler(() => 
            {
                CallChangeSelectTacticsActor();
            });
            selectCharacter.SetTacticsCommandData(tacticsCommandData);
            //selectCharacter.UpdateSmoothSelect();
            selectCharacter.gameObject.SetActive(true);
            trainAnimation.OpenCharacterListAnimation(selectCharacter.transform,null);
        }

        public void CallChangeSelectTacticsActor()
        {
            var listData = selectCharacter.CharacterData;
            if (listData != null && listData.Enable)
            {
                var data = (TacticsActorInfo)listData.Data;
                var eventData = new TrainViewEvent(CommandType.ChangeSelectTacticsActor)
                {
                    template = data.ActorInfo.ActorId
                };
                _commandData(eventData);
            }
        }

        public void RefreshTacticsActor(List<ListData> tacticsActorInfo)
        {
            selectCharacter.SetTacticsCharacter(tacticsActorInfo);
        }

        public void HideSelectCharacter()
        {
            //attributeList.gameObject.SetActive(false);
            battleSelectCharacter.gameObject.SetActive(false);
            statusLevelUp.SetActive(false);
            selectCharacter.gameObject.SetActive(false); 
            SetHelpInputInfo("TACTICS");
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

        public void ShowCharacterDetail(ActorInfo actorInfo,List<ActorInfo> party,List<ListData> skillInfos,bool tabSelect = false)
        {
            battleSelectCharacter.AttributeList.gameObject.SetActive(false);
            battleSelectCharacter.gameObject.SetActive(true);
            statusLevelUp.SetActive(true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.SkillTrigger,false);
            if (tabSelect)
            {
                battleSelectCharacter.SelectCharacterTab((int)SelectCharacterTabType.Detail);
            }
            battleSelectCharacter.SetActorInfo(actorInfo,party);
            battleSelectCharacter.SetSkillInfos(skillInfos);
            battleSelectCharacter.SelectCharacterTab((int)SelectCharacterTabType.Detail);
            SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
            trainAnimation.OpenAnimation(battleSelectCharacter.transform,null);
        }

        public void ShowLeaningList(List<ListData> learnMagicList)
        {
            battleSelectCharacter.AttributeList.gameObject.SetActive(true);
            battleSelectCharacter.gameObject.SetActive(true);
            statusLevelUp.SetActive(true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.SkillTrigger,false);
            battleSelectCharacter.SelectCharacterTab((int)SelectCharacterTabType.Magic);
            battleSelectCharacter.SetSkillInfos(learnMagicList);
            battleSelectCharacter.ShowActionList();
            SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
            trainAnimation.OpenAnimation(battleSelectCharacter.transform,null);
        }

        public void CommandSelectActorAlchemy()
        {
            if (battleSelectCharacter.MagicList.Index == -1)
            {
                battleSelectCharacter.MagicList.Refresh(0);
            }
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
                var eventData = new TrainViewEvent(CommandType.SelectSkillTrigger)
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
                var eventData = new StatusViewEvent(Status.CommandType.LearnMagic);
                var data = listData;
                eventData.template = listData;
                _statusCommandData(eventData);
            }
        }

        public void UpdateInputKeyActive(TrainViewEvent viewEvent,TacticsCommandType currentTacticsCommandType)
        {
            _inputKeyEvent?.Invoke(viewEvent,currentTacticsCommandType);
            switch (viewEvent.commandType)
            {
                case CommandType.SelectTacticsCommand:
                    var tacticsCommandType = (TacticsCommandType)viewEvent.template;
                    switch (tacticsCommandType)
                    {
                        case TacticsCommandType.Paradigm:
                            break;
                        case TacticsCommandType.Train:
                            break;
                        case TacticsCommandType.Alchemy:
                            selectCharacter.CharacterList.Activate(); 
                            battleSelectCharacter.SetBusy(true);
                            if (battleSelectCharacter.MagicList.Index != -1)
                            {
                                battleSelectCharacter.MagicList.Refresh(-1);
                            }
                            break;
                        case TacticsCommandType.Status:
                            SetBusy(true);
                            break;
                    }
                    break;
                case CommandType.SelectTacticsActor:
                    if (currentTacticsCommandType == TacticsCommandType.Alchemy)
                    {
                        battleSelectCharacter.SetBusy(false);
                        selectCharacter.CharacterList.Deactivate();             
                    }
                    break;
                case CommandType.ActorLearnMagic:
                    battleSelectCharacter.SetBusy(true);
                    break;
            }
        }
    }
}