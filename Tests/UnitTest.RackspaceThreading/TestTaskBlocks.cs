// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
#if !NET40PLUS
    extern alias tpl;
#endif

    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

#if !NET40PLUS
    using tpl::System.Threading;
    using tpl::System.Threading.Tasks;
    using AggregateException = tpl::System.AggregateException;
#else
    using System.Threading;
    using System.Threading.Tasks;
#endif

    [TestClass]
    public class TestTaskBlocks
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

        #region While 1

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhile1_NullConditionFunction()
        {
            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = null;
            Func<Task> body = new WhileBody().ExecuteAsync;

            TaskBlocks.While(condition, body);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhile1_NullBodyFunction()
        {
            WhileCondition whileCondition = new WhileCondition(0);

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = null;

            try
            {
                TaskBlocks.While(condition, body);
            }
            catch
            {
                Assert.AreEqual(0, whileCondition.EvaluateCount);
                throw;
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop0_Success()
        {
            WhileCondition whileCondition = new WhileCondition(0);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = TaskBlocks.While(condition, body);
            whileTask.Wait();

            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_Success()
        {
            WhileCondition whileCondition = new WhileCondition(3);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = TaskBlocks.While(condition, body);
            whileTask.Wait();

            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop0_FaultedCondition()
        {
            WhileCondition whileCondition = new WhileCondition(0, DelegateBehavior.Faulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_FaultedCondition()
        {
            WhileCondition whileCondition = new WhileCondition(3, DelegateBehavior.Faulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_CanceledBody()
        {
            WhileCondition whileCondition = WhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.Canceled);

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Canceled, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_FaultedBody()
        {
            WhileCondition whileCondition = WhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.Faulted);

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileBody.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_SyncFaultedBody()
        {
            WhileCondition whileCondition = WhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.SyncFaulted);

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileBody.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions - 1, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_NullTaskBody()
        {
            WhileCondition whileCondition = WhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.NullTask);

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(InvalidOperationException));

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions - 1, whileBody.ExecutionCount);
            }
        }

        #endregion

        #region While 1

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhile2_NullConditionFunction()
        {
            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = null;
            Func<Task> body = new WhileBody().ExecuteAsync;

            TaskBlocks.While(condition, body);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhile2_NullBodyFunction()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0);

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = null;

            try
            {
                TaskBlocks.While(condition, body);
            }
            catch
            {
                Assert.AreEqual(0, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(0, whileCondition.EvaluateAsyncCount);
                throw;
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop0_Success()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = TaskBlocks.While(condition, body);
            whileTask.Wait();

            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_Success()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(3);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = TaskBlocks.While(condition, body);
            whileTask.Wait();

            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop0_CanceledCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0, DelegateBehavior.Canceled);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Canceled, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_CanceledCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(3, DelegateBehavior.Canceled);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Canceled, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop0_FaultedCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0, DelegateBehavior.Faulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_FaultedCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(3, DelegateBehavior.Faulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop0_SyncFaultedCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0, DelegateBehavior.SyncFaulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_SyncFaultedCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(3, DelegateBehavior.SyncFaulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop0_NullTaskCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0, DelegateBehavior.NullTask);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(InvalidOperationException));

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_NullTaskCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(3, DelegateBehavior.NullTask);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(InvalidOperationException));

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_CanceledBody()
        {
            AsyncWhileCondition whileCondition = AsyncWhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.Canceled);

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Canceled, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_FaultedBody()
        {
            AsyncWhileCondition whileCondition = AsyncWhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.Faulted);

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileBody.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_SyncFaultedBody()
        {
            AsyncWhileCondition whileCondition = AsyncWhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.SyncFaulted);

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileBody.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions - 1, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_NullTaskBody()
        {
            AsyncWhileCondition whileCondition = AsyncWhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.NullTask);

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(InvalidOperationException));

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions - 1, whileBody.ExecutionCount);
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

        private abstract class BaseWhileCondition
        {
            protected BaseWhileCondition(int totalIterations, DelegateBehavior behavior)
            {
                if (totalIterations < 0)
                    throw new ArgumentOutOfRangeException("totalIterations");

                TotalIterations = totalIterations;
                Behavior = behavior;
                if (behavior == DelegateBehavior.Faulted || behavior == DelegateBehavior.SyncFaulted)
                    ExpectedException = new ArithmeticException();
                else if (behavior == DelegateBehavior.Canceled)
                    CancellationTokenSource = new CancellationTokenSource();
            }

            public int TotalIterations
            {
                get;
                private set;
            }

            public DelegateBehavior Behavior
            {
                get;
                private set;
            }

            public int Iteration
            {
                get;
                private set;
            }

            public Exception ExpectedException
            {
                get;
                private set;
            }

            protected int EvaluateCoreCount
            {
                get;
                private set;
            }

            protected CancellationTokenSource CancellationTokenSource
            {
                get;
                private set;
            }

            protected bool EvaluateCore()
            {
                EvaluateCoreCount++;

                if (Iteration == TotalIterations)
                    return false;

                Iteration++;
                return true;
            }
        }

        private class WhileCondition : BaseWhileCondition
        {
            public WhileCondition(int totalIterations)
                : this(totalIterations, DelegateBehavior.Success)
            {
            }

            public WhileCondition(int totalIterations, DelegateBehavior behavior)
                : base(totalIterations, behavior)
            {
                if (behavior != DelegateBehavior.Success && behavior != DelegateBehavior.Faulted)
                    throw new ArgumentException("Unsupported behavior for while condition");
            }

            public int EvaluateCount
            {
                get
                {
                    return EvaluateCoreCount;
                }
            }

            public static WhileCondition True()
            {
                return new WhileCondition(int.MaxValue);
            }

            public static WhileCondition False()
            {
                return new WhileCondition(0);
            }

            public bool Evaluate()
            {
                bool result = EvaluateCore();
                if (!result && Behavior == DelegateBehavior.Faulted)
                    throw ExpectedException;

                return result;
            }
        }

        private class AsyncWhileCondition : BaseWhileCondition
        {
            public AsyncWhileCondition(int totalIterations)
                : this(totalIterations, DelegateBehavior.Success)
            {
            }

            public AsyncWhileCondition(int totalIterations, DelegateBehavior behavior)
                : base(totalIterations, behavior)
            {
            }

            public int SyncPartEvaluateAsyncCount
            {
                get;
                private set;
            }

            public int EvaluateAsyncCount
            {
                get;
                private set;
            }

            public static AsyncWhileCondition True()
            {
                return new AsyncWhileCondition(int.MaxValue);
            }

            public static AsyncWhileCondition False()
            {
                return new AsyncWhileCondition(0);
            }

            public Task<bool> EvaluateAsync()
            {
                SyncPartEvaluateAsyncCount++;

                if (Iteration == TotalIterations)
                {
                    if (Behavior == DelegateBehavior.SyncFaulted)
                        throw ExpectedException;
                    else if (Behavior == DelegateBehavior.NullTask)
                        return null;
                }

                CancellationToken cancellationToken = CancellationTokenSource != null ? CancellationTokenSource.Token : CancellationToken.None;
                return Task.Factory.StartNew(() =>
                    {
                        EvaluateAsyncCount++;

                        bool result = EvaluateCore();
                        if (!result)
                        {
                            if (Behavior == DelegateBehavior.Faulted)
                            {
                                throw ExpectedException;
                            }
                            else if (Behavior == DelegateBehavior.Canceled)
                            {
                                CancellationTokenSource.Cancel();
                                CancellationTokenSource.Token.ThrowIfCancellationRequested();
                            }
                        }

                        return result;
                    }, cancellationToken);
            }
        }

        private sealed class WhileBody
        {
            public WhileBody()
                : this(int.MaxValue, DelegateBehavior.Success)
            {
            }

            public WhileBody(int maxExecutions, DelegateBehavior behavior)
            {
                MaxExecutions = maxExecutions;
                Behavior = behavior;
                if (behavior == DelegateBehavior.Faulted || behavior == DelegateBehavior.SyncFaulted)
                    ExpectedException = new FormatException();
                else if (behavior == DelegateBehavior.Canceled)
                    CancellationTokenSource = new CancellationTokenSource();
            }

            public int MaxExecutions
            {
                get;
                private set;
            }

            public DelegateBehavior Behavior
            {
                get;
                private set;
            }

            public int SyncPartExecutionCount
            {
                get;
                private set;
            }

            public int ExecutionCount
            {
                get;
                private set;
            }

            public CancellationTokenSource CancellationTokenSource
            {
                get;
                private set;
            }

            public Exception ExpectedException
            {
                get;
                private set;
            }

            public Task ExecuteAsync()
            {
                SyncPartExecutionCount++;

                if (ExecutionCount == MaxExecutions - 1)
                {
                    if (Behavior == DelegateBehavior.SyncFaulted)
                        throw ExpectedException;
                    else if (Behavior == DelegateBehavior.NullTask)
                        return null;
                }

                CancellationToken cancellationToken = CancellationTokenSource != null ? CancellationTokenSource.Token : CancellationToken.None;
                return Task.Factory.StartNew(() =>
                    {
                        ExecutionCount++;

                        if (ExecutionCount >= MaxExecutions)
                        {
                            if (Behavior == DelegateBehavior.Faulted)
                            {
                                throw ExpectedException;
                            }
                            else if (Behavior == DelegateBehavior.Canceled)
                            {
                                CancellationTokenSource.Cancel();
                                CancellationTokenSource.Token.ThrowIfCancellationRequested();
                            }
                        }
                    }, cancellationToken);
            }
        }

        private enum DelegateBehavior
        {
            /// <summary>
            /// The delegate completes successfully.
            /// </summary>
            Success,

            /// <summary>
            /// For delegates that return a value directly, the delegate throws an exception. For
            /// delegates that return a <see cref="Task"/>, the delegate returns a <see cref="Task"/>
            /// instance which terminates with the <see cref="TaskStatus.Faulted"/> status.
            /// </summary>
            Faulted,

            /// <summary>
            /// The delegate returns a task which terminates with the <see cref="TaskStatus.Canceled"/> status.
            /// </summary>
            /// <remarks>
            /// This behavior only applies to delegates which return a <see cref="Task"/>.
            /// </remarks>
            Canceled,

            /// <summary>
            /// The delegate throws an exception prior to returning a task.
            /// </summary>
            /// <remarks>
            /// This behavior only applies to delegates which return a <see cref="Task"/>.
            /// </remarks>
            SyncFaulted,

            /// <summary>
            /// The delegate returns <see langword="null"/> instead of a <see cref="Task"/> instance.
            /// </summary>
            /// <remarks>
            /// This behavior only applies to delegates which return a <see cref="Task"/>.
            /// </remarks>
            NullTask,
        }
    }
}
