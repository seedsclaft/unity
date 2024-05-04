using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class ActorsImporter : AssetPostprocessor {
		enum BaseColumn
		{
			Id = 0,
			NameId,
			ClassId = 4,
			ImagePath,
			InitLv,
			MaxLv,
			InitHp,
			InitMp,
			InitAtk,
			InitDef,
			InitSpd,
			
			GrowthHp,
			GrowthMp,
			GrowthAtk,
			GrowthDef,
			GrowthSpd,
			Element1,
			Element2,
			Element3,
			Element4,
			Element5,
			X,
			Y,
			Scale,
			AwakenX,
			AwakenY,
			AwakenScale,
			Kind1,
			Kind2,
			Kind3,
		}
		enum BaseLearningColumn
		{
			ActorId = 0,
			SkillId,
			Level,
		}
		enum BaseSkillTriggerColumn
		{
			ActorId = 0,
			SkillId,
			TriggerType1,
			TriggerType2,
		}
		static readonly string ExcelName = "Actors.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) {

				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateActorInfo(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}

		static void CreateActorInfo(string asset)
		{
			Debug.Log("CreateActorInfo");
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			ActorDates Data = AssetDatabase.LoadAssetAtPath<ActorDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<ActorDates>();
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

                        var ActorData = new ActorData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.Id),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameId)).Text,
                            SubName = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.NameId)).Help,

                            ClassId = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.ClassId),
                            ImagePath = AssetPostImporter.ImportString(BaseRow, (int)BaseColumn.ImagePath),
                            InitLv = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.InitLv),
                            MaxLv = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseColumn.MaxLv)
                        };

                        int InitHp = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.InitHp);
						int InitMp = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.InitMp);
						int InitAtk = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.InitAtk);
						int InitSpd = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.InitSpd);
						int InitDef = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.InitDef);
						ActorData.InitStatus = new StatusInfo();
						ActorData.InitStatus.SetParameter(InitHp,InitMp,InitAtk,InitDef,InitSpd);

						int NeedHp = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.GrowthHp);
						int NeedMp = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.GrowthMp);
						int NeedAtk = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.GrowthAtk);
						int NeedSpd = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.GrowthSpd);
						int NeedDef = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.GrowthDef);
						ActorData.NeedStatus = new StatusInfo();
						ActorData.NeedStatus.SetParameter(NeedHp,NeedMp,NeedAtk,NeedDef,NeedSpd);

						int Element1 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Element1);
						int Element2 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Element2);
						int Element3 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Element3);
						int Element4 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Element4);
						int Element5 = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Element5);
						ActorData.Attribute = new List<AttributeRank>
                        {
                            (AttributeRank)Element1,
                            (AttributeRank)Element2,
                            (AttributeRank)Element3,
                            (AttributeRank)Element4,
                            (AttributeRank)Element5
                        };

						int X = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.X);
						int Y = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Y);
						float Scale = AssetPostImporter.ImportFloat(BaseRow,(int)BaseColumn.Scale);
						int AwakenX = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.AwakenX);
						int AwakenY = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.AwakenY);
						float AwakenScale = AssetPostImporter.ImportFloat(BaseRow,(int)BaseColumn.AwakenScale);
						ActorData.X = X;
						ActorData.Y = Y;
						ActorData.Scale = Scale;
						ActorData.AwakenX = AwakenX;
						ActorData.AwakenY = AwakenY;
						ActorData.AwakenScale = AwakenScale;
						ActorData.Kinds = new List<KindType>();
						KindType Kind1 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Kind1);
						if (Kind1 != 0) ActorData.Kinds.Add(Kind1);
						KindType Kind2 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Kind2);
						if (Kind2 != 0) ActorData.Kinds.Add(Kind2);
						KindType Kind3 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseColumn.Kind3);
						if (Kind3 != 0) ActorData.Kinds.Add(Kind3);

						
						Data.Data.Add(ActorData);
					}
					// 習得スキル情報設定
					BaseSheet = Book.GetSheetAt(1);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						int ActorId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseLearningColumn.ActorId);
						ActorData Actor = Data.Data.Find(a => a.Id == ActorId);
						string[] list = AssetPostImporter.ImportString(BaseRow,(int)BaseLearningColumn.SkillId).Split(',');
						foreach (string item in list)
						{
                            var LearningData = new LearningData
                            {
                                SkillId = int.Parse(item),
                                Level = AssetPostImporter.ImportNumeric(BaseRow, (int)BaseLearningColumn.Level)
                            };
                            Actor.LearningSkills.Add(LearningData);
						}
					}
					// トリガースキル情報設定
					BaseSheet = Book.GetSheetAt(2);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						int ActorId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseLearningColumn.ActorId);
						ActorData Actor = Data.Data.Find(a => a.Id == ActorId);
						
						var SkillTriggerData = new SkillTriggerActorData();
						SkillTriggerData.SkillId = AssetPostImporter.ImportNumeric(BaseRow,(int)BaseSkillTriggerColumn.SkillId);
						var skillTypes = new List<TriggerType>();
						SkillTriggerData.Trigger1 = (int)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseSkillTriggerColumn.TriggerType1);
						SkillTriggerData.Trigger2 = (int)AssetPostImporter.ImportNumeric(BaseRow,(int)BaseSkillTriggerColumn.TriggerType2);
						Actor.SkillTriggerDates.Add(SkillTriggerData);
						
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