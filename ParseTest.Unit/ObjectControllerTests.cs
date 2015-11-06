﻿using LeanCloud;
using LeanCloud.Internal;
using NUnit.Framework;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace LeanCloudTest {
  [TestFixture]
  public class ObjectControllerTests {
    [SetUp]
    public void SetUp() {
      AVClient.HostName = new Uri("https://api.leancloud.cn/1.1");
      AVClient.Initialize("z6dDeIIRLMn9VeqQpMDgawMK", "dBQa05LeoppSypcVRjq7wFg1");
    }

    [TearDown]
    public void TearDown() {
      AVClient.HostName = null;
    }

    [Test]
    public void AddNewObject()
    {
        AVObject rebecca = new AVObject("Girl");
        rebecca["name"] = "Rebecca";
        rebecca.SaveAsync().Wait();
    }

    [Test]
    [AsyncStateMachine(typeof(ObjectControllerTests))]
    public Task TestFetch() {
      var state = new MutableObjectState {
        ClassName = "Corgi",
        ObjectId = "st4nl3yW",
        ServerData = new Dictionary<string, object>() {
          { "corgi", "isNotDoge" }
        }
      };

      var responseDict = new Dictionary<string, object>() {
        { "__type", "Object" },
        { "className", "Corgi" },
        { "objectId", "st4nl3yW" },
        { "doge", "isShibaInu" },
        { "createdAt", "2015-09-18T18:11:28.943Z" }
      };
      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.Accepted, responseDict);
      var mockRunner = CreateMockRunner(response);

      var controller = new AVObjectController(mockRunner.Object);
      return controller.FetchAsync(state, null, CancellationToken.None).ContinueWith(t => {
        Assert.IsFalse(t.IsFaulted);
        Assert.IsFalse(t.IsCanceled);

        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/classes/Corgi/st4nl3yW"),
            It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
            It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));

        var newState = t.Result;
        Assert.AreEqual("isShibaInu", newState["doge"]);
        Assert.False(newState.ContainsKey("corgi"));
        Assert.NotNull(newState.CreatedAt);
        Assert.NotNull(newState.UpdatedAt);
      });
    }

    [Test]
    [AsyncStateMachine(typeof(ObjectControllerTests))]
    public Task TestSave() {
      var state = new MutableObjectState {
        ClassName = "Corgi",
        ObjectId = "st4nl3yW",
        ServerData = new Dictionary<string, object>() {
          { "corgi", "isNotDoge" },
        }
      };
      var operations = new Dictionary<string, IAVFieldOperation>() {
        { "gogo", new Mock<IAVFieldOperation>().Object }
      };

      var responseDict = new Dictionary<string, object>() {
        { "__type", "Object" },
        { "className", "Corgi" },
        { "objectId", "st4nl3yW" },
        { "doge", "isShibaInu" },
        { "createdAt", "2015-09-18T18:11:28.943Z" }
      };
      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.Accepted, responseDict);
      var mockRunner = CreateMockRunner(response);

      var controller = new AVObjectController(mockRunner.Object);
      return controller.SaveAsync(state, operations, null, CancellationToken.None).ContinueWith(t => {
        Assert.IsFalse(t.IsFaulted);
        Assert.IsFalse(t.IsCanceled);

        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/classes/Corgi/st4nl3yW"),
            It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
            It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));

        var newState = t.Result;
        Assert.AreEqual("isShibaInu", newState["doge"]);
        Assert.False(newState.ContainsKey("corgi"));
        Assert.False(newState.ContainsKey("gogo"));
        Assert.NotNull(newState.CreatedAt);
        Assert.NotNull(newState.UpdatedAt);
      });
    }

    [Test]
    [AsyncStateMachine(typeof(ObjectControllerTests))]
    public Task TestSaveNewObject() {
      var state = new MutableObjectState {
        ClassName = "Corgi",
        ServerData = new Dictionary<string, object>() {
          { "corgi", "isNotDoge" },
        }
      };
      var operations = new Dictionary<string, IAVFieldOperation>() {
        { "gogo", new Mock<IAVFieldOperation>().Object }
      };

      var responseDict = new Dictionary<string, object>() {
        { "__type", "Object" },
        { "className", "Corgi" },
        { "objectId", "st4nl3yW" },
        { "doge", "isShibaInu" },
        { "createdAt", "2015-09-18T18:11:28.943Z" }
      };
      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.Created, responseDict);
      var mockRunner = CreateMockRunner(response);

      var controller = new AVObjectController(mockRunner.Object);
      return controller.SaveAsync(state, operations, null, CancellationToken.None).ContinueWith(t => {
        Assert.IsFalse(t.IsFaulted);
        Assert.IsFalse(t.IsCanceled);

        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/classes/Corgi"),
            It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
            It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));

        var newState = t.Result;
        Assert.AreEqual("isShibaInu", newState["doge"]);
        Assert.False(newState.ContainsKey("corgi"));
        Assert.False(newState.ContainsKey("gogo"));
        Assert.AreEqual("st4nl3yW", newState.ObjectId);
        Assert.True(newState.IsNew);
        Assert.NotNull(newState.CreatedAt);
        Assert.NotNull(newState.UpdatedAt);
      });
    }

    [Test]
    [AsyncStateMachine(typeof(ObjectControllerTests))]
    public Task TestSaveAll() {
      var states = new List<IObjectState>();
      for (int i = 0; i < 30; ++i) {
        states.Add(new MutableObjectState {
          ClassName = "Corgi",
          ObjectId = ((i % 2 == 0) ? null : "st4nl3yW" + i),
          ServerData = new Dictionary<string, object>() {
            { "corgi", "isNotDoge" },
          }
        });
      }
      var operationsList = new List<IDictionary<string, IAVFieldOperation>>();
      for (int i = 0; i < 30; ++i) {
        operationsList.Add(new Dictionary<string, IAVFieldOperation>() {
          { "gogo", new Mock<IAVFieldOperation>().Object }
        });
      }

      var results = new List<IDictionary<string, object>>();
      for (int i = 0; i < 30; ++i) {
        results.Add(new Dictionary<string, object>() {
          { "success", new Dictionary<string, object> {
            { "__type", "Object" },
            { "className", "Corgi" },
            { "objectId", "st4nl3yW" + i },
            { "doge", "isShibaInu" },
            { "createdAt", "2015-09-18T18:11:28.943Z" }
          }}
        });
      }
      var responseDict = new Dictionary<string, object>() {
        { "results", results }
      };

      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.OK, responseDict);
      var mockRunner = CreateMockRunner(response);

      var controller = new AVObjectController(mockRunner.Object);
      var tasks = controller.SaveAllAsync(states, operationsList, null, CancellationToken.None);

      return Task.WhenAll(tasks).ContinueWith(_ => {
        Assert.True(tasks.All(task => task.IsCompleted && !task.IsCanceled && !task.IsFaulted));

        for (int i = 0; i < 30; ++i) {
          var serverState = tasks[i].Result;
          Assert.AreEqual("st4nl3yW" + i, serverState.ObjectId);
          Assert.False(serverState.ContainsKey("gogo"));
          Assert.False(serverState.ContainsKey("corgi"));
          Assert.AreEqual("isShibaInu", serverState["doge"]);
          Assert.NotNull(serverState.CreatedAt);
          Assert.NotNull(serverState.UpdatedAt);
        }

        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/batch"),
            It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
            It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));
      });
    }

    [Test]
    [AsyncStateMachine(typeof(ObjectControllerTests))]
    public Task TestSaveAllManyObjects() {
      var states = new List<IObjectState>();
      for (int i = 0; i < 102; ++i) {
        states.Add(new MutableObjectState {
          ClassName = "Corgi",
          ObjectId = "st4nl3yW" + i,
          ServerData = new Dictionary<string, object>() {
            { "corgi", "isNotDoge" },
          }
        });
      }
      var operationsList = new List<IDictionary<string, IAVFieldOperation>>();
      for (int i = 0; i < 102; ++i) {
        operationsList.Add(new Dictionary<string, IAVFieldOperation>() {
          { "gogo", new Mock<IAVFieldOperation>().Object }
        });
      }

      // Make multiple response since the batch will be splitted.
      var results = new List<IDictionary<string, object>>();
      for (int i = 0; i < 50; ++i) {
        results.Add(new Dictionary<string, object>() {
          { "success", new Dictionary<string, object> {
            { "__type", "Object" },
            { "className", "Corgi" },
            { "objectId", "st4nl3yW" + i },
            { "doge", "isShibaInu" },
            { "createdAt", "2015-09-18T18:11:28.943Z" }
          }}
        });
      }
      var responseDict = new Dictionary<string, object>() {
        { "results", results }
      };
      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.OK, responseDict);

      var results2 = new List<IDictionary<string, object>>();
      for (int i = 0; i < 2; ++i) {
        results2.Add(new Dictionary<string, object>() {
          { "success", new Dictionary<string, object> {
            { "__type", "Object" },
            { "className", "Corgi" },
            { "objectId", "st4nl3yW" + i },
            { "doge", "isShibaInu" },
            { "createdAt", "2015-09-18T18:11:28.943Z" }
          }}
        });
      }
      var responseDict2 = new Dictionary<string, object>() {
        { "results", results2 }
      };
      var response2 = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.OK, responseDict2);

      var mockRunner = new Mock<IAVCommandRunner>();
      mockRunner.SetupSequence(obj => obj.RunCommandAsync(It.IsAny<AVCommand>(),
          It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
          It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
          It.IsAny<CancellationToken>()))
          .Returns(Task<Tuple<HttpStatusCode, IDictionary<string, object>>>.FromResult(response))
          .Returns(Task<Tuple<HttpStatusCode, IDictionary<string, object>>>.FromResult(response))
          .Returns(Task<Tuple<HttpStatusCode, IDictionary<string, object>>>.FromResult(response2));

      var controller = new AVObjectController(mockRunner.Object);
      var tasks = controller.SaveAllAsync(states, operationsList, null, CancellationToken.None);

      return Task.WhenAll(tasks).ContinueWith(_ => {
        Assert.True(tasks.All(task => task.IsCompleted && !task.IsCanceled && !task.IsFaulted));

        for (int i = 0; i < 102; ++i) {
          var serverState = tasks[i].Result;
          Assert.AreEqual("st4nl3yW" + (i % 50), serverState.ObjectId);
          Assert.False(serverState.ContainsKey("gogo"));
          Assert.False(serverState.ContainsKey("corgi"));
          Assert.AreEqual("isShibaInu", serverState["doge"]);
          Assert.NotNull(serverState.CreatedAt);
          Assert.NotNull(serverState.UpdatedAt);
        }

        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/batch"),
            It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
            It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(3));
      });
    }

    [Test]
    [AsyncStateMachine(typeof(ObjectControllerTests))]
    public Task TestDelete() {
      var state = new MutableObjectState {
        ClassName = "Corgi",
        ObjectId = "st4nl3yW",
        ServerData = new Dictionary<string, object>() {
          { "corgi", "isNotDoge" },
        }
      };

      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.OK, new Dictionary<string, object>());
      var mockRunner = CreateMockRunner(response);

      var controller = new AVObjectController(mockRunner.Object);
      return controller.DeleteAsync(state, null, CancellationToken.None).ContinueWith(t => {
        Assert.IsFalse(t.IsFaulted);
        Assert.IsFalse(t.IsCanceled);

        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/classes/Corgi/st4nl3yW"),
            It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
            It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));
      });
    }

    [Test]
    [AsyncStateMachine(typeof(ObjectControllerTests))]
    public Task TestDeleteAll() {
      var states = new List<IObjectState>();
      for (int i = 0; i < 30; ++i) {
        states.Add(new MutableObjectState {
          ClassName = "Corgi",
          ObjectId = "st4nl3yW" + i,
          ServerData = new Dictionary<string, object>() {
            { "corgi", "isNotDoge" },
          }
        });
      }

      var results = new List<IDictionary<string, object>>();
      for (int i = 0; i < 30; ++i) {
        results.Add(new Dictionary<string, object>() {
          { "success", null }
        });
      }
      var responseDict = new Dictionary<string, object>() {
        { "results", results }
      };

      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.OK, responseDict);
      var mockRunner = CreateMockRunner(response);

      var controller = new AVObjectController(mockRunner.Object);
      var tasks = controller.DeleteAllAsync(states, null, CancellationToken.None);

      return Task.WhenAll(tasks).ContinueWith(_ => {
        Assert.True(tasks.All(task => task.IsCompleted && !task.IsCanceled && !task.IsFaulted));

        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/batch"),
            It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
            It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));
      });
    }

    [Test]
    [AsyncStateMachine(typeof(ObjectControllerTests))]
    public Task TestDeleteAllManyObjects() {
      var states = new List<IObjectState>();
      for (int i = 0; i < 102; ++i) {
        states.Add(new MutableObjectState {
          ClassName = "Corgi",
          ObjectId = "st4nl3yW" + i,
          ServerData = new Dictionary<string, object>() {
            { "corgi", "isNotDoge" },
          }
        });
      }

      // Make multiple response since the batch will be splitted.
      var results = new List<IDictionary<string, object>>();
      for (int i = 0; i < 50; ++i) {
        results.Add(new Dictionary<string, object>() {
          { "success", null }
        });
      }
      var responseDict = new Dictionary<string, object>() {
        { "results", results }
      };
      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.OK, responseDict);

      var results2 = new List<IDictionary<string, object>>();
      for (int i = 0; i < 2; ++i) {
        results2.Add(new Dictionary<string, object>() {
          { "success", null }
        });
      }
      var responseDict2 = new Dictionary<string, object>() {
        { "results", results2 }
      };
      var response2 = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.OK, responseDict2);

      var mockRunner = new Mock<IAVCommandRunner>();
      mockRunner.SetupSequence(obj => obj.RunCommandAsync(It.IsAny<AVCommand>(),
          It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
          It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
          It.IsAny<CancellationToken>()))
          .Returns(Task<Tuple<HttpStatusCode, IDictionary<string, object>>>.FromResult(response))
          .Returns(Task<Tuple<HttpStatusCode, IDictionary<string, object>>>.FromResult(response))
          .Returns(Task<Tuple<HttpStatusCode, IDictionary<string, object>>>.FromResult(response2));

      var controller = new AVObjectController(mockRunner.Object);
      var tasks = controller.DeleteAllAsync(states, null, CancellationToken.None);

      return Task.WhenAll(tasks).ContinueWith(_ => {
        Assert.True(tasks.All(task => task.IsCompleted && !task.IsCanceled && !task.IsFaulted));

        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/batch"),
            It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
            It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(3));
      });
    }

    [Test]
    [AsyncStateMachine(typeof(ObjectControllerTests))]
    public Task TestDeleteAllFailSome() {
      var states = new List<IObjectState>();
      for (int i = 0; i < 30; ++i) {
        states.Add(new MutableObjectState {
          ClassName = "Corgi",
          ObjectId = ((i % 2 == 0) ? null : "st4nl3yW" + i),
          ServerData = new Dictionary<string, object>() {
            { "corgi", "isNotDoge" },
          }
        });
      }

      var results = new List<IDictionary<string, object>>();
      for (int i = 0; i < 15; ++i) {
        if (i % 2 == 0) {
          results.Add(new Dictionary<string, object> {{
            "error", new Dictionary<string, object>() {
              { "code", (long)AVException.ErrorCode.ObjectNotFound },
              { "error", "Object not found." }
            }
          }});
        } else {
          results.Add(new Dictionary<string, object> {
            { "success", null }
          });
        }
      }
      var responseDict = new Dictionary<string, object>() {
        { "results", results }
      };

      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.OK, responseDict);
      var mockRunner = CreateMockRunner(response);

      var controller = new AVObjectController(mockRunner.Object);
      var tasks = controller.DeleteAllAsync(states, null, CancellationToken.None);

      return Task.WhenAll(tasks).ContinueWith(_ => {
        for (int i = 0; i < 15; ++i) {
          if (i % 2 == 0) {
            Assert.True(tasks[i].IsFaulted);
            Assert.IsInstanceOf<AVException>(tasks[i].Exception.InnerException);
            AVException exception = tasks[i].Exception.InnerException as AVException;
            Assert.AreEqual(AVException.ErrorCode.ObjectNotFound, exception.Code);
          } else {
            Assert.True(tasks[i].IsCompleted);
            Assert.False(tasks[i].IsFaulted || tasks[i].IsCanceled);
          }
        }

        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/batch"),
            It.IsAny<IProgress<AVUploadProgressEventArgs>>(),
            It.IsAny<IProgress<AVDownloadProgressEventArgs>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(1));
      });
    }

    [Test]
    [AsyncStateMachine(typeof(ObjectControllerTests))]
    public Task TestDeleteAllInconsistent() {
      var states = new List<IObjectState>();
      for (int i = 0; i < 30; ++i) {
        states.Add(new MutableObjectState {
          ClassName = "Corgi",
          ObjectId = "st4nl3yW" + i,
          ServerData = new Dictionary<string, object>() {
            { "corgi", "isNotDoge" },
          }
        });
      }

      var results = new List<IDictionary<string, object>>();
      for (int i = 0; i < 36; ++i) {
        results.Add(new Dictionary<string, object>() {
          { "success", null }
        });
      }
      var responseDict = new Dictionary<string, object>() {
        { "results", results }
      };

      var response = new Tuple<HttpStatusCode, IDictionary<string, object>>(HttpStatusCode.OK, responseDict);
      var mockRunner = CreateMockRunner(response);

      var controller = new AVObjectController(mockRunner.Object);
      var tasks = controller.DeleteAllAsync(states, null, CancellationToken.None);

      return Task.WhenAll(tasks).ContinueWith(_ => {
        Assert.True(tasks.All(task => task.IsFaulted));
        Assert.IsInstanceOf<InvalidOperationException>(tasks[0].Exception.InnerException);

        mockRunner.Verify(obj => obj.RunCommandAsync(It.Is<AVCommand>(command => command.Uri.AbsolutePath == "/1/batch"),
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
