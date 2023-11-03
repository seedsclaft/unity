using System.Collections.Generic;

public class TitleModel : BaseModel
{
    public List<ListData> TitleCommand() {
        var list = new List<ListData>();
        foreach (var commandData in DataSystem.TitleCommand)
        {
            var listData = new ListData(commandData);
            list.Add(listData);
        }
        if (!ExistsLoadFile())
        {
            list[1].SetEnable(false);
        }
        return list;
    }

    public bool ExistsLoadFile()
    {
        return SaveSystem.ExistsLoadFile();
    }

    public string VersionText()
    {
        return GameSystem.Version;
    }

    public List<SystemData.CommandData> SideMenu()
    {
        var list = new List<SystemData.CommandData>();
        var menucommand = new SystemData.CommandData();
        menucommand.Id = 1;
        menucommand.Name = DataSystem.System.GetTextData(700).Text;
        menucommand.Key = "Licence";
        list.Add(menucommand);
        return list;
    }
}
