using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class TacticsComponent : MonoBehaviour
    {    
        [SerializeField] private ActorInfoComponent actorInfoComponent;

        [SerializeField] private Toggle checkToggle;
        [SerializeField] private TextMeshProUGUI afterLv;
        [SerializeField] private TextMeshProUGUI trainCost;


        [SerializeField] private TextMeshProUGUI recoveryCost;
        [SerializeField] private TextMeshProUGUI attributeType;
        [SerializeField] private TextMeshProUGUI attributeLearnCost;
        
        [SerializeField] private SkillInfoComponent skillInfoComponent;


        [SerializeField] private StatusInfoComponent statusInfoComponent;

        [SerializeField] private EnemyInfoComponent enemyInfoComponent;
        [SerializeField] private TextMeshProUGUI battleIndex;
        [SerializeField] private List<Toggle> battlePositionToggles;



        [SerializeField] private GameObject busyRoot;
        [SerializeField] private TextMeshProUGUI busyText;

        [SerializeField] private List<GameObject> viewObjects;
        public void UpdateInfo(ActorInfo actorInfo,TacticsCommandType tacticsCommandType)
        {
            viewObjects.ForEach(a => a.SetActive(false));
            UpdateViewObjects(tacticsCommandType);

            if (actorInfoComponent != null)
            {
                actorInfoComponent.UpdateInfo(actorInfo,null);
            }
            
            if (checkToggle != null)
            {
                if (tacticsCommandType == TacticsCommandType.Paradigm)
                {
                    checkToggle.SetIsOnWithoutNotify(actorInfo.BattleIndex >= 0);
                } else
                {
                    checkToggle.SetIsOnWithoutNotify(false);
                }
            }

            if (afterLv != null && tacticsCommandType == TacticsCommandType.Train)
            {
                afterLv.text = (actorInfo.LinkedLevel()+1).ToString();
            }

            if (trainCost != null)
            {
                trainCost.text = TacticsUtility.TrainCost(actorInfo).ToString();
            }
            
            if (recoveryCost != null)
            {
                recoveryCost.text = "-" + TacticsUtility.RecoveryCost(actorInfo).ToString();
            }

            if (attributeType != null)
            {
                skillInfoComponent.Clear();
            }

            if (battleIndex != null)
            {
                if (actorInfo.BattleIndex >= 0)
                {
                    battleIndex.SetText(BattleIndexText(actorInfo.BattleIndex));
                } else
                { 
                    battleIndex.SetText("");
                }
            }

            if (attributeLearnCost != null)
            {
                attributeLearnCost.SetText("");
            }
            
            if (statusInfoComponent != null)
            {
            }

            if (enemyInfoComponent != null)
            {
            }

            if (battlePositionToggles.Count > 0)
            {
                var lineIndex = actorInfo.LineIndex;
                for (int i = 0; i < battlePositionToggles.Count; i++)
                {
                    battlePositionToggles[i].SetIsOnWithoutNotify((int)lineIndex == i);            
                }
            }
        }

        private void UpdateViewObjects(TacticsCommandType tacticsCommandType)
        {
            var idx = 1;
            foreach (var viewObject in viewObjects)
            {
                viewObject.SetActive((int)tacticsCommandType == idx);
                idx++;
            }
        }

        private string BattleIndexText(int battleIndex)
        {
            return DataSystem.System.GetTextData(battleIndex + 19600 - 1).Text;
        }
    }
}