using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public interface IBaseAnimation
    {    
        public void OpenAnimation(Transform transform,System.Action endEvent,float duration = 0.1f);
    }
}
