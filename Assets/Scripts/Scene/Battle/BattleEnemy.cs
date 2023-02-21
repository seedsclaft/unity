using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using Effekseer;

public class BattleEnemy : ListItem 
{
    [SerializeField] private BattlerInfoComponent battlerInfoComponent;
    [SerializeField] private GameObject cursorObject;
    [SerializeField] private GameObject imageObject;
    [SerializeField] private Image enemyImage;
    [SerializeField] private EffekseerEmitter effekseerEmitter;
    private GameObject _battleDamageRoot;
    [SerializeField] private GameObject battleDamagePrefab;
    [SerializeField] private _2dxFX_DestroyedFX deathAnimation;
    private float _deathAnimation = 0.0f;
    public bool IsBusy {
        get {return effekseerEmitter.exists || _damageTiming > 0 || _battleDamages.Find(a => a.IsBusy);}
    }
    private bool _sizeInit = false;
    private List<BattleDamage> _battleDamages = new List<BattleDamage>();

    private BattlerInfo _battlerInfo;
    private int _index; 
    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    private System.Action<int> _damageHandler;
    private int _damageTiming = 0; 
    public void SetData(BattlerInfo battlerInfo,int index)
    {
        battlerInfoComponent.UpdateInfo(battlerInfo);
        _battlerInfo = battlerInfo;
        _index = index;
    }
    
    public void SetDamageRoot(GameObject damageRoot)
    {
        _battleDamageRoot = damageRoot;
        _battleDamageRoot.SetActive(true);
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        if (_battlerInfo == null) return;
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
        if (_battlerInfo.IsAlive() == false) return;
        _selectHandler(_index);
	}


    public void StartAnimation(EffekseerEffectAsset effectAsset)
    {
        effekseerEmitter.Play(effectAsset);
    }

    public void SetStartSkillDamage(int damageTiming,System.Action<int> callEvent)
    {
        ClearDamagePopup();
        _battleDamages.Clear();
        _damageTiming = damageTiming;
        _damageHandler = callEvent;
    }

    private BattleDamage CreatePrefab()
    {
        GameObject prefab = Instantiate(battleDamagePrefab);
        prefab.transform.SetParent(_battleDamageRoot.transform, false);
        return prefab.GetComponent<BattleDamage>();
    }

    public void ClearDamagePopup()
    {
        foreach ( Transform n in _battleDamageRoot.transform )
        {
            GameObject.Destroy(n.gameObject);
        }
        _battleDamages.Clear();
    }

    public void StartDamage(DamageType damageType,int value)
    {
        var battleDamage = CreatePrefab();
        battleDamage.StartDamage(damageType,value);
        _battleDamages.Add(battleDamage);
        battlerInfoComponent.ChangeHp(value * -1 + _battlerInfo.Hp);
    }

    public void StartHeal(DamageType damageType,int value)
    {
        var battleDamage = CreatePrefab();
        battleDamage.StartHeal(damageType,value);
        _battleDamages.Add(battleDamage);
        battlerInfoComponent.ChangeHp(value + _battlerInfo.Hp);
    }
    
    public void StartStatePopup(DamageType damageType,string stateName)
    {
        var battleDamage = CreatePrefab();
        battleDamage.StartStatePopup(damageType,stateName,_battleDamages.Count);
    }

    public void StartDeathAnimation()
    {
        _deathAnimation = 0.01f;
        battlerInfoComponent.HideUI();
    }

    public void RefreshStatus()
    {
        battlerInfoComponent.RefreshStatus();
    }

    private void Update() {
        if (_sizeInit == false && enemyImage.sprite != null)
        {
            UpdateSizeDelta();
            _sizeInit = true;
        }
        UpdateDamageTiming();
        UpdateDeathAnimation();
    }

    private void UpdateSizeDelta()
    {
        RectTransform objectRect = gameObject.GetComponent < RectTransform > ();
        RectTransform rect = cursorObject.gameObject.GetComponent < RectTransform > ();
        RectTransform imagerect = imageObject.gameObject.GetComponent < RectTransform > ();
        rect.sizeDelta = new Vector2(imagerect.sizeDelta.x,rect.sizeDelta.y);
        objectRect.sizeDelta = new Vector2(imagerect.sizeDelta.x,rect.sizeDelta.y);
    }

    private void UpdateDamageTiming()
    {
        if (_damageTiming > 0)
        {
            _damageTiming--;
            if (_damageTiming == 0)
            {
                _damageHandler(_battlerInfo.Index);
                _damageHandler = null;
            }
        }
    }
    
    private void UpdateDeathAnimation()
    {
        if (deathAnimation == null) return;
        if (_deathAnimation <= 0) return;

        deathAnimation.Destroyed = _deathAnimation;
        if (_deathAnimation >= 1)
        {
            _deathAnimation = 0;
            enemyImage.enabled = false;
        } 
        else
        {
            _deathAnimation += 0.01f;
        }
    }
}
