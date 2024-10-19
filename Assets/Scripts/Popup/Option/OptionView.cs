using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Option;
using Ryneus;

namespace Ryneus
{
    public class OptionView : BaseView
    {
        private new System.Action<OptionViewEvent> _commandData = null;
        [SerializeField] private BaseList optionCategoryList = null;
        [SerializeField] private BaseList optionList = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        public OptionInfo OptionCommandInfo => optionList.ListItemData<OptionInfo>();
        public int OptionCategoryIndex => optionCategoryList.Index;

        public override void Initialize() 
        {
            base.Initialize();
            optionCategoryList.Initialize();
            optionList.Initialize();
            SetInputHandler(optionList.gameObject);
            SetInputHandler(optionCategoryList.gameObject);
            optionList.Deactivate();
            optionCategoryList.Activate();
            SetBaseAnimation(popupAnimation);
            new OptionPresenter(this);
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }
        
        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }

        public void SetOptionCategoryList(List<ListData> optionData)
        {
            optionCategoryList.SetData(optionData);
            optionCategoryList.SetSelectedHandler(() =>
            {
                var eventData = new OptionViewEvent(CommandType.SelectCategory);
                _commandData(eventData);
            });
            optionCategoryList.SetInputHandler(InputKeyType.Decide,() => CallDecideCategory());
            optionCategoryList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
        }

        public void SetOptionList(List<ListData> optionData)
        {
            optionList.ResetScrollRect();
            optionList.SetData(optionData);
            optionList.SetInputHandler(InputKeyType.Right,() => CallChangeOptionValue(InputKeyType.Right));
            optionList.SetInputHandler(InputKeyType.Left,() => CallChangeOptionValue(InputKeyType.Left));
            optionList.SetInputHandler(InputKeyType.Option1,() => CallChangeOptionValue(InputKeyType.Option1));
            optionList.SetInputHandler(InputKeyType.Decide,() => OnClickOptionList());
            optionList.SetInputHandler(InputKeyType.Cancel,() => CallCancelOptionList());
        }

        private void OnClickOptionList()
        {
            var eventData = new OptionViewEvent(CommandType.SelectOptionList);
            _commandData(eventData);
        }

        public void CommandRefresh()
        {
            if (optionCategoryList.Active)
            {
                SetHelpInputInfo("OPTION_CATEGORY");
            } else
            {
                SetHelpInputInfo("OPTION");
            }
            optionList.UpdateAllItems();
        }

        public void SetHelpWindow()
        {
            SetHelpText(DataSystem.GetHelp(500));
        }

        public void SetEvent(System.Action<OptionViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public new void SetBackEvent(System.Action backEvent)
        {
            SetBackCommand(() => 
            {    
                SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                GameSystem.ConfigData.InputType = GameSystem.TempData.TempInputType;
                if (GameSystem.ConfigData.InputType == false)
                {
                    SetHelpInputInfo("");
                }
                if (backEvent != null) backEvent();
            });
            ChangeBackCommandActive(true);
        }

        public void CallChangeOptionValue(InputKeyType inputKeyType)
        {
            var listData = optionList.ListData;
            if (listData == null)
            {
                return;
            }
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            var optionResultInfo = (OptionInfo)listData.Data;
            var eventData = new OptionViewEvent(CommandType.ChangeOptionValue);
            var optionInfo = new OptionInfo
            {
                OptionCommand = optionResultInfo.OptionCommand,
                keyType = inputKeyType
            };
            eventData.template = optionInfo;
            _commandData(eventData);
        }

        private void CallDecideCategory()
        {
            var eventData = new OptionViewEvent(CommandType.DecideCategory);
            _commandData(eventData);  
        }
        
        public void DecideCategory()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            optionCategoryList.Deactivate();
            optionList.Activate();
            optionList.UpdateSelectIndex(0);
        }

        private void CallCancelOptionList()
        {
            var eventData = new OptionViewEvent(CommandType.CancelOptionList);
            _commandData(eventData);   
        }

        public void CancelOptionList()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
            optionCategoryList.Activate();
            optionList.Deactivate();
            optionList.UpdateSelectIndex(-1);
        }
    }
}

namespace Option
{
    public enum CommandType
    {
        None = 0,
        SelectCategory = 101,
        SelectOptionList = 102,
        CancelOptionList = 103,
        DecideCategory = 104,
        ChangeOptionValue = 2000,
    }
}

public class OptionViewEvent
{
    public CommandType commandType;
    public object template;

    public OptionViewEvent(CommandType type)
    {
        commandType = type;
    }
}

public class PopupInfo
{
    public PopupType PopupType;
    public System.Action EndEvent;
    public object template;
}