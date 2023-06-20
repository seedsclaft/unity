﻿using System.Collections.Generic;

public class TitleModel : BaseModel
{
    public List<SystemData.MenuCommandData> TitleCommand
    {
        get { return DataSystem.TitleCommand;}
    }

    public bool ExistsLoadFile()
    {
        return SaveSystem.ExistsLoadFile();
    }

    public string VersionText()
    {
        return GameSystem.Version.ToString();
    }
}
