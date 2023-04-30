using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;
using UnityEngine.EventSystems;

public class TacticsComponent : MonoBehaviour
{    
    private ActorInfo _actorInfo = null;
    [SerializeField] private ActorInfoComponent actorInfoComponent;

    [SerializeField] private Toggle checkToggle;
    [SerializeField] private TextMeshProUGUI afterLv;
    [SerializeField] private TextMeshProUGUI trainCost;


    [SerializeField] private TextMeshProUGUI attributeType;
    [SerializeField] private TextMeshProUGUI attributeLearnCost;
    


    [SerializeField] private StatusInfoComponent statusInfoComponent;

    [SerializeField] private EnemyInfoComponent enemyInfoComponent;


    [SerializeField] private TextMeshProUGUI resourceCost;

    [SerializeField] private GameObject busyRoot;
    [SerializeField] private TextMeshProUGUI busyText;
    public void UpdateInfo(ActorInfo actorInfo,TacticsComandType tacticsComandType)
    {
        _actorInfo = actorInfo;
        TacticsComandType currentTacticsComandType = actorInfo.TacticsComandType;
        if (actorInfoComponent != null)
        {
            actorInfoComponent.UpdateInfo(actorInfo,null);
        }
        
        if (checkToggle != null)
        {
            checkToggle.isOn = (actorInfo.TacticsComandType == tacticsComandType);
        }

        if (afterLv != null && tacticsComandType == TacticsComandType.Train)
        {
            afterLv.gameObject.SetActive(checkToggle.isOn);
            afterLv.text = (actorInfo.Level+1).ToString();
        }

        if (trainCost != null)
        {
            trainCost.text = TacticsUtility.TrainCost(actorInfo).ToString();
        }
        
        if (attributeType != null)
        {
            attributeType.text = actorInfo.NextLearnAttribute.ToString();
        }

        if (attributeLearnCost != null)
        {
            attributeLearnCost.text = actorInfo.NextLearnCost.ToString();
        }
        
        if (statusInfoComponent != null)
        {
            int Hp = Mathf.Min(actorInfo.CurrentHp + actorInfo.TacticsCost * 10,actorInfo.MaxHp);
            int Mp = Mathf.Min(actorInfo.CurrentMp + actorInfo.TacticsCost * 10,actorInfo.MaxMp);
            statusInfoComponent.UpdateHp(Hp,actorInfo.MaxHp);
            statusInfoComponent.UpdateMp(Mp,actorInfo.MaxMp);
        }

        if (enemyInfoComponent != null)
        {
            EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == actorInfo.NextBattleEnemyId);
            enemyInfoComponent.UpdateData(enemyData);
        }

        if (resourceCost != null)
        {
            resourceCost.gameObject.SetActive(actorInfo.TacticsComandType == TacticsComandType.Resource);
            resourceCost.text = TacticsUtility.ResourceCost(actorInfo).ToString();
        }

        if (busyRoot != null && busyText != null)
        {
            busyRoot.gameObject.SetActive(false);
            if (currentTacticsComandType != actorInfo.TacticsComandType && actorInfo.TacticsComandType != TacticsComandType.None)
            {
                busyRoot.gameObject.SetActive(true);
                TextData textData = DataSystem.System.GetTextData((int)actorInfo.TacticsComandType);
                TextData subtextData = DataSystem.System.GetTextData(1020);
                busyText.text = textData.Text + subtextData.Text;
            }
        }
    }

    public void SetToggleHandler(System.Action<int> handler){
        checkToggle.onValueChanged.AddListener((a) => handler(_actorInfo.ActorId));
    }
}
