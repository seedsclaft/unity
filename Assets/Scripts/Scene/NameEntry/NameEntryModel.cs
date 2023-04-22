using System.Collections;
using System.Collections.Generic;

public class NameEntryModel : BaseModel
{
    public void SetPlayerName(string name)
    {
        CurrentData.SetPlayerName(name);
    }
}
