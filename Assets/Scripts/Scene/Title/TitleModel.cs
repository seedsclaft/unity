using System.Collections.Generic;

namespace Ryneus
{
    public class TitleModel : BaseModel
    {
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
            var optionCommand = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(701),
                Key = "Option"
            };
            list.Add(optionCommand);
            var menuCommand = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(700),
                Key = "License"
            };
            list.Add(menuCommand);
            var initCommand = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(708),
                Key = "InitializeData"
            };
            list.Add(initCommand);
            var endCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(710),
                Key = "EndGame"
            };
            list.Add(endCommand);
            return MakeListData(list);
        }
    }
}