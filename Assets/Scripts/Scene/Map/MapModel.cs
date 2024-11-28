using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class MapModel : BaseModel
    {
        public GameObject LeaderActorPrefab()
        {
            return ResourceSystem.LoadActor3DModel(PartyInfo.ActorInfos[0].Master.ImagePath);
        }

        public bool IsCurrentSeekSymbolInfo(SymbolInfo symbolInfo)
        {
            return symbolInfo?.Master.Seek == PartyInfo.Seek;
        }
    }
}