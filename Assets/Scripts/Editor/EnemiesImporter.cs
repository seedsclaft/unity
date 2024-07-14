
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
		enum BaseColumn
		{
			Id = 0,
			NameId,
			ImagePath,
			Hp,
			Mp,
			Atk,
			Def,
			Spd,
			Kind1,
			Kind2,
			Kind3,
			HpGrowth,
			MpGrowth,
			AtkGrowth,
			DefGrowth,
			SpdGrowth,
		}

		enum BaseLearningColumn
		{

			ActorId = 0,
			SkillId,
			_SkillName,
			Level,
			Weight,
			TriggerType1,
			TriggerType2,
		}
		enum BaseTriggersColumn
		{
			ActorId = 0,
			SkillId,
			TriggerType,
			TriggerTiming,
			Param1,
			Param2,
			Param3
		}

		enum BaseTextColumn
		{
			Id = 0,
			Text,
			Help
		}

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

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var EnemyData = new EnemyData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Id),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameId)).Text,
                            ImagePath = AssetPostImporter.ImportString(BaseRow, (int)BaseColumn.ImagePath)
                        };

                        int Hp = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Hp);
						int Mp = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Mp);
						int Atk = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Atk);
						int Def = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Def);
						int Spd = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Spd);
						EnemyData.Kinds = new List<KindType>();
						KindType Kind1 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Kind1);
						if (Kind1 != 0) EnemyData.Kinds.Add(Kind1);
						KindType Kind2 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Kind2);
						if (Kind2 != 0) EnemyData.Kinds.Add(Kind2);
						KindType Kind3 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Kind3);
						if (Kind3 != 0) EnemyData.Kinds.Add(Kind3);
						EnemyData.BaseStatus = new StatusInfo();
						EnemyData.BaseStatus.SetParameter(Hp,Mp,Atk,Def,Spd);
						EnemyData.HpGrowth = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.HpGrowth);
						EnemyData.MpGrowth = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.MpGrowth);
						EnemyData.AtkGrowth = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.AtkGrowth);
						EnemyData.DefGrowth = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.DefGrowth);
						EnemyData.SpdGrowth = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.SpdGrowth);
						Data.Data.Add(EnemyData);
					}

					BaseSheet = Book.GetSheetAt(1);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);
						var LearningData = new LearningData();

						int ActorId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseLearningColumn.ActorId);
						EnemyData Enemy = Data.Data.Find(a => a.Id == ActorId);
						
						LearningData.SkillId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseLearningColumn.SkillId);
						LearningData.Level = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseLearningColumn.Level);
						LearningData.Weight = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseLearningColumn.Weight);
						LearningData.TriggerDates = new List<SkillData.TriggerData>();
						Enemy.LearningSkills.Add(LearningData);

                        var SkillTriggerData = new SkillTriggerActorData
                        {
                            SkillId = LearningData.SkillId
                        };
                        var skillTypes = new List<TriggerType>();
						SkillTriggerData.Trigger1 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseLearningColumn.TriggerType1);
						SkillTriggerData.Trigger2 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseLearningColumn.TriggerType2);
						Enemy.SkillTriggerDates.Add(SkillTriggerData);
					}

					
					BaseSheet = Book.GetSheetAt(2);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						int ActorId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggersColumn.ActorId);
						EnemyData Enemy = Data.Data.Find(a => a.Id == ActorId);
						int SkillId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseTriggersColumn.SkillId);
						LearningData learningData = Enemy.LearningSkills.Find(a => a.SkillId == SkillId);
						if (learningData != null)
						{
                            SkillData.TriggerData triggerData = new SkillData.TriggerData
                            {
                                TriggerType = (TriggerType)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggersColumn.TriggerType),
                                TriggerTiming = (TriggerTiming)AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggersColumn.TriggerTiming),
                                Param1 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggersColumn.Param1),
                                Param2 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggersColumn.Param2),
                                Param3 = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseTriggersColumn.Param3)
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