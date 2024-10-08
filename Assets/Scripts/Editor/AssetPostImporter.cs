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
	public class AssetPostImporter
	{
		static public string ExcelPath = "Assets/Data";
		static public string ExportExcelPath = "Assets/Resources/Data";

		static public bool CheckOnPostprocessAllAssets(string asset,string ExcelName)
		{
			string ext = Path.GetExtension(asset);
			if (ext != ".xls" && ext != ".xlsx" && ext != ".xlsm") return false;

			// エクセルを開いているデータはスキップ
			string fileName = Path.GetFileName(asset);
			if (fileName.StartsWith("~$")) return false;

			// 同じパスのみ
			string filePath = Path.GetDirectoryName(asset);
			filePath = filePath.Replace("\\", "/");
			if (filePath != AssetPostImporter.ExcelPath) return false;

			// 同じファイルのみ
			if (fileName != ExcelName) return false;
			return true;
		}

		public static int ImportNumeric(IRow BaseRow,int Column)
		{
			var cell = BaseRow.GetCell(Column);
			if (cell != null)
			{
				return (int)cell?.SafeNumericCellValue();
			}
			return 0;
		}

		public static int ImportNumeric(IRow BaseRow,string key)
		{
			var cell = BaseRow.GetCell(GetKeyNameIndex(key));
			if (cell != null)
			{
				return (int)cell?.SafeNumericCellValue();
			}
			return 0;
		}

		public static float ImportFloat(IRow BaseRow,int Column)
		{
			var cell = BaseRow.GetCell(Column);
			if (cell != null)
			{
				return (float)cell.SafeNumericCellValue();
			}
			return 0;
		}

		public static float ImportFloat(IRow BaseRow,string key)
		{
			var cell = BaseRow.GetCell(GetKeyNameIndex(key));
			if (cell != null)
			{
				return (float)cell.SafeNumericCellValue();
			}
			return 0;
		}

		public static string ImportString(IRow BaseRow,int Column)
		{
			var cell = BaseRow.GetCell(Column);
			if (cell != null)
			{
				return cell.SafeStringCellValue();
			}
			return "";
		}

		public static string ImportString(IRow BaseRow,string key)
		{
			var cell = BaseRow.GetCell(GetKeyNameIndex(key));
			if (cell != null)
			{
				return cell.SafeStringCellValue();
			}
			return "";
		}

		// エクセルワークブックを作成
		public static void CreateBook(string path, Stream stream, out IWorkbook Workbook)
		{
			// 拡張子が".xls"の場合
			if (Path.GetExtension(path) == ".xls")
			{
				Workbook = new HSSFWorkbook(stream);
			}
			// 拡張子がそれ以外の場合
			else
			{
				Workbook = new XSSFWorkbook(stream);
			}
		}

		// テキストデータを作成
		public static List<TextData> CreateText(ISheet BaseSheet)
		{
			var textData = new List<TextData>();

			for (int i = 1; i <= BaseSheet.LastRowNum; i++)
			{
				IRow BaseRow = BaseSheet.GetRow(i);
                var TextData = new TextData
                {
                    Id = ImportNumeric(BaseRow, (int)BaseTextColumn.Id),
                    Text = ImportString(BaseRow, (int)BaseTextColumn.Text),
                    Help = ImportString(BaseRow, (int)BaseTextColumn.Help),
                    Feature = ImportString(BaseRow, (int)BaseTextColumn.Feature),
                };

                textData.Add(TextData);
			}

			return textData;
		}

		// 文字列を分解
		static string[] StringSplit(string str, int count)
		{
			List<string> List = new List<string>();

			int Length = (int)Math.Ceiling((double)str.Length / count);

			for (int i = 0; i < Length; i++)
			{
				int Start = count * i;

				// 始まりが文字列の長さより多かったら
				if (str.Length <= Start)
				{
					break;
				}
				// 読み取る大きさが文字列の長さより多かったら終わりを指定しない
				if (str.Length < Start + count)
				{
					List.Add(str.Substring(Start));
				}
				// 始まりの位置と終わりの位置を指定（始まりの値は含むが終わりの値は含まない）
				else
				{
					List.Add(str.Substring(Start, count));
				}
			}

			return List.ToArray();
		}

		private static List<string> _formatKeys = new List<string>();

		public static void SetKeyNames(List<ICell> cells)
		{
			var keyNames = new List<string>();
			foreach (var cell in cells)
			{
				keyNames.Add(cell.ToString());
			}
			_formatKeys = keyNames;
		}

		public static int GetKeyNameIndex(string keyName)
		{
			return _formatKeys.FindIndex(a => a == keyName);
		}
	}

	public enum BaseTextColumn
	{
		Id = 0,
		Text,
		Help,
		Feature,
	}
}