using System.Collections.Generic;

public class TitleModel : BaseModel
{
    public List<ListData> TitleCommand() {
        var listDates = MakeListData(DataSystem.TitleCommand);
        if (!ExistsLoadFile())
        {
            listDates[1].SetEnable(false);
        }
        return listDates;
    }

    public bool ExistsLoadFile()
    {
        return SaveSystem.ExistsLoadPlayerFile();
    }

    public string VersionText()
    {
        return GameSystem.Version;
    }

    public List<SystemData.CommandData> SideMenu()
    {
        var list = new List<SystemData.CommandData>();
        var menuCommand = new SystemData.CommandData();
        menuCommand.Id = 1;
        menuCommand.Name = DataSystem.GetTextData(700).Text;
        menuCommand.Key = "License";
        list.Add(menuCommand);
        return list;
    }
}
