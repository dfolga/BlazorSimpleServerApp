using Newtonsoft.Json;
using NLog;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemovePagesFromPdfAndMoveService
{
    class RemovePagesFromPdfAndMoveService
    {
        private string SourceDirectory { get; set; }
        private string DestinationDirectory { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public string FilterFileType { get; set; }

        public RemovePagesFromPdfAndMoveService()
        {
            SourceDirectory = ConfigurationManager.AppSettings["sourceDirectory"];
            DestinationDirectory = ConfigurationManager.AppSettings["destinationDirectory"];
            FilterFileType = ConfigurationManager.AppSettings["filterFileType"];
            if (string.IsNullOrEmpty(SourceDirectory) || string.IsNullOrEmpty(DestinationDirectory) || string.IsNullOrEmpty(FilterFileType))
            {
                logger.Info("Missing config arguments");
            }
        }

        public void Start()
        {
            logger.Info("Service started!");



            try
            {
                var files = Directory.GetFiles(SourceDirectory, FilterFileType);
                foreach (var path in files)
                {
                    RemovePages(path);
                    FileInfo jsonFileInfo = new FileInfo(path);
                    string tempPath = Path.Combine(DestinationDirectory, jsonFileInfo.Name);
                    File.Move(Path.Combine(SourceDirectory, jsonFileInfo.Name.Replace(".json", ".pdf")), Path.Combine(DestinationDirectory, jsonFileInfo.Name.Replace(".json", ".pdf")));
                    jsonFileInfo.MoveTo(tempPath);
                }
               
            }
            catch (Exception ex)
            {
                logger.Info("Exception " + ex);
            }
            finally
            {
                var watcher = new FileSystemWatcher(SourceDirectory);

                watcher.NotifyFilter = NotifyFilters.Attributes
                                     | NotifyFilters.CreationTime
                                     | NotifyFilters.DirectoryName
                                     | NotifyFilters.FileName
                                     | NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.Security
                                     | NotifyFilters.Size;

                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.Renamed += OnRenamed;
                watcher.Error += OnError;

                watcher.Filter = FilterFileType;

                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;
            }

        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            logger.Info("Error " + e);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            try
            {
                RemovePagesAndMove(SourceDirectory, DestinationDirectory);
            }
            catch (Exception ex)
            {
                logger.Info("Exception " + ex);
            }
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                RemovePagesAndMove(SourceDirectory, DestinationDirectory);
            }
            catch (Exception ex)
            {
                logger.Info("Exception " + ex);
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                RemovePagesAndMove(SourceDirectory, DestinationDirectory);
            }
            catch (InvalidOperationException ex)
            {
                logger.Info("Exception " + ex);
            }
            catch (Exception ex)
            {
                logger.Info("Exception " + ex);
            }
        }
        private void RemovePagesAndMove(string sourceDirectory, string destinationDirectory)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirectory);

                if (!dir.Exists)
                {
                    logger.Info("Source directory does not exist or could not be found: " + sourceDirectory);
                    throw new DirectoryNotFoundException(
                            "Source directory does not exist or could not be found: "
                            + sourceDirectory);
                }

                Directory.CreateDirectory(destinationDirectory);

                FileInfo[] files = dir.GetFiles(FilterFileType);

                var file = files.OrderByDescending(f => f.LastWriteTime).First();

                string tempSourcePath = Path.Combine(sourceDirectory, file.Name);
                RemovePages(tempSourcePath);
                string tempPath = Path.Combine(destinationDirectory, file.Name);
                File.Move(Path.Combine(SourceDirectory, file.Name.Replace(".json", ".pdf")),Path.Combine(DestinationDirectory, file.Name.Replace(".json", ".pdf")));
                file.MoveTo(tempPath);
            }
            catch (Exception ex) {
                logger.Info("Exception " + ex);
            }


        }
        private void RemovePages(string filePath)
        {
            try
            {
                int[] pagesToRemove = ExtractPagesToRemove(filePath);
                string pdfFilePath = filePath.Replace(".json", ".pdf");
                if (File.Exists(pdfFilePath))
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
            }
            catch (Exception ex)
            {
                logger.Info("Exception " + ex);
            }
            

            
        }
        class PagesFromJson
        {
            public string PagesToRemove { get; set; }
        }
        private int[] ExtractPagesToRemove(string filePath) {
            
                PagesFromJson pagesFromJson = new PagesFromJson();
                using (StreamReader r = new StreamReader(filePath))
                {
                    string json = r.ReadToEnd();
                    pagesFromJson = JsonConvert.DeserializeObject<PagesFromJson>(json);
                }
                string pagesToRemoveField = pagesFromJson.PagesToRemove;
                string[] result = pagesToRemoveField.Split(',');
                return Array.ConvertAll(result, s => int.Parse(s));
            }
        public void Stop()
        {
            logger.Info("Service stopped!");
        }
    }
}
