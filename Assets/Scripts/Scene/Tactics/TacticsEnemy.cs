using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class TacticsEnemy : ListItem ,IListViewItem ,IClickHandlerEvent 
{
    [SerializeField] private EnemyInfoComponent enemyInfoComponent;
    [SerializeField] private GetItemList getItemList = null;
    private BattlerInfo _enemyInfo; 
    private int _index; 
    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    private new void Awake() {
        getItemList.Initialize();
    }

    public void SetData(BattlerInfo data,int index){
        _enemyInfo = data;
        _index = index;
    }

    public void SetGetItemList(List<GetItemInfo> getItemInfos)
    {
        getItemList.Refresh(getItemInfos);
        //SetInputHandler(skillAttributeList.GetComponent<IInputHandlerEvent>());
        getItemList.gameObject.SetActive(true);
    }

    public int listIndex(){
        return _index;
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        clickButton.onClick.AddListener(() => handler((int)_index));
    }

    public void SetSelectHandler(System.Action<int> handler){
        //　EventTriggerコンポーネントを取り付ける
		eventTrigger = clickButton.gameObject.AddComponent<EventTrigger> ();
        //　ボタン内にマウスが入った時のイベントリスナー登録（ラムダ式で設定）
		entry1 = new EventTrigger.Entry ();
		entry1.eventID = EventTriggerType.PointerEnter;
		entry1.callback.AddListener (data => OnMyPointerEnter((BaseEventData) data));
		eventTrigger.triggers.Add (entry1);

        _selectHandler = handler;
    }

    public void UpdateViewItem()
    {
        if (_enemyInfo == null) return;
        enemyInfoComponent.UpdateInfo(_enemyInfo);
    }

    public void ClickHandler()
    {
    }

    void OnMyPointerEnter(BaseEventData data) {
        _selectHandler(_index);
	}
}
