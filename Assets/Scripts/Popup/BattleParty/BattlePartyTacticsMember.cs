using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class BattlePartyTacticsMember : ListItem ,IListViewItem 
    {    
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private OnOffButton levelUpButton;
        [SerializeField] private OnOffButton learnMagicButton;
        [SerializeField] private OnOffButton skillTriggerButton;
        [SerializeField] private TextMeshProUGUI trainCost;
        [SerializeField] private TextMeshProUGUI disableText;

        private bool _levelUpHandler = false;
        private bool _learnMagicHandler = false;        
        private bool _skillTriggerHandler = false;

        public void SetLevelUpHandler(System.Action handler)
        {
            if (_levelUpHandler) return;
            _levelUpHandler = true;
            levelUpButton.SetCallHandler(() => handler());
        }

        public void SetLearnMagicHandler(System.Action handler)
        {
            if (_learnMagicHandler) return;
            _learnMagicHandler = true;
            learnMagicButton.SetCallHandler(() => handler());
        }

        public void SetSkillTriggerHandler(System.Action handler)
        {
            if (_skillTriggerHandler) return;
            _skillTriggerHandler = true;
            skillTriggerButton.SetCallHandler(() => handler());
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = (ActorInfo)ListData.Data;
            actorInfoComponent.UpdateInfo(data,null);
            trainCost?.SetText(TacticsUtility.TrainCost(data).ToString() + DataSystem.GetText(1000));
            Disable?.SetActive(!ListData.Enable);
        }
    }
}
