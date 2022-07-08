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
        public XmlController(ILogger<XmlController> logger)
        {
            _logger = logger;
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
            var path = new Uri(Directory.GetCurrentDirectory());

            XmlReader reader = null;
            string result = "";
            try       
            {
                //XmlSchemaSet schema = new XmlSchemaSet();
                //schema.Add("urn://roskazna.ru/gisgmp/xsd/services/import-payments/2.4.0", path + "\\ImportPayments.xsd");
                //schema.Add("urn://roskazna.ru/gisgmp/xsd/services/import-charges/2.4.0", path + "\\ImportCharges.xsd");
                //schema.XmlResolver = new XmlUrlResolver();
                //XmlReader rd = XmlReader.Create(filePath);
                //XDocument doc = XDocument.Load(rd);
                //doc.Validate(schema, ValidationEventHandle);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas.Add("urn://roskazna.ru/gisgmp/xsd/services/import-payments/2.4.0", path + "\\ImportPayments.xsd");
                settings.Schemas.Add("urn://roskazna.ru/gisgmp/xsd/services/import-charges/2.4.0", path + "\\ImportCharges.xsd");
                settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                settings.ValidationEventHandler += new ValidationEventHandler(ValidationEventHandle);
                settings.XmlResolver = new XmlUrlResolver();
                reader = XmlReader.Create(filePath, settings);
                while (reader.Read()) { }
                result = "Валидация пройдена успешно!";
            }
            catch (Exception ex)
            {
                result = "Ошибка валидации: " + ex.Message;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();        
                    System.IO.File.Delete(fullPath);
                }
                
            }
            return result;
        }

        static void ValidationEventHandle(object sender, ValidationEventArgs e)
        {
            throw new Exception("Валидация не пройдена: " + e.Message);
        }
    }
}