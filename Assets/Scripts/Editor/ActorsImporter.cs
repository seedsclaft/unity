using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class ActorsImporter : AssetPostprocessor 
	{
		static readonly string ExcelName = "Actors.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) 
		{
			foreach (string asset in importedAssets) 
			{
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

                        var ActorData = new ActorData
                        {
                            Id = AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Text,
                            SubName = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Help,
							Profile = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Feature,

                            ClassId = AssetPostImporter.ImportNumeric(BaseRow, "ClassId"),
                            UnitType = (UnitType)AssetPostImporter.ImportNumeric(BaseRow, "UnitType"),
                            ImagePath = AssetPostImporter.ImportString(BaseRow, "ImagePath"),
                            InitLv = AssetPostImporter.ImportNumeric(BaseRow, "InitLv"),
                            MaxLv = AssetPostImporter.ImportNumeric(BaseRow, "MaxLv")
                        };

                        int InitHp = AssetPostImporter.ImportNumeric(BaseRow,"InitHp");
						int InitMp = AssetPostImporter.ImportNumeric(BaseRow,"InitMp");
						int InitAtk = AssetPostImporter.ImportNumeric(BaseRow,"InitAtk");
						int InitSpd = AssetPostImporter.ImportNumeric(BaseRow,"InitSpd");
						int InitDef = AssetPostImporter.ImportNumeric(BaseRow,"InitDef");
						ActorData.InitStatus = new StatusInfo();
						ActorData.InitStatus.SetParameter(InitHp,InitMp,InitAtk,InitDef,InitSpd);

						int NeedHp = AssetPostImporter.ImportNumeric(BaseRow,"GrowthHp");
						int NeedMp = AssetPostImporter.ImportNumeric(BaseRow,"GrowthMp");
						int NeedAtk = AssetPostImporter.ImportNumeric(BaseRow,"GrowthAtk");
						int NeedSpd = AssetPostImporter.ImportNumeric(BaseRow,"GrowthSpd");
						int NeedDef = AssetPostImporter.ImportNumeric(BaseRow,"GrowthDef");
						ActorData.NeedStatus = new StatusInfo();
						ActorData.NeedStatus.SetParameter(NeedHp,NeedMp,NeedAtk,NeedDef,NeedSpd);

						int Element1 = AssetPostImporter.ImportNumeric(BaseRow,"Element1");
						int Element2 = AssetPostImporter.ImportNumeric(BaseRow,"Element2");
						int Element3 = AssetPostImporter.ImportNumeric(BaseRow,"Element3");
						int Element4 = AssetPostImporter.ImportNumeric(BaseRow,"Element4");
						int Element5 = AssetPostImporter.ImportNumeric(BaseRow,"Element5");
						ActorData.Attribute = new List<AttributeRank>
                        {
                            (AttributeRank)Element1,
                            (AttributeRank)Element2,
                            (AttributeRank)Element3,
                            (AttributeRank)Element4,
                            (AttributeRank)Element5
                        };

						int X = AssetPostImporter.ImportNumeric(BaseRow,"X");
						int Y = AssetPostImporter.ImportNumeric(BaseRow,"Y");
						float Scale = AssetPostImporter.ImportFloat(BaseRow,"Scale");
						int AwakenX = AssetPostImporter.ImportNumeric(BaseRow,"AwakenX");
						int AwakenY = AssetPostImporter.ImportNumeric(BaseRow,"AwakenY");
						float AwakenScale = AssetPostImporter.ImportFloat(BaseRow,"AwakenScale");
						ActorData.X = X;
						ActorData.Y = Y;
						ActorData.Scale = Scale;
						ActorData.AwakenX = AwakenX;
						ActorData.AwakenY = AwakenY;
						ActorData.AwakenScale = AwakenScale;
						ActorData.Kinds = new List<KindType>();
						KindType Kind1 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,"Kind1");
						if (Kind1 != 0) ActorData.Kinds.Add(Kind1);
						KindType Kind2 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,"Kind2");
						if (Kind2 != 0) ActorData.Kinds.Add(Kind2);
						KindType Kind3 = (KindType)AssetPostImporter.ImportNumeric(BaseRow,"Kind3");
						if (Kind3 != 0) ActorData.Kinds.Add(Kind3);

						
						Data.Data.Add(ActorData);
					}
					// 習得スキル情報設定
					BaseSheet = Book.GetSheetAt(1);
					KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						int ActorId = AssetPostImporter.ImportNumeric(BaseRow,"ActorId");
						ActorData Actor = Data.Data.Find(a => a.Id == ActorId);
						string[] list = AssetPostImporter.ImportString(BaseRow,"SkillId").Split(',');
						foreach (string item in list)
						{
                            var LearningData = new LearningData
                            {
                                SkillId = int.Parse(item),
                                Level = AssetPostImporter.ImportNumeric(BaseRow, "Level")
                            };
                            Actor.LearningSkills.Add(LearningData);
						}
					}
					// トリガースキル情報設定
					BaseSheet = Book.GetSheetAt(2);
					KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);
					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

						int ActorId = AssetPostImporter.ImportNumeric(BaseRow,"ActorId");
						ActorData Actor = Data.Data.Find(a => a.Id == ActorId);

                        var SkillTriggerData = new SkillTriggerActorData
                        {
                            SkillId = AssetPostImporter.ImportNumeric(BaseRow, "SkillId")
                        };
                        var skillTypes = new List<TriggerType>();
						SkillTriggerData.Trigger1 = AssetPostImporter.ImportNumeric(BaseRow,"TriggerType1");
						SkillTriggerData.Trigger2 = AssetPostImporter.ImportNumeric(BaseRow,"TriggerType2");
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