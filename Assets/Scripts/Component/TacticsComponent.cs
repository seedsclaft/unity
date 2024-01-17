using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TacticsComponent : MonoBehaviour
{    
    private ActorInfo _actorInfo = null;
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



    [SerializeField] private GameObject busyRoot;
    [SerializeField] private TextMeshProUGUI busyText;

    [SerializeField] private List<GameObject> viewObjects;
    public void UpdateInfo(ActorInfo actorInfo,TacticsCommandType tacticsCommandType)
    {
        viewObjects.ForEach(a => a.SetActive(false));
        UpdateViewObjects(tacticsCommandType);

        _actorInfo = actorInfo;
        var currentTacticsCommandType = actorInfo.TacticsCommandType;
        if (actorInfoComponent != null)
        {
            actorInfoComponent.UpdateInfo(actorInfo,null);
        }
        
        if (checkToggle != null)
        {
            checkToggle.SetIsOnWithoutNotify(actorInfo.TacticsCommandType == tacticsCommandType);
        }

        if (afterLv != null && tacticsCommandType == TacticsCommandType.Train)
        {
            afterLv.text = (actorInfo.Level+1).ToString();
        }

        if (trainCost != null)
        {
            trainCost.text = "-" + TacticsUtility.TrainCost(actorInfo).ToString();
        }
        
        if (recoveryCost != null)
        {
            recoveryCost.text = "-" + TacticsUtility.RecoveryCost(actorInfo).ToString();
        }

        if (attributeType != null)
        {
            if (actorInfo.NextLearnSkillId > 0)
            {
                skillInfoComponent.UpdateSkillData(actorInfo.NextLearnSkillId);
            } else{
                skillInfoComponent.Clear();
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
            var enemyData = DataSystem.Enemies.Find(a => a.Id == actorInfo.NextBattleEnemyId);
            enemyInfoComponent.UpdateData(enemyData);
        }


        if (busyRoot != null && busyText != null)
        {
            busyRoot.gameObject.SetActive(false);
            if (tacticsCommandType != actorInfo.TacticsCommandType && actorInfo.TacticsCommandType != TacticsCommandType.None)
            {
                busyRoot.gameObject.SetActive(true);
                var textData = DataSystem.GetTextData((int)actorInfo.TacticsCommandType);
                var subtextData = DataSystem.GetTextData(1020);
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
