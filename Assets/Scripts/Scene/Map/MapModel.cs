using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class MapModel : BaseModel
    {
        public GameObject LeaderActorPrefab()
        {
            return ResourceSystem.LoadActor3DModel("0002");
        }
    }
}