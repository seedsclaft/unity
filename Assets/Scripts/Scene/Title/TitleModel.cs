using System.Collections.Generic;

public class TitleModel : BaseModel
{
    public List<SystemData.MenuCommandData> TitleCommand => DataSystem.TitleCommand;

    public bool ExistsLoadFile()
    {
        return SaveSystem.ExistsLoadFile();
    }

    public string VersionText()
    {
        return GameSystem.Version.ToString();
    }

    public List<SystemData.MenuCommandData> SideMenu()
    {
        var list = new List<SystemData.MenuCommandData>();
        var menucommand = new SystemData.MenuCommandData();
        menucommand.Id = 1;
        menucommand.Name = DataSystem.System.GetTextData(700).Text;
        menucommand.Key = "Licence";
        list.Add(menucommand);
        return list;
    }
}
