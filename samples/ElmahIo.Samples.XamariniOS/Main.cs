using Elmah.Io.Xamarin;
using System;
using System.Threading.Tasks;
using UIKit;

namespace ElmahIo.Samples.XamariniOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");

            ElmahIoXamarin.Init(new ElmahIoXamarinOptions
            {
                ApiKey = "API_KEY",
                LogId = new Guid("LOG_ID"),
            });
        }
    }
}