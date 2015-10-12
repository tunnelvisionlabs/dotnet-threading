// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

    [TestClass]
    public class TestCoreTaskExtensions_Finally : TaskTestingBase
    {
        #region Finally 1

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Action{Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFinally1_NullAntecedent_CompletedCleanup()
        {
            // declaring these makes it clear we are testing the correct overload
            Task antecedent = null;
            Action<Task> finallyAction = task =>
                {
                };

            CoreTaskExtensions.Finally(antecedent, finallyAction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Action{Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFinally1_CompletedAntecedent_NullCleanup()
        {
            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;
            Action<Task> finallyAction = null;

            CoreTaskExtensions.Finally(antecedent, finallyAction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Action{Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally1_CompletedAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;
            Action<Task> finallyAction = task => executed = true;

            Task combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Action{Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally1_CanceledAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();
            Action<Task> finallyAction = task => executed = true;

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Action{Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally1_FaultedAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;
            Action<Task> finallyAction = task => executed = true;

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Action{Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally1_CompletedAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;

            Exception cleanupException = new InvalidOperationException();
            Action<Task> finallyAction = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Action{Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally1_CanceledAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            Exception cleanupException = new InvalidOperationException();
            Action<Task> finallyAction = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Action{Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally1_FaultedAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Action<Task> finallyAction = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        #endregion

        #region Finally 2

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFinally2_NullAntecedent_CompletedCleanup()
        {
            // declaring these makes it clear we are testing the correct overload
            Task antecedent = null;
            Func<Task, Task> finallyFunc = task =>
                Task.Factory.StartNew(() =>
                {
                });

            CoreTaskExtensions.Finally(antecedent, finallyFunc);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFinally2_CompletedAntecedent_NullCleanupFunction()
        {
            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;
            Func<Task, Task> finallyFunc = null;

            CoreTaskExtensions.Finally(antecedent, finallyFunc);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_CompletedAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;
            Func<Task, Task> finallyFunc = task => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_CanceledAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();
            Func<Task, Task> finallyFunc = task => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_FaultedAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;
            Func<Task, Task> finallyFunc = task => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_CompletedAntecedent_CanceledCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task, Task> finallyFunc = task =>
                Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        cts.Cancel();
                        cts.Token.ThrowIfCancellationRequested();
                    }, cts.Token);

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_CanceledAntecedent_CanceledCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task, Task> finallyFunc = task =>
                Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        cts.Cancel();
                        cts.Token.ThrowIfCancellationRequested();
                    }, cts.Token);

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_FaultedAntecedent_CanceledCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task, Task> finallyFunc = task =>
                Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        cts.Cancel();
                        cts.Token.ThrowIfCancellationRequested();
                    }, cts.Token);

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_CompletedAntecedent_FaultedPreCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Task> finallyFunc = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_CanceledAntecedent_FaultedPreCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Task> finallyFunc = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_FaultedAntecedent_FaultedPreCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Task> finallyFunc = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_CompletedAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Task> finallyFunc = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_CanceledAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Task> finallyFunc = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_FaultedAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Task> finallyFunc = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_CompletedAntecedent_NullCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Task> finallyFunc =
                task =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_CanceledAntecedent_NullCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Task> finallyFunc =
                task =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally(Task, Func{Task, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally2_FaultedAntecedent_NullCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Task> finallyFunc =
                task =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        #endregion

        #region Finally 3

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Action{Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFinally3_NullAntecedent_CompletedCleanup()
        {
            // declaring these makes it clear we are testing the correct overload
            Task<object> antecedent = null;
            Action<Task<object>> finallyAction = task =>
                {
                };

            CoreTaskExtensions.Finally(antecedent, finallyAction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Action{Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFinally3_CompletedAntecedent_NullCleanup()
        {
            // declaring these makes it clear we are testing the correct overload
            object result = new object();
            Task<object> antecedent = CompletedTask.FromResult(result);
            Action<Task<object>> finallyAction = null;

            CoreTaskExtensions.Finally(antecedent, finallyAction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Action{Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally3_CompletedAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            object result = new object();
            Task<object> antecedent = CompletedTask.FromResult(result);
            Action<Task<object>> finallyAction = task => executed = true;

            Task<object> combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(result, combinedTask.Result);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Action{Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally3_CanceledAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<object> antecedent = CompletedTask.Canceled<object>();
            Action<Task<object>> finallyAction = task => executed = true;

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Action{Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally3_FaultedAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<object> antecedent = faultedCompletionSource.Task;
            Action<Task<object>> finallyAction = task => executed = true;

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Action{Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally3_CompletedAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            object result = new object();
            Task<object> antecedent = CompletedTask.FromResult(result);

            Exception cleanupException = new InvalidOperationException();
            Action<Task<object>> finallyAction = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Action{Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally3_CanceledAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<object> antecedent = CompletedTask.Canceled<object>();

            Exception cleanupException = new InvalidOperationException();
            Action<Task<object>> finallyAction = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Action{Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally3_FaultedAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<object> antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Action<Task<object>> finallyAction = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyAction);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        #endregion

        #region Finally 4

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFinally4_NullAntecedent_CompletedCleanup()
        {
            // declaring these makes it clear we are testing the correct overload
            Task<object> antecedent = null;
            Func<Task<object>, Task> finallyFunc = task =>
                Task.Factory.StartNew(() =>
                {
                });

            CoreTaskExtensions.Finally(antecedent, finallyFunc);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestFinally4_CompletedAntecedent_NullCleanupFunction()
        {
            // declaring these makes it clear we are testing the correct overload
            object result = new object();
            Task<object> antecedent = CompletedTask.FromResult(result);
            Func<Task<object>, Task> finallyFunc = null;

            CoreTaskExtensions.Finally(antecedent, finallyFunc);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_CompletedAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            object result = new object();
            Task<object> antecedent = CompletedTask.FromResult(result);
            Func<Task<object>, Task> finallyFunc = task => Task.Factory.StartNew(() => executed = true);

            Task<object> combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(result, combinedTask.Result);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_CanceledAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<object> antecedent = CompletedTask.Canceled<object>();
            Func<Task<object>, Task> finallyFunc = task => Task.Factory.StartNew(() => executed = true);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_FaultedAntecedent_CompletedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<object> antecedent = faultedCompletionSource.Task;
            Func<Task<object>, Task> finallyFunc = task => Task.Factory.StartNew(() => executed = true);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_CompletedAntecedent_CanceledCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            object result = new object();
            Task<object> antecedent = CompletedTask.FromResult(result);

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<object>, Task> finallyFunc = task =>
                Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        cts.Cancel();
                        cts.Token.ThrowIfCancellationRequested();
                    }, cts.Token);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_CanceledAntecedent_CanceledCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<object> antecedent = CompletedTask.Canceled<object>();

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<object>, Task> finallyFunc = task =>
                Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        cts.Cancel();
                        cts.Token.ThrowIfCancellationRequested();
                    }, cts.Token);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_FaultedAntecedent_CanceledCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<object> antecedent = faultedCompletionSource.Task;

            CancellationTokenSource cts = new CancellationTokenSource();
            Func<Task<object>, Task> finallyFunc = task =>
                Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        cts.Cancel();
                        cts.Token.ThrowIfCancellationRequested();
                    }, cts.Token);

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an ArgumentException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_CompletedAntecedent_FaultedPreCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            object result = new object();
            Task<object> antecedent = CompletedTask.FromResult(result);

            Exception cleanupException = new InvalidOperationException();
            Func<Task<object>, Task> finallyFunc = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_CanceledAntecedent_FaultedPreCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<object> antecedent = CompletedTask.Canceled<object>();

            Exception cleanupException = new InvalidOperationException();
            Func<Task<object>, Task> finallyFunc = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_FaultedAntecedent_FaultedPreCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<object> antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task<object>, Task> finallyFunc = task =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_CompletedAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            object result = new object();
            Task<object> antecedent = CompletedTask.FromResult(result);

            Exception cleanupException = new InvalidOperationException();
            Func<Task<object>, Task> finallyFunc = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_CanceledAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<object> antecedent = CompletedTask.Canceled<object>();

            Exception cleanupException = new InvalidOperationException();
            Func<Task<object>, Task> finallyFunc = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_FaultedAntecedent_FaultedCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<object> antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task<object>, Task> finallyFunc = task =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(cleanupException, ex.InnerExceptions[0]);
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_CompletedAntecedent_NullCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            object result = new object();
            Task<object> antecedent = CompletedTask.FromResult(result);

            Exception cleanupException = new InvalidOperationException();
            Func<Task<object>, Task> finallyFunc =
                task =>
                {
                    executed = true;
                    return null;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_CanceledAntecedent_NullCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<object> antecedent = CompletedTask.Canceled<object>();

            Exception cleanupException = new InvalidOperationException();
            Func<Task<object>, Task> finallyFunc =
                task =>
                {
                    executed = true;
                    return null;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Finally{TResult}(Task{TResult}, Func{Task{TResult}, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestFinally4_FaultedAntecedent_NullCleanup()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<object> antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task<object>, Task> finallyFunc =
                task =>
                {
                    executed = true;
                    return null;
                };

            Task<object> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Finally(antecedent, finallyFunc);
                combinedTask.Wait();
                Assert.Fail("Expected an InvalidOperationException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsTrue(executed);
            }
        }

        #endregion
    }
}
