using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovingFilesService
{
    class MoveFilesService
    {
        private string SourceDirectory { get; set; }
        private string DestinationDirectory { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public string FilterFileType { get; set; }

        public MoveFilesService()
        {
            SourceDirectory= ConfigurationManager.AppSettings["sourceDirectory"]; 
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
            try {
                var files = Directory.GetFiles(SourceDirectory, FilterFileType);
                foreach (var path in files)
                {
                    try
                    {
                        string fileName = Path.GetFileName(path);
                        if (File.Exists(DestinationDirectory + fileName)) {
                            File.Move(path, DestinationDirectory + new FileInfo(path).CreationTime.ToString("yyyy.MM.dd.THH.mm.ss.fff.tt") + fileName);
                        }
                        else
                            File.Move(path, DestinationDirectory + fileName);
                    }
                    catch (Exception ex) {
                        logger.Info("Exception "+ex);
                    }
                    
                }
            }
            catch (Exception ex) {
                logger.Info("Exception " + ex);
            }
            finally {
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
                DirectoryMove(SourceDirectory, DestinationDirectory);
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
                DirectoryMove(SourceDirectory, DestinationDirectory);
            }
            catch (Exception ex)
            {
                logger.Info("Exception " + ex);
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            try {
                DirectoryMove(SourceDirectory, DestinationDirectory);
            }
            catch (InvalidOperationException ex) {
                logger.Info("Exception " + ex);
            }
            catch (Exception ex)
            {
                logger.Info("Exception " + ex);
            }
        }
        private  void DirectoryMove(string sourceDirectory, string destinationDirectory)
        {
                DirectoryInfo dir = new DirectoryInfo(sourceDirectory);

                if (!dir.Exists)
                {
                logger.Info("Source directory does not exist or could not be found: " + sourceDirectory);
                throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirectory); 
            }

                DirectoryInfo[] dirs = dir.GetDirectories();
   
                Directory.CreateDirectory(destinationDirectory);

                FileInfo[] files = dir.GetFiles(FilterFileType);

                var file = files.OrderByDescending(f => f.LastWriteTime).First();

                string tempPath = Path.Combine(destinationDirectory, file.Name);

                file.MoveTo(tempPath);

        }

        public void Stop()
        {
            logger.Info("Service stopped!");
        }
    }
}
