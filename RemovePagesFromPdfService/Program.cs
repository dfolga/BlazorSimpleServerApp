using System;
using Topshelf;

namespace RemovePagesFromPdfAndMoveService
{
    class Program
    {
        static void Main(string[] args)
        {

            var rc = HostFactory.Run(x =>
            {
                x.Service<RemovePagesFromPdfAndMoveService>(s =>
                {
                    s.ConstructUsing(moveFilesService => new RemovePagesFromPdfAndMoveService());
                    s.WhenStarted(moveFilesService => moveFilesService.Start());
                    s.WhenStopped(moveFilesService => moveFilesService.Stop());
                });
                x.RunAsLocalSystem();
                x.UseNLog();

                x.SetServiceName("RemovePagesFromPdfService");
                x.SetDisplayName("RemovePagesFromPdfService");
                x.SetDescription("This is a service which is removing pages from pdf files based on JSON file.");
            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;

        }
    }
}
