
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

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
		AwakenScale
    }
    enum BaseLearningColumn
    {
		ActorId = 0,
		SkillId,
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

		ActorDatas Data = AssetDatabase.LoadAssetAtPath<ActorDatas>(ExportPath);
		if (!Data)
		{
			// データがなければ作成
			Data = ScriptableObject.CreateInstance<ActorDatas>();
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
				List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(2));

				// 情報の初期化
				Data.Data.Clear();

				// エクセルシートからセル単位で読み込み
				ISheet BaseSheet = Book.GetSheetAt(0);

				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);

					var ActorData = new ActorData();
					ActorData.Id = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Id);
					ActorData.Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.NameId)).Text;
					ActorData.SubName = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.NameId)).Help;
					
					ActorData.ClassId = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.ClassId);
					ActorData.ImagePath = AssetPostImporter.ImportString(Baserow,(int)BaseColumn.ImagePath);
					ActorData.InitLv = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.InitLv);
					ActorData.MaxLv = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.MaxLv);

					int InitHp = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.InitHp);
					int InitMp = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.InitMp);
					int InitAtk = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.InitAtk);
					int InitSpd = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.InitSpd);
					int InitDef = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.InitDef);
					ActorData.InitStatus = new StatusInfo();
					ActorData.InitStatus.SetParameter(InitHp,InitMp,InitAtk,InitDef,InitSpd);

					int NeedHp = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.GrowthHp);
					int NeedMp = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.GrowthMp);
					int NeedAtk = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.GrowthAtk);
					int NeedSpd = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.GrowthSpd);
					int NeedDef = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.GrowthDef);
					ActorData.NeedStatus = new StatusInfo();
					ActorData.NeedStatus.SetParameter(NeedHp,NeedMp,NeedAtk,NeedDef,NeedSpd);

					int Element1 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Element1);
					int Element2 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Element2);
					int Element3 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Element3);
					int Element4 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Element4);
					int Element5 = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Element5);
					ActorData.Attribute = new List<int>();
					ActorData.Attribute.Add(Element1);
					ActorData.Attribute.Add(Element2);
					ActorData.Attribute.Add(Element3);
					ActorData.Attribute.Add(Element4);
					ActorData.Attribute.Add(Element5);

					int X = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.X);
					int Y = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.Y);
					float Scale = AssetPostImporter.ImportFloat(Baserow,(int)BaseColumn.Scale);
					int AwakenX = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.AwakenX);
					int AwakenY = AssetPostImporter.ImportNumeric(Baserow,(int)BaseColumn.AwakenY);
					float AwakenScale = AssetPostImporter.ImportFloat(Baserow,(int)BaseColumn.AwakenScale);
					ActorData.X = X;
					ActorData.Y = Y;
					ActorData.Scale = Scale;
					ActorData.AwakenX = AwakenX;
					ActorData.AwakenY = AwakenY;
					ActorData.AwakenScale = AwakenScale;

					
					Data.Data.Add(ActorData);
				}
				// 習得スキル情報設定
				BaseSheet = Book.GetSheetAt(1);
				for (int i = 1; i <= BaseSheet.LastRowNum; i++)
				{
					IRow Baserow = BaseSheet.GetRow(i);
					var LearningData = new LearningData();

					int ActorId = AssetPostImporter.ImportNumeric(Baserow,(int)BaseLearningColumn.ActorId);
					ActorData Actor = Data.Data.Find(a => a.Id == ActorId);
					
					LearningData.SkillId = AssetPostImporter.ImportNumeric(Baserow,(int)BaseLearningColumn.SkillId);
					Actor.LearningSkills.Add(LearningData);
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

