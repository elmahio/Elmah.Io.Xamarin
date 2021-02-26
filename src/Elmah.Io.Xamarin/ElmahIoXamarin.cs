using Elmah.Io.Client;
using System;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Elmah.Io.Xamarin.Test")]

namespace Elmah.Io.Xamarin
{
    /// <summary>
    /// This class is used to configure logging to elmah.io from Xamarin.
    /// </summary>
    public class ElmahIoXamarin
    {
        internal static string _assemblyVersion = typeof(ElmahIoXamarin).Assembly.GetName().Version.ToString();
        private static ElmahIoXamarin instance;
        private static readonly object padlock = new object();

        /// <summary>
        /// Get the current instance of ElmahIoXamarin. This property can only be fetched after calling the Init method.
        /// </summary>
        public static ElmahIoXamarin Instance
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
    }
}
