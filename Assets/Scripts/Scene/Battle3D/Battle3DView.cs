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
        [SerializeField] private GameObject cursorPrefab = null;
        [SerializeField] private Camera battleCamera = null;
        public Camera BattleCamera => battleCamera;

        private Dictionary<int,VirtualModelController> _virtualModelControls = new ();
        public Dictionary<int,VirtualModelController> VirtualModelControls => _virtualModelControls;
        private Dictionary<int,GameObject> _selectCursor = new ();
        private VirtualCamera _virtualCamera;

        public void Initialize(List<BattlerInfo> battlerInfos) 
        {
            _virtualCamera = new VirtualCamera(battleCamera);
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
                        var cursorObject = Instantiate(cursorPrefab);
                        _virtualModelControls[battlerInfo.Index].SetStatusPrefab(cursorObject);
                        var battlerInfoComponent = cursorObject.GetComponent<BattlerInfoComponent>();
                        _selectCursor[battlerInfo.Index] = cursorObject;
                        //_battlers[battlerInfo.Index].UpdateInfo(battlerInfo);
                    }
                }                        
                _virtualModelControls[battlerInfo.Index].Initialize();
                _virtualModelControls[battlerInfo.Index].CameraOff();
                idx++;
            }
            HideSelectCursor();
        }

        public void ResetCameraPosition()
        {
            _virtualCamera.SetZoomPosition(0);
            _virtualCamera.ResetInitialize();
            UpdateCameraZoom();
        }

        public void UpdateSelectCursor(List<int> selectIndexes)
        {
            foreach (var item in _selectCursor)
            {
                item.Value.SetActive(selectIndexes.Contains(item.Key));
            }
        }

        public void HideSelectCursor()
        {
            foreach (var item in _selectCursor)
            {
                item.Value.SetActive(false);
            }
        }

        public void PlayEffect(int targetIndex,Effekseer.EffekseerEffectAsset effectAsset,int animationPosition,float animationScale,float animationSpeed)
        {
            _virtualModelControls[targetIndex].PlayEffect(effectAsset,animationPosition,animationScale,animationSpeed);
            
            SetCamera(_virtualModelControls[targetIndex].gameObject.transform);
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

        public void StartDamage(int index,DamageType damageType,int value)
        {
            StartAnimation(index,AnimationState.Damaged);
        }

        public void StartHeal(int index,DamageType damageType,int value)
        {
        }

        public void Death(int index)
        {
            StartAnimation(index,AnimationState.Death);
        }

        public void BattleReady(int index)
        {
            StartAnimation(index,AnimationState.Ready);
            ResetCameraPosition();
        }

        public void BattleVictory(int mvpActorId)
        {
            battleCamera.enabled = false;
            foreach (var _virtualModelControl in _virtualModelControls)
            {
                _virtualModelControl.Value.StartAnimation(AnimationState.Victory);
                if (_virtualModelControl.Key == mvpActorId)
                {
                    _virtualModelControls[mvpActorId].CameraOn();
                    _virtualModelControls[mvpActorId].SetVictoryCamera();
                }
            }
        }

        public void SetCamera(Transform target)
        {
            Vector3 angle = new Vector3(target.transform.position.x + -15,0,0);
            
            //transform.RotateAround()をしようしてメインカメラを回転させる
            battleCamera.transform.RotateAround(target.transform.position, Vector3.up, angle.x);
            battleCamera.transform.RotateAround(target.transform.position, battleCamera.transform.right, angle.y);
            //selfCamera.transform.parent.transform.localPosition = new Vector3(0,1,0); 
            _virtualCamera.SetZoomPosition(2.5f);
            UpdateCameraZoom();
            battleCamera.transform.position += new Vector3(target.transform.position.x + 2f,0,0);
        }        
        
        private void UpdateCameraZoom()
        {
            _virtualCamera.UpdateCameraZoom();
        }
    }
}
