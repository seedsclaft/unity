using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;

abstract public class DataSystem 
{
    private static DataManager _data;
    public static DataManager Data {get{return _data;}}
    private static List<ActorsData.ActorData> _actors;
    public static List<ActorsData.ActorData> Actors {get{return _actors;}}
    private static SystemData _system;
    public static SystemData System {get{return _system;}}

    private static ClassesData _classes;
    public static List<ClassesData.ClassData> Classes {get{return _classes._data;}}
    
    private static EnemiesData _enemies;
    public static List<EnemiesData.EnemyData> Enemies {get{return _enemies._data;}}

    private static SkillsData _skills;
    public static List<SkillsData.SkillData> Skills {get{return _skills._data;}}
    public static List<SystemData.MenuCommandData> TacticsCommand {get { return _system.TacticsCommandData;}}
    public static List<SystemData.MenuCommandData> TitleCommand {get { return _system.TitleCommandData;}}
    public static List<SystemData.MenuCommandData> StatusCommand {get { return _system.StatusCommandData;}}
    public static List<int> InitActors {get { return _system.InitActors;}}
    
    private static StagesData _stages;
    public static List<StagesData.StageData> Stages {get{return _stages._data;}}
    
    private static TroopsData _troops;
    public static List<TroopsData.TroopData> Troops {get{return _troops._data;}}
    
    private static StatesData _states;
    public static List<StatesData.StateData> States {get{return _states._data;}}

    private static AlcanaData _alcana;
    public static List<AlcanaData.Alcana> Alcana {get{return _alcana._data;}}
    /*
    public async Task LoadData()
    {
        var asset = Addressables.LoadAssetAsync<ActorsInfo>("Assets/Data/Actors.asset");
        asset.WaitForCompletion();
        DataSystem._actors = asset.Result._actors;
        Addressables.Release(asset);
        
        var asset2 = Addressables.LoadAssetAsync<SystemInfo>("Assets/Data/System.asset");
        asset2.WaitForCompletion();
        DataSystem._system = asset2.Result;
        Addressables.Release(asset2);

    }
    */
    public async Task LoadData(){
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
       
        AddressablesKey.LoadAssetAsync<DataManager>("Assets/Data/MainData.asset",(data) => {DataSystem._data = data;});
    }
}
