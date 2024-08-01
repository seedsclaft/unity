using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Status;
using TMPro;

namespace Ryneus
{
    public class StatusView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private BattleSelectCharacter selectCharacter = null;
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private Button decideButton = null;
        [SerializeField] private OnOffButton characterListButton = null;
        private new System.Action<StatusViewEvent> _commandData = null;
        [SerializeField] private Button leftButton = null;
        [SerializeField] private Button rightButton = null;
        [SerializeField] private GameObject decideAnimation = null;
        [SerializeField] private MagicList magicList = null;
        [SerializeField] private BaseList commandList = null;
        [SerializeField] private OnOffButton levelUpButton = null;
        [SerializeField] private OnOffButton learnMagicButton = null;
        [SerializeField] private Button learnMagicBackButton = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        [SerializeField] private TextMeshProUGUI lvUpCostText = null;

        public SkillInfo SelectMagic => (SkillInfo)magicList.ListData?.Data;
        private StatusViewInfo _statusViewInfo = null; 

        private System.Action _backEvent = null;
        private bool _isDisplayDecide => _statusViewInfo != null && _statusViewInfo.DisplayDecideButton;
        private string _helpText;
        private bool _isDisplayBack => _statusViewInfo != null && _statusViewInfo.DisplayBackButton;
        public override void Initialize() 
        {
            base.Initialize();
            selectCharacter.Initialize();
            InitializeSelectCharacter();
            
            SetDecideAnimation();
            magicList.Initialize();
            SetInputHandler(magicList.gameObject);
            commandList.Initialize();
            SetInputHandler(commandList.gameObject);

            levelUpButton?.SetCallHandler(() => 
            {
                if (levelUpButton.gameObject.activeSelf == false) return;
                var eventData = new StatusViewEvent(CommandType.LevelUp);
                _commandData(eventData);
            });
            learnMagicButton?.SetCallHandler(() => 
            {
                if (learnMagicButton.gameObject.activeSelf == false) return;
                var eventData = new StatusViewEvent(CommandType.ShowLearnMagic);
                _commandData(eventData);
            });
            learnMagicBackButton?.onClick.AddListener(() => 
            {
                if (learnMagicBackButton.gameObject.activeSelf == false) return;
                var eventData = new StatusViewEvent(CommandType.HideLearnMagic);
                _commandData(eventData);
            });
            SetLearnMagicButtonActive(false);
            new StatusPresenter(this);
            selectCharacter.SetBusy(false);
        }

        private void InitializeSelectCharacter()
        {
            selectCharacter.SetInputHandlerAction(InputKeyType.Decide,() => CallSkillAction());
            selectCharacter.SetInputHandlerAction(InputKeyType.Cancel,() => OnClickBack());
            selectCharacter.SelectCharacterTab((int)SelectCharacterTabType.Detail);
            selectCharacter.ShowActionList();
        }
        
        public void SetUIButton(List<ListData> commandListData)
        {
            leftButton.onClick.AddListener(() => OnClickLeft());
            rightButton.onClick.AddListener(() => OnClickRight());
            decideButton.onClick.AddListener(() => OnClickDecide());
            characterListButton.SetCallHandler(() => OnClickCharacterList());
            commandList.SetInputHandler(InputKeyType.Decide,() => OnClickCommand());
            commandList.SetData(commandListData);
        }

