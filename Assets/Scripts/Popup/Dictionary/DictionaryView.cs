using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class DictionaryView : BaseView
    {
        [SerializeField] private BaseList categoryList = null;
        [SerializeField] private MagicList magicList = null;
        [SerializeField] private TextMeshProUGUI completeRateText = null;
        [SerializeField] private PopupAnimation popupAnimation = null;
        private new System.Action<DictionaryViewEvent> _commandData = null;
        
        public override void Initialize() 
        {
            base.Initialize();
            SetBaseAnimation(popupAnimation);
            categoryList.Initialize();
            categoryList.SetSelectedHandler(() => 
            {
                if (categoryList.ListData != null)
                {
                    var eventData = new DictionaryViewEvent(Dictionary.CommandType.SelectCategory)
                    {
                        template = categoryList.ListItemData<SkillType>()
                    };
                    _commandData(eventData);
                }
            });
            magicList.Initialize();
            new DictionaryPresenter(this);
        }
        
        public void OpenAnimation()
        {
            popupAnimation.OpenAnimation(UiRoot.transform,null);
        }

        public void SetCategoryList(List<ListData> categorySkillList)
        {
            categoryList.SetData(categorySkillList);
        }
        
        public void SetMagicList(List<ListData> categorySkillList)
        {
            magicList.SetData(categorySkillList);
        }
        
        public void SetEvent(System.Action<DictionaryViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetCompleteRateText(int value)
        {
            completeRateText?.SetText(value.ToString());
        }
    }

    namespace Dictionary
    {
        public enum CommandType
        {
            None = 0,
            SelectCategory,
        }
    }

    public class DictionaryViewEvent
    {
        public Dictionary.CommandType commandType;
        public object template;

        public DictionaryViewEvent(Dictionary.CommandType type)
        {
            commandType = type;
        }
    }
}