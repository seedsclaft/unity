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
        [SerializeField] private List<GameObject> troopPositions = null;

        private Dictionary<int,VirtualModelController> _actorStateControls = new ();

        public void Initialize(List<BattlerInfo> battlerInfos) 
        {
            troopRoot.AddComponent<CanvasGroup>();
            var CanvasGroup = troopRoot.GetComponent<CanvasGroup>();
            CanvasGroup.alpha = 0;

            var idx = 0;
            foreach (var battlerInfo in battlerInfos)
            {
                if (battlerInfo.IsActor)
                {
                    var prefab = Instantiate(ResourceSystem.LoadActor3DModel(battlerInfo.ActorInfo.Master.ImagePath));
                    if (prefab != null)
                    {
                        prefab.transform.SetParent(partyRoot.transform,false);
                        _actorStateControls[battlerInfo.Index] = prefab.GetComponent<VirtualModelController>();
                        _actorStateControls[battlerInfo.Index].Initialize();
                    }
                } else
                {
                    var prefab = Instantiate(ResourceSystem.LoadEnemy3DModel(battlerInfo.EnemyData.ImagePath));
                    if (prefab != null)
                    {
                        prefab.transform.SetParent(troopPositions[idx].transform,false);
                        _actorStateControls[battlerInfo.Index] = prefab.GetComponent<VirtualModelController>();
                        _actorStateControls[battlerInfo.Index].Initialize();
                    }
                }
                idx++;
            }    
        }

        private void StartAnimation(int index,AnimationState animationState)
        {
            if (_actorStateControls.ContainsKey(index))
            {
                _actorStateControls[index].StartAnimation(animationState);
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
            if (_actorStateControls.ContainsKey(index) && index < 100)
            {
                _actorStateControls[index].RunForward();
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
