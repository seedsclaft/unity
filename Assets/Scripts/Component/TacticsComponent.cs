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

    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    public void UpdateInfo(ActorInfo actorInfo)
    {
        _actorInfo = actorInfo;
        actorInfoComponent.UpdateInfo(actorInfo);
        
        if (trainCheckToggle != null){
            trainCheckToggle.isOn = (actorInfo.TacticsComandType == TacticsComandType.Train);
        }
        if (afterLv != null){
            afterLv.gameObject.SetActive(trainCheckToggle.isOn);
            afterLv.text = (actorInfo.Level+1).ToString();
        }
        if (trainCost != null){
            trainCost.text = actorInfo.Level.ToString();
        }
    }

    public void SetToggleHandler(System.Action<int> handler){
        //　EventTriggerコンポーネントを取り付ける
		eventTrigger = trainCheckToggle.gameObject.AddComponent<EventTrigger> ();
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
