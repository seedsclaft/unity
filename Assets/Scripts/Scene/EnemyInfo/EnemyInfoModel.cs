using System.Collections;
using System.Collections.Generic;

public class EnemyInfoModel : BaseModel
{
    public EnemyInfoModel(List<BattlerInfo> enemyInfos)
    {
        _enemyBattlerInfos = enemyInfos;
        _currentIndex = enemyInfos.Count - 1;
    }
    
    private List<BattlerInfo> _enemyBattlerInfos = new();

    private int _currentIndex = 0; 
    public int CurrentIndex => _currentIndex;

    public List<ListData> EnemyInfoListDates(){
        return ListData.MakeListData(_enemyBattlerInfos,true);
    }

    public List<int> EnemyIndexes(){
        var list = new List<int>();
        foreach (var enemy in _enemyBattlerInfos)
        {
            list.Add(enemy.Index);
        }
        return list;
    }

    public void SelectEnemyIndex(int selectIndex)
    {
        _currentIndex = selectIndex;
    }

    public BattlerInfo CurrentEnemy => _enemyBattlerInfos[_currentIndex];
    
    public List<ListData> SkillActionList()
    {
        var skillInfos = CurrentEnemy.Skills;//.FindAll(a => a.Master.Attribute != AttributeType.None);
        skillInfos.ForEach(a => a.SetEnable(true));
        skillInfos.Sort((a,b) => {return a.Id - b.Id;});
        return MakeListData(skillInfos);
    }

    public List<ListData> SelectCharacterConditions()
    {
        return MakeListData(CurrentEnemy.StateInfos);
    }
}
