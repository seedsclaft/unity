using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BattleParty;

namespace Ryneus
{
    public class BattlePartyView : BaseView
    {
        [SerializeField] private BattleSelectCharacter battleSelectCharacter = null;
        [SerializeField] private BaseList partyMemberList = null;
        [SerializeField] private BaseList enemyMemberList = null;
        [SerializeField] private BaseList tacticsMemberList = null;
        [SerializeField] private Button commandHelpButton = null;
        [SerializeField] private TrainAnimation trainAnimation = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        [SerializeField] private Button learnMagicBackButton = null;
        [SerializeField] private BaseList commandList = null;
        [SerializeField] private OnOffButton battleStartButton;
        [SerializeField] private OnOffButton battleReplayButton;
        [SerializeField] private OnOffButton enemyInfoButton;
        public SkillInfo SelectMagic => battleSelectCharacter.ActionData;
        public AttributeType AttributeType => battleSelectCharacter.AttributeType;
        private bool _isEditMode = false;
        
        private new Action<ViewEvent> _commandData = null;
        public new void SetEvent(Action<ViewEvent> commandData)
        {
            _commandData = commandData;
        }
        public void CallEvent(CommandType battleCommandType,object sendData = null)
        {
            var commandType = new ViewCommandType(ViewCommandSceneType.BattleParty,battleCommandType);
            var eventData = new ViewEvent(commandType)
            {
                template = sendData
            };
            _commandData(eventData);
        }

        public override void Initialize() 
        {
            base.Initialize();
            //partyMemberList.Initialize();
            //enemyMemberList.Initialize();
            InitializeTacticsMember();
            InitializeCommandList();
            battleSelectCharacter.Initialize();
            commandHelpButton.onClick.AddListener(() => 
            {
                CallEvent(CommandType.CommandHelp);
            });
            SetBaseAnimation(trainAnimation);
            InitializeSelectCharacter();
            SetInputHandler(battleSelectCharacter.MagicList);
            SideMenuButton.OnClickAddListener(() => 
            {
                CallSideMenu();
            });
            new BattlePartyPresenter(this);
        }

        private void InitializeTacticsMember()
        {
            tacticsMemberList.Initialize();
            tacticsMemberList.SetInputHandler(InputKeyType.Decide,OnClickDecideActor);
            tacticsMemberList.SetInputHandler(InputKeyType.Cancel,() => CallEvent(CommandType.CommandEndEdit));
            tacticsMemberList.SetSelectedHandler(() =>
            {
                var listData = tacticsMemberList.ListData;
                if (listData != null)
                {
                    var data = (ActorInfo)listData.Data;
                    CallEvent(CommandType.SelectTacticsMember,data);
                }
            });
            SetInputHandler(tacticsMemberList.gameObject);
            AddViewActives(tacticsMemberList);
        }

        private void InitializeCommandList()
        {
            commandList.Initialize();
            commandList.SetInputHandler(InputKeyType.Decide,CallCommandList);
            commandList.SetInputHandler(InputKeyType.Cancel,() => BackEvent?.Invoke());
            SetInputHandler(commandList.gameObject);
            AddViewActives(commandList);
            SetActivate(commandList);
        }

        public void OpenAnimation()
        {
            trainAnimation.OpenAnimation(UiRoot.transform,null);
        }
        
        private void CallSideMenu()
        {
            CallEvent(CommandType.SelectSideMenu);
        }

        private void OnClickDecideActor()
        {
            var listData = tacticsMemberList.ListData;
            if (listData != null)
            {
                var data = (ActorInfo)listData.Data;
                CallEvent(CommandType.DecideTacticsMember,data);
            }
        }

        public void SetCommandList(List<ListData> commandDates)
        {
            commandList.SetData(commandDates,true,() => 
            {
                commandList.UpdateSelectIndex(commandDates.Count-1);
            });
        }

        private void CallCommandList()
        {
            var listData = commandList.ListItemData<SystemData.CommandData>();
            if (listData != null)
            {
                CallEvent(CommandType.CallCommandList,listData);
            }
        }

        public void SetEditMode(bool isEditMode)
        {
            _isEditMode = isEditMode;
            if (_isEditMode)
            {            
                SetActivate(tacticsMemberList);
                tacticsMemberList.UpdateSelectIndex(0);
            } else
            {
                tacticsMemberList.UpdateSelectIndex(-1);
                SetActivate(commandList);
            }
        }

        public void SetTacticsMembers(List<ListData> tacticsMembers)
        {
            tacticsMemberList.SetData(tacticsMembers,false);
        }

        public void RefreshTacticsMembers(List<ListData> tacticsMembers)
        {
            tacticsMemberList.RefreshListData(tacticsMembers);
        }

