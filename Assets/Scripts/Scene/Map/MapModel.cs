using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class MapModel : BaseModel
    {
        private List<SymbolInfo> _symbolInfos = new ();
        public void SetCurrentSymbolInfos(List<SymbolInfo> symbolInfos)
        {
            _symbolInfos = symbolInfos;
            var seekIndex = PartyInfo.SeekIndex;
            if (seekIndex >= symbolInfos.Count)
            {
                PartyInfo.SetSeekIndex(symbolInfos.Count-1);
            }
        }

        public SymbolInfo SelectSymbolInfo()
        {
            var seekIndex = PartyInfo.SeekIndex;
            if (seekIndex >= 0 && _symbolInfos.Count > seekIndex)
            {
                return _symbolInfos[seekIndex];
            }
            return null;
        }

        public bool IsCurrentSeekSymbolInfo()
        {
            var symbolInfo = SelectSymbolInfo();
            return symbolInfo?.Master.Seek == PartyInfo.Seek;
        }

        public GameObject LeaderActorPrefab()
        {
            return ResourceSystem.LoadActor3DModel(PartyInfo.ActorInfos[0].Master.ImagePath);
        }

        public void GainSeekIndex(int gain)
        {
            var current = PartyInfo.SeekIndex;
            if ((current + gain) < 0)
            {
                gain = 0;
            }
            if ((current + gain) >= _symbolInfos.Count)
            {
                gain = 0;
            }
            PartyInfo.SetSeekIndex(current + gain);
        }

    }
}