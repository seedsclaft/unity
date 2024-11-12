using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Ryneus
{
    public class Battle3DView : MonoBehaviour
    {
        [SerializeField] private GameObject partyRoot = null;
        [SerializeField] private GameObject troopRoot = null;
        [SerializeField] private List<GameObject> partyPositions = null;
        [SerializeField] private List<GameObject> troopPositions = null;
        [SerializeField] private GameObject statusPrefab = null;
        [SerializeField] private Camera battleCamera = null;

        private Dictionary<int,VirtualModelController> _virtualModelControls = new ();
        private Dictionary<BattlerInfo,BattlerInfoComponent> _battlers = new ();

        public void Initialize(List<BattlerInfo> battlerInfos) 
        {
            var idx = 0;
            foreach (var battlerInfo in battlerInfos)
            {
                if (battlerInfo.IsActor)
                {
                    var prefab = Instantiate(ResourceSystem.LoadActor3DModel(battlerInfo.ActorInfo.Master.ImagePath));
                    if (prefab != null)
                    {
                        prefab.transform.SetParent(partyPositions[idx].transform,false);
                        _virtualModelControls[battlerInfo.Index] = prefab.GetComponent<VirtualModelController>();
                    }
                    _virtualModelControls[battlerInfo.Index].StartAnimation(AnimationState.Ready);
                } else
                {
                    var prefab = Instantiate(ResourceSystem.LoadEnemy3DModel(battlerInfo.EnemyData.ImagePath));
                    if (prefab != null)
                    {
                        prefab.transform.SetParent(troopPositions[idx].transform,false);
                        _virtualModelControls[battlerInfo.Index] = prefab.GetComponent<VirtualModelController>();
                        var statusObject = Instantiate(statusPrefab);
                        _virtualModelControls[battlerInfo.Index].SetStatusPrefab(statusObject);
                        var battlerInfoComponent = statusObject.GetComponent<BattlerInfoComponent>();
                        _battlers[battlerInfo] = battlerInfoComponent;
                    }
                }                        
                _virtualModelControls[battlerInfo.Index].Initialize();
                _virtualModelControls[battlerInfo.Index].CameraOff();
                idx++;
            }    
        }

        public void UpdateSelectCursor(List<int> selectIndexes)
        {
            foreach (var item in _battlers)
            {
                item.Value.UpdateSelectCursor(selectIndexes.Contains(item.Key.Index));
            }
        }

        public void HideSelectCursor()
        {
            foreach (var item in _battlers)
            {
                item.Value.UpdateSelectCursor(false);
            }
        }

        public void PlayEffect(int targetIndex,Effekseer.EffekseerEffectAsset effectAsset,int animationPosition,float animationScale,float animationSpeed)
        {
            _virtualModelControls[targetIndex].PlayEffect(effectAsset,animationPosition,animationScale,animationSpeed);
        }

        public void RefreshStatus()
        {
            foreach (var item in _battlers)
            {
                item.Value.UpdateInfo(item.Key);
                item.Value.RefreshStatus();
            }
        }

        private void StartAnimation(int index,AnimationState animationState)
        {
            if (_virtualModelControls.ContainsKey(index))
            {
                _virtualModelControls[index].StartAnimation(animationState);
            }
        }

        public void DisplayTroop()
        {
            var CanvasGroup = troopRoot.GetComponent<CanvasGroup>();
            CanvasGroup.alpha = 0;
            
            var sequence = DOTween.Sequence()
                .Append(CanvasGroup.DOFade(1, 1));
        }

        public void SetIdle(int index)
        {
            StartAnimation(index,AnimationState.Idle);
        }

        public void RunForward(int index)
        {
            StartAnimation(index,AnimationState.RunForward);
            if (_virtualModelControls.ContainsKey(index) && index < 100)
            {
                _virtualModelControls[index].RunForward();
            }
            DisplayTroop();
        }

        public void Attack(int index)
        {
            StartAnimation(index,AnimationState.Attack);
        }

        public void StartDamage(int index)
        {
            StartAnimation(index,AnimationState.Damaged);
        }

        public void Death(int index)
        {
            StartAnimation(index,AnimationState.Death);
        }
    }
}
