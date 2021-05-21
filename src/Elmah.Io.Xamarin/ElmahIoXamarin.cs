using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Xamarin.Essentials;

[assembly: InternalsVisibleTo("Elmah.Io.Xamarin.Test")]

namespace Elmah.Io.Xamarin
{
    /// <summary>
    /// This class is used to configure logging to elmah.io from Xamarin.
    /// </summary>
    public partial class ElmahIoXamarin
    {
        internal static string _assemblyVersion = typeof(ElmahIoXamarin).Assembly.GetName().Version.ToString();
        private static ElmahIoXamarin instance;
        private static readonly object padlock = new object();
        private const int MaximumBreadcrumbCount = 10;
        private List<Breadcrumb> breadcrumbs = new List<Breadcrumb>();

        /// <summary>
        /// Get the current instance of ElmahIoXamarin. This property can only be fetched after calling the Init method.
        /// </summary>
        internal static ElmahIoXamarin Instance
        {
            get
            {
                if (instance == null) throw new ApplicationException("Tried to get ElmahIoXamarin instance without calling Init first");
                return instance;
            }
        }

        /// <summary>
        /// Get the configured elmah.io client. You can use this to set up custom events, timeout, etc.
        /// </summary>
        public IElmahioAPI ElmahIoClient { get; internal set; }

        /// <summary>
        /// Get the options provided in the Init method.
        /// </summary>
        public ElmahIoXamarinOptions Options { get; }

        /// <summary>
        /// Initialize a new instance of the ElmahIoXamarin object with the provided options.
        /// </summary>
        public static void Init(ElmahIoXamarinOptions options)
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new ElmahIoXamarin(options);
                    InitPlatform();
                }
            }
        }

        private ElmahIoXamarin(ElmahIoXamarinOptions options)
        {
            Options = options;
            var client = (ElmahioAPI)ElmahioAPI.Create(Options.ApiKey);
            client.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("Elmah.Io.Xamarin", _assemblyVersion)));
            client.Messages.OnMessage += (sender, args) =>
            {
                Options.OnMessage?.Invoke(args.Message);
            };
            client.Messages.OnMessageFail += (sender, args) =>
            {
                Options.OnError?.Invoke(args.Message, args.Error);
            };
            ElmahIoClient = client;
        }

        private void AddBreadcrumbInternal(Breadcrumb breadcrumb)
        {
            breadcrumbs.Add(breadcrumb);

            if (breadcrumbs.Count > MaximumBreadcrumbCount)
            {
                var oldest = breadcrumbs.OrderBy(b => b.DateTime).First();
                breadcrumbs.Remove(oldest);
            }
        }

        private void LogInternal(Exception exception)
        {
            var baseException = exception?.GetBaseException();
            var errorMessage = baseException?.Message ?? "Unhandled exception";

            var utcNow = DateTime.UtcNow;
            var createMessage = new CreateMessage
            {
                Data = Data(exception),
                ServerVariables = ServerVariables(),
                DateTime = utcNow,
                Detail = exception?.ToString(),
                Severity = Severity.Error.ToString(),
                Source = baseException?.Source,
                Title = errorMessage,
                Type = baseException?.GetType().FullName,
                Version = Version(),
                Application = Application(),
                Breadcrumbs = Breadcrumbs(utcNow),
            };

            if (Options.OnFilter != null && Options.OnFilter(createMessage))
            {
                return;
            }

            ElmahIoClient.Messages.CreateAndNotify(Options.LogId, createMessage);
        }

        /// <summary>
        /// jdjdj
        /// </summary>
        /// <param name="message"></param>
        /// <param name="dateTime"></param>
        /// <param name="severity"></param>
        /// <param name="action"></param>
        public static void AddBreadcrumb(string message, DateTime? dateTime, string severity = "Information", string action = "Log")
        {
            if (Instance == null) throw new ApplicationException("Call ElmahIoXamarin.Init before logging breadcrumbs");

            Instance.AddBreadcrumbInternal(new Breadcrumb(dateTime.HasValue ? dateTime.Value : DateTime.UtcNow, severity, action, message));
        }

        /// <summary>
        /// dsjakldsa
        /// </summary>
        /// <param name="exception"></param>
        public static void Log(Exception exception)
        {
            if (Instance == null) throw new ApplicationException("Call ElmahIoXamarin.Init before logging");

            Instance.LogInternal(exception);
        }

        private IList<Breadcrumb> Breadcrumbs(DateTime utcNow)
        {
            if (breadcrumbs.Count == 0) return null;
            // Set default values on properties not set
            foreach (var breadcrumb in breadcrumbs)
            {
                if (!breadcrumb.DateTime.HasValue) breadcrumb.DateTime = utcNow;
                if (string.IsNullOrWhiteSpace(breadcrumb.Severity)) breadcrumb.Severity = "Information";
                if (string.IsNullOrWhiteSpace(breadcrumb.Action)) breadcrumb.Action = "Log";
            }

            var breadcrumbsToReturn = breadcrumbs.OrderByDescending(l => l.DateTime).ToList();
            breadcrumbs.Clear();
            return breadcrumbsToReturn;
        }

        private IList<Item> ServerVariables()
        {
            var serverVariables = new List<Item>();
            try
            {
                // Generate a pseudo user agent for elmah.io to use for grouping, search, etc.
                serverVariables.Add(new Item("User-Agent", $"X-ELMAHIO-MOBILE; OS={DeviceInfo.Platform}; OSVERSION={DeviceInfo.VersionString}; ENGINE=Xamarin"));
            }
            catch { }
            return serverVariables;
        }

        private List<Item> Data(Exception exception)
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
            catch { }
            return data;
        }

        private string Version()
        {
            if (!string.IsNullOrWhiteSpace(Options.Version)) return Options.Version;
            try
            {
                return AppInfo.VersionString;
            }
            catch { }

            return null;
        }

        private string Application()
        {
            if (!string.IsNullOrWhiteSpace(Options.Application)) return Options.Application;

            try
            {
                return AppInfo.PackageName;
            }
            catch { }

            return null;
        }
    }
}
