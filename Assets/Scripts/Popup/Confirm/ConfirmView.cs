using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Confirm;

namespace Ryneus
{
    public class ConfirmView : BaseView,IInputHandlerEvent
    {
        [SerializeField] private BaseList commandList = null;
        [SerializeField] private TextMeshProUGUI titleText = null;
        [SerializeField] private BaseList skillInfoList = null;
        [SerializeField] private ConfirmAnimation confirmAnimation = null;
        private System.Action<ConfirmCommandType> _confirmEvent = null;
        private new System.Action<ConfirmViewEvent> _commandData = null;
        private ConfirmInfo _confirmInfo = null;

        public override void Initialize() 
        {
            base.Initialize();
            commandList.Initialize();
            skillInfoList.Initialize();
            SetBaseAnimation(confirmAnimation);
            new ConfirmPresenter(this);
        }

        public void OpenAnimation()
        {
            confirmAnimation.OpenAnimation(UiRoot.transform,null);
        }
        
        public void SetTitle(string title)
        {
            titleText?.SetText(title);
        }

        public void SetSkillInfo(List<ListData> skillInfos)
        {
            if (skillInfos == null) return;
            skillInfoList.SetData(skillInfos);
        }

        public void SetIsNoChoice(bool isNoChoice)
        {
            var commandType = isNoChoice ? CommandType.IsNoChoice : CommandType.IsChoice;
            var eventData = new ConfirmViewEvent(commandType);
            _commandData(eventData);
        }

        public void SetDisableIds(List<int> disableIds)
        {
            if (disableIds.Count > 0)
            {
                var eventData = new ConfirmViewEvent(CommandType.DisableIds)
                {
                    template = disableIds
                };
                _commandData(eventData);
            }
        }

        public void SetSelectIndex(int selectIndex)
        {
            commandList.Refresh(selectIndex);
        }

        public void SetConfirmEvent(System.Action<ConfirmCommandType> commandData)
        {
            _confirmEvent = commandData;
        }

        public void SetViewInfo(ConfirmInfo confirmInfo)
        {
            _confirmInfo = confirmInfo;
            SetIsNoChoice(confirmInfo.IsNoChoice);
            SetSelectIndex(confirmInfo.SelectIndex);
            SetTitle(confirmInfo.Title);
            SetSkillInfo(confirmInfo.SkillInfos());
            SetConfirmEvent(confirmInfo.CallEvent);
            SetDisableIds(confirmInfo.DisableIds);
        }

        public void SetEvent(System.Action<ConfirmViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetConfirmCommand(List<ListData> menuCommands)
        {
            commandList.SetData(menuCommands);
            commandList.SetInputHandler(InputKeyType.Decide,() => CallConfirmCommand());
            SetInputHandler(commandList.GetComponent<IInputHandlerEvent>());
        }

        public void CommandDisableIds(List<int> disableIds)
        {
            commandList.SetDisableIds(disableIds);
        }

        private void CallConfirmCommand()
        {
            var data = (SystemData.CommandData)commandList.ListData.Data;
            if (data != null)
            {
                var commandType = data.Key == "Yes" ? ConfirmCommandType.Yes : ConfirmCommandType.No;
                if (data.Key == "Yes")
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Decide);
                } else
                {
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
                _confirmEvent(commandType);
                BackEvent();
            }
        }

        public void InputHandler(InputKeyType keyType,bool pressed)
        {

        }

        public new void MouseCancelHandler()
        {
            if (_confirmInfo.IsNoChoice)
            {
                CallConfirmCommand();
            } else
            {
                CallConfirmCommand();
            }
        }
    }
}

namespace Confirm
{
    public enum CommandType
    {
        None = 0,
        IsChoice = 100,
        IsNoChoice = 101,
        DisableIds = 102,
    }
}
public class ConfirmViewEvent
{
    public Confirm.CommandType commandType;
    public object template;

    public ConfirmViewEvent(Confirm.CommandType type)
    {
        commandType = type;
    }
}