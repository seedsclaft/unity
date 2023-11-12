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
    
    [SerializeField] private SkillInfoComponent skillInfoComponent;


    [SerializeField] private StatusInfoComponent statusInfoComponent;

    [SerializeField] private EnemyInfoComponent enemyInfoComponent;


    [SerializeField] private TextMeshProUGUI resourceCost;

    [SerializeField] private GameObject busyRoot;
    [SerializeField] private TextMeshProUGUI busyText;

    [SerializeField] private List<GameObject> viewObjects;
    public void UpdateInfo(ActorInfo actorInfo,TacticsCommandType tacticsCommandType)
    {
        viewObjects.ForEach(a => a.SetActive(false));
        UpdateViewObjects(tacticsCommandType);

        _actorInfo = actorInfo;
        TacticsCommandType currentTacticsCommandType = actorInfo.TacticsCommandType;
        if (actorInfoComponent != null)
        {
            actorInfoComponent.UpdateInfo(actorInfo,null);
        }
        
        if (checkToggle != null)
        {
            checkToggle.isOn = (actorInfo.TacticsCommandType == tacticsCommandType);
        }

        if (afterLv != null && tacticsCommandType == TacticsCommandType.Train)
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
            if (actorInfo.NextLearnSkillId > 0)
            {
                skillInfoComponent.UpdateSkillData(actorInfo.NextLearnSkillId);
                //attributeType.text = DataSystem.Skills.Find(a => a.Id == actorInfo.NextLearnSkillId).Name;
            } else{
                skillInfoComponent.Clear();
                //attributeType.text = "";
            }
        }

        if (attributeLearnCost != null)
        {
            attributeLearnCost.gameObject.SetActive(actorInfo.NextLearnCost > 0);
            if (actorInfo.NextLearnCost > 0)
            {
                attributeLearnCost.text = actorInfo.NextLearnCost.ToString();
            } else
            {
                attributeLearnCost.text = "";
            }
        }
        
        if (statusInfoComponent != null)
        {
            int RecoveryCost = 0;
            if (actorInfo.TacticsCommandType == TacticsCommandType.Recovery)
            {
                RecoveryCost = actorInfo.TacticsCost;
            }
            int Hp = Mathf.Min(actorInfo.CurrentHp + RecoveryCost * 10,actorInfo.MaxHp);
            int Mp = Mathf.Min(actorInfo.CurrentMp + RecoveryCost * 10,actorInfo.MaxMp);
            statusInfoComponent.UpdateHp(Hp,actorInfo.MaxHp);
            statusInfoComponent.UpdateMp(Mp,actorInfo.MaxMp);
        }

        if (enemyInfoComponent != null)
        {
            EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == actorInfo.NextBattleEnemyId);
            enemyInfoComponent.UpdateData(enemyData);
        }

        if (resourceCost != null)
        {
            resourceCost.gameObject.SetActive(actorInfo.TacticsCommandType == TacticsCommandType.Resource);
            resourceCost.text = "+" + TacticsUtility.ResourceGain(actorInfo).ToString();
        }

        if (busyRoot != null && busyText != null)
        {
            busyRoot.gameObject.SetActive(false);
            if (tacticsCommandType != actorInfo.TacticsCommandType && actorInfo.TacticsCommandType != TacticsCommandType.None)
            {
                busyRoot.gameObject.SetActive(true);
                TextData textData = DataSystem.System.GetTextData((int)actorInfo.TacticsCommandType);
                TextData subtextData = DataSystem.System.GetTextData(1020);
                busyText.text = textData.Text + subtextData.Text;
            }
        }
    }

    public void SetToggleHandler(System.Action<int> handler){
        //checkToggle.onValueChanged.AddListener((a) => handler(_actorInfo.ActorId));
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
}
