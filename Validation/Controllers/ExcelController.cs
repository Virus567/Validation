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

            var rows = worksheet.RangeUsed().RowsUsed().Skip(2);

            foreach (var row in rows)
            {
                var cells = row.CellsUsed();
                for (int i = 0; i < cells.Count(); i++)
                {

                    if (_intCells.Contains(i) && row.Cell(i + 1).Value != "")
                    {
                        errorValidate = !int.TryParse(row.Cell(i + 1).Value.ToString(), out int tmp);
                    }
                    if (_dateCells.Contains(i) && row.Cell(i + 1).Value != "")
                    {
                        errorValidate = !DateOnly.TryParse(row.Cell(i + 1).Value.ToString(), out DateOnly tmp);
                    }
                    if (_stringCells.Contains(i) && row.Cell(i + 1).Value != "")
                    {
                        errorValidate = DateOnly.TryParse(row.Cell(i + 1).Value.ToString(), out DateOnly dateTmp);
                    }
                    if (errorValidate)
                    {
                        result = $"Ошибка валидации! В строке {row.RowNumber()} ячейка № {i} - неверного формата";
                    }
                    result = ValidateRequiredCell(i, row);
                    if (result != "" && result != "Валидация прошла успешно!")
                    {
                        break;
                    }
                }
            }

            workbook.Dispose();
            System.IO.File.Delete(fullPath);
            return result;
        }

        private string ValidateRequiredCell(int i, IXLRangeRow row)
        {
            string result = "Валидация прошла успешно!";
            if (i >= 0 && i <= 5 && row.Cell(i + 1).Value == "")
            {
                return $"Ошибка валидации! В строке {row.RowNumber()} ячейка № {i} - обязательна для заполнения";
            }

            if ((i == 6 || i == 7) && row.Cell(7).Value == "" && row.Cell(8).Value == "")
            {
                return $"Ошибка валидации! В строке {row.RowNumber()} не заполнена ни одна ячейка из  ячеек 6 и 7";
            }
            if (i >= 8 && i <= 15)
            {
                if (row.Cell(9).Value != "" && row.Cell(10).Value != "" &&
                  (row.Cell(11).Value != "" || row.Cell(12).Value != ""
                  || row.Cell(13).Value != "" || row.Cell(14).Value != ""
                  || row.Cell(15).Value != "" || row.Cell(16).Value != ""))
                {
                    return $"Ошибка валидации! В строке {row.RowNumber()} должны быть заполнены либо ячейки 8 и 9, либо ячейки 10,11,12,13,14 и 15";
                }
                if (row.Cell(11).Value != "" && row.Cell(12).Value != ""
                  && row.Cell(13).Value != "" && row.Cell(14).Value != ""
                  && row.Cell(15).Value != "" && row.Cell(16).Value != "" &&
                    (row.Cell(9).Value != "" || row.Cell(10).Value != ""))
                {
                    return $"Ошибка валидации! В строке {row.RowNumber()} должны быть заполнены либо ячейки 8 и 9, либо ячейки 10,11,12,13,14 и 15";
                }

                if ((row.Cell(9).Value == "" || row.Cell(10).Value == "") &&
                  (row.Cell(11).Value == "" || row.Cell(12).Value == "" ||
                  row.Cell(13).Value == "" || row.Cell(14).Value == "" ||
                  row.Cell(15).Value == "" || row.Cell(16).Value == ""))
                {
                    return $"Ошибка валидации! В строке {row.RowNumber()} должны быть заполнены либо ячейки 8 и 9, либо ячейки 10,11,12,13,14 и 15";
                }
            }

            if (i >= 16 && i < 18)
            {
                if (row.Cell(17).Value != "" && (row.Cell(18).Value != "" || row.Cell(19).Value != ""))
                {
                    return $"Ошибка валидации! В строке {row.RowNumber()} может быть заполнена только одна ячейка из трех: 16,17 и 18";
                }
                if (row.Cell(18).Value != "" && (row.Cell(17).Value != "" || row.Cell(19).Value != ""))
                {
                    return $"Ошибка валидации! В строке {row.RowNumber()} может быть заполнена только одна ячейка из трех: 16,17 и 18";
                }
                if (row.Cell(19).Value != "" && (row.Cell(18).Value != "" || row.Cell(17).Value != ""))
                {
                    return $"Ошибка валидации! В строке {row.RowNumber()} может быть заполнена только одна ячейка из трех: 16,17 и 18";
                }
            }
            if (i == 19 && row.Cell(20).Value == "" && (row.Cell(17).Value != "" || row.Cell(18).Value != "" || row.Cell(19).Value != ""))
            {
                return $"Ошибка валидации! В строке {row.RowNumber()} ячейка № {i} - обязательна для заполнения";
            }

            return result;
        }
    }
}
