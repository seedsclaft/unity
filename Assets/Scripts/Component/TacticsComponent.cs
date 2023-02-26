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

    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    public void UpdateInfo(ActorInfo actorInfo)
    {
        _actorInfo = actorInfo;
        if (actorInfoComponent != null)
        {
            actorInfoComponent.UpdateInfo(actorInfo);
        }
        
        if (trainCheckToggle != null)
        {
            trainCheckToggle.isOn = (actorInfo.TacticsComandType == TacticsComandType.Train);
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
        }
        if (skillInfoComponent != null)
        {
            skillInfoComponent.UpdateSkillData(actorInfo.NextLearnSkillId);
        }

        
        if (recoveryCheckToggle != null)
        {
            recoveryCheckToggle.isOn = (actorInfo.TacticsComandType == TacticsComandType.Recovery);
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
        }
        if (enemyInfoComponent != null)
        {
            EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == actorInfo.NextBattleEnemyIndex);
            enemyInfoComponent.UpdateData(enemyData);
        }

        
        if (resourceCheckToggle != null)
        {
            resourceCheckToggle.isOn = (actorInfo.TacticsComandType == TacticsComandType.Resource);
        }
        if (resourceCost != null)
        {
            resourceCost.gameObject.SetActive(actorInfo.TacticsComandType == TacticsComandType.Resource);
            resourceCost.text = TacticsUtility.ResourceCost(actorInfo).ToString();
        }
    }

    public void SetToggleHandler(System.Action<int> handler){
        if (trainCheckToggle != null)
        {
            //　EventTriggerコンポーネントを取り付ける
	    	eventTrigger = trainCheckToggle.gameObject.AddComponent<EventTrigger> ();
        }
        if (alchemyCheckToggle != null)
        {
	    	eventTrigger = alchemyCheckToggle.gameObject.AddComponent<EventTrigger> ();
        }
        if (recoveryCheckToggle != null)
        {
	    	eventTrigger = recoveryCheckToggle.gameObject.AddComponent<EventTrigger> ();
        }
        if (battleCheckToggle != null)
        {
	    	eventTrigger = battleCheckToggle.gameObject.AddComponent<EventTrigger> ();
        }
        if (resourceCheckToggle != null)
        {
	    	eventTrigger = resourceCheckToggle.gameObject.AddComponent<EventTrigger> ();
        }
        //　ボタン内にマウスが入った時のイベントリスナー登録（ラムダ式で設定）
		entry1 = new EventTrigger.Entry ();
		entry1.eventID = EventTriggerType.PointerClick;
		entry1.callback.AddListener (data => OnMyPointerClick((BaseEventData) data));
		eventTrigger.triggers.Add (entry1);

        _selectHandler = handler;
    }

    void OnMyPointerClick(BaseEventData data) {
        _selectHandler(_actorInfo.ActorId);
	}
}
