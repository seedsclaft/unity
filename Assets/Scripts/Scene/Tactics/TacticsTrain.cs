using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class TacticsTrain : ListItem ,IListViewItem 
    {
        [SerializeField] private TacticsComponent tacticsComponent;
        [SerializeField] private Toggle checkToggle;
        [SerializeField] private Button skillTriggerButton;
        [SerializeField] private Button plusButton;
        [SerializeField] private Button minusButton;
        [SerializeField] private List<Toggle> battlePositionToggles;
        [SerializeField] private TextMeshProUGUI disableText;

        private bool _setToggleHandler = false;
        private bool _setBattleFrontToggleHandler = false;
        private bool _setBattleBackToggleHandler = false;
        private bool _setSkillTriggerHandler = false;

        public void SetToggleHandler(System.Action handler)
        {
            if (_setToggleHandler) return;
            _setToggleHandler = true;
            checkToggle.onValueChanged.AddListener((a) => handler());
        }    
        
        public void SetBattleFrontToggleHandler(System.Action handler)
        {
            if (_setBattleFrontToggleHandler) return;
            _setBattleFrontToggleHandler = true;
            battlePositionToggles[0].onValueChanged.AddListener((a) => handler());
        }

        public void SetBattleBackToggleHandler(System.Action handler)
        {
            if (_setBattleBackToggleHandler) return;
            _setBattleBackToggleHandler = true;
            battlePositionToggles[1].onValueChanged.AddListener((a) => handler());
        }

        public void SetSkillTriggerHandler(System.Action handler)
        {
            if (_setSkillTriggerHandler) return;
            _setSkillTriggerHandler = true;
            skillTriggerButton.onClick.AddListener(() => handler());
        }

        public void SetPlusHandler(System.Action handler)
        {
            plusButton.onClick.AddListener(() => handler());
        }

        public void SetMinusHandler(System.Action handler)
        {
            minusButton.onClick.AddListener(() => handler());
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<TacticsActorInfo>();
            tacticsComponent.UpdateInfo(data.ActorInfo,data.TacticsCommandType);
            Disable?.SetActive(!ListData.Enable);
            if (data.DisableText != null)
            {
                disableText?.SetText(data.DisableText);
            }
        }
    }
}