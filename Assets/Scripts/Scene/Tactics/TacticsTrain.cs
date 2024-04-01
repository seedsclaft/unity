using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        public void SetToggleHandler(System.Action handler)
        {
            checkToggle.onValueChanged.AddListener((a) => handler());
        }    
        
        public void SetBattleFrontToggleHandler(System.Action handler)
        {
            battlePositionToggles[0].onValueChanged.AddListener((a) => handler());
        }

        public void SetBattleBackToggleHandler(System.Action handler)
        {
            battlePositionToggles[1].onValueChanged.AddListener((a) => handler());
        }

        public void SetSkillTriggerHandler(System.Action handler)
        {
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
            var data = (TacticsActorInfo)ListData.Data;
            tacticsComponent.UpdateInfo(data.ActorInfo,data.TacticsCommandType);
            Disable.SetActive(false);
        }
    }
}