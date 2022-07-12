using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Validation.Models;

namespace Validation.Controllers
{
    public class XmlController : Controller
    {
        private readonly ILogger<XmlController> _logger;
        private readonly XmlSchemaSet _schemas;
        private static List<string> _errors;
        private static int _errorCount;
        public XmlController(ILogger<XmlController> logger)
        {
            _errors = new List<string>();
            _errorCount = 0;
            _logger = logger;
            _schemas = new XmlSchemaSet();
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\ImportCharges.xsd"), ValidationEventHandle!)!);
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\ImportPayments.xsd"), ValidationEventHandle!)!);
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\commons\\Common.xsd"), ValidationEventHandle!)!);
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\commons\\Package.xsd"), ValidationEventHandle!)!);
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\commons\\Charge.xsd"), ValidationEventHandle!)!);
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\commons\\Clarification.xsd"), ValidationEventHandle!)!);
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\commons\\Income.xsd"), ValidationEventHandle!)!);
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\commons\\Payment.xsd"), ValidationEventHandle!)!);
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\commons\\Organization.xsd"), ValidationEventHandle!)!);
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\commons\\Refund.xsd"), ValidationEventHandle!)!);
            _schemas.Add(XmlSchema.Read(new XmlTextReader(Directory.GetCurrentDirectory() + "\\commons\\Renouncement.xsd"), ValidationEventHandle!)!);
        }

        public IActionResult XmlValidate()
        {
            return View();
        }
        [HttpPost]
        public IActionResult XmlValidate(FileModel model)
        {
            var file = Request.Form.Files.First();
            ViewBag.result = Validate(file);
            return View("XmlResult");
        }
        public IActionResult XmlResult()
        {
            return View("XmlValidate");
        }

        public string Validate(IFormFile file)
        {

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), $"file.xml");
            var filePath = new Uri(fullPath).LocalPath;
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
            XmlReader reader = null;
            string result = "";
            try
            {

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = _schemas;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += ValidationEventHandle!;
                reader = XmlReader.Create(filePath, settings);
                while (reader.Read())
                {
                }
            }
            catch (Exception ex)
            {
                _errorCount++;
                result =$"{_errorCount}. Ошибка валидации!{ex.Message} \n";

            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    System.IO.File.Delete(fullPath);
                }

            }

            if (_errors.Count == 0 && result=="")
            {
                result = "Валидация прошла успешно!";
            }
        
            foreach (var error in _errors)
            {
                result += error + "\n";
            }
            return result;

        }


        static void ValidationEventHandle(object sender, ValidationEventArgs e)
        {
            _errorCount++;
            _errors.Add($"{_errorCount}. Ошибка валидации! В строке {e.Exception.LineNumber}, позиция {e.Exception.LinePosition} - {e.Message}");
        }
    }
}