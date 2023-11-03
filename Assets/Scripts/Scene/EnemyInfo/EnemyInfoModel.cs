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
    private AttributeType _currentAttributeType = AttributeType.Fire;
    public AttributeType CurrentAttributeType => _currentAttributeType;

    public List<BattlerInfo> Enemies(){
        return _enemies;
    }

    public BattlerInfo CurrentActor
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
    
    public List<ListData> SkillActionList(AttributeType attributeType)
    {
        _currentAttributeType = attributeType;
        List<SkillInfo> skillInfos = CurrentActor.Skills.FindAll(a => a.Master.Attribute != AttributeType.None);
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
}
