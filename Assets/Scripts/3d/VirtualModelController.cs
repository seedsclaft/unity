using UnityEngine;
using DG.Tweening;
using Effekseer;
using System;

namespace Ryneus
{
    public class VirtualModelController : MonoBehaviour
    {        
        [SerializeField] private Animator animator = null;
        [SerializeField] private float scaleSize = 1f;
        [SerializeField] private CharacterController characterController = null;
        [SerializeField] private Camera selfCamera = null;
        [SerializeField] private GameObject statusRoot = null;
        [SerializeField] private GameObject damagePosition = null;
        public GameObject DamagePosition => damagePosition;
        [SerializeField] private MakerEffekseerEmitter effectEmitter = null;
        [SerializeField] float gravity = 20.0f;
        [SerializeField] float jumpSpeed = 8.0f;
        private float _speed = 10f;
        private Vector3 _moveDirection;
        private bool _jump = false;
        private bool _jumping = false;
        private float _rotation = 0;
        private float _rotationSpeed = 4f;
        private Vector2 _cameraRotation = Vector2.zero;
        private Vector3 _lastMousePosition = Vector3.zero;
        private VirtualCamera _virtualCamera;
        private AnimationState _lastState = AnimationState.None;

        private bool _isActor;
        public bool IsActor => _isActor;

        public void Initialize(bool isActor) 
        { 
            _isActor = isActor;
            transform.localScale = new Vector3(scaleSize,scaleSize,scaleSize);
            _virtualCamera = new VirtualCamera(selfCamera);
            UpdateCameraZoom();
        }

        public void CameraOff()
        {
            selfCamera.enabled = false;
        }

        public void CameraOn()
        {
            selfCamera.enabled = true;
        }

        public void SetStatusPrefab(GameObject gameObject)
        {
            gameObject.transform.SetParent(statusRoot.transform,false);
        }

        public void Stop()
        {
            if (!characterController.isGrounded)
            {
                return;
            }
            StartAnimation(AnimationState.Idle);
            _moveDirection = new Vector3(0, 0, 0);
            _cameraRotation = Vector2.zero;
            _rotation = 0;
        }

        public void Forward()
        {
            if (!characterController.isGrounded)
            {
                return;
            }
            StartAnimation(AnimationState.RunForward);
            var moveZ = Math.Min(1,_moveDirection.z+1);
            _moveDirection.Set(_moveDirection.x,_moveDirection.y,moveZ);
            if (_virtualCamera.IsChanged())
            {
                var plusY = Mathf.Abs(selfCamera.transform.localEulerAngles.y) + Mathf.Abs(transform.localEulerAngles.y);
                transform.localRotation = Quaternion.Euler(0,plusY,0);
                _virtualCamera.ResetInitialize();
                UpdateCameraZoom();
            }
            _cameraRotation = Vector2.zero;
        }

        public void BackForward()
        {
            if (!characterController.isGrounded)
            {
                return;
            }
            StartAnimation(AnimationState.RunForward);
            var moveZ = Math.Max(-1,_moveDirection.z-1);
            _moveDirection.Set(_moveDirection.x,_moveDirection.y,moveZ);
        }

        public void RightForward()
        {
            if (!characterController.isGrounded)
            {
                return;
            }
            StartAnimation(AnimationState.RunForward);
            var moveX = Math.Min(1,_moveDirection.x+1);
            _moveDirection.Set(moveX,_moveDirection.y,_moveDirection.z);
        }

        public void LeftForward()
        {
            if (!characterController.isGrounded)
            {
                return;
            }
            StartAnimation(AnimationState.RunForward);
            var moveX = Math.Max(-1,_moveDirection.x-1);
            _moveDirection.Set(moveX,_moveDirection.y,_moveDirection.z);
        }

        public void Jump()
        {
            if (!characterController.isGrounded)
            {
                return;
            }
            StartAnimation(AnimationState.Jump);
            _jump = true;
        }

        public void RightRotation()
        {
            _rotation = 1;
        }

        public void LeftRotation()
        {
            _rotation = -1;
        }

        public void RightCamera()
        {
            _cameraRotation.x = 1;
        }

        public void LeftCamera()
        {
            _cameraRotation.x = -1;
        }

        public void UpCamera()
        {
            _cameraRotation.y = 0.2f;
        }

        public void DownCamera()
        {
            _cameraRotation.y = -0.2f;
        }

        public void ZoomIn()
        {
            _virtualCamera.SetZoomPosition(10,_rotationSpeed);
            UpdateCameraZoom();
        }

        public void ZoomOut()
        {    
            _virtualCamera.SetZoomPosition(-10,_rotationSpeed);
            UpdateCameraZoom();
        }

