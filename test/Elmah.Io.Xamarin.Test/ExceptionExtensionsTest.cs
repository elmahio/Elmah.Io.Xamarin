using Elmah.Io.Client;
using NSubstitute;
using NUnit.Framework;
using System;

namespace Elmah.Io.Xamarin.Test
{
    public class ExceptionExtensionsTest
    {
        [Test]
        public void CanLog()
        {
            // Arrange
            var logId = Guid.NewGuid();
            ElmahIoXamarin.Init(new ElmahIoXamarinOptions { LogId = logId, ApiKey = "API_KEY" });
            var instance = ElmahIoXamarin.Instance;
            var elmahIoClientMock = Substitute.For<IElmahioAPI>();
            var messagesClientMock = Substitute.For<IMessagesClient>();
            elmahIoClientMock.Messages.Returns(messagesClientMock);
            instance.ElmahIoClient = elmahIoClientMock;
            var inner = new ArgumentException("Inner");
            var outer = new Exception("Outer", inner);

            // Act
            outer.Log();

            // Assert
            messagesClientMock.Received(1).CreateAndNotify(Arg.Is(logId), Arg.Is<CreateMessage>(msg =>
                msg.Title == "Inner"
                && msg.Type == "System.ArgumentException"
                && !string.IsNullOrWhiteSpace(msg.Detail)
                && msg.Detail.Contains("System.Exception: Outer")
                && msg.Detail.Contains("System.ArgumentException: Inner")));
        }
    }
}
