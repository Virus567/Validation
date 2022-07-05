using Microsoft.AspNetCore.Mvc;
using Validation.Models;

namespace Validation.Controllers
{
    public class ExcelController : Controller
    {
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
        public IActionResult XmlResult()
        {
            return View("ExcelValidate");
        }

        public string Validate(IFormFile file)
        {

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), $"fileExcel.xml");
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
          
            return fullPath;//Исправить
        }


    }
}
