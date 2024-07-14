using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CharacterListModel : BaseModel
    {
        public List<ListData> CharacterList()
        {
            return MakeListData(StatusActors());
        }
    }
}