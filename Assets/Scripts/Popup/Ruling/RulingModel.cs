using System.Collections;
using System.Collections.Generic;
using System;

namespace Ryneus
{
    public class RulingModel : BaseModel
    {
        private int _currentCategory = 0; 
        public int CurrentCategory => _currentCategory;
        public void SetCategory(int category)
        {
            _currentCategory = category;
            var command = RulingCommand()[0];
            var data = (SystemData.CommandData)command.Data;
            SetId(data.Id);
        }

        private int _currentId = DataSystem.Rules.Count > 0 ? DataSystem.Rules[0].Id : 1; 

        public void SetId(int id)
        {
            _currentId = id;
        }

        public List<ListData> RulingCommand()
        {
            var list = new List<SystemData.CommandData>();
            foreach (var rule in DataSystem.Rules)
            {
                if (rule.Category == _currentCategory || _currentCategory == 0)
                {
                    SystemData.CommandData ruleCommand = new SystemData.CommandData
                    {
                        Key = rule.Id.ToString(),
                        Name = rule.Name,
                        Help = rule.Help,
                        Id = rule.Id
                    };
                    list.Add(ruleCommand);
                }
            }
            return MakeListData(list);
        }

        public List<ListData> RuleHelp()
        {
            var helpList = new List<string>();
            var rule = DataSystem.Rules.Find(a => a.Id == _currentId);
            if (rule != null)
            {
                foreach (var item in rule.Help.Split("\n"))
                {
                    helpList.Add(item);
                }
            }
            return MakeListData(helpList);
        }
    }
}
