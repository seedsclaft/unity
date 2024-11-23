using System.Collections.Generic;

namespace Ryneus
{
    public class TitleModel : BaseModel
    {
        public List<ListData> TitleCommand()
        {
            var selectIndex = ExistsLoadFile() ? 1 : 0;
            return ListData.MakeListData(DataSystem.TitleCommand,(a) => 
            { 
                switch (a.Key)
                {
                    case "CONTINUE":
                        return ExistsLoadFile();
                }
                return true;
            },selectIndex);
        }

        public bool ExistsLoadFile()
        {
            return SaveSystem.ExistsStageFile();
        }

        public string VersionText()
        {
            return GameSystem.Version;
        }

        public List<SystemData.CommandData> SideMenu()
        {
            var list = new List<SystemData.CommandData>();
            var optionCommand = new SystemData.CommandData
            {
                Id = 1,
                Name = DataSystem.GetText(13410),
                Key = "Option"
            };
            list.Add(optionCommand);
            var menuCommand = new SystemData.CommandData
            {
                Id = 2,
                Name = DataSystem.GetText(13400),
                Key = "License"
            };
            list.Add(menuCommand);
            var deleteStage = new SystemData.CommandData
            {
                Id = 3,
                Name = DataSystem.GetText(13420),
                Key = "DeleteStage"
            };
            list.Add(deleteStage);
            var initCommand = new SystemData.CommandData
            {
                Id = 4,
                Name = DataSystem.GetText(13421),
                Key = "InitializeData"
            };
            list.Add(initCommand);
#if !UNITY_WEBGL
            var endCommand = new SystemData.CommandData
            {
                Id = 5,
                Name = DataSystem.GetText(13430),
                Key = "EndGame"
            };
            list.Add(endCommand);
#endif
            return list;
        }

        public void InitializeNewGame()
        {
            var actorInfos = new List<ActorInfo>();
            var b = new ActorInfo(DataSystem.FindActor(1));
            b.SetBattleIndex(1);
            actorInfos.Add(b);
            var a = new ActorInfo(DataSystem.FindActor(3));
            a.SetBattleIndex(2);
            actorInfos.Add(a);
            PartyInfo.SetActorInfos(actorInfos);
        }
    }
}