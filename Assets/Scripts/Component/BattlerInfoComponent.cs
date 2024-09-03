using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Effekseer;

namespace Ryneus
{
    public class BattlerInfoComponent : MonoBehaviour
    {
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private EnemyInfoComponent enemyInfoComponent;
        [SerializeField] private StatusInfoComponent statusInfoComponent;
        [SerializeField] private EffekseerEmitter effekseerEmitter;
        [SerializeField] private _2dxFX_DestroyedFX deathAnimation;
        private GameObject _battleDamageRoot;
        public GameObject BattleDamageRoot => _battleDamageRoot;
        private GameObject _battleStatusRoot;
        [SerializeField] private GameObject battleDamagePrefab;
        [SerializeField] private BattleStateOverlay battleStateOverlay;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI battlePosition;
        [SerializeField] private TextMeshProUGUI evaluate;
        [SerializeField] private Image additiveFaceThumb;
        
        private BattlerInfo _battlerInfo = null;

        private List<BattleDamage> _battleDamages = new ();
        private float _deathAnimation = 0.0f;
        public void UpdateInfo(BattlerInfo battlerInfo)
        {
            if (battlerInfo == null)
            {
                return;
            }
            _battlerInfo = battlerInfo;
            if (battlerInfo.IsActor)
            {
                actorInfoComponent.UpdateInfo(battlerInfo.ActorInfo,null);
            } else
            {
                enemyInfoComponent.UpdateInfo(battlerInfo);
                if (battlerInfo.IsActorView)
                {
                    actorInfoComponent?.UpdateData(battlerInfo.ActorInfo.Master);
                    enemyInfoComponent?.Clear();
                } else
                {
                    actorInfoComponent?.Clear();
                }
            }
            if (evaluate != null)
            {
                evaluate.text = battlerInfo.Evaluate().ToString();
            }
            if (additiveFaceThumb != null)
            {
                if (battlerInfo.IsActor || battlerInfo.IsActorView)
                {
                    var handle = ResourceSystem.LoadActorMainFaceSprite(battlerInfo.ActorInfo.Master.ImagePath);
                    if (additiveFaceThumb != null) 
                    {
                        additiveFaceThumb.sprite = handle;
                    }
                } else
                {
                    UpdateMainThumb(battlerInfo.EnemyData.ImagePath,0,0,1.0f);
                }
            }
        }

        private void UpdateMainThumb(string imagePath,int x,int y,float scale)
        {
            var handle = ResourceSystem.LoadEnemySprite(imagePath);
            if (additiveFaceThumb != null)
            {
                additiveFaceThumb.gameObject.SetActive(true);
                var rect = additiveFaceThumb.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                additiveFaceThumb.sprite = handle;
            }
        }

        public void SetDamageRoot(GameObject damageRoot)
        {
            _battleDamageRoot = damageRoot;
            _battleDamageRoot.SetActive(true);
            if (battleStateOverlay != null) 
            {
                battleStateOverlay.Initialize();
            }
        }

        public void SetStatusRoot(GameObject statusRoot)
        {
            _battleStatusRoot = statusRoot;
            if (statusInfoComponent == null)
            {
                return;
            }
            statusInfoComponent.HideStatus();
        }

        public void ChangeHp(int value)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            statusInfoComponent.UpdateHp(value,_battlerInfo.MaxHp);
        }

