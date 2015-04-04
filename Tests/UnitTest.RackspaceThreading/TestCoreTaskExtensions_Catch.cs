// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

    [TestClass]
    public class TestCoreTaskExtensions_Catch : TaskTestingBase
    {
        private static readonly string AntecedentValue = "Hello World 1";
        private static readonly string HandlerValue = "Hello World 2";

        #region Catch 1

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCatch1_NullAntecedent_CompletedHandler()
        {
            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = null;
            Func<Task<string>, ArgumentException, Task<string>> handlerFunction =
                (task, ex) => CompletedTask.FromResult(string.Empty);

            CoreTaskExtensions.Catch(antecedent, handlerFunction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCatch1_CompletedAntecedent_NullHandler()
        {
            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.FromResult(AntecedentValue);
            Func<Task<string>, ArgumentException, Task<string>> handlerFunction = null;

            CoreTaskExtensions.Catch(antecedent, handlerFunction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CompletedAntecedent_CompletedHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.FromResult(AntecedentValue);
            Func<Task<string>, Exception, Task<string>> handlerFunction =
                (task, ex) => Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        return HandlerValue;
                    });

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(AntecedentValue, combinedTask.Result);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CompletedAntecedent_CanceledHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.FromResult(AntecedentValue);
            Func<Task<string>, Exception, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return CompletedTask.Canceled<string>();
                };

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(AntecedentValue, combinedTask.Result);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CompletedAntecedent_FaultedHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.FromResult(AntecedentValue);

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, Exception, Task<string>> handlerFunction = (task, ex) =>
                Task.Factory.StartNew<string>(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(AntecedentValue, combinedTask.Result);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CompletedAntecedent_SyncFaultedHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.FromResult(AntecedentValue);

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, Exception, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(AntecedentValue, combinedTask.Result);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CompletedAntecedent_NullTaskHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.FromResult(AntecedentValue);
            Func<Task<string>, Exception, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return null;
                };

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(AntecedentValue, combinedTask.Result);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CanceledAntecedent_CompletedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();
            Func<Task<string>, ArgumentException, Task<string>> handlerFunction =
                (task, ex) => Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        return HandlerValue;
                    });

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CanceledAntecedent_CanceledHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();
            Func<Task<string>, ArgumentException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return CompletedTask.Canceled<string>();
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CanceledAntecedent_FaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, ArgumentException, Task<string>> handlerFunction = (task, ex) =>
                Task.Factory.StartNew<string>(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CanceledAntecedent_SyncFaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, ArgumentException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CanceledAntecedent_NullTaskHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();
            Func<Task<string>, ArgumentException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return null;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CanceledAntecedent_CompletedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();
            Func<Task<string>, TaskCanceledException, Task<string>> handlerFunction =
                (task, ex) => Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        return HandlerValue;
                    });

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(HandlerValue, combinedTask.Result);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CanceledAntecedent_CanceledHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();
            Func<Task<string>, TaskCanceledException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return CompletedTask.Canceled<string>();
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CanceledAntecedent_FaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, TaskCanceledException, Task<string>> handlerFunction = (task, ex) =>
                Task.Factory.StartNew<string>(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CanceledAntecedent_SyncFaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, TaskCanceledException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_CanceledAntecedent_NullTaskHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();
            Func<Task<string>, TaskCanceledException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return null;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_FaultedAntecedent_CompletedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Func<Task<string>, ArgumentOutOfRangeException, Task<string>> handlerFunction =
                (task, ex) => Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        return HandlerValue;
                    });

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_FaultedAntecedent_CanceledHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Func<Task<string>, ArgumentOutOfRangeException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return CompletedTask.Canceled<string>();
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_FaultedAntecedent_FaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, ArgumentOutOfRangeException, Task<string>> handlerFunction = (task, ex) =>
                Task.Factory.StartNew<string>(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_FaultedAntecedent_SyncFaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, ArgumentOutOfRangeException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_FaultedAntecedent_NullTaskHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Func<Task<string>, ArgumentOutOfRangeException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return null;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_FaultedAntecedent_CompletedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Func<Task<string>, ArgumentException, Task<string>> handlerFunction =
                (task, ex) => Task.Factory.StartNew(
                    () =>
                    {
                        executed = true;
                        return HandlerValue;
                    });

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(HandlerValue, combinedTask.Result);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_FaultedAntecedent_CanceledHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Func<Task<string>, ArgumentException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return CompletedTask.Canceled<string>();
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_FaultedAntecedent_FaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, ArgumentException, Task<string>> handlerFunction = (task, ex) =>
                Task.Factory.StartNew<string>(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_FaultedAntecedent_SyncFaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, ArgumentException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, Task{TResult}})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch1_FaultedAntecedent_NullTaskHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Func<Task<string>, ArgumentException, Task<string>> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return null;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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

        #region Catch 2

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCatch2_NullAntecedent_CompletedHandler()
        {
            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = null;
            Func<Task<string>, ArgumentException, string> handlerFunction =
                (task, ex) => string.Empty;

            CoreTaskExtensions.Catch(antecedent, handlerFunction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCatch2_CompletedAntecedent_NullHandler()
        {
            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.FromResult(string.Empty);
            Func<Task<string>, ArgumentException, string> handlerFunction = null;

            CoreTaskExtensions.Catch(antecedent, handlerFunction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch2_CompletedAntecedent_CompletedHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.FromResult(AntecedentValue);
            Func<Task<string>, Exception, string> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return HandlerValue;
                };

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(AntecedentValue, combinedTask.Result);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch2_CompletedAntecedent_FaultedHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.FromResult(AntecedentValue);

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, Exception, string> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(AntecedentValue, combinedTask.Result);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch2_CanceledAntecedent_CompletedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();
            Func<Task<string>, ArgumentException, string> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return HandlerValue;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch2_CanceledAntecedent_FaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, ArgumentException, string> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch2_CanceledAntecedent_CompletedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();
            Func<Task<string>, TaskCanceledException, string> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return HandlerValue;
                };

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(HandlerValue, combinedTask.Result);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch2_CanceledAntecedent_FaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task<string> antecedent = CompletedTask.Canceled<string>();

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, Exception, string> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch2_FaultedAntecedent_CompletedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Func<Task<string>, ArgumentOutOfRangeException, string> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return HandlerValue;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch2_FaultedAntecedent_FaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, ArgumentOutOfRangeException, string> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch2_FaultedAntecedent_CompletedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Func<Task<string>, ArgumentException, string> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return HandlerValue;
                };

            Task<string> combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.AreSame(HandlerValue, combinedTask.Result);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException, TResult}(Task{TResult}, Func{Task{TResult}, TException, TResult})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch2_FaultedAntecedent_FaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<string> faultedCompletionSource = new TaskCompletionSource<string>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task<string> antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task<string>, Exception, string> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task<string> combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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

        #region Catch 3

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCatch3_NullAntecedent_CompletedHandler()
        {
            // declaring these makes it clear we are testing the correct overload
            Task antecedent = null;
            Action<Task, ArgumentException> handlerFunction =
                (task, ex) =>
                {
                };

            CoreTaskExtensions.Catch(antecedent, handlerFunction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCatch3_CompletedAntecedent_NullHandler()
        {
            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;
            Action<Task, ArgumentException> handlerFunction = null;

            CoreTaskExtensions.Catch(antecedent, handlerFunction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch3_CompletedAntecedent_CompletedHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;
            Action<Task, Exception> handlerAction =
                (task, ex) => executed = true;

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerAction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch3_CompletedAntecedent_FaultedHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;

            Exception cleanupException = new InvalidOperationException();
            Action<Task, Exception> handlerAction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerAction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch3_CanceledAntecedent_CompletedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();
            Action<Task, ArgumentException> handlerAction =
                (task, ex) => executed = true;

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerAction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch3_CanceledAntecedent_FaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            Exception cleanupException = new InvalidOperationException();
            Action<Task, ArgumentException> handlerAction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerAction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch3_CanceledAntecedent_CompletedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();
            Action<Task, TaskCanceledException> handlerAction =
                (task, ex) => executed = true;

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerAction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch3_CanceledAntecedent_FaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            Exception cleanupException = new InvalidOperationException();
            Action<Task, Exception> handlerAction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerAction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch3_FaultedAntecedent_CompletedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Action<Task, ArgumentOutOfRangeException> handlerAction =
                (task, ex) => executed = true;

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerAction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch3_FaultedAntecedent_FaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Action<Task, ArgumentOutOfRangeException> handlerAction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerAction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch3_FaultedAntecedent_CompletedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Action<Task, ArgumentException> handlerAction =
                (task, ex) => executed = true;

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerAction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Action{Task, TException})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch3_FaultedAntecedent_FaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Action<Task, Exception> handlerAction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerAction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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

        #region Catch 4

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCatch4_NullAntecedent_CompletedHandler()
        {
            // declaring these makes it clear we are testing the correct overload
            Task antecedent = null;
            Func<Task, ArgumentException, Task> handlerFunction =
                (task, ex) => CompletedTask.Default;

            CoreTaskExtensions.Catch(antecedent, handlerFunction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCatch4_CompletedAntecedent_NullHandler()
        {
            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;
            Func<Task, ArgumentException, Task> handlerFunction = null;

            CoreTaskExtensions.Catch(antecedent, handlerFunction);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CompletedAntecedent_CompletedHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;
            Func<Task, Exception, Task> handlerFunction =
                (task, ex) => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CompletedAntecedent_CanceledHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;
            Func<Task, Exception, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return CompletedTask.Canceled();
                };

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CompletedAntecedent_FaultedHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Exception, Task> handlerFunction = (task, ex) =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CompletedAntecedent_SyncFaultedHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, Exception, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CompletedAntecedent_NullTaskHandler()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Default;
            Func<Task, Exception, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CanceledAntecedent_CompletedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();
            Func<Task, ArgumentException, Task> handlerFunction =
                (task, ex) => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CanceledAntecedent_CanceledHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();
            Func<Task, ArgumentException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return CompletedTask.Canceled();
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CanceledAntecedent_FaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            Exception cleanupException = new InvalidOperationException();
            Func<Task, ArgumentException, Task> handlerFunction = (task, ex) =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CanceledAntecedent_SyncFaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            Exception cleanupException = new InvalidOperationException();
            Func<Task, ArgumentException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CanceledAntecedent_NullTaskHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();
            Func<Task, ArgumentException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Canceled, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CanceledAntecedent_CompletedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();
            Func<Task, TaskCanceledException, Task> handlerFunction =
                (task, ex) => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CanceledAntecedent_CanceledHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();
            Func<Task, TaskCanceledException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return CompletedTask.Canceled();
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CanceledAntecedent_FaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            Exception cleanupException = new InvalidOperationException();
            Func<Task, TaskCanceledException, Task> handlerFunction = (task, ex) =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CanceledAntecedent_SyncFaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();

            Exception cleanupException = new InvalidOperationException();
            Func<Task, TaskCanceledException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_CanceledAntecedent_NullTaskHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            Task antecedent = CompletedTask.Canceled();
            Func<Task, TaskCanceledException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_FaultedAntecedent_CompletedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Func<Task, ArgumentOutOfRangeException, Task> handlerFunction =
                (task, ex) => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_FaultedAntecedent_CanceledHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Func<Task, ArgumentOutOfRangeException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return CompletedTask.Canceled();
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_FaultedAntecedent_FaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, ArgumentOutOfRangeException, Task> handlerFunction = (task, ex) =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_FaultedAntecedent_SyncFaultedHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, ArgumentOutOfRangeException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_FaultedAntecedent_NullTaskHandler_Unhandled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Func<Task, ArgumentOutOfRangeException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(combinedTask, "Failed to create the combined task.");
                Assert.AreEqual(TaskStatus.Faulted, combinedTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(expectedException, ex.InnerExceptions[0]);
                Assert.IsFalse(executed);
            }
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_FaultedAntecedent_CompletedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Func<Task, ArgumentException, Task> handlerFunction =
                (task, ex) => Task.Factory.StartNew(() => executed = true);

            Task combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
            combinedTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, combinedTask.Status);
            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This method test the behavior of the
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_FaultedAntecedent_CanceledHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Func<Task, ArgumentException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return CompletedTask.Canceled();
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_FaultedAntecedent_FaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, ArgumentException, Task> handlerFunction = (task, ex) =>
                Task.Factory.StartNew(() =>
                {
                    executed = true;
                    throw cleanupException;
                });

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_FaultedAntecedent_SyncFaultedHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Exception cleanupException = new InvalidOperationException();
            Func<Task, ArgumentException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    throw cleanupException;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
        /// <see cref="CoreTaskExtensions.Catch{TException}(Task, Func{Task, TException, Task})"/>
        /// method.
        /// </summary>
        [TestMethod]
        public void TestCatch4_FaultedAntecedent_NullTaskHandler_Handled()
        {
            bool executed = false;

            // declaring these makes it clear we are testing the correct overload
            TaskCompletionSource<object> faultedCompletionSource = new TaskCompletionSource<object>();
            Exception expectedException = new ArgumentException();
            faultedCompletionSource.SetException(expectedException);
            Task antecedent = faultedCompletionSource.Task;

            Func<Task, ArgumentException, Task> handlerFunction =
                (task, ex) =>
                {
                    executed = true;
                    return null;
                };

            Task combinedTask = null;

            try
            {
                combinedTask = CoreTaskExtensions.Catch(antecedent, handlerFunction);
                combinedTask.Wait();
                Assert.Fail("Expected an AggregateException");
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
