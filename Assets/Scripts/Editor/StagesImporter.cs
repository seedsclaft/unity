
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class StagesInfoImporter : AssetPostprocessor 
	{	
		static readonly string ExcelName = "Stages.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
		{
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateStagesData(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateStagesData(string asset)
		{
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			var Data = AssetDatabase.LoadAssetAtPath<StageDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<StageDates>();
				AssetDatabase.CreateAsset(Data, ExportPath);
				//Data.hideFlags = HideFlags.NotEditable;
			}
			Data.hideFlags = HideFlags.None;

			try
			{
				// ファイルを開く
				using (var Mainstream = File.Open(asset, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					// エクセルブックを作成
					AssetPostImporter.CreateBook(asset, Mainstream, out IWorkbook Book);
					var textData = AssetPostImporter.CreateText(Book.GetSheetAt(5));

					// 情報の初期化
					Data.Data.Clear();

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);
					ISheet EventSheet = Book.GetSheetAt(1);
					ISheet SymbolSheet = Book.GetSheetAt(2);
					ISheet TutorialSheet = Book.GetSheetAt(4);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						var KeyRow = BaseSheet.GetRow(0);
						AssetPostImporter.SetKeyNames(KeyRow.Cells);
						IRow BaseRow = BaseSheet.GetRow(i);

                        var StageData = new StageData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Text,
                            AchieveText = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "AchieveTextId"))?.Text,
                            Selectable = AssetPostImporter.ImportNumeric(BaseRow, "Selectable") == 1,
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Help,
                            StageLv = AssetPostImporter.ImportNumeric(BaseRow, "StageLv"),
                            Turns = AssetPostImporter.ImportNumeric(BaseRow, "Turns"),
                            InitMembers = new List<int>()
                        };
                        string[] list = AssetPostImporter.ImportString(BaseRow,"InitMembers").Split(',');
						foreach (string item in list)
						{
							StageData.InitMembers.Add(int.Parse(item));
						}
						StageData.RandomTroopCount = AssetPostImporter.ImportNumeric(BaseRow,"RandomTroopCount");
						StageData.BackGround = AssetPostImporter.ImportString(BaseRow,"BackGround");
						StageData.BGMId = AssetPostImporter.ImportNumeric(BaseRow,"BGMId");
						StageData.BossBGMId = AssetPostImporter.ImportNumeric(BaseRow,"BossBGMId");
						StageData.StageRect = AssetPostImporter.ImportNumeric(BaseRow,"StageRect");
						StageData.Reborn = AssetPostImporter.ImportNumeric(BaseRow,"Reborn") == 1;
						StageData.Alcana = AssetPostImporter.ImportNumeric(BaseRow,"Alcana") == 1;
						StageData.SaveLimit = AssetPostImporter.ImportNumeric(BaseRow,"SaveLimit");
						StageData.ContinueLimit = AssetPostImporter.ImportNumeric(BaseRow,"ContinueLimit");
						StageData.RankingStage = (RankingType)AssetPostImporter.ImportNumeric(BaseRow,"RankingStage");
						StageData.SlotSave = AssetPostImporter.ImportNumeric(BaseRow,"SlotSave") == 1;
						StageData.SubordinateValue = AssetPostImporter.ImportNumeric(BaseRow,"SubordinateValue");
						StageData.UseSlot = AssetPostImporter.ImportNumeric(BaseRow,"UseSlot") == 1;
						
						StageData.StageEvents = new List<StageEventData>();
						
						KeyRow = EventSheet.GetRow(0);
						AssetPostImporter.SetKeyNames(KeyRow.Cells);
						for (int j = 1; j <= EventSheet.LastRowNum; j++)
						{
							IRow EventRow = EventSheet.GetRow(j);
							var EventData = new StageEventData();
							var StageId = AssetPostImporter.ImportNumeric(EventRow,"Id");
							
							if (StageId == StageData.Id)
							{
								EventData.Turns = AssetPostImporter.ImportNumeric(EventRow,"Turns");
								EventData.Timing = (EventTiming)AssetPostImporter.ImportNumeric(EventRow,"Timing");
								EventData.Type = (StageEventType)AssetPostImporter.ImportNumeric(EventRow,"Type");
								EventData.Param = AssetPostImporter.ImportNumeric(EventRow,"Param");
								EventData.ReadFlag = AssetPostImporter.ImportNumeric(EventRow,"ReadFlag") == 1;
								
								EventData.EventKey = EventData.Turns.ToString() + EventData.Timing.ToString() + EventData.Type.ToString() + EventData.Param.ToString();

								StageData.StageEvents.Add(EventData);
							}
						}
						StageData.StageSymbols = new ();
						
						KeyRow = SymbolSheet.GetRow(0);
						AssetPostImporter.SetKeyNames(KeyRow.Cells);
						for (int j = 1; j <= SymbolSheet.LastRowNum; j++)
						{
							IRow SymbolRow = SymbolSheet.GetRow(j);
							var SymbolData = new StageSymbolData();
							var StageId = AssetPostImporter.ImportNumeric(SymbolRow,"Id");
							
							if (StageId == StageData.Id)
							{
								SymbolData.StageId = AssetPostImporter.ImportNumeric(SymbolRow, "Id");
								SymbolData.Seek = AssetPostImporter.ImportNumeric(SymbolRow, "Seek");
								SymbolData.SeekIndex = AssetPostImporter.ImportNumeric(SymbolRow, "SeekIndex");
								SymbolData.SymbolType = (SymbolType)AssetPostImporter.ImportNumeric(SymbolRow, "SymbolType");
								SymbolData.Rate = AssetPostImporter.ImportNumeric(SymbolRow, "Rate");
								SymbolData.Param1 = AssetPostImporter.ImportNumeric(SymbolRow, "Param1");
								SymbolData.Param2 = AssetPostImporter.ImportNumeric(SymbolRow, "Param2");
								SymbolData.PrizeSetId = AssetPostImporter.ImportNumeric(SymbolRow, "PrizeSetId");
								SymbolData.ClearCount = AssetPostImporter.ImportNumeric(SymbolRow, "ClearCount");
								
								StageData.StageSymbols.Add(SymbolData);
							}
						}
						

						Data.Data.Add(StageData);
					}

					Data.SymbolGroupData = new List<SymbolGroupData>();
					ISheet SymbolGroupSheet = Book.GetSheetAt(3);
					var KeyRow2 = SymbolGroupSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow2.Cells);
					for (int i = 1; i <= SymbolGroupSheet.LastRowNum; i++)
					{
						IRow BaseRow = SymbolGroupSheet.GetRow(i);

                        var SymbolGroupData = new SymbolGroupData
                        {
                            GroupId = AssetPostImporter.ImportNumeric(BaseRow, "GroupId"),
                            SymbolType = (SymbolType)AssetPostImporter.ImportNumeric(BaseRow, "SymbolType"),
                            Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1"),
                            Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2"),
                            Rate = AssetPostImporter.ImportNumeric(BaseRow, "Rate"),
                            PrizeSetId = AssetPostImporter.ImportNumeric(BaseRow, "PrizeSetId")
                        };
                        Data.SymbolGroupData.Add(SymbolGroupData);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
			}

			EditorUtility.SetDirty(Data);
		}
	}
}