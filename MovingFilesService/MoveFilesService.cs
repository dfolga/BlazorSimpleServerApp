using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Timers;

namespace MovingFilesService
{

    class MoveFilesService
    {
        private string SourceDirectory { get; set; }
        private string DestinationDirectory { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public string FilterFileType { get; set; }
        private System.Timers.Timer _timer;
        private static object s_lock = new object();
        public double timerIntervalInMs { get; set; }

        public MoveFilesService()
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
            _timer.Elapsed += new ElapsedEventHandler(MoveFilesEvent);
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _timer.Start();
        }
        public void MoveFilesEvent(object sender, ElapsedEventArgs e)
        {
            // Prevents the job firing until it finishes its job 
            if (Monitor.TryEnter(s_lock))
            {
                try
                {
                    try
                    {
                        var files = Directory.GetFiles(SourceDirectory, FilterFileType);
                        foreach (var path in files)
                        {
                            try
                            {
                                string fileName = Path.GetFileName(path);
                                if (File.Exists(Path.Combine(DestinationDirectory, fileName)))
                                {
                                    File.Move(path, (Path.Combine(DestinationDirectory, String.Concat((DateTime.Now.ToString("yyyy.MM.dd.THH.mm.ss.fff.tt")), fileName))));
                                }
                                else
                                    File.Move(path, DestinationDirectory + fileName);
                            }
                            catch (Exception ex)
                            {
                                logger.Info("Exception " + ex);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Info("Exception " + ex);
                    }
                }
                finally
                {
                    // unlock the job 
                    Monitor.Exit(s_lock);

                }
            }
            _timer.Interval = timerIntervalInMs;

        }

        public void Stop()
        {
            _timer.Stop();
            logger.Info("Service stopped!");
        }
    }
}
