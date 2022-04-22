using System.Configuration;
using System.ServiceProcess;

namespace WindowsServiceAppTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new WebserviceService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
