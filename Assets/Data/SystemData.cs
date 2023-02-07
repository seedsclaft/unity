using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SystemData : ScriptableObject
{	
	public List<MenuCommandData> MenuCommandDataList;
	public List<MenuCommandData> TitleCommandData;
	public List<int> InitActors;

	[System.SerializableAttribute]
	public class MenuCommandData
	{
		
		public int Id;
		public string Key;
		public string Name;
		public string Help;
	}
}

public enum MenuComandType {
	None,
	Item,
	Skill
}

public enum TitleComandType {
	NewGame = 0,
	Continue,
	Option
}