using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;

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
        [SerializeField] private Camera dummyCamera = null;
        public Camera BattleCamera => battleCamera;
        [SerializeField] Vector3 readyPosition;
        [SerializeField] Vector3 readyAngel;
        [SerializeField] float readyZoom;
        

        private Dictionary<int,VirtualModelController> _virtualModelControls = new ();
        public Dictionary<int,VirtualModelController> VirtualModelControls => _virtualModelControls;
        private Dictionary<int,GameObject> _selectCursor = new ();
        //private VirtualCamera _virtualCamera;
        private VirtualCamera _virtualDummyCamera;
        private int _actorCount = 0;
        private int _enemyCount = 0;

        

        public void Initialize(List<BattlerInfo> battlerInfos) 
        {
            //_virtualCamera = new VirtualCamera(battleCamera);
            _virtualDummyCamera = new VirtualCamera(dummyCamera);
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
                _virtualModelControls[battlerInfo.Index].Initialize(battlerInfo.IsActor);
                _virtualModelControls[battlerInfo.Index].CameraOff();
                idx++;
            }
            HideSelectCursor();
        }

        public void UpdateParentPosition(int actorNum,int enemyNum)
        {
            _actorCount = actorNum;
            _enemyCount = enemyNum;
            var positionX = enemyNum - actorNum;
            var position = partyRoot.transform.localPosition;
            partyRoot.transform.localPosition = new Vector3(positionX,position.y,-2.5f - (positionX * 0.5f));
        }

        public void ResetCameraPosition()
        {
            //_virtualCamera.SetZoomPosition(0);
            //_virtualCamera.ResetInitialize();
            _virtualDummyCamera.SetZoomPosition(0);
            _virtualDummyCamera.ResetInitialize();
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

        public void StartBattle(int enemyCount)
        {
            _enemyCount = enemyCount;
            ResetCameraPosition();
            SetCamera(EnemyPosition(),
                new Vector3(1*_enemyCount,0,0),new Vector3(-20,0,0),-1.5f);
        }

        public void PlayEffect(int targetIndex,Effekseer.EffekseerEffectAsset effectAsset,int animationPosition,float animationScale,float animationSpeed)
        {
            _virtualModelControls[targetIndex].PlayEffect(effectAsset,animationPosition,animationScale,animationSpeed);
            if (_virtualModelControls[targetIndex].IsActor)
            {
                ResetCameraPosition();
                SetCamera(_virtualModelControls[targetIndex].gameObject.transform.position
                    ,new Vector3(0,1,0),new Vector3(-180,0,0),-0.5f);
            } else
            {
                ResetCameraPosition();
                SetCamera(_virtualModelControls[targetIndex].gameObject.transform.position,
                    new Vector3(0.5f,1,0),new Vector3(-10,0,0),-0.5f);
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

        private Vector3 EnemyPosition()
        {
            if (_enemyCount % 2 == 1)
            {
                return new Vector3(0,0,0);
            }
            return new Vector3(_enemyCount * 0.5f,0,0);
        }

        public void ActorBattleReady(int index)
        {
            ResetCameraPosition();
            var partyRootPos = partyRoot.transform.localPosition;
            var troopRootPos = troopRoot.transform.localPosition;
            
            // 距離
            var zPos = troopRootPos.z - partyRootPos.z - 1;

            // 横のずれ
            var xCenter = _actorCount-1;
            var actorPosition = (index-1)*2;
            var xPos = actorPosition + xCenter;
            troopRoot.transform.localPosition = new Vector3(xPos*-1,troopRootPos.y,troopRootPos.z);

            foreach (var _virtualModelControl in _virtualModelControls)
            {
                var virtualModelController = _virtualModelControl.Value; 
                if (virtualModelController.IsActor)
                {
                    if (_virtualModelControls[index] != virtualModelController)
                    {
                        virtualModelController.transform.localPosition = virtualModelController.transform.forward * -1;
                    } else
                    {
                        virtualModelController.transform.localPosition = new Vector3(virtualModelController.transform.localPosition.x,virtualModelController.transform.localPosition.y,0);
                    }
                }
            }
            SetCamera(_virtualModelControls[index].gameObject.transform.position,readyPosition + new Vector3(0.25f,0,0),readyAngel + new Vector3(-10f,0f,0),zPos*-1,0f);
            ResetCameraPosition();
            SetCamera(_virtualModelControls[index].gameObject.transform.position,readyPosition,readyAngel,zPos*-1,0.8f);
        }

        public void EnemyBattleReady(int index)
        {
            ResetCameraPosition();
            SetCamera(_virtualModelControls[index].gameObject.transform.position,
                new Vector3(0.5f,1,0),new Vector3(-10,0,0),-0.5f);
        }

        public void BattleVictory(int mvpActorId)
        {
            /*
            battleCamera.enabled = false;
            dummyCamera.enabled = false;
            foreach (var _virtualModelControl in _virtualModelControls)
            {
                if (_virtualModelControl.Key == mvpActorId)
                {
                    _virtualModelControl.Value.StartAnimation(AnimationState.Victory);
                    _virtualModelControls[mvpActorId].CameraOn();
                    _virtualModelControls[mvpActorId].SetVictoryCamera();
                }
            }
            */
        }
        
        public void SetCamera(Vector3 targetPosition,Vector3 position,Vector3 angle,float zoom,float duration = 0)
        {
            dummyCamera.transform.RotateAround(targetPosition, Vector3.up, angle.x);
            dummyCamera.transform.RotateAround(targetPosition, dummyCamera.transform.right, angle.y);
            dummyCamera.transform.RotateAround(targetPosition, Vector3.right, angle.z);
            
            _virtualDummyCamera.SetZoomPosition(zoom);
            UpdateCameraZoom();
            dummyCamera.transform.position += new Vector3(targetPosition.x + position.x,targetPosition.y + position.y,0);

            var sequence =  DOTween.Sequence()
                .Join(battleCamera.transform.DOLocalMove(dummyCamera.transform.localPosition, duration))
                .Join(battleCamera.transform.DOLocalRotate(dummyCamera.transform.localEulerAngles, duration))
                .SetEase(Ease.OutCubic)
                .OnComplete(() => 
                {
                });
        }

        private void UpdateCameraZoom()
        {
            _virtualDummyCamera.UpdateCameraZoom();
        }

        public void SelectActor(List<ListData> targets)
        {
            return;
            int targetIndex = -1;
            // Idleにして整列させる
            foreach (var target in targets)
            {
                var battlerInfo = (BattlerInfo)target.Data;
                _virtualModelControls[battlerInfo.Index].StartAnimation(AnimationState.Idle);
                if (targetIndex == -1)
                {
                    targetIndex = battlerInfo.Index;
                }
            }
            ResetCameraPosition();
            SetCamera(_virtualModelControls[targetIndex].gameObject.transform.position
                ,new Vector3(0,1,0),new Vector3(-180,0,0),-1);
        }
    }
}
