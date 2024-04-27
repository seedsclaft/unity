using System.Collections.Generic;

namespace Ryneus
{
    public class TitleModel : BaseModel
    {
        public List<ListData> TitleCommand() 
        {
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

        public List<ListData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var menuCommand = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(700),
                Key = "License"
            };
            list.Add(menuCommand);
            var initCommand = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(708),
                Key = "InitializeData"
            };
            list.Add(initCommand);
            return MakeListData(list);
        }
    }
}