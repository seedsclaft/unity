using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class BattleEnemy : ListItem 
{
    [SerializeField] private BattlerInfoComponent battlerInfoComponent;
    [SerializeField] private GameObject cursorObject;
    [SerializeField] private GameObject imageObject;
    [SerializeField] private Image enemyImage;
    private bool _sizeInit = false;

    private BattlerInfo _data;
    private int _index; 
    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    public void SetData(BattlerInfo battlerInfo,int index)
    {
        battlerInfoComponent.UpdateInfo(battlerInfo);
        _data = battlerInfo;
        _index = index;
    }
    
    public void SetCallHandler(System.Action<int> handler)
    {
        if (_data == null) return;
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

    void OnMyPointerEnter(BaseEventData data) {
        _selectHandler(_index);
	}

    private void UpdateSizeDelta()
    {
        RectTransform objectRect = gameObject.GetComponent < RectTransform > ();
        RectTransform rect = cursorObject.gameObject.GetComponent < RectTransform > ();
        RectTransform imagerect = imageObject.gameObject.GetComponent < RectTransform > ();
        rect.sizeDelta = new Vector2(imagerect.sizeDelta.x,rect.sizeDelta.y);
        objectRect.sizeDelta = new Vector2(imagerect.sizeDelta.x,rect.sizeDelta.y);
    }

    private void Update() {
        if (_sizeInit == false && enemyImage.sprite != null)
        {
            UpdateSizeDelta();
            _sizeInit = true;
        }
    }

    
}
