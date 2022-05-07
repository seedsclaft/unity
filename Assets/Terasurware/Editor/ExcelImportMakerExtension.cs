using NPOI.SS.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExcelImportMakerExtension
{
    public static string SafeStringCellValue(this ICell self)
    {
        string str = "";
        if (self == null) { return str; }

        switch (self.CellType)
        {
            case CellType.Unknown:
                break;
            case CellType.Numeric:
                str = self.NumericCellValue.ToString();
                break;
            case CellType.String:
                str = self.StringCellValue;
                break;
            case CellType.Formula:
                break;
            case CellType.Blank:
                break;
            case CellType.Boolean:
                str = self.BooleanCellValue.ToString();
                break;
            case CellType.Error:
                break;
            default:
                break;
        }
        return str;
    }

    public static double SafeNumericCellValue(this ICell self, double initValue = 0.0f)
    {
        var value = initValue;
        if (self == null) { return value; }

        switch (self.CellType)
        {
            case CellType.Unknown:
                break;
            case CellType.Numeric:
                value = self.NumericCellValue;
                break;
            case CellType.String:
                break;
            case CellType.Formula:
                break;
            case CellType.Blank:
                break;
            case CellType.Boolean:
                break;
            case CellType.Error:
                break;
            default:
                break;
        }
        return value;
    }

    public static T SafeEnumCellValue<T>(this ICell self) where T: struct
    {
        T value = default;
        switch (self.CellType)
        {
            case CellType.Unknown:
                break;
            case CellType.Numeric:
                break;
            case CellType.String:
                Enum.TryParse(self.SafeStringCellValue(), true, out value);
                break;
            case CellType.Formula:
                break;
            case CellType.Blank:
                break;
            case CellType.Boolean:
                break;
            case CellType.Error:
                break;
            default:
                break;
        }
        return value;
    }

    public static bool SafeNumericCellValue(this ICell self, out double value)
    {
        if (self != null && self.CellType == CellType.Numeric)
        {
            value = SafeNumericCellValue(self, double.MaxValue);
            return true;
        }

        value = 0;
        return false;
    }
}