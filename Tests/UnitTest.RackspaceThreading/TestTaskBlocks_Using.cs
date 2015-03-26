// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

    [TestClass]
    public class TestTaskBlocks_Using
    {
        #region Using 1

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestUsing1_NullResourceFunction_CompletedBody_Disposable()
        {
            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = null;
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                });

            TaskBlocks.Using(resource, body);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestUsing1_CompletedResource_NullBodyFunction_Disposable()
        {
            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    Assert.Fail("Should not have attempted to create the resource.");
                    throw new InvalidOperationException("Unreachable");
                };
            Func<Task<IDisposable>, Task> body = null;

            TaskBlocks.Using(resource, body);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedResource_CompletedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };
            Func<Task<IDisposable>, Task> body = task => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = TaskBlocks.Using(resource, body);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsNotNull(resourceObject);
            Assert.IsTrue(resourceObject.Disposed);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedNullResource_CompletedBody_Disposable()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () => CompletedTask.FromResult(default(IDisposable));
            Func<Task<IDisposable>, Task> body = task => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = TaskBlocks.Using(resource, body);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CanceledResource_CompletedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.Canceled<IDisposable>();
                };
            Func<Task<IDisposable>, Task> body = task => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedResource_CompletedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };
            Func<Task<IDisposable>, Task> body = task => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedPreResource_CompletedBody_Disposable()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    throw expectedException;
                };
            Func<Task<IDisposable>, Task> body = task => Task.Factory.StartNew(() => executed = true);

            try
            {
                TaskBlocks.Using(resource, body);
                Assert.Fail("Expected an ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.AreSame(expectedException, ex);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestUsing1_NullResourceTask_CompletedBody_Disposable()
        {
            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () => null;
            Func<Task<IDisposable>, Task> body = task =>
                {
                    throw new NotSupportedException();
                };

            TaskBlocks.Using(resource, body);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedResource_CanceledBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                }, cts.Token);

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CanceledResource_CanceledBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                }, cts.Token);

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedResource_CanceledBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                }, cts.Token);

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedResource_FaultedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(bodyException, ex.InnerExceptions[0]);
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CanceledResource_FaultedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedResource_FaultedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedResource_FaultedPreBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(bodyException, ex.InnerExceptions[0]);
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CanceledResource_FaultedPreBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedResource_FaultedPreBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedResource_NullBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CanceledResource_NullBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedResource_NullBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedResource_CompletedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };
            Func<Task<IDisposable>, Task> body = task => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = TaskBlocks.Using(resource, body);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsNotNull(resourceObject);
            Assert.IsTrue(resourceObject.Disposed);
            Assert.IsTrue(resourceObject.AsyncDisposed);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CanceledResource_CompletedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.Canceled<IDisposable>();
                };
            Func<Task<IDisposable>, Task> body = task => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(resourceObject.AsyncDisposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedResource_CompletedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };
            Func<Task<IDisposable>, Task> body = task => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedResource_CanceledBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                }, cts.Token);

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(resourceObject.AsyncDisposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CanceledResource_CanceledBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                }, cts.Token);

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(resourceObject.AsyncDisposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedResource_CanceledBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                }, cts.Token);

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedResource_FaultedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(bodyException, ex.InnerExceptions[0]);
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(resourceObject.AsyncDisposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CanceledResource_FaultedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(resourceObject.AsyncDisposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedResource_FaultedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedResource_FaultedPreBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(bodyException, ex.InnerExceptions[0]);
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(resourceObject.AsyncDisposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CanceledResource_FaultedPreBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(resourceObject.AsyncDisposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedResource_FaultedPreBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CompletedResource_NullBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(resourceObject.AsyncDisposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_CanceledResource_NullBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(resourceObject.AsyncDisposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing1_FaultedResource_NullBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Func<Task<IDisposable>, Task> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        #endregion

        #region Using 2

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestUsing2_NullResourceFunction_CompletedBody_Disposable()
        {
            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = null;
            object result = new object();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew(() => result);

            TaskBlocks.Using(resource, body);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestUsing2_CompletedResource_NullBodyFunction_Disposable()
        {
            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    Assert.Fail("Should not have attempted to create the resource.");
                    throw new InvalidOperationException("Unreachable");
                };
            Func<Task<IDisposable>, Task<object>> body = null;

            TaskBlocks.Using(resource, body);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedResource_CompletedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };
            object result = new object();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    return result;
                });

            Task<object> combinedTask = TaskBlocks.Using(resource, body);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsNotNull(resourceObject);
            Assert.IsTrue(resourceObject.Disposed);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedNullResource_CompletedBody_Disposable()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () => CompletedTask.FromResult(default(IDisposable));
            object result = new object();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    return result;
                });

            Task<object> combinedTask = TaskBlocks.Using(resource, body);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CanceledResource_CompletedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.Canceled<IDisposable>();
                };
            object result = new object();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    return result;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedResource_CompletedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };
            object result = new object();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    return result;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedPreResource_CompletedBody_Disposable()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    throw expectedException;
                };
            object result = new object();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    return result;
                });

            try
            {
                TaskBlocks.Using(resource, body);
                Assert.Fail("Expected an ArgumentException");
            }
            catch (ArgumentException ex)
            {
                Assert.AreSame(expectedException, ex);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestUsing2_NullResourceTask_CompletedBody_Disposable()
        {
            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () => null;
            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    throw new NotSupportedException();
                };

            TaskBlocks.Using(resource, body);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedResource_CanceledBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                    throw new InvalidOperationException("Unreachable");
                }, cts.Token);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CanceledResource_CanceledBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                    throw new InvalidOperationException("Unreachable");
                }, cts.Token);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedResource_CanceledBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                    throw new InvalidOperationException("Unreachable");
                }, cts.Token);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedResource_FaultedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(bodyException, ex.InnerExceptions[0]);
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CanceledResource_FaultedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedResource_FaultedBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedResource_FaultedPreBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(bodyException, ex.InnerExceptions[0]);
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CanceledResource_FaultedPreBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedResource_FaultedPreBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedResource_NullBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CanceledResource_NullBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new Disposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedResource_NullBody_Disposable()
        {
            bool executed = false;
            Disposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedResource_CompletedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };
            object result = new object();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    return result;
                });

            Task<object> combinedTask = TaskBlocks.Using(resource, body);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsNotNull(resourceObject);
            Assert.IsTrue(resourceObject.Disposed);
            Assert.IsTrue(resourceObject.AsyncDisposed);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CanceledResource_CompletedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.Canceled<IDisposable>();
                };
            object result = new object();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    return result;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(resourceObject.AsyncDisposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedResource_CompletedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };
            object result = new object();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    return result;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedResource_CanceledBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                    throw new InvalidOperationException("Unreachable");
                }, cts.Token);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(resourceObject.AsyncDisposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CanceledResource_CanceledBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                    throw new InvalidOperationException("Unreachable");
                }, cts.Token);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(resourceObject.AsyncDisposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedResource_CanceledBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    cts.Cancel();
                    cts.Token.ThrowIfCancellationRequested();
                    throw new InvalidOperationException("Unreachable");
                }, cts.Token);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedResource_FaultedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(bodyException, ex.InnerExceptions[0]);
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(resourceObject.AsyncDisposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CanceledResource_FaultedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(resourceObject.AsyncDisposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedResource_FaultedBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                Task.Factory.StartNew<object>(() =>
                {
                    executed = true;
                    throw bodyException;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedResource_FaultedPreBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(bodyException, ex.InnerExceptions[0]);
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(resourceObject.AsyncDisposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CanceledResource_FaultedPreBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(resourceObject.AsyncDisposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedResource_FaultedPreBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Exception bodyException = new InvalidOperationException();
            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    throw bodyException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CompletedResource_NullBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.FromResult<IDisposable>(resourceObject);
                };

            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsTrue(resourceObject.Disposed);
                Assert.IsTrue(resourceObject.AsyncDisposed);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_CanceledResource_NullBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Func<Task<IDisposable>> resource = () =>
                {
                    resourceObject = new AsyncDisposable();
                    return CompletedTask.Canceled<IDisposable>();
                };

            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsNotNull(resourceObject);
                Assert.IsFalse(resourceObject.Disposed);
                Assert.IsFalse(resourceObject.AsyncDisposed);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestUsing2_FaultedResource_NullBody_AsyncDisposable()
        {
            bool executed = false;
            AsyncDisposable resourceObject = null;

            // declaring these makes it clear we are testing the correct overload
            Exception expectedException = new ArgumentException();
            Func<Task<IDisposable>> resource = () =>
                {
                    TaskCompletionSource<IDisposable> faultedCompletionSource = new TaskCompletionSource<IDisposable>();
                    faultedCompletionSource.SetException(expectedException);
                    return faultedCompletionSource.Task;
                };

            Func<Task<IDisposable>, Task<object>> body = task =>
                {
                    executed = true;
                    return null;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = TaskBlocks.Using(resource, body);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsNull(resourceObject);
                Assert.IsFalse(executed);
            }
        }

        #endregion

        private sealed class Disposable : IDisposable
        {
            public bool Disposed
            {
                get;
                private set;
            }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        private sealed class AsyncDisposable : IAsyncDisposable, IDisposable
        {
            public bool Disposed
            {
                get;
                private set;
            }

            public bool AsyncDisposed
            {
                get;
                private set;
            }

            public void Dispose()
            {
                Disposed = true;
            }

            public Task DisposeAsync()
            {
                return Task.Factory.StartNew(
                    () =>
                    {
                        // Dispose followed by DisposeAsync doesn't count as AsyncDisposed
                        if (!Disposed)
                        {
                            Disposed = true;
                            AsyncDisposed = true;
                        }
                    });
            }
        }
    }
}
