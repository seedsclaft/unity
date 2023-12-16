using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using Cysharp.Threading.Tasks;

abstract public class DataSystem 
{
    private static DataManager _data;
    public static DataManager Data => _data;
    public static List<ActorData> Actors = new ();
    public static List<AdvData> Adventures = new ();
    public static List<EnemyData> Enemies = new ();
    public static List<RuleData> Rules = new ();
    public static List<SkillData> Skills = new ();
    public static List<StageData> Stages = new ();
    public static List<StateData> States = new ();
    public static List<TroopData> Troops = new ();
    public static List<AnimationData> Animations = new ();
    public static SystemData System;



    public static List<SystemData.CommandData> TacticsCommand => System.TacticsCommandData;
    public static List<SystemData.CommandData> TitleCommand => System.TitleCommandData;
    public static List<SystemData.CommandData> StatusCommand => System.StatusCommandData;
    public static List<SystemData.OptionCommand> OptionCommand => System.OptionCommandData;
    public static List<int> InitActors => System.InitActors;
    
    



    private static AlcanaData _alcana;
    public static List<AlcanaData.Alcana> Alcana => _alcana._data;





    public static void LoadData(){
        Actors = Resources.Load<ActorDates>("Data/Actors").Data;
        Adventures = Resources.Load<AdvDates>("Data/Adventures").Data;
        Enemies = Resources.Load<EnemyDates>("Data/Enemies").Data;
        Rules = Resources.Load<RuleDates>("Data/Rules").Data;
        Skills = Resources.Load<SkillDates>("Data/Skills").Data;
        Stages = Resources.Load<StageDates>("Data/Stages").Data;
        States = Resources.Load<StateDates>("Data/States").Data;
        System = Resources.Load<SystemData>("Data/System");
        Troops = Resources.Load<TroopDates>("Data/Troops").Data;
        Animations = Resources.Load<AnimationDates>("Data/Animations").Data;
        DataSystem._alcana = Resources.Load<AlcanaData>("Data/Alcana");
        DataSystem._data = Resources.Load<DataManager>("Data/MainData");
    }
}

[Serializable]
public class TextData {
    public int Id;
    public string Text;
    public string Help;
}