using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CharacterListModel : BaseModel
    {
        private List<ActorInfo> _actorInfos = null;
        public CharacterListModel(List<ActorInfo> actorInfos)
        {
            _actorInfos = actorInfos;
        }

        public List<ListData> CharacterList()
        {
            return MakeListData(_actorInfos);
        }
    }
}