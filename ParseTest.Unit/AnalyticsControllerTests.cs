﻿using Moq;
using NUnit.Framework;
using LeanCloud;
using LeanCloud.Internal;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LeanCloudTest {
  [TestFixture]
  public class AnalyticsControllerTests {
    [SetUp]
    public void SetUp() {
      AVClient.HostName = new Uri("https://api.leancloud.cn");
    }

    [TearDown]
    public void TearDown() {
      AVClient.HostName = null;
    }

    [Test]
    [AsyncStateMachine(typeof(AnalyticsControllerTests))]
    public Task TestTrackEventWithEmptyDimension() {
      var responseDict = new Dictionary<string, object>();
      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.Accepted, responseDict);
      var mockRunner = CreateMockRunner(response);

      var controller = new AVAnalyticsController(mockRunner.Object);
      return controller.TrackEventAsync("SomeEvent",
        dimensions: null,
        sessionToken: null,
        cancellationToken: CancellationToken.None).ContinueWith(t => {
        Assert.IsFalse(t.IsFaulted);
        Assert.IsFalse(t.IsCanceled);
        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/events/SomeEvent"),
          It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
          It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
          It.IsAny<CancellationToken>()), Times.Exactly(1));
      });
    }

    [Test]
    [AsyncStateMachine(typeof(AnalyticsControllerTests))]
    public Task TestTrackAppOpenedWithEmptyPushHash() {
      var responseDict = new Dictionary<string, object>();
      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.Accepted, responseDict);
      var mockRunner = CreateMockRunner(response);

      var controller = new AVAnalyticsController(mockRunner.Object);
      return controller.TrackAppOpenedAsync(null,
        sessionToken: null,
        cancellationToken: CancellationToken.None).ContinueWith(t => {
          Assert.IsFalse(t.IsFaulted);
          Assert.IsFalse(t.IsCanceled);
          mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/events/AppOpened"),
            It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
            It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));
        });
    }

    private Mock<IAVCommandRunner> CreateMockRunner(Tuple<HttpStatusCode, IDictionary<string, object>> response) {
      var mockRunner = new Mock<IAVCommandRunner>();
      mockRunner.Setup(obj => obj.RunCommandAsync(It.IsAny<AVCommand>(),
          It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
          It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
          It.IsAny<CancellationToken>()))
          .Returns(Task<Tuple<HttpStatusCode, IDictionary<string, object>>>.FromResult(response));

      return mockRunner;
    }
  }
}
