using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace MovingFilesService
{
    class Program
    {
        static void Main(string[] args)
        {

            var rc = HostFactory.Run(x =>
            {
                x.UseNLog();
                x.Service<MoveFilesService>(s =>
                {
                    s.ConstructUsing(moveFilesService => new MoveFilesService());
                    s.WhenStarted(moveFilesService => moveFilesService.Start());
                    s.WhenStopped(moveFilesService => moveFilesService.Stop());
                });

                x.RunAsLocalSystem();
               
                x.SetServiceName("MovingService");
                x.SetDisplayName("MovingService");
                x.SetDescription("This is a service which is moving files.");
            });

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            Environment.ExitCode = exitCode;
        }
    }
}
