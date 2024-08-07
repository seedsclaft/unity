using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CharacterListModel : BaseModel
    {
        private List<ActorInfo> _actorInfos = null;
        public List<ActorInfo> ActorInfos => _actorInfos;
        public CharacterListModel(List<ActorInfo> actorInfos)
        {
            _actorInfos = actorInfos;
        }
    }
}