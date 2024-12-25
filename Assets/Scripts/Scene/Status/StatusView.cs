using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

namespace Ryneus
{
    using Status;
    public class StatusView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private Button helpButton = null;
        [SerializeField] private BaseList commandList = null;
        [SerializeField] private BaseList memberList = null;
        [SerializeField] private MagicList equipSkillList = null;
        [SerializeField] private MagicList changeSkillList = null;
        [SerializeField] private ActorInfoComponent selectingActorInfoComponent = null;
        [SerializeField] private GameObject topLayer = null;
        [SerializeField] private GameObject statusLayer = null;

        private new Action<ViewEvent> _commandData = null;
        public void SetStatusEvent(Action<ViewEvent> commandData) => _commandData = commandData;
        public void CallEvent(CommandType statusCommandType,object sendData = null)
        {
            var commandType = new ViewCommandType(ViewCommandSceneType.Status,statusCommandType);
            var eventData = new ViewEvent(commandType)
            {
                template = sendData
            };
            _commandData(eventData);
        }

        private StatusViewInfo _statusViewInfo = null; 

        private Action _backEvent = null;
        private bool _isDisplayDecide => _statusViewInfo != null && _statusViewInfo.DisplayDecideButton;
        public bool DisplayDecide => _isDisplayDecide;
        private string _helpText;
        public bool IsRanking => _statusViewInfo != null && _statusViewInfo.IsRanking;
        public void Initialize(List<ActorInfo> actorInfos) 
        {
            base.Initialize();
            
            InitializeCommandList();
            InitializeMemberList();
            InitializeEquipSkillList();
            InitializeChangeSkillList();

            new StatusPresenter(this,actorInfos);
        }

        private void InitializeCommandList()
        {
            commandList.Initialize();
            SetInputHandler(commandList.gameObject);
            commandList.SetInputHandler(InputKeyType.Decide,OnClickCommand);
            commandList.SetInputHandler(InputKeyType.Cancel,OnClickBack);
            commandList.SetInputHandler(InputKeyType.Option1,() => 
            {
            });
            commandList.SetInputHandler(InputKeyType.Option2,() => 
            {
            });
            AddViewActives(commandList);
        }

        public void SetCommandList(List<ListData> commandListData)
        {
            commandList.SetData(commandListData);
        }

        private void InitializeMemberList()
        {
            memberList.Initialize();
            SetInputHandler(memberList.gameObject);
            memberList.SetInputHandler(InputKeyType.Decide,OnSelectActor);
            memberList.SetInputHandler(InputKeyType.Cancel,OnCancelActor);
            AddViewActives(memberList);
        }

        public void SetMemberList(List<ListData> commandListData)
        {
            memberList.SetData(commandListData);
        }

        private void OnSelectActor()
        {
            var data = memberList.ListItemData<ActorInfo>();
            if (data != null)
            {
                CallEvent(CommandType.SelectActor,data);
            }
        }

        private void OnCancelActor()
        {
            CallEvent(CommandType.CancelActor);
        }

        private void InitializeEquipSkillList()
        {
            equipSkillList.Initialize();
            equipSkillList.SetInputHandler(InputKeyType.Decide,OnSelectEquipSkill);
            equipSkillList.SetInputHandler(InputKeyType.Cancel,OnCancelEquipSkill);
            SetInputHandler(equipSkillList.gameObject);
            AddViewActives(equipSkillList);
        }

        public void SetEquipSkillList(List<ListData> skillInfos)
        {
            equipSkillList.SetData(skillInfos);
        }

        public void SetActorInfo(ActorInfo actorInfo,List<ActorInfo> partyInfo)
        {
            selectingActorInfoComponent.UpdateInfo(actorInfo,partyInfo);
        }

        private void OnSelectEquipSkill()
        {
            var data = equipSkillList.ListItemData<SkillInfo>();
            if (data != null)
            {
                CallEvent(CommandType.SelectEquipSkill,data);
            }
        }

        private void OnCancelEquipSkill()
        {
            CallEvent(CommandType.CancelEquipSkill);
        }

        private void InitializeChangeSkillList()
        {
            changeSkillList.Initialize();
            changeSkillList.SetInputHandler(InputKeyType.Decide,OnSelectChangeSkill);
            changeSkillList.SetInputHandler(InputKeyType.Cancel,OnCancelEquipSkill);
            SetInputHandler(changeSkillList.gameObject);
            AddViewActives(changeSkillList);
        }

        public void SetChangeSkillList(List<ListData> skillInfos)
        {
            changeSkillList.SetData(skillInfos);
        }

        private void OnSelectChangeSkill()
        {
            var data = changeSkillList.ListItemData<SkillInfo>();
            if (data != null)
            {
                CallEvent(CommandType.SelectChangeSkill,data);
            }
        }

        public void CommandTopLayer()
        {
            statusLayer.SetActive(false);
            topLayer.SetActive(true);
        }

        public void CommandStatusLayer()
        {
            statusLayer.SetActive(true);
            topLayer.SetActive(false);
        }

        public void CallCommandList()
        {
            memberList.UpdateSelectIndex(-1);
            SetActivate(commandList);
        }

        public void CallMemberList(int lastMemberIndex)
        {
            SetActivate(memberList);
            memberList.UpdateSelectIndex(lastMemberIndex);
        }
        