        public void SetBattleMembers(List<ListData> battlerInfos)
        {
            partyMemberList.SetData(battlerInfos,false);
        }

        private void CallChangeLineIndex(ActorInfo actorInfo)
        {
            CallEvent(CommandType.ChangeLineIndex,actorInfo);
        }

        public void SetEnemyMembers(List<ListData> enemyInfos)
        {
            enemyMemberList.SetData(enemyInfos);
        }

        public void SetStatusButtonEvent(System.Action statusEvent)
        {
            battleSelectCharacter.SetStatusButtonEvent(statusEvent);
            tacticsMemberList.SetInputHandler(InputKeyType.SideRight1,() => 
            {
                statusEvent.Invoke();
            });
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
                CallEvent(CommandType.SelectAttribute,data);
            }
        }

        private void InitializeSelectCharacter()
        {
            battleSelectCharacter.HideActionList();
        }

        public void ShowCharacterDetail(ActorInfo actorInfo,List<ActorInfo> party,List<ListData> skillInfos,bool tabSelect = false)
        {
            battleSelectCharacter.AttributeList.gameObject.SetActive(false);
            battleSelectCharacter.gameObject.SetActive(true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,false);
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
            trainAnimation.OpenAnimation(battleSelectCharacter.transform,null);
        }

        public void ShowLeaningList(List<ListData> learnMagicList)
        {
            battleSelectCharacter.AttributeList.gameObject.SetActive(true);
            battleSelectCharacter.AttributeList.UpdateSelectIndex(0);
            battleSelectCharacter.gameObject.SetActive(true);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Magic,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.Detail,false);
            battleSelectCharacter.SetActiveTab(SelectCharacterTabType.SkillTrigger,false);
            battleSelectCharacter.SelectCharacterTab((int)SelectCharacterTabType.Magic);
            battleSelectCharacter.SetSkillInfos(learnMagicList);
            battleSelectCharacter.ShowActionList();
            trainAnimation.OpenAnimation(battleSelectCharacter.transform,null);
        }
        
        public void RefreshLeaningList(List<ListData> learnMagicList)
        {
            battleSelectCharacter.MagicList.SetData(learnMagicList);
            battleSelectCharacter.MagicList.SetInputHandler(InputKeyType.Decide,() => CallSkillAlchemy());
            battleSelectCharacter.MagicList.SetInputHandler(InputKeyType.Cancel,() => 
            {
            });
            battleSelectCharacter.MagicList.SetInputHandler(InputKeyType.SideLeft1,() => 
            {
                CallEvent(CommandType.LeftAttribute);
            });
            battleSelectCharacter.MagicList.SetInputHandler(InputKeyType.SideRight1,() => 
            {
                CallEvent(CommandType.RightAttribute);
            });
        }
        
        private void CallSkillAlchemy()
        {
            var listData = battleSelectCharacter.ActionData;
            if (listData != null && listData.Enable)
            {
            }
        }

        public void SetNuminous(int numinous)
        {
            numinousText.SetText(DataSystem.GetReplaceDecimalText(numinous));
        }

        public void SetLearnMagicButtonActive(bool IsActive)
        {
            learnMagicBackButton.gameObject.SetActive(IsActive);
        }

        public void SetBattleReplayEnable(bool isEnable)
        {
            battleReplayButton.Disable?.SetActive(!isEnable);
        }

        public void SelectAttribute(int index)
        {
            battleSelectCharacter.AttributeList.UpdateSelectIndex(index);
        }

        public void CommandRefresh()
        {
            if (_isEditMode)
            {
                if (learnMagicBackButton.gameObject.activeSelf)
                {
                    SetHelpInputInfo("LEARN_MAGIC");
                } else
                {
                    SetHelpInputInfo("EDIT_PARTY");
                }
            } else
            {
                SetHelpInputInfo("BATTLE_PARTY");
            }
/*
            if (GameSystem.ConfigData.InputType)
            {
                if (_isEditMode)
                {
                    if (learnMagicBackButton.gameObject.activeSelf)
                    {
                        battleSelectCharacter.MagicList.Activate();
                        tacticsMemberList.Deactivate();
                    } else
                    {
                        battleSelectCharacter.MagicList.Deactivate();
                        tacticsMemberList.Activate();
                    }
                    commandList.Deactivate();
                } else
                {
                    commandList.Activate();
                    tacticsMemberList.Deactivate();
                }
            }
            */
        }
    }
}

namespace BattleParty
{
    public enum CommandType
    {
        None = 0,
        CallCommandList,
        CommandEndEdit,
        SelectSideMenu,
        SelectAttribute,
        LeftAttribute,
        RightAttribute,
        DecideTacticsMember,
        SelectTacticsMember,
        EnemyInfo,
        BattleStart,
        BattleReplay,
        CommandHelp,
        ChangeLineIndex,
    }
}