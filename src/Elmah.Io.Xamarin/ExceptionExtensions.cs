using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using System;
using System.Collections.Generic;
using Xamarin.Essentials;

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
            if (exception == null) return;

            var elmahIoXamarin = ElmahIoXamarin.Instance;
            var options = elmahIoXamarin.Options;

            var baseException = exception?.GetBaseException();
            var errorMessage = baseException?.Message ?? "Unhandled exception";

            var createMessage = new CreateMessage
            {
                Data = Data(exception),
                ServerVariables = ServerVariables(),
                DateTime = DateTime.UtcNow,
                Detail = exception?.ToString(),
                Severity = Severity.Error.ToString(),
                Source = baseException?.Source,
                Title = errorMessage,
                Type = baseException?.GetType().FullName,
                Version = Version(options),
                Application = Application(options),
            };

            if (options.OnFilter != null && options.OnFilter(createMessage))
            {
                return;
            }

            elmahIoXamarin.ElmahIoClient.Messages.CreateAndNotify(options.LogId, createMessage);
        }

        private static IList<Item> ServerVariables()
        {
            var serverVariables = new List<Item>();
            try
            {
                // Generate a pseudo user agent for elmah.io to use for grouping, search, etc.
                serverVariables.Add(new Item("User-Agent", $"X-ELMAHIO-MOBILE; OS={DeviceInfo.Platform}; OSVERSION={DeviceInfo.VersionString}; ENGINE=Xamarin"));
            }
            catch {}
            return serverVariables;
        }

        private static List<Item> Data(Exception exception)
        {
            var data = exception?.ToDataList() ?? new List<Item>();
            data.Add(new Item("X-ELMAHIO-SEARCH-isMobile", "true"));
            try
            {
                var width = (int)DeviceDisplay.MainDisplayInfo.Width;
                var height = (int)DeviceDisplay.MainDisplayInfo.Height;
                var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
                if (width > 0) data.Add(new Item("Screen-Width", width.ToString()));
                if (height > 0) data.Add(new Item("Screen-Height", height.ToString()));
                if (orientation != DisplayOrientation.Unknown) data.Add(new Item("Screen-Orientation", orientation == DisplayOrientation.Landscape ? "landscape" : "portrait"));
                data.Add(new Item("X-ELMAHIO-DevicePlatform", DeviceInfo.Platform.ToString()));
                data.Add(new Item("X-ELMAHIO-DeviceType", DeviceInfo.DeviceType.ToString()));
                data.Add(new Item("X-ELMAHIO-DeviceManufacturer", DeviceInfo.Manufacturer));
                data.Add(new Item("X-ELMAHIO-DeviceModel", DeviceInfo.Model));
                data.Add(new Item("X-ELMAHIO-DeviceName", DeviceInfo.Name));
                data.Add(new Item("X-ELMAHIO-DeviceVersion", DeviceInfo.VersionString));
            }
            catch {}
            return data;
        }

        private static string Version(ElmahIoXamarinOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Version)) return options.Version;
            try
            {
                return AppInfo.VersionString;
            }
            catch {}

            return null;
        }

        private static string Application(ElmahIoXamarinOptions options)
        {
            if (!string.IsNullOrWhiteSpace(options.Application)) return options.Application;

            try
            {
                return AppInfo.PackageName;
            }
            catch {}

            return null;
        }
    }
}
