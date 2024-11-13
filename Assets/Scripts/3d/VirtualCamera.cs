using UnityEditor.PackageManager;
using UnityEngine;

namespace Ryneus
{
    public class VirtualCamera 
    {
        private Camera _selfCamera = null;
        private Quaternion _initRotation;
        private Vector3 _initPosition;
        float _zoomPosition = -1;
        public void SetZoomPosition(float value)
        {
            _zoomPosition = value;
        }

        public VirtualCamera(Camera camera)
        {
            _selfCamera = camera;
            Initialize();
        }

        public void Initialize() 
        { 
            _initRotation = _selfCamera.transform.localRotation;
            _initPosition = _selfCamera.transform.localPosition;
            _zoomPosition = _selfCamera.transform.localPosition.z;
            UpdateCameraZoom();
        }

        public void UpdateCameraZoom()
        {
            _selfCamera.transform.position = _selfCamera.transform.parent.transform.position + (_selfCamera.transform.forward * _zoomPosition);
        }

        public bool IsChanged()
        {
            return _selfCamera.transform.localRotation != _initRotation;
        }

        public void ResetInitialize()
        {
            ResetRotation();
            ResetPosition();
        }

        private void ResetPosition()
        {
            _selfCamera.transform.localPosition = _initPosition;
        }

        private void ResetRotation()
        {
            _selfCamera.transform.localRotation = _initRotation;
        }

        public void SetZoomPosition(float value,float speed)
        {   
            _zoomPosition = Mathf.MoveTowards(_zoomPosition, value, speed * Time.deltaTime);
        }
    }
}