        public void MouseMove(Vector3 position)
        {
            if (_lastMousePosition != position)
            {
                if (_lastMousePosition != Vector3.zero)
                {
                    var moveX = position.x - _lastMousePosition.x;
                    var moveY = position.y - _lastMousePosition.y;
                    if (moveX > 0)
                    {
                        RightCamera();
                    } else
                    if (moveX < 0)
                    {
                        LeftCamera();
                    }
                    if (moveY > 0)
                    {
                        UpCamera();
                    } else
                    if (moveY < 0)
                    {
                        DownCamera();
                    }
                }
                _lastMousePosition = position;
            }
        }

        public void MouseWheel(Vector2 position)
        {
            var wheelY = position.y;
            if (wheelY > 0)
            {
                ZoomIn();
            } else
            if (wheelY < 0)
            {
                ZoomOut();
            }
        }

        public void RunForward()
        {
            var duration = 0.8f;
            gameObject.transform.DOLocalMoveX(0, 0);
            var sequence = DOTween.Sequence()
                .SetDelay(1)
                .Append(gameObject.transform.DOLocalMoveX(560, duration));
        }

        public void StartAnimation(AnimationState animationState)
        {
            if (_lastState != animationState)
            {
                if (_lastState == AnimationState.Death)
                {
                    return;
                }
                _lastState = animationState;
                animator.SetInteger("State",(int)animationState);
            }
        }

        private void Update() 
        {
            RotateCamera();
            transform.Rotate(0, _rotation * _rotationSpeed, 0);
            //地面についている時
            if (characterController.isGrounded)
            {
                if (_jumping)
                {
                    // 着地
                    StartAnimation(AnimationState.Idle);
                    _jumping = false;
                }
                var moveDirection = transform.TransformDirection(_moveDirection) * _speed;
                //体の向きを滑らかに変更する
                //Quaternion rotation = Quaternion.LookRotation(_moveDirection);
                //transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * _smooth);

                //Transform.TransformDirectionはローカル空間からワールド空間へ方向Vectorを変換する
                //ジャンプ↓
                if (_jump)
                {
                    _jump = false;
                    _jumping = true;
                    _moveDirection.y = jumpSpeed;
                    moveDirection.y = jumpSpeed;
                } 
                //重力分変更する
                moveDirection.y -= gravity * Time.deltaTime;
                characterController.Move(moveDirection * Time.deltaTime); 
                return;
            } 
            _moveDirection.y -= gravity * Time.deltaTime;
            characterController.Move(_moveDirection * Time.deltaTime); 
        }   
        
        private void RotateCamera()
        {
            //Vector3でX,Y方向の回転の度合いを定義
            Vector3 angle = new Vector3(_cameraRotation.x * _rotationSpeed,_cameraRotation.y * _rotationSpeed, 0);
            
            var cameraAngle = selfCamera.transform.localEulerAngles;
            //transform.RotateAround()をしようしてメインカメラを回転させる
            selfCamera.transform.RotateAround(transform.position, Vector3.up, angle.x);
            
            var nextAngle = cameraAngle.x + angle.y;
            if (nextAngle < 30 || nextAngle > 330)
            {
                selfCamera.transform.RotateAround(transform.position, selfCamera.transform.right, angle.y);
            }
            UpdateCameraZoom();
        }

        public void SetVictoryCamera()
        {
            //Vector3でX,Y方向の回転の度合いを定義
            Vector3 angle = new Vector3(180,10, 0);
            _virtualCamera.SetZoomPosition(-1.25f);
            
            //transform.RotateAround()をしようしてメインカメラを回転させる
            selfCamera.transform.RotateAround(transform.position, Vector3.up, angle.x);
            selfCamera.transform.RotateAround(transform.position, selfCamera.transform.right, angle.y);
            selfCamera.transform.parent.transform.localPosition = new Vector3(0,1,0); 
            UpdateCameraZoom();
        }

        private void UpdateCameraZoom()
        {
            _virtualCamera?.UpdateCameraZoom();
        }

        public void PlayEffect(EffekseerEffectAsset effectAsset,int animationPosition,float animationScale,float animationSpeed)
        {
            if (effectAsset == null)
            {
                effectEmitter.Stop();
                return;
            } 
            var effectRect = effectEmitter.gameObject.GetComponent<Transform>();
            if (animationPosition == 0)
            {
                effectRect.localPosition = new Vector2(0,0);
            } else
            if (animationPosition == 1)
            {
                effectRect.localPosition = new Vector2(0,-48);
            }
            effectRect.localScale = new Vector3(animationScale,animationScale,animationScale);
            effectEmitter.enabled = true;
            effectEmitter.Stop();
            effectEmitter.speed = animationSpeed;
            effectEmitter.Play(effectAsset);
        }
    }
}

public enum AnimationState
{
    None = -1,
    Idle = 0,
    RunForward = 1,
    Attack = 2,
    Jump = 3,
    Damaged = 4,
    Death = 5,

    Ready = 6,
    Victory = 7,

}