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
        SavePlayerData();
    }

    public bool CheckStageStart()
    {
        if (_selectedAlcanaList.Count == DataSystem.System.AlcanaSelectCount) return true;
        return false;
    }

    public void SetStageAlcanaList()
    {
    }
}
