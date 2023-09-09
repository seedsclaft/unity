using System.Collections.Generic;

public class TitleModel : BaseModel
{
    public List<SystemData.CommandData> TitleCommand => DataSystem.TitleCommand;

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
