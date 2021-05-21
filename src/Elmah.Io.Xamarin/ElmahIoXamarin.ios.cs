using System;
using System.Threading.Tasks;

namespace Elmah.Io.Xamarin
{
    public partial class ElmahIoXamarin
    {
        internal static void InitPlatform()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                (e.ExceptionObject as Exception).Log();
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                e.Exception.Log();
                e.SetObserved();
            };
        }
    }
}
