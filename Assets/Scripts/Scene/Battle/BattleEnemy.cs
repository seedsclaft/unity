using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using Effekseer;
using DG.Tweening;

public class BattleEnemy : ListItem 
{
    [SerializeField] private BattlerInfoComponent battlerInfoComponent;
    public BattlerInfoComponent BattlerInfoComponent {get {return battlerInfoComponent;}}
    [SerializeField] private GameObject cursorObject;
    [SerializeField] private GameObject imageObject;
    [SerializeField] private Image enemyImage;
    [SerializeField] private EffekseerEmitter effekseerEmitter;
    private float _deathAnimation = 0.0f;

    private List<BattleDamage> _battleDamages = new List<BattleDamage>();

    private BattlerInfo _battlerInfo;
    private int _index; 
    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    private string textureName = null;

    public int EnemyIndex{
        get {return _battlerInfo.Index;}
    }
    public void SetData(BattlerInfo battlerInfo,int index)
    {
        battlerInfoComponent.UpdateInfo(battlerInfo);
        _battlerInfo = battlerInfo;
        _index = index;
    }
    
    public void SetDamageRoot(GameObject damageRoot)
    {
        battlerInfoComponent.SetDamageRoot(damageRoot);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        if (_battlerInfo == null) return;
        clickButton.onClick.AddListener(() => handler(_battlerInfo.Index));
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
        if (_battlerInfo.IsAlive() == false) return;
        _selectHandler(_battlerInfo.Index);
	}

    private void Update() {
        if (enemyImage.mainTexture != null && textureName != enemyImage.mainTexture.name)
        {
            UpdateSizeDelta();
            textureName = enemyImage.mainTexture.name;
        }
    }

    private void UpdateSizeDelta()
    {
        int width = enemyImage.mainTexture.width - 40;
        int height = enemyImage.mainTexture.height - 40;
        RectTransform objectRect = gameObject.GetComponent < RectTransform > ();
        RectTransform rect = cursorObject.gameObject.GetComponent < RectTransform > ();
        RectTransform effectRect = effekseerEmitter.gameObject.GetComponent < RectTransform > ();
        rect.sizeDelta = new Vector2(width,height);
        objectRect.sizeDelta = new Vector2(width,height);
        effectRect.sizeDelta = new Vector2(width,height);
    }
}
