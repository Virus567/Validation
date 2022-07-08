using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Validation.Models;

namespace Validation.Controllers
{
    public class ExcelController : Controller
    {
        private readonly int[] _stringCells = { 4, 5, 8, 9, 10, 11, 12, 13, 14, 15, 20, 22, 23, 26, 28 };
        private readonly int[] _intCells = { 0, 1, 2, 3, 6, 7, 16 };
        private readonly int[] _dateCells = { 19, 21, 25 };
        private int _countCells = 29;
        int errorCount;

        private readonly ILogger<ExcelController> _logger;
        public ExcelController(ILogger<ExcelController> logger)
        {
            _logger = logger;
        }

        public IActionResult ExcelValidate()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ExcelValidate(FileModel model)
        {
            var file = Request.Form.Files.First();
            ViewBag.excel = Validate(file);
            return View("ExcelResult");
        }
        public IActionResult ExcelResult()
        {
            return View("ExcelValidate");
        }

        public string Validate(IFormFile file)
        {

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), $"fileExcel.xlsx");
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            string result = "";
            bool errorValidate = false;
            var workbook = new XLWorkbook(fullPath);
            var worksheet = workbook.Worksheet(1);
            errorCount = 0;
            var rows = worksheet.RangeUsed().RowsUsed().Skip(2);
            foreach (var row in rows)
            {
                for (int i = 0; i < _countCells; i++)
                {
                    result += ValidateRequiredCell(i, row);
                    result += ValidateSymbolsAmount(i, row);

                    if (_intCells.Contains(i) && row.Cell(i + 1).Value != "")
                    {
                        errorValidate = row.Cell(i + 1).DataType != XLDataType.Number;
                        errorValidate = !int.TryParse(row.Cell(i + 1).Value.ToString(), out int tmp);
                    }

                    if (_dateCells.Contains(i) && row.Cell(i + 1).Value != "")
                    {
                        errorValidate = row.Cell(i + 1).DataType != XLDataType.DateTime;
                        errorValidate = !DateTime.TryParse(row.Cell(i + 1).Value.ToString(), out DateTime tmp);
                    }

                    if (_stringCells.Contains(i) && row.Cell(i + 1).Value != "")
                    {
                        errorValidate = row.Cell(i + 1).DataType != XLDataType.Text;
                    }

                    if (errorValidate)
                    {
                        errorCount++;
                        result += $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} ячейка № {i} - неверного формата или имеет неверный тип\n";
                    }
                }

            }

            workbook.Dispose();
            System.IO.File.Delete(fullPath);
            if (result == "")
            {
                result = "Валидация прошла успешно!";
            }
            return result;
        }

        private string ValidateRequiredCell(int i, IXLRangeRow row)
        {
            string result = "";
            if (i >= 0 && i <= 5 && row.Cell(i + 1).Value == "")
            {
                errorCount++;
                return $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} ячейка № {i} - обязательна для заполнения\n";
            }

            if ((i == 6) && row.Cell(7).Value == "" && row.Cell(8).Value == "")
            {
                errorCount++;
                return $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} не заполнена ни одна ячейка из  ячеек 6 и 7\n";
            }
            if (i == 8)
            {
                List<bool> mainFlags = new List<bool>();
                mainFlags.Add(row.Cell(9).Value != "" && row.Cell(10).Value != "");
                mainFlags.Add(row.Cell(11).Value != "");
                mainFlags.Add(row.Cell(12).Value != "" && row.Cell(13).Value != "");
                mainFlags.Add(row.Cell(14).Value != "" && row.Cell(15).Value != "" && row.Cell(16).Value != "");

                List<bool> flags = new List<bool>();
                flags.Add(row.Cell(9).Value != "" || row.Cell(10).Value != "");
                flags.Add(row.Cell(11).Value != "");
                flags.Add(row.Cell(12).Value != "" || row.Cell(13).Value != "");
                flags.Add(row.Cell(14).Value != "" || row.Cell(15).Value != "" || row.Cell(16).Value != "");

                mainFlags = mainFlags.Where(x => x).ToList();
                flags = flags.Where(x => x).ToList();
                if (mainFlags.Count != 1 || flags.Count != 1)
                {
                    errorCount++;
                    return $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} должны быть заполнены либо ячейки 8 и 9, либо ячейка  10, либо ячейки 11,12, либо ячейки 13,14 и 15\n";
                }
            }

            if (i == 16)
            {
                if (row.Cell(17).Value != "" && (row.Cell(18).Value != "" || row.Cell(19).Value != ""))
                {
                    errorCount++;
                    return $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} может быть заполнена только одна ячейка из трех: 16,17 и 18\n";
                }
                if (row.Cell(18).Value != "" && (row.Cell(17).Value != "" || row.Cell(19).Value != ""))
                {
                    errorCount++;
                    return $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} может быть заполнена только одна ячейка из трех: 16,17 и 18\n";
                }
                if (row.Cell(19).Value != "" && (row.Cell(18).Value != "" || row.Cell(17).Value != ""))
                {
                    errorCount++;
                    return $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} может быть заполнена только одна ячейка из трех: 16,17 и 18\n";
                }
            }
            if (i == 19 && row.Cell(20).Value == "" && (row.Cell(17).Value != "" || row.Cell(18).Value != "" || row.Cell(19).Value != ""))
            {
                errorCount++;
                return $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} ячейка № {i} - обязательна для заполнения\n";
            }
            if (i == 19 && row.Cell(20).Value != "" && row.Cell(17).Value == "" && row.Cell(18).Value == "" && row.Cell(19).Value == "")
            {
                errorCount++;
                return $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} ячейка № {i} - не может быть заполнена, если ячейки 16, 17 или 18 пустые\n";
            }

            return result;
        }


        private string ValidateSymbolsAmount(int i, IXLRangeRow row)
        {
            string result = "";

            if ((i == 4 || i == 8)&& row.Cell(i + 1).Value.ToString().Length != 2)
            {
                errorCount++;
                return $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} длина ячейки № {i} должна быть 2 символа\n";
            }
            if(i==9 && row.Cell(i + 1).Value.ToString().Length != 10)
            {
                errorCount++;
                return $"{errorCount}. Ошибка валидации! В строке {row.RowNumber()} длина ячейки № {i} должна быть 10 символов\n";
            }
            return result;
        }
    }
}