        public void CallEquipSkillList()
        {
            SetActivate(equipSkillList);
            equipSkillList.gameObject.SetActive(true);
            changeSkillList.gameObject.SetActive(false);
        }

        public void CallChangeSkillList()
        {
            SetActivate(changeSkillList);
            changeSkillList.gameObject.SetActive(true);
            equipSkillList.gameObject.SetActive(false);
        }

        public void OpenAnimation(Action endEvent)
        {
        }

        public void SetHelpWindow(string helpText)
        {
            _helpText = helpText;
        }

        public void SetViewInfo(StatusViewInfo statusViewInfo)
        {
            _statusViewInfo = statusViewInfo;
            _backEvent = statusViewInfo.BackEvent;
            SetBackEvent(statusViewInfo.BackEvent);
            if (statusViewInfo.StartIndex != -1)
            {
                CallEvent(CommandType.SelectCharacter);
            }
        }

        public void CommandBack()
        {
            _backEvent?.Invoke();
        }

        private void OnClickCommand()
        {
            var data = commandList.ListItemData<SystemData.CommandData>();
            if (data != null)
            {
                CallEvent(CommandType.SelectCommandList,data);
            }
        }

        public new void SetBusy(bool busy)
        {
            base.SetBusy(busy);
        }

        private void OnClickBack()
        {
            CallEvent(CommandType.Back);
        }

        private void OnClickHelp()
        {
            CallEvent(CommandType.CallHelp);
        }

        public int SelectedSkillId()
        {
            return -1;
        }

        public void CommandRefresh()
        {
            if (_isDisplayDecide)
            {
                SetHelpText(_helpText);
                SetHelpInputInfo("SELECT_HEROINE");
            } else
            {
                if (commandList.Active == false)
                {
                    SetHelpInputInfo("LEARN_MAGIC");
                } else
                {
                    SetHelpText(DataSystem.GetHelp(202));
                    SetHelpInputInfo("STATUS");
                }
            }
        }

        public void InputHandler(InputKeyType keyType,bool pressed)
        {
            switch (keyType)
            {
                case InputKeyType.Cancel:
                    break;
                case InputKeyType.Option1:
                    break;
                case InputKeyType.Option2:
                    break;
                case InputKeyType.Start:
                    break;
                case InputKeyType.SideLeft1:
                    break;
                case InputKeyType.SideLeft2:
                    break;
                case InputKeyType.SideRight1:
                    break;
            }
        }

        public new void MouseCancelHandler()
        {
            CallEvent(CommandType.Back);
        }
    }

    public class StatusViewInfo
    {
        private Action _backEvent = null;
        public Action BackEvent => _backEvent;
        private bool _displayDecideButton = false;
        public bool DisplayDecideButton => _displayDecideButton;
        private bool _displayBackButton = true;
        public bool DisplayBackButton => _displayBackButton;
        private bool _displayCharacterList = true;
        public bool DisplayCharacterList => _displayCharacterList;
        private bool _displayLvResetButton = false;
        public bool DisplayLvResetButton => _displayLvResetButton;
        private List<ActorInfo> _actorInfos = null;
        public List<ActorInfo> ActorInfos => _actorInfos;
        private List<BattlerInfo> _enemyInfos = null;
        public List<BattlerInfo> EnemyInfos => _enemyInfos;
        private bool _isBattle = false;
        public bool IsBattle => _isBattle;
        private bool _isRanking = false;
        public bool IsRanking => _isRanking;
        private int _startIndex = -1;
        public int StartIndex => _startIndex;
        private Action<int> _charaLayerEvent = null;
        public Action<int> CharaLayerEvent => _charaLayerEvent;
        
        public StatusViewInfo(Action backEvent)
        {
            _backEvent = backEvent;
        }

        public void SetDisplayDecideButton(bool isDisplay)
        {
            _displayDecideButton = isDisplay;
        }
        
        public void SetDisplayBackButton(bool isDisplay)
        {
            _displayBackButton = isDisplay;
        }
        
        public void SetDisplayCharacterList(bool isDisplay)
        {
            _displayCharacterList = isDisplay;
        }

        public void SetDisplayLevelResetButton(bool isDisplay)
        {
            _displayLvResetButton = isDisplay;
        }

        public void SetEnemyInfos(List<BattlerInfo> enemyInfos,bool isBattle)
        {
            _enemyInfos = enemyInfos;
            _isBattle = isBattle;
        }

        public void SetActorInfos(List<ActorInfo> actorInfos,bool isBattle)
        {
            _actorInfos = actorInfos;
            _isBattle = isBattle;
        }

        public void SetStartIndex(int actorIndex)
        {
            _startIndex = actorIndex;
        }        
        
        public void SetCharaLayerEvent(System.Action<int> charaLayerEvent)
        {
            _charaLayerEvent = charaLayerEvent;
        }

        public void SetIsRanking(bool isRanking)
        {
            _isRanking = isRanking;
        }        
    }
    
    namespace Status
    {
        public enum CommandType
        {
            None = 0,
            SelectActor,
            CancelActor,
            LeftActor,
            RightActor,
            SelectEquipSkill,
            CancelEquipSkill,
            SelectChangeSkill,
            DecideStage,
            CharacterList,
            SelectCharacter,
            SelectCommandList,
            LvReset,
            LevelUp,
            ShowLearnMagic,
            LearnMagic,
            HideLearnMagic,
            CallHelp,
            Back
        }
    }
}