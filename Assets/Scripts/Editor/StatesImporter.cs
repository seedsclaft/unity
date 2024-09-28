
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using NPOI.SS.UserModel;

namespace Ryneus
{
	public class StatesImporter : AssetPostprocessor 
	{
		static readonly string ExcelName = "States.xlsx";

		// アセット更新があると呼ばれる
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			foreach (string asset in importedAssets) 
			{
				if (AssetPostImporter.CheckOnPostprocessAllAssets(asset,ExcelName))
				{
					CreateStateInfo(asset);
					AssetDatabase.SaveAssets();
					return;
				}
			}
		}


		static void CreateStateInfo(string asset)
		{
			// 拡張子なしのファイル名を取得
			string FileName = Path.GetFileNameWithoutExtension(asset);

			// ディレクトリ情報とファイル名の文字列を結合してアセット名を指定
			string ExportPath = $"{Path.Combine(AssetPostImporter.ExportExcelPath, FileName)}.asset";

			StateDates Data = AssetDatabase.LoadAssetAtPath<StateDates>(ExportPath);
			if (!Data)
			{
				// データがなければ作成
				Data = ScriptableObject.CreateInstance<StateDates>();
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
					List<TextData> textData = AssetPostImporter.CreateText(Book.GetSheetAt(1));

					// 情報の初期化
					Data.Data.Clear();

					// エクセルシートからセル単位で読み込み
					ISheet BaseSheet = Book.GetSheetAt(0);
					var KeyRow = BaseSheet.GetRow(0);
					AssetPostImporter.SetKeyNames(KeyRow.Cells);

					for (int i = 1; i <= BaseSheet.LastRowNum; i++)
					{
						IRow BaseRow = BaseSheet.GetRow(i);

                        var StateData = new StateData
                        {
                            StateType = (StateType)AssetPostImporter.ImportNumeric(BaseRow, "Id"),
                            Name = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Text,
                            Help = textData.Find(a => a.Id == AssetPostImporter.ImportNumeric(BaseRow, "NameId")).Help,
                            IconPath = AssetPostImporter.ImportString(BaseRow, "IconIndex"),
                            RemovalTiming = (RemovalTiming)AssetPostImporter.ImportNumeric(BaseRow, "RemovalTiming"),
                            OverWrite = AssetPostImporter.ImportNumeric(BaseRow, "OverWrite") == 1,
                            EffectPath = AssetPostImporter.ImportString(BaseRow, "EffectPath"),
                            EffectPosition = (EffectPositionType)AssetPostImporter.ImportNumeric(BaseRow, "EffectPosition"),
                            EffectScale = AssetPostImporter.ImportFloat(BaseRow, "EffectScale"),
                            OverLap = AssetPostImporter.ImportNumeric(BaseRow, "OverLap"),
                            Removal = AssetPostImporter.ImportNumeric(BaseRow, "Removal") == 1,
                            Abnormal = AssetPostImporter.ImportNumeric(BaseRow, "Abnormal") == 1,
                            Buff = AssetPostImporter.ImportNumeric(BaseRow, "Buff") == 1,
                            DeBuff = AssetPostImporter.ImportNumeric(BaseRow, "DeBuff") == 1,
                            CheckHit = AssetPostImporter.ImportNumeric(BaseRow, "HitCheck") == 1,
                            RemoveByAttack = AssetPostImporter.ImportNumeric(BaseRow, "RemoveByAttack") == 1,
                            RemoveByDeath = AssetPostImporter.ImportNumeric(BaseRow, "RemoveByDeath") == 1
                        };

                        Data.Data.Add(StateData);
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