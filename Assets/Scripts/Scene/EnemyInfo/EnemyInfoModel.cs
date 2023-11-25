using System.Collections;
using System.Collections.Generic;

public class EnemyInfoModel : BaseModel
{
    public EnemyInfoModel(List<BattlerInfo> enemyInfos)
    {
        _enemies = enemyInfos;
        _currentIndex = enemyInfos.Count - 1;
    }
    
    private List<BattlerInfo> _enemies = new();

    private int _currentIndex = 0; 
    public int CurrentIndex => _currentIndex;

    public List<BattlerInfo> Enemies(){
        return _enemies;
    }

    public List<int> EnemyIndexes(){
        var list = new List<int>();
        foreach (var enemy in _enemies)
        {
            list.Add(enemy.Index);
        }
        return list;
    }

    public void SelectEnemy(int enemyIndex)
    {
        _currentIndex = _enemies.FindIndex(a => a.Index == enemyIndex);
    }

    public BattlerInfo CurrentEnemy
    {
        get {return Enemies()[_currentIndex];}
    }

    public void ChangeActorIndex(int value){
        _currentIndex += value;
        if (_currentIndex > Enemies().Count-1){
            _currentIndex = 0;
        } else
        if (_currentIndex < 0){
            _currentIndex = Enemies().Count-1;
        }
    }
    
    public List<ListData> SkillActionList()
    {
        var skillInfos = CurrentEnemy.Skills;//.FindAll(a => a.Master.Attribute != AttributeType.None);
        skillInfos.ForEach(a => a.SetEnable(true));
        skillInfos.Sort((a,b) => {return a.Id - b.Id;});
        var list = new List<ListData>();
        var idx = 0;
        foreach (var skillInfo in skillInfos)
        {
            var listData = new ListData(skillInfo,idx);
            list.Add(listData);
        }
        return list;
    }

    public List<ListData> SelectCharacterConditions()
    {
        var list = new List<ListData>();
        foreach (var stateInfo in CurrentEnemy.StateInfos)
        {
            var listData = new ListData(stateInfo);
        }
        return list;
    }
}
