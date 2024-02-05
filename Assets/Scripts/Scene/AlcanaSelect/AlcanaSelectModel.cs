using System.Collections;
using System.Collections.Generic;

public class AlcanaSelectModel : BaseModel
{
    private List<SkillInfo> _selectedAlcanaList = new ();
    public List<SkillInfo> SelectedAlcanaList => _selectedAlcanaList;

    private SkillInfo _deleteAlcanaInfo = null;
    public SkillInfo DeleteAlcanaInfo => _deleteAlcanaInfo;
    public void SetDeleteAlcana(SkillInfo skillInfo)
    {
        _deleteAlcanaInfo = skillInfo;
    }

    public List<ListData> SkillActionList()
    {
        var skillInfos = CurrentData.AlcanaInfo.OwnAlcanaList;
        for (int i = 0; i < skillInfos.Count;i++)
        {
            skillInfos[i].SetEnable(_selectedAlcanaList.Contains(skillInfos[i]) == false);
        }
        return MakeListData(skillInfos);
    }

    public void ChangeSelectAlcana(SkillInfo skillInfo)
    {
        if (!_selectedAlcanaList.Contains(skillInfo))
        {
            _selectedAlcanaList.Add(skillInfo);
        } else
        {
            var findIndex = _selectedAlcanaList.FindIndex(a => a == skillInfo);
            if (findIndex > -1)
            {
                _selectedAlcanaList.RemoveAt(findIndex);
            }
        }
    }

    public void DeleteAlcana()
    {
        if (_selectedAlcanaList.Contains(_deleteAlcanaInfo))
        {
            var index = _selectedAlcanaList.FindIndex(a => a == _deleteAlcanaInfo);
            if (index > -1)
            {
                _selectedAlcanaList.RemoveAt(index);
            }
        }
        CurrentData.AlcanaInfo.DeleteAlcana(_deleteAlcanaInfo);
        SavePlayerData();
    }

    public bool CheckStageStart()
    {
        if (_selectedAlcanaList.Count == DataSystem.System.AlcanaSelectCount) return true;
        if (_selectedAlcanaList.Count == CurrentData.AlcanaInfo.OwnAlcanaList.Count) return true;
        return false;
    }

    public void SetStageAlcanaList()
    {
        CurrentSaveData.StageAlcana.ClearOwnAlcanaList();
        foreach (var selectedAlcana in _selectedAlcanaList)
        {
            selectedAlcana.SetEnable(true);
            CurrentSaveData.StageAlcana.AddAlcana(selectedAlcana);
        }
    }
}
