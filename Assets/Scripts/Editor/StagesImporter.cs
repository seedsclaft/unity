
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Ryneus
{
	public class StagesInfoImporter : AssetPostprocessor {
		enum BaseColumn
		{
			Id = 0,
			NameId,
			AchieveTextId,
			Selectable,
			StageLv,
			Turns,
			InitMembers,
			RandomTroopCount,
			BGMId,
			BossBGMId,
			BackGround,
			StageRect,
			Reborn,
			Alcana,
			SaveLimit,
			ContinueLimit,
			RankingStage,
			SlotSave,
			SubordinateValue,
			UseSlot,
		}
		enum BaseEventColumn
		{
			Id = 0,
			Turns,
			Timing,
			_TIming,
			Type,
			_Type,
			Param,
			ReadFlag
		}
		
		enum BaseSymbolColumn
		{
			Id = 0,
			Seek,
			SeekIndex,
			SymbolType,
			_SymbolType,
			Rate,
			Param1,
			Param2,
			PrizeSetId
		}
		enum BaseSymbolGroupColumn
		{
			GroupId = 0,
			SymbolType,
			_SymbolType,
			Rate,
			Param1,
			Param2,
			PrizeSetId
		}
		enum BaseTutorialColumn
		{
			Id = 0,
			Turns,
			Timing,
			Type,
			X,
			Y,
			Width,
			Height,
		}

		static readonly string ExcelName = "Stages.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) {

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
				Data.hideFlags = HideFlags.NotEditable;
			}

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
						IRow BaseRow = BaseSheet.GetRow(i);

						var StageData = new StageData();
						StageData.Id = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Id);
						StageData.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameId)).Text;
						StageData.AchieveText = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.AchieveTextId))?.Text;
						StageData.Selectable = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Selectable) == 1;
						StageData.Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.NameId)).Help;
						StageData.StageLv = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.StageLv);
						StageData.Turns = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Turns);
						StageData.InitMembers = new List<int>();
						string[] list = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.InitMembers).Split(',');
						foreach (string item in list)
						{
							StageData.InitMembers.Add(int.Parse(item));
						}
						StageData.RandomTroopCount = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.RandomTroopCount);
						StageData.BackGround = AssetPostImporter.ImportString(BaseRow,(int)BaseColumn.BackGround);
						StageData.BGMId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.BGMId);
						StageData.BossBGMId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.BossBGMId);
						StageData.StageRect = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.StageRect);
						StageData.Reborn = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Reborn) == 1;
						StageData.Alcana = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Alcana) == 1;
						StageData.SaveLimit = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.SaveLimit);
						StageData.ContinueLimit = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.ContinueLimit);
						StageData.RankingStage = (RankingType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.RankingStage);
						StageData.SlotSave = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.SlotSave) == 1;
						StageData.SubordinateValue = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.SubordinateValue);
						StageData.UseSlot = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.UseSlot) == 1;
						
						StageData.StageEvents = new List<StageEventData>();
						for (int j = 1; j <= EventSheet.LastRowNum; j++)
						{
							IRow EventRow = EventSheet.GetRow(j);
							var EventData = new StageEventData();
							var StageId = (int)EventRow.GetCell((int)BaseEventColumn.Id)?.SafeNumericCellValue();
							
							if (StageId == StageData.Id)
							{
								EventData.Turns = (int)EventRow.GetCell((int)BaseEventColumn.Turns)?.SafeNumericCellValue();
								EventData.Timing = (EventTiming)EventRow.GetCell((int)BaseEventColumn.Timing)?.SafeNumericCellValue();
								EventData.Type = (StageEventType)EventRow.GetCell((int)BaseEventColumn.Type)?.SafeNumericCellValue();
								EventData.Param = (int)EventRow.GetCell((int)BaseEventColumn.Param)?.SafeNumericCellValue();
								EventData.ReadFlag = (bool)(EventRow.GetCell((int)BaseEventColumn.ReadFlag)?.SafeNumericCellValue() == 1);
								
								EventData.EventKey = EventData.Turns.ToString() + EventData.Timing.ToString() + EventData.Type.ToString() + EventData.Param.ToString();

								StageData.StageEvents.Add(EventData);
							}
						}
						StageData.StageSymbols = new ();
						
						for (int j = 1; j <= SymbolSheet.LastRowNum; j++)
						{
							IRow SymbolRow = SymbolSheet.GetRow(j);
							var SymbolData = new StageSymbolData();
							var StageId = (int)SymbolRow.GetCell((int)BaseSymbolColumn.Id)?.SafeNumericCellValue();
							
							if (StageId == StageData.Id)
							{
								SymbolData.StageId = (int)SymbolRow.GetCell((int)BaseSymbolColumn.Id)?.SafeNumericCellValue();
								SymbolData.Seek = (int)SymbolRow.GetCell((int)BaseSymbolColumn.Seek)?.SafeNumericCellValue();
								SymbolData.SeekIndex = (int)SymbolRow.GetCell((int)BaseSymbolColumn.SeekIndex)?.SafeNumericCellValue();
								SymbolData.SymbolType = (SymbolType)SymbolRow.GetCell((int)BaseSymbolColumn.SymbolType)?.SafeNumericCellValue();
								SymbolData.Rate = (int)SymbolRow.GetCell((int)BaseSymbolColumn.Rate)?.SafeNumericCellValue();
								SymbolData.Param1 = (int)SymbolRow.GetCell((int)BaseSymbolColumn.Param1)?.SafeNumericCellValue();
								SymbolData.Param2 = (int)SymbolRow.GetCell((int)BaseSymbolColumn.Param2)?.SafeNumericCellValue();
								SymbolData.PrizeSetId = (int)SymbolRow.GetCell((int)BaseSymbolColumn.PrizeSetId)?.SafeNumericCellValue();
								
								StageData.StageSymbols.Add(SymbolData);
							}
						}
						
						StageData.Tutorials = new List<StageTutorialData>();
						for (int j = 1; j <= TutorialSheet.LastRowNum; j++)
						{
							IRow EventRow = TutorialSheet.GetRow(j);
							var TutorialData = new StageTutorialData();
							var StageId = (int)EventRow.GetCell((int)BaseTutorialColumn.Id)?.SafeNumericCellValue();
							
							if (StageId == StageData.Id)
							{
								TutorialData.Turns = (int)EventRow.GetCell((int)BaseTutorialColumn.Turns)?.SafeNumericCellValue();
								TutorialData.Timing = (EventTiming)EventRow.GetCell((int)BaseTutorialColumn.Timing)?.SafeNumericCellValue();
								TutorialData.Type = (TutorialType)EventRow.GetCell((int)BaseTutorialColumn.Type)?.SafeNumericCellValue();
								TutorialData.X = (int)EventRow.GetCell((int)BaseTutorialColumn.X)?.SafeNumericCellValue();
								TutorialData.Y = (int)EventRow.GetCell((int)BaseTutorialColumn.Y)?.SafeNumericCellValue();
								TutorialData.Width = (int)EventRow.GetCell((int)BaseTutorialColumn.Width)?.SafeNumericCellValue();
								TutorialData.Height = (int)EventRow.GetCell((int)BaseTutorialColumn.Height)?.SafeNumericCellValue();
								
								StageData.Tutorials.Add(TutorialData);
							}
						}


						Data.Data.Add(StageData);
					}

					Data.SymbolGroupData = new List<SymbolGroupData>();
					ISheet SymbolGroupSheet = Book.GetSheetAt(3);
					for (int i = 1; i <= SymbolGroupSheet.LastRowNum; i++)
					{
						IRow BaseRow = SymbolGroupSheet.GetRow(i);

						var SymbolGroupData = new SymbolGroupData();
						SymbolGroupData.GroupId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseSymbolGroupColumn.GroupId);
						SymbolGroupData.SymbolType = (SymbolType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseSymbolGroupColumn.SymbolType);
						SymbolGroupData.Param1 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseSymbolGroupColumn.Param1);
						SymbolGroupData.Param2 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseSymbolGroupColumn.Param2);
						SymbolGroupData.Rate = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseSymbolGroupColumn.Rate);
						SymbolGroupData.PrizeSetId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseSymbolGroupColumn.PrizeSetId);
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