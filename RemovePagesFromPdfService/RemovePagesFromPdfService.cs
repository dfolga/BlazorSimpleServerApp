using Newtonsoft.Json;
using NLog;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Threading;
using System.Timers;

namespace RemovePagesFromPdfAndMoveService
{
    class RemovePagesFromPdfAndMoveService
    {
        private string SourceDirectory { get; set; }
        private string DestinationDirectory { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public string FilterFileType { get; set; }
        private System.Timers.Timer _timer;
        private static object s_lock = new object();
        public double timerIntervalInMs { get; set; }
        public RemovePagesFromPdfAndMoveService()
        {
            SourceDirectory = ConfigurationManager.AppSettings["sourceDirectory"];
            DestinationDirectory = ConfigurationManager.AppSettings["destinationDirectory"];
            FilterFileType = ConfigurationManager.AppSettings["filterFileType"];
            timerIntervalInMs = Double.Parse(ConfigurationManager.AppSettings["timerIntervalInMs"]);
            if (string.IsNullOrEmpty(SourceDirectory) || string.IsNullOrEmpty(DestinationDirectory) || string.IsNullOrEmpty(FilterFileType) || string.IsNullOrEmpty(ConfigurationManager.AppSettings["timerIntervalInMs"]))
            {
                logger.Info("Missing config arguments");
            }
        }

        public void Start()
        {
            logger.Info("Service started!");
            _timer = new System.Timers.Timer();
            _timer.Interval = 10;
            _timer.Elapsed += new ElapsedEventHandler(RemovePagesFromPdfAndMoveEvent);
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Start();
        }
        public void RemovePagesFromPdfAndMoveEvent(object sender, ElapsedEventArgs e)
        {
            // Prevents the job firing until it finishes its job 
            if (Monitor.TryEnter(s_lock))
            {

                try
                {
                    var files = Directory.GetFiles(SourceDirectory, FilterFileType);
                    if (files.Length > 0)
                    {
                        foreach (var path in files)
                        {
                            RemovePages(path);
                            FileInfo jsonFileInfo = new FileInfo(path);
                            string tempPath = Path.Combine(DestinationDirectory, jsonFileInfo.Name);
                            File.Move(Path.Combine(SourceDirectory, jsonFileInfo.Name.Replace(".json", ".pdf")), Path.Combine(DestinationDirectory, jsonFileInfo.Name.Replace(".json", ".pdf")));
                            jsonFileInfo.MoveTo(tempPath);
                        }
                    }


                }
                catch (Exception ex)
                {
                    logger.Info("Exception " + ex);
                }
                finally
                {
                    // unlock the job 
                    Monitor.Exit(s_lock);

                }
            }
            _timer.Interval = timerIntervalInMs;
        }
        private void RemovePages(string filePath)
        {
            try
            {
                int[] pagesToRemove = ExtractPagesToRemove(filePath);
                string jsonFilePath = filePath;
                string pdfFilePath = filePath.Replace(".json", ".pdf");
                if (File.Exists(pdfFilePath) && pagesToRemove.Length>0)
                {

                    FileInfo fileInfo = new FileInfo(pdfFilePath);
                    if (pagesToRemove != null)
                    {
                        PdfDocument inputDocument = PdfReader.Open((Path.Combine(SourceDirectory, fileInfo.Name.Replace(".json", ".pdf"))), PdfDocumentOpenMode.Modify);
                        int helperCounter = 0;
                        foreach (var page in pagesToRemove)
                        {
                            inputDocument.Pages.RemoveAt((page - 1) - helperCounter);
                            helperCounter++;
                        }
                        inputDocument.Save(Path.Combine(SourceDirectory, fileInfo.Name.Replace(".json", ".pdf")));
                    }
                }
                ResetPagesFieldInJson(jsonFilePath);
                
            }
            catch (Exception ex)
            {
                logger.Info("Exception " + ex);
            }



        }

        public void ResetPagesFieldInJson(string jsonFilePath) {
            try
            {
                string result;
                using (StreamReader r = new StreamReader(jsonFilePath))
                {
                    string json = r.ReadToEnd();
                    dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(json);
                    obj.PagesToRemove = "";
                    result = JsonConvert.SerializeObject(obj, Formatting.Indented);
                }
                if (result != null)
                {
                    using (FileStream fs = File.Create(jsonFilePath))
                    {
                        using (StreamWriter sr = new StreamWriter(fs)) {
                            sr.Write(result);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        class PagesFromJson
        {
            public string PagesToRemove { get; set; }
        }
        private int[] ExtractPagesToRemove(string filePath)
        {

            PagesFromJson pagesFromJson = new PagesFromJson();
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                pagesFromJson = JsonConvert.DeserializeObject<PagesFromJson>(json);
            }
            string pagesToRemoveField = pagesFromJson.PagesToRemove;
            if (!String.IsNullOrEmpty(pagesToRemoveField))
            {   
                string[] result = pagesToRemoveField.Split(',');
                return Array.ConvertAll(result, s => int.Parse(s));
            }
            else return new int[0];
        }
        public void Stop()
        {
            _timer.Stop();
            logger.Info("Service stopped!");
        }
    }
}
