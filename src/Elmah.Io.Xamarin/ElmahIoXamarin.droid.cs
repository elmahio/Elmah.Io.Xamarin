using Android.Runtime;
using System.Threading.Tasks;

namespace Elmah.Io.Xamarin
{
    public partial class ElmahIoXamarin
    {
        internal static void InitPlatform()
        {
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, e) =>
            {
                e.Exception.Log();
                e.Handled = true;
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                e.Exception.Log();
                e.SetObserved();
            };
        }
    }
}
