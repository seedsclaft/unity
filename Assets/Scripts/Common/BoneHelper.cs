using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BoneHelper : MonoBehaviour
    {
        private MMD4MecanimBone mMD4MecanimBone;
        [SerializeField] private float x;
        [SerializeField] private float y;
        [SerializeField] private float z;
        private void OnEnable() 
        {
            mMD4MecanimBone = gameObject.GetComponent<MMD4MecanimBone>();
            if (mMD4MecanimBone != null)
            {
                UpdateParams();
            }
        }

        private void UpdateParams()
        {
            mMD4MecanimBone.userEulerAngles = new Vector3(x,y,z);
        }


    }
}
