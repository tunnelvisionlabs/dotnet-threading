// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

    [TestClass]
    public class TestDelayedTask_WhenAll : TaskTestingBase
    {
        #region WhenAll 1

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhenAll1_NullTasks()
        {
            IEnumerable<Task> tasks = null;
            DelayedTask.WhenAll(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAll1_NullMemberTask()
        {
            IEnumerable<Task> tasks = new[] { CompletedTask.FromResult(string.Empty), null };
            DelayedTask.WhenAll(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_EmptyTasks()
        {
            IEnumerable<Task> tasks = Enumerable.Empty<Task>();
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_Completed1()
        {
            IEnumerable<Task> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty) };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_Completed3()
        {
            IEnumerable<Task> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                };

            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_Canceled1()
        {
            IEnumerable<Task> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled()) };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsCanceled);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_Canceled3()
        {
            IEnumerable<Task> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled()),
                };

            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsCanceled);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_Faulted1()
        {
            Exception expectedException = new FormatException();
            Action<Task> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            IEnumerable<Task> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector) };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                Assert.AreSame(expectedException, task.Exception.InnerExceptions[0]);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_Faulted3()
        {
            Exception expectedException = new FormatException();
            Action<Task> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            IEnumerable<Task> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                };

            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(3, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                Assert.AreSame(expectedException, task.Exception.InnerExceptions[0]);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_Completed1_Canceled1()
        {
            IEnumerable<Task> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled())
                };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            Task firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, firstTask.Status);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, secondTask.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_Completed1_Faulted1()
        {
            Exception expectedException = new FormatException();
            Action<Task> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            IEnumerable<Task> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector)
                };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            Task firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, firstTask.Status);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, secondTask.Status);
            Assert.IsNotNull(secondTask.Exception);
            Assert.AreEqual(1, secondTask.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, secondTask.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_Canceled1_Faulted1()
        {
            Exception expectedException = new FormatException();
            Action<Task> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            IEnumerable<Task> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector)
                };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            Task firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.IsTrue(firstTask.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, firstTask.Status);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, secondTask.Status);
            Assert.IsNotNull(secondTask.Exception);
            Assert.AreEqual(1, secondTask.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, secondTask.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll1_MultiFaulted1()
        {
            Exception[] expectedExceptions = { new FormatException(), new NotSupportedException() };
            Func<Task, Task> exceptionSelector =
                task =>
                {
                    TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                    tcs.SetException(expectedExceptions);
                    return tcs.Task;
                };
            IEnumerable<Task> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(exceptionSelector) };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(expectedExceptions.Length, ex.InnerExceptions.Count);
                for (int i = 0; i < expectedExceptions.Length; i++)
                {
                    Assert.AreSame(expectedExceptions[i], ex.InnerExceptions[i]);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(expectedExceptions.Length, task.Exception.InnerExceptions.Count);
                for (int i = 0; i < expectedExceptions.Length; i++)
                {
                    Assert.AreSame(expectedExceptions[i], task.Exception.InnerExceptions[i]);
                }
            }
        }

        #endregion

        #region WhenAll 2

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhenAll2_NullTasks()
        {
            Task[] tasks = null;
            DelayedTask.WhenAll(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAll2_NullMemberTask()
        {
            Task[] tasks = { CompletedTask.FromResult(string.Empty), null };
            DelayedTask.WhenAll(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_EmptyTasks()
        {
            Task[] tasks = new Task[0];
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_Completed1()
        {
            Task[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty) };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_Completed3()
        {
            Task[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                };

            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_Canceled1()
        {
            Task[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled()) };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsCanceled);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_Canceled3()
        {
            Task[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled()),
                };

            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsCanceled);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_Faulted1()
        {
            Exception expectedException = new FormatException();
            Action<Task> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            Task[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector) };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                Assert.AreSame(expectedException, task.Exception.InnerExceptions[0]);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_Faulted3()
        {
            Exception expectedException = new FormatException();
            Action<Task> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            Task[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                };

            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(3, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                Assert.AreSame(expectedException, task.Exception.InnerExceptions[0]);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_Completed1_Canceled1()
        {
            Task[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled())
                };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            Task firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, firstTask.Status);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, secondTask.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_Completed1_Faulted1()
        {
            Exception expectedException = new FormatException();
            Action<Task> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            Task[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector)
                };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            Task firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, firstTask.Status);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, secondTask.Status);
            Assert.IsNotNull(secondTask.Exception);
            Assert.AreEqual(1, secondTask.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, secondTask.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_Canceled1_Faulted1()
        {
            Exception expectedException = new FormatException();
            Action<Task> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            Task[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector)
                };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            Task firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.IsTrue(firstTask.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, firstTask.Status);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, secondTask.Status);
            Assert.IsNotNull(secondTask.Exception);
            Assert.AreEqual(1, secondTask.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, secondTask.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll2_MultiFaulted1()
        {
            Exception[] expectedExceptions = { new FormatException(), new NotSupportedException() };
            Func<Task, Task> exceptionSelector =
                task =>
                {
                    TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                    tcs.SetException(expectedExceptions);
                    return tcs.Task;
                };
            Task[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(exceptionSelector) };
            Task delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(expectedExceptions.Length, ex.InnerExceptions.Count);
                for (int i = 0; i < expectedExceptions.Length; i++)
                {
                    Assert.AreSame(expectedExceptions[i], ex.InnerExceptions[i]);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(expectedExceptions.Length, task.Exception.InnerExceptions.Count);
                for (int i = 0; i < expectedExceptions.Length; i++)
                {
                    Assert.AreSame(expectedExceptions[i], task.Exception.InnerExceptions[i]);
                }
            }
        }

        #endregion

        #region WhenAll 3

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhenAll3_NullTasks()
        {
            IEnumerable<Task<string>> tasks = null;
            DelayedTask.WhenAll(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAll3_NullMemberTask()
        {
            IEnumerable<Task<string>> tasks = new[] { CompletedTask.FromResult(string.Empty), null };
            DelayedTask.WhenAll(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_EmptyTasks()
        {
            IEnumerable<Task<string>> tasks = Enumerable.Empty<Task<string>>();
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.AreEqual(0, delayed.Result.Length);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_Completed1()
        {
            IEnumerable<Task<string>> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty) };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.AreEqual(tasks.Count(), delayed.Result.Length);

            int i = 0;
            foreach (Task<string> task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                Assert.AreSame(delayed.Result[i], task.Result);
                i++;
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_Completed3()
        {
            IEnumerable<Task<string>> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                };

            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.AreEqual(tasks.Count(), delayed.Result.Length);

            int i = 0;
            foreach (Task<string> task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                Assert.AreSame(delayed.Result[i], task.Result);
                i++;
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_Canceled1()
        {
            IEnumerable<Task<string>> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>()) };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsCanceled);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_Canceled3()
        {
            IEnumerable<Task<string>> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>()),
                };

            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsCanceled);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_Faulted1()
        {
            Exception expectedException = new FormatException();
            Func<Task, string> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            IEnumerable<Task<string>> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector) };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                Assert.AreSame(expectedException, task.Exception.InnerExceptions[0]);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_Faulted3()
        {
            Exception expectedException = new FormatException();
            Func<Task, string> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            IEnumerable<Task<string>> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                };

            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(3, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                Assert.AreSame(expectedException, task.Exception.InnerExceptions[0]);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_Completed1_Canceled1()
        {
            IEnumerable<Task<string>> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>())
                };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            Task<string> firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, firstTask.Status);
            Assert.AreSame(string.Empty, firstTask.Result);

            Task<string> secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, secondTask.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_Completed1_Faulted1()
        {
            Exception expectedException = new FormatException();
            Func<Task, string> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            IEnumerable<Task<string>> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector)
                };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            Task<string> firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, firstTask.Status);
            Assert.AreSame(string.Empty, firstTask.Result);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, secondTask.Status);
            Assert.IsNotNull(secondTask.Exception);
            Assert.AreEqual(1, secondTask.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, secondTask.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_Canceled1_Faulted1()
        {
            Exception expectedException = new FormatException();
            Func<Task, string> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            IEnumerable<Task<string>> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector)
                };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            Task firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.IsTrue(firstTask.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, firstTask.Status);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, secondTask.Status);
            Assert.IsNotNull(secondTask.Exception);
            Assert.AreEqual(1, secondTask.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, secondTask.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll3_MultiFaulted1()
        {
            Exception[] expectedExceptions = { new FormatException(), new NotSupportedException() };
            Func<Task, Task<string>> exceptionSelector =
                task =>
                {
                    TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
                    tcs.SetException(expectedExceptions);
                    return tcs.Task;
                };
            IEnumerable<Task<string>> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(exceptionSelector) };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(expectedExceptions.Length, ex.InnerExceptions.Count);
                for (int i = 0; i < expectedExceptions.Length; i++)
                {
                    Assert.AreSame(expectedExceptions[i], ex.InnerExceptions[i]);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(expectedExceptions.Length, task.Exception.InnerExceptions.Count);
                for (int i = 0; i < expectedExceptions.Length; i++)
                {
                    Assert.AreSame(expectedExceptions[i], task.Exception.InnerExceptions[i]);
                }
            }
        }

        #endregion

        #region WhenAll 4

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhenAll4_NullTasks()
        {
            Task<string>[] tasks = null;
            DelayedTask.WhenAll(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAll4_NullMemberTask()
        {
            Task<string>[] tasks = { CompletedTask.FromResult(string.Empty), null };
            DelayedTask.WhenAll(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_EmptyTasks()
        {
            Task<string>[] tasks = new Task<string>[0];
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.AreEqual(0, delayed.Result.Length);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_Completed1()
        {
            Task<string>[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty) };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.AreEqual(tasks.Count(), delayed.Result.Length);

            int i = 0;
            foreach (Task<string> task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                Assert.AreSame(delayed.Result[i], task.Result);
                i++;
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_Completed3()
        {
            Task<string>[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                };

            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.AreEqual(tasks.Count(), delayed.Result.Length);

            int i = 0;
            foreach (Task<string> task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
                Assert.AreSame(delayed.Result[i], task.Result);
                i++;
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_Canceled1()
        {
            Task<string>[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>()) };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsCanceled);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_Canceled3()
        {
            Task<string>[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>()),
                };

            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsCanceled);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_Faulted1()
        {
            Exception expectedException = new FormatException();
            Func<Task, string> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            Task<string>[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector) };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                Assert.AreSame(expectedException, task.Exception.InnerExceptions[0]);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_Faulted3()
        {
            Exception expectedException = new FormatException();
            Func<Task, string> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            Task<string>[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(2 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector),
                };

            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(3, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(1, task.Exception.InnerExceptions.Count);
                Assert.AreSame(expectedException, task.Exception.InnerExceptions[0]);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_Completed1_Canceled1()
        {
            Task<string>[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>())
                };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Canceled, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Status);

            Task<string> firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, firstTask.Status);
            Assert.AreSame(string.Empty, firstTask.Result);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, secondTask.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_Completed1_Faulted1()
        {
            Exception expectedException = new FormatException();
            Func<Task, string> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            Task<string>[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector)
                };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            Task<string> firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, firstTask.Status);
            Assert.AreSame(string.Empty, firstTask.Result);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, secondTask.Status);
            Assert.IsNotNull(secondTask.Exception);
            Assert.AreEqual(1, secondTask.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, secondTask.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_Canceled1_Faulted1()
        {
            Exception expectedException = new FormatException();
            Func<Task, string> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            Task<string>[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(3 * TimingGranularity.TotalMilliseconds)).Select(exceptionSelector)
                };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                foreach (Exception innerException in ex.InnerExceptions)
                {
                    Assert.AreSame(expectedException, innerException);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            Task firstTask = tasks.ElementAt(0);
            Assert.IsTrue(firstTask.IsCompleted);
            Assert.IsTrue(firstTask.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, firstTask.Status);

            Task secondTask = tasks.ElementAt(1);
            Assert.IsTrue(secondTask.IsCompleted);
            Assert.IsTrue(secondTask.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, secondTask.Status);
            Assert.IsNotNull(secondTask.Exception);
            Assert.AreEqual(1, secondTask.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, secondTask.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAll{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAll4_MultiFaulted1()
        {
            Exception[] expectedExceptions = { new FormatException(), new NotSupportedException() };
            Func<Task, Task<string>> exceptionSelector =
                task =>
                {
                    TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
                    tcs.SetException(expectedExceptions);
                    return tcs.Task;
                };
            Task<string>[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(1 * TimingGranularity.TotalMilliseconds)).Then(exceptionSelector) };
            Task<string[]> delayed = DelayedTask.WhenAll(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            try
            {
                delayed.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(TaskStatus.Faulted, delayed.Status);
                Assert.AreEqual(expectedExceptions.Length, ex.InnerExceptions.Count);
                for (int i = 0; i < expectedExceptions.Length; i++)
                {
                    Assert.AreSame(expectedExceptions[i], ex.InnerExceptions[i]);
                }
            }

            Assert.IsTrue(delayed.IsCompleted);
            Assert.IsTrue(delayed.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Status);

            foreach (Task task in tasks)
            {
                Assert.IsTrue(task.IsCompleted);
                Assert.IsTrue(task.IsFaulted);
                Assert.AreEqual(TaskStatus.Faulted, task.Status);
                Assert.IsNotNull(task.Exception);
                Assert.AreEqual(expectedExceptions.Length, task.Exception.InnerExceptions.Count);
                for (int i = 0; i < expectedExceptions.Length; i++)
                {
                    Assert.AreSame(expectedExceptions[i], task.Exception.InnerExceptions[i]);
                }
            }
        }

        #endregion
    }
}
