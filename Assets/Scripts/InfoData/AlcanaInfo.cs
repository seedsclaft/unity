using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class AlcanaInfo{
    private bool _IsAlcana;
    public bool IsAlcana {get {return _IsAlcana;}}
	private List<int> _alcanaIds = new List<int>();
    public List<int> AlcanaIds {get {return _alcanaIds;}}
    private AlcanaType _currentAlcanaId;
    private List<int> _ownAlcanaIds = new List<int>();
    public List<int> OwnAlcanaIds {get {return _ownAlcanaIds;}}
    private bool _usedAlcana = false;
    public bool UsedAlcana {get {return _usedAlcana;}}
    private StateInfo _alcanaState = null;
    public StateInfo AlcanaState {get {return _alcanaState;}}

    public AlcanaInfo(){

    }

    public void InitData()
    {
        _IsAlcana = false;
        _ownAlcanaIds.Clear();
        _usedAlcana = false;
		MakeAlcanaData();
    }

	private void MakeAlcanaData()
    {
        _alcanaIds.Clear();
        List<AlcanaData.Alcana> alcana = DataSystem.Alcana;
        while (_alcanaIds.Count < alcana.Count-1)
        {
            int rand = new Random().Next(0, alcana.Count-1);
            if (_alcanaIds.FindIndex(a => a == rand) == -1)
            {
                _alcanaIds.Add(rand);
            }
        }
    }

    public void AddAlcanaId()
    {
        int alcanaId = _alcanaIds[0];
        _alcanaIds.RemoveAt(0);
        _ownAlcanaIds.Add(alcanaId);
    }
    
    public void OpenAlcana()
    {
        int id = _ownAlcanaIds[0];
        _ownAlcanaIds.RemoveAt(0);
        _currentAlcanaId = (AlcanaType)id;
    }
    
    public AlcanaData.Alcana CurrentSelectAlcana()
    {
        return DataSystem.Alcana.Find(a => a.Id == (int)_currentAlcanaId);
    }

    public void SetIsAlcana(bool isAlcana)
    {
        _IsAlcana = isAlcana;
    }

    public void UseAlcana(bool isUse)
    {
        _usedAlcana = isUse;
    }

    public void SetAlacanaState(StateInfo stateInfo)
    {
        _alcanaState = stateInfo;
    }

    public void DeleteAlcana()
    {
        _currentAlcanaId = (AlcanaType)(-1);
    }

    public void ChangeNextAlcana()
    {
        List<AlcanaData.Alcana> alcana = DataSystem.Alcana;
        int rand = new Random().Next(0, alcana.Count-1);
        _alcanaIds[0] = rand;
    }
}
