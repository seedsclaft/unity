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
    public static List<AdvData> Advs = new ();
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
        /*
        var asset = await Addressables.LoadAssetAsync<ActorsData>("Assets/Data/Actors.asset").Task;
        DataSystem._actors = asset._data;
        var asset2 = await Addressables.LoadAssetAsync<SystemData>("Assets/Data/System.asset").Task;
        DataSystem._system = asset2;
        var asset3 = await Addressables.LoadAssetAsync<ClassesData>("Assets/Data/Classes.asset").Task;
        DataSystem._classes = asset3;
        var asset4 = await Addressables.LoadAssetAsync<EnemiesData>("Assets/Data/Enemies.asset").Task;
        DataSystem._enemies = asset4;
        var asset5 = await Addressables.LoadAssetAsync<SkillsData>("Assets/Data/Skills.asset").Task;
        DataSystem._skills = asset5;
        var asset6 = await Addressables.LoadAssetAsync<StagesData>("Assets/Data/Stages.asset").Task;
        DataSystem._stages = asset6;
        var asset7 = await Addressables.LoadAssetAsync<TroopsData>("Assets/Data/Troops.asset").Task;
        DataSystem._troops = asset7;
        var asset8 = await Addressables.LoadAssetAsync<StatesData>("Assets/Data/States.asset").Task;
        DataSystem._states = asset8;
        var asset9 = await Addressables.LoadAssetAsync<AlcanaData>("Assets/Data/Alcana.asset").Task;
        DataSystem._alcana = asset9;
        var asset10 = await Addressables.LoadAssetAsync<AdvsData>("Assets/Data/Advs.asset").Task;
        DataSystem._advs = asset10;
        var asset11 = await Addressables.LoadAssetAsync<TipsData>("Assets/Data/Tips.asset").Task;
        DataSystem._tips = asset11;
        
        AddressablesKey.LoadAssetAsync<DataManager>("Assets/Data/MainData.asset",(data) => {DataSystem._data = data;});
        */
        Actors = Resources.Load<ActorDates>("Data/Actors").Data;
        Advs = Resources.Load<AdvDates>("Data/Advs").Data;
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