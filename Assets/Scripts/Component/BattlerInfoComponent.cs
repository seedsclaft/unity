using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Effekseer;

public class BattlerInfoComponent : MonoBehaviour
{
    
    [SerializeField] private ActorInfoComponent actorInfoComponent;
    [SerializeField] private EnemyInfoComponent enemyInfoComponent;
    [SerializeField] private EffekseerEmitter effekseerEmitter;
    [SerializeField] private _2dxFX_DestroyedFX deathAnimation;
    private GameObject _battleDamageRoot;
    [SerializeField] private GameObject battleDamagePrefab;
    private BattlerInfo _battlerInfo = null;


    public bool IsBusy {
        get {return _animationEndTiming > 0 || _damageTiming > 0 || _battleDamages.Find(a => a.IsBusy);}
    }

    private System.Action<int> _damageHandler;
    private int _damageTiming = 0;
    private int _animationEndTiming = 0;
    private List<BattleDamage> _battleDamages = new List<BattleDamage>();
    private float _deathAnimation = 0.0f;
    public void UpdateInfo(BattlerInfo battlerInfo)
    {
        _battlerInfo = battlerInfo;
        if (battlerInfo.isActor)
        {
            actorInfoComponent.UpdateInfo(battlerInfo.ActorInfo);
        } else
        {
            enemyInfoComponent.UpdateInfo(battlerInfo);
        }
    }

    public void SetDamageRoot(GameObject damageRoot)
    {
        _battleDamageRoot = damageRoot;
        _battleDamageRoot.SetActive(true);
    }
    
    public void SetStartSkillDamage(int damageTiming,System.Action<int> callEvent)
    {
        ClearDamagePopup();
        _damageTiming = damageTiming;
        _damageHandler = callEvent;
    }

    public void ChangeHp(int value)
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.ChangeHp(value,_battlerInfo.ActorInfo.MaxHp);
        } else
        {
            enemyInfoComponent.ChangeHp(value,_battlerInfo.Status.Hp);
        }
    }

    public void ChangeMp(int value)
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.ChangeMp(value,_battlerInfo.ActorInfo.CurrentMp);
        } else
        {
            enemyInfoComponent.ChangeMp(value,_battlerInfo.Status.Mp);
        }
    }

    public void RefreshStatus()
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.UpdateInfo(_battlerInfo.ActorInfo);
            actorInfoComponent.SetAwakeMode(_battlerInfo.IsState(StateType.Demigod));
        } else
        {
            enemyInfoComponent.UpdateInfo(_battlerInfo);
        }
        ChangeHp(_battlerInfo.Hp);
        ChangeMp(_battlerInfo.Mp);
    }

    public void HideUI()
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.HideUI();
        } else
        {
            enemyInfoComponent.HideUI();
        }
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
        ChangeHp(value * -1 + _battlerInfo.Hp);
    }

    public void StartBlink()
    {
        Image image = BattleImage();
        Sequence sequence = DOTween.Sequence()
            .Append(image.DOFade(0f, 0.05f))
            .Append(image.DOFade(1f, 0.05f))
            .SetLoops(3);
    }

    public void StartHeal(DamageType damageType,int value)
    {
        var battleDamage = CreatePrefab();
        battleDamage.StartHeal(damageType,value);
        _battleDamages.Add(battleDamage);
        ChangeHp(value + _battlerInfo.Hp);
    }

    public void StartStatePopup(DamageType damageType,string stateName)
    {
        var battleDamage = CreatePrefab();
        _battleDamages.Add(battleDamage);
        battleDamage.StartStatePopup(damageType,stateName,_battleDamages.Count);
    }

    public void StartDeathAnimation()
    {
        deathAnimation.enabled = true;
        _deathAnimation = 0.01f;
        HideUI();
    }
    
    public void StartAnimation(EffekseerEffectAsset effectAsset,int animationPosition)
    {
        if (!_battlerInfo.isActor)
        {
            RectTransform imagerect = BattleImage().gameObject.GetComponent < RectTransform > ();
            RectTransform effectRect = effekseerEmitter.gameObject.GetComponent < RectTransform > ();
            if (animationPosition == 0){
                effectRect.localPosition = new Vector2(0,imagerect.sizeDelta.y / 2);
            } else
            if (animationPosition == 1)
            {
                effectRect.localPosition = new Vector2(0,0);
            }
        }
        effekseerEmitter.Play(effectAsset);
    }

    public void SetSelectable(bool isSelectable)
    {
        if (isSelectable)
        {
            BattleImage().color = new Color(255,255,255,255);
        } else{
            BattleImage().color = new Color(255,255,255,128);
        }
    }

    public void SetEnemyGridKey(int index)
    {
        enemyInfoComponent.SetGridKey(index);
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
                _damageHandler(_battlerInfo.Index);
                _damageHandler = null;
                _animationEndTiming = 48;
            }
        }
        if (_animationEndTiming > 0)
        {
            _animationEndTiming--;
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
            gameObject.SetActive(false);
            //deathAnimation.enabled = false;
        } 
        else
        {
            _deathAnimation += 0.01f;
        }
    }
    
    private Image BattleImage()
    {
        Image image;
        if (_battlerInfo.isActor)
        {
            if (_battlerInfo.IsAwaken)
            {
                image = actorInfoComponent.AwakenFaceThumb;
            } else{
                image = actorInfoComponent.FaceThumb;
            }
        } else
        {
            image = enemyInfoComponent.MainThumb;
        }
        return image;
    }
}
