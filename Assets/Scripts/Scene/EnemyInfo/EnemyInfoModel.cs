using System.Collections;
using System.Collections.Generic;

public class EnemyInfoModel : BaseModel
{
    public EnemyInfoModel(List<BattlerInfo> enemyInfos)
    {
        _enemies = enemyInfos;
    }
    
    private List<BattlerInfo> _enemies = new List<BattlerInfo>();

    private int _currentIndex = 0; 
    public int CurrentIndex
    {
        get {return _currentIndex;}
    }
    private AttributeType _currentAttributeType = AttributeType.Fire;
    public AttributeType CurrentAttributeType
    {
        get {return _currentAttributeType;}
    }

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
    
    public List<SkillInfo> SkillActionList(AttributeType attributeType)
    {
        _currentAttributeType = attributeType;
        List<SkillInfo> skillInfos = CurrentActor.Skills.FindAll(a => a.Id > 100);
        skillInfos.ForEach(a => a.SetEnable(true));
        skillInfos.Sort((a,b) => {return a.Id - b.Id;});
        return skillInfos;
    }
}
