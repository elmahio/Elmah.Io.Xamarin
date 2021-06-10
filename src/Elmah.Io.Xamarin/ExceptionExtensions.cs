using System;

namespace Elmah.Io.Xamarin
{
    /// <summary>
    /// Contains extension methods for easily logging errors to elmah.io from an exception.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Log the provided exception to elmah.io. This method requires an initialized ElmahIoXamarin instance by calling the ElmahIoXamarin.Init method.
        /// </summary>
        public static void Log(this Exception exception)
        {
            ElmahIoXamarin.Log(exception);
        }
    }
}
