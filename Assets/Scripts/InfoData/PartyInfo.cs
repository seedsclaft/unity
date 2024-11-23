using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class PartyInfo 
    {
        public PartyInfo()
        {
        }

        // 所持アクターリスト
        private List<ActorInfo> _actorInfos = new();
        public List<ActorInfo> ActorInfos => _actorInfos;
        public void SetActorInfos(List<ActorInfo> actorInfos) => _actorInfos = actorInfos;
    }
}