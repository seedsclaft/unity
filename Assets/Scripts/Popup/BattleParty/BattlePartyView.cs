using System.Collections;
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
        [SerializeField] private Button enemyInfoButton;
        [SerializeField] private OnOffButton battleStartButton;
        [SerializeField] private OnOffButton battleReplayButton;
        private new System.Action<BattlePartyViewEvent> _commandData = null;
        private System.Action<StatusViewEvent> _statusCommandData = null;
        public SkillInfo SelectMagic => battleSelectCharacter.ActionData;
        public AttributeType AttributeType => battleSelectCharacter.AttributeType;
        
        public override void Initialize() 
        {
            base.Initialize();
            partyMemberList.Initialize();
            tacticsMemberList.Initialize();
            enemyMemberList.Initialize();
            battleSelectCharacter.Initialize();
            commandHelpButton.onClick.AddListener(() => 
            {
                var eventData = new BattlePartyViewEvent(CommandType.CommandHelp);
                _commandData(eventData);
            });
            SetBaseAnimation(trainAnimation);
            InitializeSelectCharacter();
            tacticsMemberList.SetInputHandler(InputKeyType.Decide,() => OnClickDecideActor());
            tacticsMemberList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            //characterList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            //characterList.SetInputHandler(InputKeyType.Decide,() => CallDecideActor());
            SetInputHandler(partyMemberList.gameObject);
            SetInputHandler(tacticsMemberList.gameObject);
            SetInputHandler(enemyMemberList.gameObject);
            SideMenuButton.SetCallHandler(() => 
            {
                CallSideMenu();
            });
            learnMagicBackButton?.onClick.AddListener(() => 
            {
                if (learnMagicBackButton.gameObject.activeSelf == false) return;
                var eventData = new StatusViewEvent(Status.CommandType.HideLearnMagic);
                _statusCommandData(eventData);
            });
            enemyInfoButton.onClick.AddListener(() =>
            {
                var eventData = new BattlePartyViewEvent(CommandType.EnemyInfo);
                _commandData(eventData);
            });
            battleStartButton.SetCallHandler(() =>
            {
                var eventData = new BattlePartyViewEvent(CommandType.BattleStart);
                _commandData(eventData);
            });
            battleStartButton.SetText(DataSystem.GetText(30020));
            battleReplayButton.SetCallHandler(() =>
            {
                var eventData = new BattlePartyViewEvent(CommandType.BattleReplay);
                _commandData(eventData);
            });
            battleReplayButton.SetText(DataSystem.GetText(30010));
            new BattlePartyPresenter(this);
        }

        public void OpenAnimation()
        {
            trainAnimation.OpenAnimation(UiRoot.transform,null);
        }
        
        public void SetEvent(System.Action<BattlePartyViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetStatusEvent(System.Action<StatusViewEvent> commandData)
        {
            _statusCommandData = commandData;
        }

        private void CallSideMenu()
        {
            var eventData = new BattlePartyViewEvent(CommandType.SelectSideMenu);
            _commandData(eventData);
        }

        private void OnClickBack()
        {
            var eventData = new BattlePartyViewEvent(CommandType.Back);
            _commandData(eventData);
        }

        private void OnClickDecideActor()
        {
            var listData = tacticsMemberList.ListData;
            if (listData != null)
            {
                var data = (ActorInfo)listData.Data;
                var eventData = new BattlePartyViewEvent(CommandType.DecideTacticsMember)
                {
                    template = data
                };
                _commandData(eventData);
            }
        }

        public void SetTacticsMembers(List<ListData> tacticsMembers)
        {
            tacticsMemberList.SetData(tacticsMembers,false,() => 
            {
                SetTacticsMemberHandler();
            });
            tacticsMemberList.SetSelectedHandler(() =>
            {
                var listData = tacticsMemberList.ListData;
                if (listData != null)
                {
                    var data = (ActorInfo)listData.Data;
                    var eventData = new BattlePartyViewEvent(CommandType.SelectTacticsMember)
                    {
                        template = data
                    };
                    _commandData(eventData);
                }
            });
        }

        public void RefreshTacticsMembers(List<ListData> tacticsMembers)
        {
            tacticsMemberList.RefreshListData(tacticsMembers);
        }

        public void SetBattleMembers(List<ListData> battlerInfos)
        {
            partyMemberList.SetData(battlerInfos,false,() => 
            {
                for (int i = 0; i < partyMemberList.ItemPrefabList.Count;i++)
                {
                    var battlePartyMember = partyMemberList.ItemPrefabList[i].GetComponent<BattlePartyMemberItem>();
                    battlePartyMember.SetLineIndexHandler((a) => 
                    {
                        var eventData = new BattlePartyViewEvent(CommandType.ChangeLineIndex)
                        {
                            template = a
                        };
                        _commandData(eventData);
                    });
                }
            });
        }

        public void SetEnemyMembers(List<ListData> enemyInfos)
        {
            enemyMemberList.SetData(enemyInfos);
        }

        public void SetStatusButtonEvent(System.Action statusEvent)
        {
            battleSelectCharacter.SetStatusButtonEvent(statusEvent);
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
                var eventData = new BattlePartyViewEvent(CommandType.SelectAttribute)
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

        private void SetTacticsMemberHandler()
        {
            for (int i = 0; i < tacticsMemberList.ItemPrefabList.Count;i++)
            {
                var battlePartyTacticsMember = tacticsMemberList.ItemPrefabList[i].GetComponent<BattlePartyTacticsMember>();
                battlePartyTacticsMember.SetSkillTriggerHandler(() => 
                {
                    var eventData = new StatusViewEvent(Status.CommandType.SelectCommandList);
                    _statusCommandData(eventData);
                });
                battlePartyTacticsMember.SetLevelUpHandler(() => 
                {
                    var eventData = new StatusViewEvent(Status.CommandType.LevelUp);
                    _statusCommandData(eventData);
                });
                battlePartyTacticsMember.SetLearnMagicHandler(() => 
                {
                    var eventData = new StatusViewEvent(Status.CommandType.ShowLearnMagic);
                    _statusCommandData(eventData);
                });
            }
        }

        public void ShowCharacterDetail(ActorInfo actorInfo,List<ActorInfo> party,List<ListData> skillInfos,bool tabSelect = false)
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
            battleSelectCharacter.SetSkillInfos(skillInfos);
            battleSelectCharacter.SelectCharacterTab((int)SelectCharacterTabType.Detail);
            SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
            trainAnimation.OpenAnimation(battleSelectCharacter.transform,null);
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
            SetHelpInputInfo("ALCHEMY_ATTRIBUTE");
            trainAnimation.OpenAnimation(battleSelectCharacter.transform,null);
        }
        
        public void RefreshLeaningList(List<ListData> learnMagicList)
        {
            battleSelectCharacter.MagicList.SetData(learnMagicList);
        }
        
        private void CallSkillAlchemy()
        {
            var listData = battleSelectCharacter.ActionData;
            if (listData != null && listData.Enable)
            {
                var eventData = new StatusViewEvent(Status.CommandType.LearnMagic)
                {
                    template = listData
                };
                _statusCommandData(eventData);
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
    }
}

namespace BattleParty
{
    public enum CommandType
    {
        None = 0,
        Back,
        SelectSideMenu,
        SelectAttribute,
        DecideTacticsMember,
        SelectTacticsMember,
        EnemyInfo,
        BattleStart,
        BattleReplay,
        CommandHelp,
        ChangeLineIndex,
    }
}

public class BattlePartyViewEvent
{
    public BattleParty.CommandType commandType;
    public object template;

    public BattlePartyViewEvent(BattleParty.CommandType type)
    {
        commandType = type;
    }
}