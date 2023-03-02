using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class GetItem : ListItem ,IListViewItem ,IClickHandlerEvent 
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleName;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI resultName;
    private GetItemInfo _data; 
    private int _index; 
    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    public void SetData(GetItemInfo data,int index){
        _data = data;
        _index = index;
    }

    public int listIndex(){
        return _index;
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        //clickButton.onClick.AddListener(() => handler((int)_data.Id));
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
        if (_data == null) return;
    }

    public void ClickHandler()
    {
    }

    void OnMyPointerEnter(BaseEventData data) {
        _selectHandler(_index);
	}
}
