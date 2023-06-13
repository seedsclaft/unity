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
    private GameObject _battleStatusRoot;
    [SerializeField] private GameObject battleDamagePrefab;
    [SerializeField] private BattleStateOverlay battleStateOverlay;
    [SerializeField] private CanvasGroup canvasGroup;
    
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
            actorInfoComponent.UpdateInfo(battlerInfo.ActorInfo,null);
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

    public void SetStatusRoot(GameObject statusRoot)
    {
        _battleStatusRoot = statusRoot;
        _battleStatusRoot.SetActive(false);
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
            actorInfoComponent.ChangeHp(value,_battlerInfo.MaxHp);
        } else
        {
            enemyInfoComponent.ChangeHp(value,_battlerInfo.MaxHp);
        }
    }

    public void ChangeMp(int value)
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.ChangeMp(value,_battlerInfo.MaxMp);
        } else
        {
            enemyInfoComponent.ChangeMp(value,_battlerInfo.MaxMp);
        }
    }

    public void RefreshStatus()
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.UpdateInfo(_battlerInfo.ActorInfo,null);
            actorInfoComponent.SetAwakeMode(_battlerInfo.IsState(StateType.Demigod));
        } else
        {
            enemyInfoComponent.UpdateInfo(_battlerInfo);
        }
        ChangeHp(_battlerInfo.Hp);
        ChangeMp(_battlerInfo.Mp);
        if (battleStateOverlay != null) battleStateOverlay.SetStates(_battlerInfo.StateInfos);
        
    }
    
    public void ShowUI()
    {
        if (_battlerInfo.isActor)
        {
            actorInfoComponent.ShowUI();
        } else
        {
            enemyInfoComponent.ShowUI();
        }
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
        battleDamage.StartDamage(damageType,value,() => {
            if (_battleDamages.Contains(battleDamage))
            {
                _battleDamages.Remove(battleDamage);
            }
            if (battleDamage != null)
            {
                Destroy(battleDamage.gameObject);
            }
        });
        _battleDamages.Add(battleDamage);
        ChangeHp(value * -1 + _battlerInfo.Hp);
    }

    public void StartBlink()
    {
        Image image = BattleImage();
        if (image == null) return;
        Sequence sequence = DOTween.Sequence()
            .Append(image.DOFade(0f, 0.05f))
            .Append(image.DOFade(1f, 0.05f))
            .SetLoops(3);
    }

    public void StartHeal(DamageType damageType,int value)
    {
        var battleDamage = CreatePrefab();
        battleDamage.StartHeal(damageType,value,() => {
            if (_battleDamages.Contains(battleDamage))
            {
                _battleDamages.Remove(battleDamage);
            }
            if (battleDamage != null)
            {
                Destroy(battleDamage.gameObject);
            }
        });
        _battleDamages.Add(battleDamage);
        if (damageType == DamageType.HpHeal)
        {
            ChangeHp(value + _battlerInfo.Hp);
        } else
        if (damageType == DamageType.MpHeal)
        {
            ChangeMp(value + _battlerInfo.Mp);
        }
    }

    public void StartStatePopup(DamageType damageType,string stateName)
    {
        var battleDamage = CreatePrefab();
        battleDamage.StartStatePopup(damageType,stateName,_battleDamages.Count,() => {
            if (_battleDamages.Contains(battleDamage))
            {
                _battleDamages.Remove(battleDamage);
            }
            if (battleDamage != null)
            {
                Destroy(battleDamage.gameObject);
            }
        });
        _battleDamages.Add(battleDamage);
    }

    public void StartDeathAnimation()
    {
        if (_battlerInfo.isActor)
        {

        } else{
            deathAnimation.enabled = true;
            _deathAnimation = 0.01f;
            HideUI();
        }
    }

    public void StartAliveAnimation()
    {
        if (_battlerInfo.isActor)
        {

        } else{
            _deathAnimation = 0;
            deathAnimation.enabled = false;
            deathAnimation.Destroyed = 0;
            gameObject.SetActive(true);
            Image image = BattleImage();
            Sequence sequence = DOTween.Sequence()
                .Append(image.DOFade(0f, 0))
                .Append(image.DOFade(1f, 0.5f));
            }
    }
    
    public void StartAnimation(EffekseerEffectAsset effectAsset,int animationPosition)
    {
        Image image = BattleImage();
        if (image == null) return;
        if (!_battlerInfo.isActor)
        {
            RectTransform imagerect = image.gameObject.GetComponent < RectTransform > ();
            RectTransform effectRect = effekseerEmitter.gameObject.GetComponent < RectTransform > ();
            if (animationPosition == 0){
                effectRect.localPosition = new Vector2(0,imagerect.sizeDelta.y / 2);
            } else
            if (animationPosition == 1)
            {
                effectRect.localPosition = new Vector2(0,0);
            }
        }
        effekseerEmitter.enabled = true;
        effekseerEmitter.Stop();
        effekseerEmitter.Play(effectAsset);
    }

    public void SetSelectable(bool isSelectable)
    {
        
        Image image = BattleImage();
        if (image == null) return;
        float alpha = isSelectable == true ? 1 : 0.25f;
        canvasGroup.alpha = alpha;
        //image.color = new Color(255,255,255,alpha);
        //effekseerEmitter.enabled = isSelectable;
    }

    public void SetActiveStatus(bool isSelectable)
    {
        if (_battleStatusRoot != null && !_battlerInfo.isActor)
        {
            _battleStatusRoot.SetActive(isSelectable);
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
                _animationEndTiming = 60;
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
            deathAnimation.enabled = false;
            deathAnimation.Destroyed = 0;
        } 
        else
        {
            _deathAnimation += 0.01f;
        }
    }
    
    private Image BattleImage()
    {
        if (_battlerInfo == null) return null;
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

    public void DisableEmitter()
    {
        effekseerEmitter.Stop();
        effekseerEmitter.enabled = false;
    }
}
