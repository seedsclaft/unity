using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SystemData : ScriptableObject
{	
	public List<MenuCommandData> TacticsCommandData;
	public List<MenuCommandData> StatusCommandData;
	public List<MenuCommandData> TitleCommandData;
	public List<int> InitActors;
	public List<TextData> SystemTextData;

	[System.SerializableAttribute]
	public class MenuCommandData
	{
		
		public int Id;
		public string Key;
		public string Name;
		public string Help;
	}
}

public enum TacticsComandType {
	Train,
	Alchemy,
	Recover,
	Battle,
	Resource,
	Status,
	Turnend
}

public enum TitleComandType {
	NewGame = 0,
	Continue,
	Option
}


public enum StatusComandType {
	Strength = 1,
	SkillActionList = 2,
}