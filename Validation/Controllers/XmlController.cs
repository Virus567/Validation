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
            string res = "";
            XmlReader reader = null;
            string result = "";
            try       
            {
                XmlSchemaSet schema = new XmlSchemaSet();
                schema.Add(new XmlSchema
                {
                    SourceUri = path + "\\ImportPayments.xsd",
                    TargetNamespace = "urn://roskazna.ru/gisgmp/xsd/services/import-payments/2.4.0"
                });

                schema.Add(new XmlSchema
                {
                    SourceUri = path + "\\ImportCharges.xsd",
                    TargetNamespace = "urn://roskazna.ru/gisgmp/xsd/services/import-charges/2.4.0"
                });
                schema.XmlResolver = new XmlUrlResolver();
                reader = XmlReader.Create(filePath);
                XDocument doc = XDocument.Load(reader);
                doc.Validate(schema, ValidationEventHandle);

                //XmlReaderSettings settings = new XmlReaderSettings();
                //settings.ValidationType = ValidationType.Schema;
                //settings.Schemas.Add(new XmlSchema
                //{
                //    SourceUri = path + "\\ImportPayments.xsd",
                //    TargetNamespace = "urn://roskazna.ru/gisgmp/xsd/services/import-payments/2.4.0"
                //});
                //settings.Schemas.Add(new XmlSchema
                //{
                //    SourceUri = path + "\\ImportCharges.xsd",
                //    TargetNamespace = "urn://roskazna.ru/gisgmp/xsd/services/import-charges/2.4.0"
                //});
                //settings.Schemas.Compile();
                //settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
                //settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                //settings.ValidationEventHandler += new ValidationEventHandler(ValidationEventHandle);
                //reader = XmlReader.Create(filePath, settings);
                //while (reader.Read()) {
                //    res = reader.Value;
                //}
                result = "Валидация пройдена успешно!";
            }
            catch (Exception ex)
            {
                result = $"Ошибка валидации! " + ex.Message;
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
            throw new Exception(e.Message);
        }
    }
}