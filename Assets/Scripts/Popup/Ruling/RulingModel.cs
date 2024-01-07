using System.Collections;
using System.Collections.Generic;
using System;

public class RulingModel : BaseModel
{
    private int _currentCategory = 1; 
    public int CurrentCategory
    {
        get {return _currentCategory;}
    }
    public void SetCategory(int category)
    {
        _currentCategory = category;
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
            SystemData.CommandData ruleCommand = new SystemData.CommandData();
            ruleCommand.Key = rule.Id.ToString();
            ruleCommand.Name = rule.Name;
            ruleCommand.Help = rule.Help;
            ruleCommand.Id = rule.Id;
            list.Add(ruleCommand);
        }
        return MakeListData(list);
    }

    public List<string> RuleHelp()
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
        return helpList;
    }
}

