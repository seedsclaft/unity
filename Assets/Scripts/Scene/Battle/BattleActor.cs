using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using Effekseer;

public class BattleActor : ListItem ,IListViewItem ,IClickHandlerEvent 
{
    [SerializeField] private BattlerInfoComponent battlerInfoComponent;
    [SerializeField] private EffekseerEmitter effekseerEmitter;
    private GameObject _battleDamageRoot;
    [SerializeField] private GameObject battleDamagePrefab;
    [SerializeField] private _2dxFX_DestroyedFX deathAnimation;
    private float _deathAnimation = 0.0f;
    public bool IsBusy {
        get {return effekseerEmitter.exists || _damageTiming > 0 || _battleDamages.Find(a => a.IsBusy);}
    }
    private BattlerInfo _data; 
    private int _index; 
    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    private System.Action<int> _damageHandler;
    private int _damageTiming = 0; 

    private List<BattleDamage> _battleDamages = new List<BattleDamage>();
    public void SetData(BattlerInfo data,int index){
        _data = data;
        _index = index;
    }

    public void SetDamageRoot(GameObject damageRoot)
    {
        _battleDamageRoot = damageRoot;
        _battleDamageRoot.SetActive(true);
    }

    public int listIndex(){
        return _index;
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        clickButton.onClick.AddListener(() => handler((int)_data.Index));
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
        battlerInfoComponent.UpdateInfo(_data);
        battlerInfoComponent.RefreshStatus();
    }

    public void ClickHandler()
    {
    }

    void OnMyPointerEnter(BaseEventData data) {
        _selectHandler(_index);
	}

    
    public void StartAnimation(EffekseerEffectAsset effectAsset)
    {
        effekseerEmitter.Play(effectAsset);
    }

    public void SetStartSkillDamage(int damageTiming,System.Action<int> callEvent)
    {
        ClearDamagePopup();
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
        battlerInfoComponent.ChangeHp(value * -1 + _data.Hp);
    }

    public void StartHeal(DamageType damageType,int value)
    {
        var battleDamage = CreatePrefab();
        battleDamage.StartHeal(damageType,value);
        _battleDamages.Add(battleDamage);
        battlerInfoComponent.ChangeHp(value + _data.Hp);
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
        if (_data == null) return;
        battlerInfoComponent.RefreshStatus();
    }

    private void Update() {
        UpdateDamageTiming();
        UpdateDeathAnimation();
    }


    private void UpdateDamageTiming()
    {
        if (_damageTiming > 0)
        {
            _damageTiming--;
            if (_damageTiming == 0)
            {
                _damageHandler(_index);
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
        } 
        else
        {
            _deathAnimation += 0.01f;
        }
    }
}