        public void ShowArrows()
        {
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);
        }

        public void HideArrows()
        {
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
        }

        public void ShowDecideButton()
        {
            if (_isDisplayDecide)
            {
                decideButton.gameObject.SetActive(true);
                HelpWindow.SetHelpText(_helpText);
                HelpWindow.SetInputInfo("SELECT_HEROINE");
            }
        }

        public void HideDecideButton()
        {
            decideButton.gameObject.SetActive(false);
        }

        public void SetHelpWindow(string helpText)
        {
            _helpText = helpText;
        }

        public void SetEvent(System.Action<StatusViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetViewInfo(StatusViewInfo statusViewInfo)
        {
            _statusViewInfo = statusViewInfo;
            _backEvent = statusViewInfo.BackEvent;
            DisplayCharacterList(statusViewInfo.DisplayCharacterList);
            DisplayLvResetButton(statusViewInfo.DisplayLvResetButton);
            SetBackEvent(statusViewInfo.BackEvent);
            DisplayDecideButton();
            if (statusViewInfo.StartIndex != -1)
            {
                var eventData = new StatusViewEvent(CommandType.SelectCharacter)
                {
                    template = statusViewInfo.StartIndex
                };
                _commandData(eventData);
            }
        }

        public void CommandBack()
        {
            _backEvent?.Invoke();
        }

        private void OnClickCommand()
        {
            var listData = commandList.ListData;
            if (listData != null)
            {
                var data = (SystemData.CommandData)listData.Data;
                if (data != null)
                {
                    var eventData = new StatusViewEvent(CommandType.SelectCommandList)
                    {
                        template = data
                    };
                    _commandData(eventData);
                }
            }
        }

        public new void SetBusy(bool busy)
        {
            base.SetBusy(busy);
            selectCharacter.SetBusy(busy);
        }

        private void DisplayDecideButton()
        {
            decideButton.gameObject.SetActive(_isDisplayDecide);
            commandList.gameObject.SetActive(!_isDisplayDecide);
            ChangeBackCommandActive(!_isDisplayDecide);
            if (_isDisplayDecide)
            {
                SetHelpText(_helpText);
                SetHelpInputInfo("SELECT_HEROINE");
            } else
            {
                SetHelpText(DataSystem.GetHelp(202));
                SetHelpInputInfo("STATUS");
            }
        }

        private void DisplayLvResetButton(bool isDisplay)
        {
            if (isDisplay == false) return;
            selectCharacter.InitializeLvReset(() => 
            {
                OnClickLvReset();
            });
        }

        public void SetNuminous(int numinous)
        {
            numinousText.SetText(DataSystem.GetReplaceDecimalText(numinous));
        }

        public void SetLvUpCost(int cost)
        {
            lvUpCostText.SetText(cost.ToString() + DataSystem.GetText(1000));
        }


        private void DisplayCharacterList(bool isDisplay)
        {
            characterListButton.gameObject.SetActive(isDisplay);
        }

        private void OnClickBack()
        {
            var eventData = new StatusViewEvent(CommandType.Back);
            _commandData(eventData);
        }

        private void OnClickCharacterList()
        {
            var eventData = new StatusViewEvent(CommandType.CharacterList);
            _commandData(eventData);
        }

        private void OnClickLeft()
        {
            if (!leftButton.gameObject.activeSelf) return;
            var eventData = new StatusViewEvent(CommandType.LeftActor);
            _commandData(eventData);
        }

        private void OnClickRight()
        {
            if (!rightButton.gameObject.activeSelf) return;
            var eventData = new StatusViewEvent(CommandType.RightActor);
            _commandData(eventData);
        }

        private void OnClickDecide()
        {
            if (!_isDisplayDecide) return;
            var eventData = new StatusViewEvent(CommandType.DecideActor);
            _commandData(eventData);
        }

        private void OnClickLvReset()
        {
            if (!_statusViewInfo.DisplayLvResetButton) return;
            var eventData = new StatusViewEvent(CommandType.LvReset);
            _commandData(eventData);
        }
        
        private void CallSkillAction()
        {
            var listData = selectCharacter.ActionData;
            if (listData != null)
            {
                var eventData = new StatusViewEvent(CommandType.SelectSkillAction)
                {
                    template = listData
                };
                _commandData(eventData);
            }
        }


        public void ShowSkillActionList()
        {
            ActivateSkillActionList();
        }

        public int SelectedSkillId()
        {
            var listData = selectCharacter.ActionData;
            if (listData != null)
            {
                return listData.Id;
            }
            return -1;
        }
        
        public void ActivateSkillActionList()
        {
            selectCharacter.MagicList.Activate();
        }

        public void DeactivateSkillActionList()
        {
            selectCharacter.MagicList.Deactivate();
        }
        
        public void CommandRefreshStatus(List<ListData> skillInfos,ActorInfo actorInfo,List<ActorInfo> party,List<ListData> skillTriggerInfos)
        {
            selectCharacter.SetActiveTab(SelectCharacterTabType.Condition,false);
            selectCharacter.SetActiveTab(SelectCharacterTabType.SkillTrigger,false);
            ShowSkillActionList();
            selectCharacter.UpdateStatus(actorInfo);
            selectCharacter.SetActorInfo(actorInfo,party);
            magicList.SetData(skillInfos);
            selectCharacter.SetSkillTriggerList(skillTriggerInfos);
            actorInfoComponent.UpdateInfo(actorInfo,party);
        }

        public void ShowLeaningList(List<ListData> learnMagicList)
        {
            magicList.SetData(learnMagicList);
            magicList.SetInputHandler(InputKeyType.Decide,() => 
            {
                var listData = magicList.ListData;
                if (listData != null && listData.Enable)
                {
                    var data = (SkillInfo)listData.Data;
                    if (data.Enable)
                    {
                        var eventData = new StatusViewEvent(CommandType.LearnMagic)
                        {
                            template = data
                        };
                        _commandData(eventData);
                    }
                }
            });
        }

        public void SetLearnMagicButtonActive(bool IsActive)
        {
            learnMagicBackButton?.gameObject.SetActive(IsActive);
        }

        public void CommandRefresh()
        {
            selectCharacter.RefreshCostInfo();
            if (_isDisplayDecide)
            {
                SetHelpText(_helpText);
                SetHelpInputInfo("SELECT_HEROINE");
            } else
            {
                SetHelpText(DataSystem.GetHelp(202));
                SetHelpInputInfo("STATUS");
            }
        }

        public void InputHandler(InputKeyType keyType,bool pressed)
        {
            switch (keyType)
            {
                case InputKeyType.Cancel:
                    OnClickBack();
                    break;
                case InputKeyType.Option1:
                    OnClickCharacterList();
                    break;
                case InputKeyType.Option2:
                    OnClickLvReset();
                    break;
                case InputKeyType.Start:
                    OnClickDecide();
                    break;
                case InputKeyType.SideLeft1:
                    OnClickLeft();
                    break;
                case InputKeyType.SideRight1:
                    OnClickRight();
                    break;
            }
        }

        public new void MouseCancelHandler()
        {
            var eventData = new StatusViewEvent(CommandType.Back);
            _commandData(eventData);
        }

        private void SetDecideAnimation()
        {
            if (decideAnimation == null) return;
            var rect = decideAnimation.GetComponent<RectTransform>();
            var canvasGroup = decideAnimation.GetComponent<CanvasGroup>();
            var duration = 1f;
            DOTween.Sequence()
                .Append(rect.DOScaleX(1.25f,duration))
                .Join(rect.DOScaleY(1.1f,duration))
                .Join(canvasGroup.DOFade(0,duration))
                .Append(canvasGroup.DOFade(0,duration)
                .SetEase(Ease.InOutQuad))
                .SetLoops(-1);
        }
    }


    public class StatusViewEvent
    {
        public CommandType commandType;
        public object template;

        public StatusViewEvent(CommandType type)
        {
            commandType = type;
        }
    }

    public class StatusViewInfo
    {
        private System.Action _backEvent = null;
        public System.Action BackEvent => _backEvent;
        private bool _displayDecideButton = false;
        public bool DisplayDecideButton => _displayDecideButton;
        private bool _displayBackButton = true;
        public bool DisplayBackButton => _displayBackButton;
        private bool _displayCharacterList = true;
        public bool DisplayCharacterList => _displayCharacterList;
        private bool _displayLvResetButton = false;
        public bool DisplayLvResetButton => _displayLvResetButton;
        private List<BattlerInfo> _enemyInfos = null;
        public List<BattlerInfo> EnemyInfos => _enemyInfos;
        private bool _isBattle = false;
        public bool IsBattle => _isBattle;
        private int _startIndex = -1;
        public int StartIndex => _startIndex;
        
        public StatusViewInfo(System.Action backEvent)
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

        public void SetStartIndex(int actorIndex)
        {
            _startIndex = actorIndex;
        }
    }
}

namespace Status
{
    public enum CommandType
    {
        None = 0,
        DecideActor,
        LeftActor,
        RightActor,
        SelectSkillAction,
        DecideStage,
        CharacterList,
        SelectCharacter,
        SelectCommandList,
        LvReset,
        LevelUp,
        ShowLearnMagic,
        LearnMagic,
        HideLearnMagic,
        Back
    }
}