        private void ChangeHpAnimation(int fromValue,int toValue)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            statusInfoComponent.UpdateHp(toValue,_battlerInfo.MaxHp);
            statusInfoComponent.UpdateHpAnimation(fromValue,toValue,_battlerInfo.MaxHp);
        }

        public void ChangeMp(int value)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            statusInfoComponent.UpdateMp(value,_battlerInfo.MaxMp);
        }

        private void ChangeMpAnimation(int fromValue,int toValue)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            statusInfoComponent.UpdateMp(toValue,_battlerInfo.MaxMp);
            statusInfoComponent.UpdateMpAnimation(fromValue,toValue,_battlerInfo.MaxMp);
        }

        public void ChangeAtk(int value)
        {
            statusInfoComponent?.UpdateAtk(value);
        }

        public void ChangeDef(int value)
        {
            statusInfoComponent?.UpdateDef(value);
        }

        public void ChangeSpd(int value)
        {
            statusInfoComponent?.UpdateSpd(value);
        }

        public void RefreshStatus()
        {
            if (_battlerInfo.IsActor)
            {
                actorInfoComponent.UpdateInfo(_battlerInfo.ActorInfo,null);
                actorInfoComponent.SetAwakeMode(_battlerInfo.IsState(StateType.Demigod));
            } else
            {
                enemyInfoComponent.UpdateInfo(_battlerInfo);
                if (_battlerInfo.IsActorView)
                {                
                    actorInfoComponent?.UpdateData(_battlerInfo.ActorInfo.Master);
                    enemyInfoComponent?.Clear();
                } else
                {
                    actorInfoComponent?.Clear();
                }
            }
            ChangeHp(_battlerInfo.Hp);
            ChangeMp(_battlerInfo.Mp);
            ChangeAtk(_battlerInfo.CurrentAtk(false));
            ChangeDef(_battlerInfo.CurrentDef(false));
            ChangeSpd(_battlerInfo.CurrentSpd(false));
            if (battlePosition != null)
            {
                var textId = _battlerInfo.LineIndex == LineType.Front ? 2012 : 2013;
                battlePosition.text = DataSystem.GetText(textId);
            }
            if (battleStateOverlay != null) battleStateOverlay.SetStates(_battlerInfo.IconStateInfos());
        }
        
        public void ShowUI()
        {
            if (statusInfoComponent != null)
            {
                statusInfoComponent.ShowStatus();
            }
            battleStateOverlay.gameObject.SetActive(true);
        }

        public void HideUI()
        {
            if (statusInfoComponent != null)
            {
                statusInfoComponent.HideStatus();
            }
            battleStateOverlay.gameObject.SetActive(false);
        }

        private BattleDamage CreatePrefab()
        {
            var prefab = Instantiate(battleDamagePrefab);
            prefab.transform.SetParent(_battleDamageRoot.transform, false);
            return prefab.GetComponent<BattleDamage>();
        }

        public void ClearDamagePopup()
        {
            foreach ( Transform n in _battleDamageRoot.transform )
            {
                //Destroy(n.gameObject);
            }
            _battleDamages.Clear();
        }

        public void StartDamage(DamageType damageType,int value,bool needPopupDelay)
        {
            var battleDamage = CreatePrefab();
            int delayCount = _battleDamages.Count;
            if (needPopupDelay == false)
            {
                delayCount = 0;
            }
            battleDamage.StartDamage(damageType,value,() => {
                if (_battleDamages.Contains(battleDamage))
                {
                    _battleDamages.Remove(battleDamage);
                }
                if (battleDamage != null)
                {
                    Destroy(battleDamage.gameObject);
                }
            },delayCount);
            _battleDamages.Add(battleDamage);
            if (damageType == DamageType.HpDamage || damageType == DamageType.HpCritical)
            {
                ChangeHpAnimation(_battlerInfo.Hp,value * -1 + _battlerInfo.Hp);
            }
            if (damageType == DamageType.MpDamage)
            {
                ChangeMpAnimation(_battlerInfo.Mp,value * -1 + _battlerInfo.Mp);
            }
        }

        public void StartBlink()
        {
            var image = BattleImage();
            if (image == null) return;
            var sequence = DOTween.Sequence()
                .Append(image.DOFade(0f, 0.05f))
                .Append(image.DOFade(1f, 0.05f))
                .SetLoops(3);
        }

        public void StartHeal(DamageType damageType,int value,bool needPopupDelay)
        {
            var battleDamage = CreatePrefab();
            int delayCount = _battleDamages.Count;
            if (needPopupDelay == false)
            {
                delayCount = 0;
            }
            battleDamage.StartHeal(damageType,value,() => {
                if (_battleDamages.Contains(battleDamage))
                {
                    _battleDamages.Remove(battleDamage);
                }
                if (battleDamage != null)
                {
                    Destroy(battleDamage.gameObject);
                }
            },delayCount);
            _battleDamages.Add(battleDamage);
            if (damageType == DamageType.HpHeal)
            {
                ChangeHpAnimation(_battlerInfo.Hp,value + _battlerInfo.Hp);
            } else
            if (damageType == DamageType.MpHeal)
            {
                ChangeMpAnimation(_battlerInfo.Mp,value + _battlerInfo.Mp);
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
            if (!_battlerInfo.IsActor)
            {
                if (deathAnimation)
                {
                    deathAnimation.enabled = true;
                    _deathAnimation = 0.01f;
                    HideUI();
                }
            }
        }

        public void StartAliveAnimation()
        {
            if (!_battlerInfo.IsActor)
            {
                if (deathAnimation)
                {
                    _deathAnimation = 0;
                    //deathAnimation.enabled = false;
                    //deathAnimation.Destroyed = 0;
                    gameObject.SetActive(true);
                    var image = BattleImage();
                    var sequence = DOTween.Sequence()
                        .Append(image.DOFade(0f, 0))
                        .Append(image.DOFade(1f, 0.5f));
                }
            }
        }
        
        public void StartAnimation(EffekseerEffectAsset effectAsset,int animationPosition,float animationScale = 1.0f,float animationSpeed = 1.0f)
        {
            if (effectAsset == null){    
                effekseerEmitter.Stop();
                return;
            } 
            var image = BattleImage();
            if (image == null) return;
            var effectRect = effekseerEmitter.gameObject.GetComponent<RectTransform>();
            if (animationPosition == 0){
                effectRect.localPosition = new Vector2(0,0);
            } else
            if (animationPosition == 1)
            {
                effectRect.localPosition = new Vector2(0,-48);
            }
            effectRect.localScale = new Vector3(animationScale,animationScale,animationScale);
            effekseerEmitter.enabled = true;
            effekseerEmitter.Stop();
            effekseerEmitter.speed = animationSpeed;
            effekseerEmitter.Play(effectAsset);
        }

        public void SetThumbAlpha(bool isSelectable)
        {
            var image = BattleImage();
            if (image == null) return;
            float alpha = isSelectable == true ? 1 : 0.25f;
            canvasGroup.alpha = alpha;
            //image.color = new Color(255,255,255,alpha);
            //effekseerEmitter.enabled = isSelectable;
        }

        public void SetActiveStatus(bool isSelectable)
        {
            if (statusInfoComponent == null)
            {
                return;
            }
            if (!_battlerInfo.IsActor)
            {
                if (isSelectable)
                {
                    statusInfoComponent.ShowStatus();
                } else
                {
                    statusInfoComponent.HideStatus();
                }
            }
        }

        public void HideEnemyStateOverlay()
        {
            if (!_battlerInfo.IsActor)
            {
                battleStateOverlay.HideStateOverlay();
            }
        }

        public void ShowStateOverlay()
        {
            battleStateOverlay.ShowStateOverlay();
        }

        public void HideStateOverlay()
        {
            battleStateOverlay.HideStateOverlay();
        }

        public void SetEnemyGridKey(int index)
        {
            enemyInfoComponent.SetGridKey(index);
        }


        private void Update() 
        {
            UpdateDeathAnimation();
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
            if (_battlerInfo.IsActor)
            {
                if (_battlerInfo.IsAwaken)
                {
                    image = actorInfoComponent.FaceThumb;
                    //image = actorInfoComponent.AwakenFaceThumb;
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

        public void SetActiveBeforeSkillThumb(bool isActive)
        {
            if (additiveFaceThumb != null)
            {
                additiveFaceThumb.gameObject.SetActive(isActive);
                if (isActive)
                {
                    additiveFaceThumb.DOFade(1f, 0);
                    DOTween.Sequence()
                    .Append(additiveFaceThumb.DOFade(0f, 0.4f))
                    .OnComplete(() => 
                    {
                        additiveFaceThumb.gameObject.SetActive(false);
                    });
                }
            }
        }
    }
}