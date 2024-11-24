using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Status;
using System;

namespace Ryneus
{
    public class StatusView : BaseView ,IInputHandlerEvent
    {

        [SerializeField] private Button helpButton = null;

        [SerializeField] private BaseList commandList = null;
        [SerializeField] private BaseList memberList = null;
        [SerializeField] private MagicList magicList = null;
        [SerializeField] private SkillAction selectingSkill = null;
        [SerializeField] private ActorInfoComponent selectingActorInfoComponent = null;
        [SerializeField] private GameObject topLayer = null;
        [SerializeField] private GameObject statusLayer = null;

        private List<BaseList> _viewActives = new ();
        private void SetActivate(BaseList baseView)
        {
            var find = _viewActives.Find(a => a == baseView);
            foreach (var viewActives in _viewActives)
            {
                if (viewActives == find)
                {
                    find.Activate();
                } else
                {
                    viewActives.Deactivate();
                }
            }
        }

        private new Action<ViewEvent> _commandData = null;
        public void SetStatusEvent(Action<ViewEvent> commandData) => _commandData = commandData;
        public void CallEvent(CommandType statusCommandType,object sendData = null)
        {
            var commandType = new ViewCommandType
            {
                StatusCommandType = statusCommandType
            };
            var eventData = new ViewEvent(commandType)
            {
                template = sendData
            };
            _commandData(eventData);
        }

        public SkillInfo SelectMagic => (SkillInfo)magicList.ListData?.Data;
        private StatusViewInfo _statusViewInfo = null; 

        private Action _backEvent = null;
        private bool _isDisplayDecide => _statusViewInfo != null && _statusViewInfo.DisplayDecideButton;
        public bool DisplayDecide => _isDisplayDecide;
        private string _helpText;
        public bool IsRanking => _statusViewInfo != null && _statusViewInfo.IsRanking;
        public void Initialize(List<ActorInfo> actorInfos) 
        {
            base.Initialize();
            InitializeSelectCharacter();
            
            InitializeCommandList();
            InitializeMemberList();
            InitializeMagicList();
            selectingSkill.gameObject.SetActive(false);

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
            _viewActives.Add(commandList);
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
            _viewActives.Add(memberList);
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

        private void InitializeMagicList()
        {
            magicList.Initialize();
            magicList.SetInputHandler(InputKeyType.Decide,OnSelectMagic);
            magicList.SetInputHandler(InputKeyType.Cancel,OnCancelSkill);
            SetInputHandler(magicList.gameObject);
            _viewActives.Add(magicList);
        }

        public void SetMagicList(List<ListData> skillInfos)
        {
            magicList.SetData(skillInfos);
        }

        public void SetActorInfo(ActorInfo actorInfo,List<ActorInfo> partyInfo)
        {
            selectingActorInfoComponent.UpdateInfo(actorInfo,partyInfo);
        }

        public void SetSelectingSkill(ListData skillInfo)
        {
            selectingSkill.gameObject.SetActive(skillInfo != null);
            selectingSkill.SetListData(skillInfo,0);
            selectingSkill.UpdateViewItem();
        }

        private void OnSelectMagic()
        {
            var data = magicList.ListItemData<SkillInfo>();
            if (data != null)
            {
                CallEvent(CommandType.SelectSkill,data);
            }
        }

        private void OnCancelSkill()
        {
            CallEvent(CommandType.CancelSkill);
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
        
        public void CallMagicList()
        {
            SetActivate(magicList);
        }

        public void OpenAnimation(Action endEvent)
        {
        }

        private void InitializeSelectCharacter()
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
}

namespace Ryneus
{    
    public partial class ViewCommandType
    {
        public CommandType StatusCommandType;
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
        SelectSkill,
        CancelSkill,
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