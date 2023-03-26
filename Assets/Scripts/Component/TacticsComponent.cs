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

    [SerializeField] private Toggle trainCheckToggle;
    [SerializeField] private TextMeshProUGUI afterLv;
    [SerializeField] private TextMeshProUGUI trainCost;


    [SerializeField] private Toggle alchemyCheckToggle;
    [SerializeField] private SkillInfoComponent skillInfoComponent;
    

    [SerializeField] private Toggle recoveryCheckToggle;
    [SerializeField] private StatusInfoComponent statusInfoComponent;

    [SerializeField] private Toggle battleCheckToggle;
    [SerializeField] private EnemyInfoComponent enemyInfoComponent;

    [SerializeField] private Toggle resourceCheckToggle;
    [SerializeField] private TextMeshProUGUI resourceCost;

    [SerializeField] private GameObject busyRoot;
    [SerializeField] private TextMeshProUGUI busyText;
    public void UpdateInfo(ActorInfo actorInfo)
    {
        _actorInfo = actorInfo;
        TacticsComandType currentTacticsComandType = TacticsComandType.None;
        if (actorInfoComponent != null)
        {
            actorInfoComponent.UpdateInfo(actorInfo);
        }
        
        if (trainCheckToggle != null)
        {
            trainCheckToggle.isOn = (actorInfo.TacticsComandType == TacticsComandType.Train);
            currentTacticsComandType = TacticsComandType.Train;
        }
        if (afterLv != null)
        {
            afterLv.gameObject.SetActive(trainCheckToggle.isOn);
            afterLv.text = (actorInfo.Level+1).ToString();
        }
        if (trainCost != null)
        {
            trainCost.text = TacticsUtility.TrainCost(actorInfo).ToString();
        }

        
        if (alchemyCheckToggle != null)
        {
            alchemyCheckToggle.isOn = (actorInfo.TacticsComandType == TacticsComandType.Alchemy);
            currentTacticsComandType = TacticsComandType.Alchemy;
        }
        if (skillInfoComponent != null)
        {
            skillInfoComponent.UpdateSkillData(actorInfo.NextLearnSkillId);
        }

        
        if (recoveryCheckToggle != null)
        {
            recoveryCheckToggle.isOn = (actorInfo.TacticsComandType == TacticsComandType.Recovery);
            currentTacticsComandType = TacticsComandType.Recovery;
        }
        if (statusInfoComponent != null)
        {
            int Hp = Mathf.Min(actorInfo.CurrentHp + actorInfo.TacticsCost * 10,actorInfo.MaxHp);
            int Mp = Mathf.Min(actorInfo.CurrentMp + actorInfo.TacticsCost * 10,actorInfo.MaxMp);
            statusInfoComponent.UpdateHp(Hp,actorInfo.MaxHp);
            statusInfoComponent.UpdateMp(Mp,actorInfo.MaxMp);
        }

        
        if (battleCheckToggle != null)
        {
            battleCheckToggle.isOn = (actorInfo.TacticsComandType == TacticsComandType.Battle);
            currentTacticsComandType = TacticsComandType.Battle;
        }
        if (enemyInfoComponent != null)
        {
            EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == actorInfo.NextBattleEnemyId);
            enemyInfoComponent.UpdateData(enemyData);
        }

        
        if (resourceCheckToggle != null)
        {
            resourceCheckToggle.isOn = (actorInfo.TacticsComandType == TacticsComandType.Resource);
            currentTacticsComandType = TacticsComandType.Resource;
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
        GameObject toggleObject = null;
        if (trainCheckToggle != null)
        {
	    	toggleObject = trainCheckToggle.gameObject;
        }
        if (alchemyCheckToggle != null)
        {
	    	toggleObject = alchemyCheckToggle.gameObject;
        }
        if (recoveryCheckToggle != null)
        {
	    	toggleObject = recoveryCheckToggle.gameObject;
        }
        if (battleCheckToggle != null)
        {
	    	toggleObject = battleCheckToggle.gameObject;
        }
        if (resourceCheckToggle != null)
        {
	    	toggleObject = resourceCheckToggle.gameObject;
        }
		ContentEnterListener enterListener = toggleObject.AddComponent<ContentEnterListener> ();
        enterListener.SetEnterEvent(() => handler(_actorInfo.ActorId));
    }
}
