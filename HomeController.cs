using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{

    public class HomeController : Controller
    {
        string _basePath = @".\FileContent\";

        public string method(string id)
        {
            return "";
        }

        public FileResult GetResourceFile(string id)
        {
            string fileNameWithExtension = id;
            string ext = Path.GetExtension(fileNameWithExtension);
            
            string mimeType = "";
            switch (ext.ToLower())
            {
                case "":
                    mimeType = "text/plain";
                    break;
                case ".txt":
                    mimeType = "text/plain";
                    break;
                case ".pdf":
                    mimeType = "application/pdf";
                    break;
                case ".jpg":
                    mimeType = "image/jpeg";
                    break;
                case ".jpeg":
                    mimeType = "image/jpeg";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                case ".tif":
                    mimeType = "image/tif";
                    break;
                case ".bmp":
                    mimeType = "image/bmp";
                    break;
                case ".html":
                    mimeType = "text/html";
                    break;
                default:
                    mimeType = "text/plain";
                    break;
            }

            string filePath = Path.Combine(_basePath, fileNameWithExtension);
            byte[] bytes;
            if (System.IO.File.Exists(filePath))
            {
                bytes = System.IO.File.ReadAllBytes(filePath);
            }
            else
            {
                mimeType = "text/plain";
                bytes = System.IO.File.ReadAllBytes(Path.Combine(_basePath, "filenotfounderrormessage.txt"));
            }

            MemoryStream ms = new MemoryStream(bytes);
            return File(ms, mimeType);

        }

        private List<string> SaveUploadedFiles(ICollection<IFormFile> files)
        {
            List<string> imageInputURL = new List<string>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        System.IO.File.WriteAllBytes(Path.Combine(_basePath, file.FileName), fileBytes);
                        imageInputURL.Add(file.FileName);
                    }
                }
            }

            return imageInputURL;
        }

        private string ProcessedFileAndGetOutputFilename(string inputFilename, string servicename)
        {
            string retVal = "";
            try
            {
                HttpClient client = new HttpClient();
                string baseAddress = "http://localhost:9000/";

                string param = servicename + "~" + inputFilename;
                var response = client.GetAsync(baseAddress + "api/values/" + param).Result;

                byte[] fileBytes = response.Content.ReadAsByteArrayAsync().Result;
                string filetypeExtn = Path.GetExtension(response.Content.Headers.ContentDisposition.FileName);
                MemoryStream ms = new MemoryStream(fileBytes);
                string outputFilename = Path.Combine(_basePath, "o_" + Path.GetFileNameWithoutExtension(inputFilename) + filetypeExtn);
                System.IO.File.WriteAllBytes(outputFilename, fileBytes);
                retVal = Path.GetFileName(outputFilename);
            }
            catch(Exception ee)
            {
                retVal = "filenotfounderrormessage.txt";
            }
            return retVal;
        }

        public IActionResult UploadFiles(ICollection<IFormFile> files, string id)
        {
            string callingViewName = id;

            string inputfilename = "";
            try
            {
                inputfilename = SaveUploadedFiles(files).ToArray()[0];
            }
            catch (Exception)
            {
                inputfilename = "filenotfounderrormessage.txt";
            }

            ViewBag.InputFrameContent = ("/Home/GetResourceFile/" + inputfilename);

            ViewBag.InputFrameContent1 = ("/Home/GetResourceFile/" + "1.jpg");
            ViewBag.InputFrameContent2 = ("/Home/GetResourceFile/" + "2.jpg");
            ViewBag.InputFrameContent3 = ("/Home/GetResourceFile/" + "3.jpg");

            ViewBag.OutputFrameContent1 = ("/Home/GetResourceFile/" + "1.txt");
            ViewBag.OutputFrameContent2 = ("/Home/GetResourceFile/" + "2.txt");
            ViewBag.OutputFrameContent3 = ("/Home/GetResourceFile/" + "3.txt");


            ViewBag.OutputFrameContent = ("/Home/GetResourceFile/" + ProcessedFileAndGetOutputFilename(inputfilename, callingViewName));

            ViewData["flag"] = true; 
            return View(callingViewName);
        }


        public IActionResult NewChart()
        {
            List<object> iData = new List<object>();
            //Creating sample data  
            DataTable dt = new DataTable();
            dt.Columns.Add("Employee", System.Type.GetType("System.String"));
            dt.Columns.Add("Credit", System.Type.GetType("System.Int32"));
            dt.Columns.Add("Debit", System.Type.GetType("System.Int32"));

            DataRow dr = dt.NewRow();
            dr["Employee"] = "Sam";
            dr["Credit"] = 123;
            dr["Debit"] = 155;
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["Employee"] = "Alex";
            dr["Credit"] = 456;
            dr["Debit"] = 355;
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["Employee"] = "Michael";
            dr["Credit"] = 587;
            dr["Debit"] = 155;
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["Employee"] = "Yogesh";
            dr["Credit"] = 74;
            dr["Debit"] = 655;
            dt.Rows.Add(dr);

            //Looping and extracting each DataColumn to List<Object>  
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }
            //Source data returned as JSON  
            ViewBag.NewData = JsonConvert.SerializeObject(iData);// Json(iData);
            return View("y_ui_classification_text", ViewBag ); //, JsonRequestBehavior.AllowGet);
        }

        public IActionResult Dashboard()
        {
            return View("Dashboard");
        }

        public IActionResult Template()
        {
            return View("Template");
        }

        public IActionResult PdfProSplitting()
        {
            return View("Template");
        }

        public IActionResult PdfProRasterization()
        {
            return View("Template");
        }

        public IActionResult PdfProConversion()
        {
            return View("PdfProConversion");
        }

        public IActionResult ImgProBinarization()
        {
            return View("ImgProBinarization");
        }

        public IActionResult ImgProGreyscale()
        {
            return View("Template");
        }

        public IActionResult ImgProColorDropout()
        {
            return View("Template");
        }

        public IActionResult ImgProRgbSeperation()
        {
            return View("Template");
        }

        public IActionResult ImgProVectorization()
        {
            return View("Template");
        }

        public IActionResult ImgProCleanup()
        {
            return View("Template");
        }

        public IActionResult ImgProDeskew()
        {
            return View("Template");
        }

        public IActionResult ImgProNoiseRemoval()
        {
            return View("Template");
        }

        public IActionResult ImgProCropping()
        {
            return View("Template");
        }

        public IActionResult ImgProConvertResolution()
        {
            return View("Template");
        }

        public IActionResult OcrProFullPageOCR()
        {
            return View("Template");
        }

        public IActionResult OcrProMarkup()
        {
            return View("Template");
        }

        public IActionResult OcrProMachinePrint()
        {
            return View("Template");
        }

        public IActionResult OcrProHandwritten()
        {
            return View("Template");
        }

        public IActionResult DocAnaLogoExraction()
        {
            return View("Template");
        }

        public IActionResult DocAnaSignature_extraction()
        {
            return View("Template");
        }

        public IActionResult DocAnaFeatureExtraction()
        {
            return View("Template");
        }

        public IActionResult DocAnaPatternMatch()
        {
            return View("Template");
        }

        public IActionResult DocAnaClassificationText()
        {
            return View("Template");
        }

        public IActionResult DocAnaPositionOfWords()
        {
            return View("Template");
        }

        public IActionResult DocAnaContentComprehension()
        {
            return View("Template");
        }

        public IActionResult MLProObjectDectection()
        {
            return View("MLProObjectDetection");
        }

        public IActionResult MLProFaceDetection()
        {
            return View("MLProFaceDetection");
        }

        public IActionResult MLProFaceRecognition()
        {
            return View("MLProFaceRecognition");
        }

        public IActionResult MLProClassificationImg()
        {
            return View("Template");
        }

        public IActionResult MLProSignatureMatching()
        {
            return View("Template");
        }

        public IActionResult MLProRelationExtraction()
        {
            return View("Template");
        }

        public IActionResult MLProDeepNeuralNetwork()
        {
            return View("Template");
        }

        public IActionResult NLProTextClassification()
        {
            return View("Template");
        }

        public IActionResult NLProRelation_extraction()
        {
            return View("Template");
        }

        public IActionResult NLProSentimentAnalysis()
        {
            return View("Template");
        }

        public IActionResult NLProNGramAnalysis()
        {
            return View("Template");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
