
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class EnemiesImporter : AssetPostprocessor 
	{
		static readonly string ExcelName = "Enemies.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
		{
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateEnemyData(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateEnemyData(string asset)
		{
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			EnemyDates Data = AssetDatabase.LoadAssetAtPath<EnemyDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<EnemyDates>();
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
					List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(3));

					// 情報の初期化
					Data.Data.Clear();

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);
					var KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var EnemyData = new EnemyData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Text,
                            ImagePath = AssetPostImporter.ImportString(BaseRow, "ImagePath")
                        };

                        int Hp = AssetPostImporter.ImportNumeric(BaseRow,"Hp");
						int Mp = AssetPostImporter.ImportNumeric(BaseRow,"Mp");
						int Atk = AssetPostImporter.ImportNumeric(BaseRow,"Atk");
						int Def = AssetPostImporter.ImportNumeric(BaseRow,"Def");
						int Spd = AssetPostImporter.ImportNumeric(BaseRow,"Spd");
						EnemyData.Kinds = new List<KindType>();
						KindType Kind1 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,"Kind1");
						if (Kind1 != 0) EnemyData.Kinds.Add(Kind1);
						KindType Kind2 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,"Kind2");
						if (Kind2 != 0) EnemyData.Kinds.Add(Kind2);
						KindType Kind3 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,"Kind3");
						if (Kind3 != 0) EnemyData.Kinds.Add(Kind3);
						EnemyData.BaseStatus = new StatusInfo();
						EnemyData.BaseStatus.SetParameter(Hp,Mp,Atk,Def,Spd);
						EnemyData.HpGrowth = AssetPostImporter.ImportNumeric(BaseRow,"HpGrowth");
						EnemyData.MpGrowth = AssetPostImporter.ImportNumeric(BaseRow,"MpGrowth");
						EnemyData.AtkGrowth = AssetPostImporter.ImportNumeric(BaseRow,"AtkGrowth");
						EnemyData.DefGrowth = AssetPostImporter.ImportNumeric(BaseRow,"DefGrowth");
						EnemyData.SpdGrowth = AssetPostImporter.ImportNumeric(BaseRow,"SpdGrowth");
						Data.Data.Add(EnemyData);
					}

					BaseSheet = Book.GetSheetAt(1);
					KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);
						var LearningData = new LearningData();

						int EnemyId = AssetPostImporter.ImportNumeric(BaseRow,"EnemyId");
						EnemyData Enemy = Data.Data.Find(a => a.Id == EnemyId);
						
						LearningData.SkillId = AssetPostImporter.ImportNumeric(BaseRow,"SkillId");
						LearningData.Level = AssetPostImporter.ImportNumeric(BaseRow,"Level");
						LearningData.Weight = AssetPostImporter.ImportNumeric(BaseRow,"Weight");
						LearningData.TriggerDates = new List<SkillData.TriggerData>();
						Enemy.LearningSkills.Add(LearningData);

                        var SkillTriggerData = new SkillTriggerActorData
                        {
                            SkillId = LearningData.SkillId
                        };
                        var skillTypes = new List<TriggerType>();
						SkillTriggerData.Trigger1 = AssetPostImporter.ImportNumeric(BaseRow,"TriggerType1");
						SkillTriggerData.Trigger2 = AssetPostImporter.ImportNumeric(BaseRow,"TriggerType2");
						Enemy.SkillTriggerDates.Add(SkillTriggerData);
					}

					
					BaseSheet = Book.GetSheetAt(2);
					KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						int EnemyId = AssetPostImporter.ImportNumeric(BaseRow,"EnemyId");
						EnemyData Enemy = Data.Data.Find(a => a.Id == EnemyId);
						int SkillId = AssetPostImporter.ImportNumeric(BaseRow,"SkillId");
						LearningData learningData = Enemy.LearningSkills.Find(a => a.SkillId == SkillId);
						if (learningData != null)
						{
                            SkillData.TriggerData triggerData = new SkillData.TriggerData
                            {
                                TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(BaseRow, "TriggerType"),
                                TriggerTiming = (TriggerTiming)AssetPostImporter.ImportNumeric(BaseRow, "TriggerTiming"),
                                Param1 = AssetPostImporter.ImportNumeric(BaseRow, "Param1"),
                                Param2 = AssetPostImporter.ImportNumeric(BaseRow, "Param2"),
                                Param3 = AssetPostImporter.ImportNumeric(BaseRow, "Param3")
                            };
                            learningData.TriggerDates.Add(triggerData);
						}
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