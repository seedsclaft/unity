using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    abstract public class BaseView : MonoBehaviour
    {
        private bool _testMode = false;
        public bool TestMode => _testMode;
        private bool _testBattleMode = false;
        public bool TestBattleMode => _testBattleMode;
        private InputSystem _input;
        private List<IInputHandlerEvent> _inputHandler = new List<IInputHandlerEvent>();
        private bool _busy = false;
        public bool Busy => _busy;
        public System.Action<ViewEvent> _commandData = null;
        [SerializeField] private Button _backCommand = null;
        [SerializeField] private SpriteRenderer _backGround = null;
        private System.Action _backEvent = null;
        public System.Action BackEvent => _backEvent;
        [SerializeField] private GameObject uiRoot = null;
        [SerializeField] private Button optionButton = null;
        [SerializeField] private Button sideMenuButton = null;
        public Button SideMenuButton => sideMenuButton;

        private HelpWindow _helpWindow = null;
        public HelpWindow HelpWindow => _helpWindow;
        public void SetHelpInputInfo(string key)
        {
            _helpWindow?.SetInputInfo(key);
        }

        public void SetHelpText(string text)
        {
            _helpWindow?.SetHelpText(text);
        }

        public void SetBackGround(string fileName)
        {
            _backGround.sprite = ResourceSystem.LoadBackGround(fileName);
        }

        private int _inputBusyFrame = 0;
        private InputKeyType _lastInputKey = InputKeyType.None;
        private int _pressedFrame = 0;
        readonly int _pressFrame = 30;
        public void SetInputFrame(int frame)
        {
            _inputBusyFrame = frame;
        }

        public virtual void Initialize()
        {
            InitializeInput();
            SetInputHandler(gameObject.GetComponent<IInputHandlerEvent>());
            if (optionButton != null)
            {
                optionButton.onClick.AddListener(() => {
                    CommandCallOption(() => {
                        CommandGameSystem(Base.CommandType.ClosePopup);
                    });
                });
            }
        }

        public void InitializeInput()
        {    
            _input = new InputSystem();
        }

        public void SetHelpWindow(HelpWindow helpWindow)
        {
            _helpWindow = helpWindow;
        }

        public void SetInputHandler(IInputHandlerEvent handler)
        {
            _inputHandler.Add(handler);
        }

        private void InputHandler(InputKeyType keyType,bool pressed)
        {
            if (_busy) return;
            for (int i = _inputHandler.Count-1;i >= 0;i--)
            {
                if (_inputHandler[i] != null && _inputBusyFrame < 0){
                    _inputHandler[i].InputHandler(keyType,pressed);
                }
            }
        }

        private void CallMouseCancel()
        {
            if (_busy) return;
            foreach (var handler in _inputHandler)
            {
                if (handler != null){
                    handler.MouseCancelHandler();
                }
            }
        }

        public void SetBusy(bool isBusy)
        {
            _busy = isBusy;
        }

        public void Update()
        {
            if (_input != null)
            {
                var keyType = _input.Update();
                if (_lastInputKey != keyType)
                {
                    _lastInputKey = keyType;
                    _pressedFrame = 0;
                } else
                {
                    if (_lastInputKey == keyType)
                    {
                        _pressedFrame += 1;
                    }
                }
                InputHandler(keyType,_pressedFrame > _pressFrame);
                if (InputSystem.IsMouseRightButtonDown())
                {
                    CallMouseCancel();
                }
            }
            UpdateInputFrame();
        }

        private void UpdateInputFrame()
        {
            if (_inputBusyFrame >= 0)
            {
                _inputBusyFrame--;
            }
        }

        public void CommandOpenSideMenu()
        {
            _helpWindow.SetInputInfo("SIDEMENU");
            _helpWindow.SetHelpText(DataSystem.GetHelp(701));
        }

        public void SetEvent(System.Action<ViewEvent> commandData)
        {
            _commandData = commandData;
        }

        private void CallSceneChangeCommand(ViewEvent eventData)
        {
            _commandData(eventData);
        }

        public void CommandGameSystem(Base.CommandType commandType)
        {
            var eventData = new ViewEvent(commandType);
            CallSceneChangeCommand(eventData);
        }

        public void CommandSceneChange(Scene scene,object sceneParam = null,SceneChangeType sceneChangeType = SceneChangeType.Push)
        {
            var eventData = new ViewEvent(Base.CommandType.SceneChange);
            var sceneInfo = new SceneInfo(){ToScene = scene,SceneChangeType = sceneChangeType,SceneParam = sceneParam};
            eventData.template = sceneInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandPopSceneChange(object sceneParam = null)
        {
            var eventData = new ViewEvent(Base.CommandType.SceneChange);
            var sceneInfo = new SceneInfo(){SceneChangeType = SceneChangeType.Pop,SceneParam = sceneParam};
            eventData.template = sceneInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandGotoSceneChange(Scene scene,object sceneParam = null)
        {
            var eventData = new ViewEvent(Base.CommandType.SceneChange);
            var sceneInfo = new SceneInfo(){ToScene = scene,SceneChangeType = SceneChangeType.Goto,SceneParam = sceneParam};
            eventData.template = sceneInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallConfirm(ConfirmInfo popupInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallConfirmView);
            eventData.template = popupInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallSkillDetail(ConfirmInfo popupInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallSkillDetailView);
            eventData.template = popupInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallCaution(CautionInfo popupInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallCautionView);
            eventData.template = popupInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallPopup(PopupInfo popupInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallPopupView);
            eventData.template = popupInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallOption(System.Action endEvent)
        {
            var eventData = new ViewEvent(Base.CommandType.CallOptionView);
            eventData.template = endEvent;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallSideMenu(SideMenuViewInfo sideMenuViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallSideMenu);
            eventData.template = sideMenuViewInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallRanking(RankingViewInfo rankingViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallRankingView);
            eventData.template = rankingViewInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallCharacterList(CharacterListInfo characterListInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallCharacterListView);
            eventData.template = characterListInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandHelpList(List<ListData> helpTextList)
        {
            var eventData = new ViewEvent(Base.CommandType.CallHelpView);
            eventData.template = helpTextList;
            CallSceneChangeCommand(eventData);
        }

        public void CommandSlotSave(SlotSaveViewInfo slotSaveViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallSlotSaveView);
            eventData.template = slotSaveViewInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallStatus(StatusViewInfo statusViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallStatusView);
            eventData.template = statusViewInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallEnemyInfo(StatusViewInfo statusViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallEnemyInfoView);
            eventData.template = statusViewInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallSkillTrigger(SkillTriggerViewInfo skillTriggerViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallSkillTriggerView);
            eventData.template = skillTriggerViewInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallSkillLog(SkillLogViewInfo skillLogViewInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallSkillLogView);
            eventData.template = skillLogViewInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallAdv(AdvCallInfo advCallInfo)
        {
            var eventData = new ViewEvent(Base.CommandType.CallAdvScene);
            eventData.template = advCallInfo;
            CallSceneChangeCommand(eventData);
        }

        public void CommandDecidePlayerName(string nameText)
        {
            var eventData = new ViewEvent(Base.CommandType.DecidePlayerName);
            eventData.template = nameText;
            CallSceneChangeCommand(eventData);
        }

        public void CommandSetRouteSelect()
        {
            var eventData = new ViewEvent(Base.CommandType.SetRouteSelect);
            CallSceneChangeCommand(eventData);
        }

        public void CommandChangeViewToTransition(System.Action<string> endEvent)
        {
            var eventData = new ViewEvent(Base.CommandType.ChangeViewToTransition);
            eventData.template = endEvent;
            CallSceneChangeCommand(eventData);
        }

        public void CommandStartTransition(System.Action endEvent)
        {
            var eventData = new ViewEvent(Base.CommandType.StartTransition);
            eventData.template = endEvent;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCallTutorialFocus(StageTutorialData stageTutorialDate)
        {
            var eventData = new ViewEvent(Base.CommandType.CallTutorialFocus);
            eventData.template = stageTutorialDate;
            CallSceneChangeCommand(eventData);
        }

        public void CommandCloseTutorialFocus()
        {
            var eventData = new ViewEvent(Base.CommandType.CloseTutorialFocus);
            CallSceneChangeCommand(eventData);
        }

        public void CommandSceneShowUI()
        {
            var eventData = new ViewEvent(Base.CommandType.SceneShowUI);
            CallSceneChangeCommand(eventData);
        }

        public void CommandSceneHideUI()
        {
            var eventData = new ViewEvent(Base.CommandType.SceneHideUI);
            CallSceneChangeCommand(eventData);
        }

        public void SetBackCommand(System.Action callEvent)
        {
            if (_backCommand != null)
            {
                _backCommand.onClick.AddListener(() => {        
                    if (!_backCommand.gameObject.activeSelf) return;
                    callEvent();
                });
            }
            _backEvent = callEvent;
        }

        public void SetBackEvent(System.Action backEvent)
        {
            SetBackCommand(() => 
            {    
                if (backEvent != null) backEvent();
            });
            ChangeBackCommandActive(true);
        }
        
        public void ChangeBackCommandActive(bool IsActive)
        {
            _backCommand?.gameObject.SetActive(IsActive);
        }

        public void ChangeUIActive(bool IsActive)
        {
            uiRoot.gameObject.SetActive(IsActive);
        }

        public void SetTestMode(bool isTest)
        {
            _testMode = isTest;
        }

        public void SetBattleTestMode(bool isTest)
        {
            _testBattleMode = isTest;
        }

        public void MouseCancelHandler()
        {

        }

        private void OnDestroy() {
            var listViews = GetComponentsInChildren<ListWindow>();
            for (int i = listViews.Length-1;i >= 0;i--)
            {
                listViews[i].Release();
            }
        }

    }

    namespace Base
    {
        public enum CommandType
        {
            None = 0,
            SceneChange,
            CallConfirmView,
            CallSkillDetailView,
            CallCautionView,
            CallPopupView,
            ClosePopup,
            CloseConfirm,
            CallOptionView,
            CallSideMenu,
            CallRankingView,
            CallCharacterListView,
            CallHelpView,
            CallSlotSaveView,
            CallStatusView,
            CloseStatus,
            CallAdvScene,
            CallEnemyInfoView,
            CallSkillTriggerView,
            CallSkillLogView,
            DecidePlayerName,
            CallLoading,
            CloseLoading,
            SetRouteSelect,
            ChangeViewToTransition,
            StartTransition,
            CallTutorialFocus,
            CloseTutorialFocus,
            SceneShowUI,
            SceneHideUI
        }
    }

    public class BaseViewEvent
    {
        public Base.CommandType commandType;
        public Scene template;

        public BaseViewEvent(Base.CommandType type)
        {
            commandType = type;
        }
    }
}