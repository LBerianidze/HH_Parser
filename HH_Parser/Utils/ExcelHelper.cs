using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HH_Parser
{
    public static class ExcelHelper
    {
        public static string GetCellValue(this ICell cell)
        {
            switch (cell?.CellType)
            {
                case CellType.Unknown:
                    return "";
                    break;
                case CellType.Numeric:
                    return cell.NumericCellValue.ToString();
                    break;
                case CellType.String:
                    return cell.StringCellValue;
                    break;
                case CellType.Formula:
                    return cell.CellFormula;
                    break;
                case CellType.Blank:
                    return "";
                    break;
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                    break;
                case CellType.Error:
                    return cell.ErrorCellValue.ToString();
                    break;
                default:
                    return "";
                    break;
            }
        }
    }
}