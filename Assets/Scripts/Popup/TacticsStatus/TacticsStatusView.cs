using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TacticsStatus;
using TMPro;

namespace Ryneus
{
    public class TacticsStatusView : BaseView ,IInputHandlerEvent
    {
        [SerializeField] private ActorInfoComponent actorInfoComponent = null;
        [SerializeField] private Button decideButton = null;
        [SerializeField] private OnOffButton characterListButton = null;
        private new System.Action<TacticsStatusViewEvent> _commandData = null;
        [SerializeField] private Button leftButton = null;
        [SerializeField] private Button rightButton = null;
        [SerializeField] private Button helpButton = null;
        [SerializeField] private GameObject decideAnimation = null;
        [SerializeField] private MagicList magicList = null;
        [SerializeField] private BaseList commandList = null;
        [SerializeField] private TextMeshProUGUI numinousText = null;
        [SerializeField] private StatusLevelUp statusLevelUp = null;
        
        [SerializeField] private StatusAnimation statusAnimation = null;
        [SerializeField] private GameObject leftRoot = null;
        [SerializeField] private GameObject rightRoot = null;

        public SkillInfo SelectMagic => (SkillInfo)magicList.ListData?.Data;
        private StatusViewInfo _statusViewInfo = null; 

        private System.Action _backEvent = null;
        private bool _isDisplayDecide => _statusViewInfo != null && _statusViewInfo.DisplayDecideButton;
        private string _helpText;
        private bool _isDisplayLevelObj => _statusViewInfo != null && _statusViewInfo.DisplayLvResetButton;
        private bool _isDisplayBack => _statusViewInfo != null && _statusViewInfo.DisplayBackButton;
        public void Initialize(List<ActorInfo> actorInfos) 
        {
            base.Initialize();
            InitializeSelectCharacter();
            
            SetDecideAnimation();
            magicList.Initialize();
            SetInputHandler(magicList.gameObject);
            commandList.Initialize();
            SetInputHandler(commandList.gameObject);

            statusLevelUp.Initialize();
            
            SetBaseAnimation(statusAnimation);
            new TacticsStatusPresenter(this,actorInfos);
        }

        public void OpenAnimation()
        {
            statusAnimation.OpenAnimation(UiRoot.transform,null);
            statusAnimation.LeftAnimation(leftRoot.transform,null);
            statusAnimation.RightAnimation(rightRoot.transform,null);
        }

        private void InitializeSelectCharacter()
        {
        }
        
        public void SetUIButton(List<ListData> commandListData)
        {
            leftButton.onClick.AddListener(() => OnClickLeft());
            rightButton.onClick.AddListener(() => OnClickRight());
            decideButton.onClick.AddListener(() => OnClickDecide());
            helpButton.onClick.AddListener(() => OnClickHelp());
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

        public void SetEvent(System.Action<TacticsStatusViewEvent> commandData)
        {
            _commandData = commandData;
            //statusLevelUp.SetEvent(commandData);
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
                var eventData = new TacticsStatusViewEvent(CommandType.SelectCharacter)
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
                    var eventData = new TacticsStatusViewEvent(CommandType.SelectCommandList)
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
        }

        private void DisplayDecideButton()
        {
            statusLevelUp.SetActive(_isDisplayLevelObj);
            decideButton.gameObject.SetActive(_isDisplayDecide);
            commandList.gameObject.SetActive(_isDisplayLevelObj);
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
        }

        public void SetNuminous(int numinous)
        {
            numinousText.SetText(DataSystem.GetReplaceDecimalText(numinous));
        }

        public void SetLvUpCost(int cost)
        {
            statusLevelUp.SetLvUpCost(cost);
        }

        public void SetToLvText(int current)
        {
            statusLevelUp.ToLvText(current);
        }

        private void DisplayCharacterList(bool isDisplay)
        {
            characterListButton.gameObject.SetActive(isDisplay);
        }

        private void OnClickBack()
        {
            var eventData = new TacticsStatusViewEvent(CommandType.Back);
            _commandData(eventData);
        }

        private void OnClickCharacterList()
        {
            var eventData = new TacticsStatusViewEvent(CommandType.CharacterList);
            _commandData(eventData);
        }

        private void OnClickLeft()
        {
            if (!leftButton.gameObject.activeSelf) return;
            var eventData = new TacticsStatusViewEvent(CommandType.LeftActor);
            _commandData(eventData);
        }

        private void OnClickRight()
        {
            if (!rightButton.gameObject.activeSelf) return;
            var eventData = new TacticsStatusViewEvent(CommandType.RightActor);
            _commandData(eventData);
        }

        private void OnClickDecide()
        {
            if (!_isDisplayDecide) return;
            var eventData = new TacticsStatusViewEvent(CommandType.DecideActor);
            _commandData(eventData);
        }

        private void OnClickHelp()
        {
            var eventData = new TacticsStatusViewEvent(CommandType.CallHelp);
            _commandData(eventData);
        }

        private void OnClickLvReset()
        {
            if (!_statusViewInfo.DisplayLvResetButton) return;
            var eventData = new TacticsStatusViewEvent(CommandType.LvReset);
            _commandData(eventData);
        }
        
        private void CallSkillAction()
        {
        }


        public void ShowSkillActionList()
        {
            ActivateSkillActionList();
        }

        public int SelectedSkillId()
        {
            return -1;
        }
        
        public void ActivateSkillActionList()
        {
        }

        public void DeactivateSkillActionList()
        {
        }
        
        public void CommandRefreshTacticsStatus(List<ListData> skillInfos,ActorInfo actorInfo,List<ActorInfo> party,List<ListData> skillTriggerInfos)
        {
            ShowSkillActionList();
            magicList.SetData(skillInfos);
            actorInfoComponent.UpdateInfo(actorInfo,party);
            _statusViewInfo?.CharaLayerEvent?.Invoke(actorInfo.ActorId);
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
                        var eventData = new TacticsStatusViewEvent(CommandType.LearnMagic)
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
            statusLevelUp.SetLearnMagicButtonActive(IsActive);
        }

        public void CommandRefresh()
        {
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
            var eventData = new TacticsStatusViewEvent(CommandType.Back);
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


    public class TacticsStatusViewEvent
    {
        public CommandType commandType;
        public object template;

        public TacticsStatusViewEvent(CommandType type)
        {
            commandType = type;
        }
    }
}

namespace TacticsStatus
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
        CallHelp,
        Back
    }
}