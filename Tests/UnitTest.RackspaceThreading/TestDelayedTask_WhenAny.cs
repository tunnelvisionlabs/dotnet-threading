// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
#if !NET40PLUS
    extern alias tpl;
#endif

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

#if !NET40PLUS
    using tpl::System.Threading.Tasks;
    using AggregateException = tpl::System.AggregateException;
#else
    using System.Threading.Tasks;
#endif

    [TestClass]
    public class TestDelayedTask_WhenAny
    {
        #region WhenAny 1

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhenAny1_NullTasks()
        {
            IEnumerable<Task> tasks = null;
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAny1_NullMemberTask()
        {
            IEnumerable<Task> tasks = new[] { CompletedTask.FromResult(string.Empty), null };
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAny1_EmptyTasks()
        {
            IEnumerable<Task> tasks = Enumerable.Empty<Task>();
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny1_Completed1()
        {
            IEnumerable<Task> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty) };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
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
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny1_Completed3()
        {
            IEnumerable<Task> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Select(_ => string.Empty),
                };

            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);

            Assert.IsTrue(tasks.Contains(delayed.Result));
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny1_Canceled1()
        {
            IEnumerable<Task> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled()) };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny1_Canceled3()
        {
            IEnumerable<Task> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Then(_ => CompletedTask.Canceled()),
                };

            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny1_Faulted1()
        {
            Exception expectedException = new FormatException();
            Action<Task> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            IEnumerable<Task> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(exceptionSelector) };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(1, delayed.Result.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, delayed.Result.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny1_Faulted3()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Select(exceptionSelector),
                };

            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(1, delayed.Result.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, delayed.Result.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny1_Completed1_Canceled1()
        {
            IEnumerable<Task> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Then(_ => CompletedTask.Canceled())
                };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny1_Completed1_Faulted1()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector)
                };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny1_Canceled1_Faulted1()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector)
                };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(IEnumerable{Task})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny1_MultiFaulted1()
        {
            Exception[] expectedExceptions = { new FormatException(), new NotSupportedException() };
            Func<Task, Task> exceptionSelector =
                task =>
                {
                    TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                    tcs.SetException(expectedExceptions);
                    return tcs.Task;
                };
            IEnumerable<Task> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(exceptionSelector) };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(expectedExceptions.Length, delayed.Result.Exception.InnerExceptions.Count);
            for (int i = 0; i < expectedExceptions.Length; i++)
            {
                Assert.AreSame(expectedExceptions[i], delayed.Result.Exception.InnerExceptions[i]);
            }
        }

        #endregion

        #region WhenAny 2

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhenAny2_NullTasks()
        {
            Task[] tasks = null;
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAny2_NullMemberTask()
        {
            Task[] tasks = { CompletedTask.FromResult(string.Empty), null };
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAny2_EmptyTasks()
        {
            Task[] tasks = new Task[0];
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny2_Completed1()
        {
            Task[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty) };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
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
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny2_Completed3()
        {
            Task[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Select(_ => string.Empty),
                };

            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);

            Assert.IsTrue(tasks.Contains(delayed.Result));
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny2_Canceled1()
        {
            Task[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled()) };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny2_Canceled3()
        {
            Task[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Then(_ => CompletedTask.Canceled()),
                };

            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny2_Faulted1()
        {
            Exception expectedException = new FormatException();
            Action<Task> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            Task[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(exceptionSelector) };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(1, delayed.Result.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, delayed.Result.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny2_Faulted3()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Select(exceptionSelector),
                };

            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(1, delayed.Result.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, delayed.Result.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny2_Completed1_Canceled1()
        {
            Task[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Then(_ => CompletedTask.Canceled())
                };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny2_Completed1_Faulted1()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector)
                };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny2_Canceled1_Faulted1()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector)
                };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny(Task[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny2_MultiFaulted1()
        {
            Exception[] expectedExceptions = { new FormatException(), new NotSupportedException() };
            Func<Task, Task> exceptionSelector =
                task =>
                {
                    TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                    tcs.SetException(expectedExceptions);
                    return tcs.Task;
                };
            Task[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(exceptionSelector) };
            Task<Task> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(expectedExceptions.Length, delayed.Result.Exception.InnerExceptions.Count);
            for (int i = 0; i < expectedExceptions.Length; i++)
            {
                Assert.AreSame(expectedExceptions[i], delayed.Result.Exception.InnerExceptions[i]);
            }
        }

        #endregion

        #region WhenAny 3

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhenAny3_NullTasks()
        {
            IEnumerable<Task<string>> tasks = null;
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAny3_NullMemberTask()
        {
            IEnumerable<Task<string>> tasks = new[] { CompletedTask.FromResult(string.Empty), null };
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAny3_EmptyTasks()
        {
            IEnumerable<Task<string>> tasks = Enumerable.Empty<Task<string>>();
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny3_Completed1()
        {
            IEnumerable<Task<string>> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty) };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);

            Assert.IsTrue(tasks.Contains(delayed.Result));
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
            Assert.AreSame(string.Empty, delayed.Result.Result);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny3_Completed3()
        {
            IEnumerable<Task<string>> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Select(_ => string.Empty),
                };

            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);

            Assert.IsTrue(tasks.Contains(delayed.Result));
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
            Assert.AreSame(string.Empty, delayed.Result.Result);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny3_Canceled1()
        {
            IEnumerable<Task<string>> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled<string>()) };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny3_Canceled3()
        {
            IEnumerable<Task<string>> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Then(_ => CompletedTask.Canceled<string>()),
                };

            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny3_Faulted1()
        {
            Exception expectedException = new FormatException();
            Func<Task, string> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            IEnumerable<Task<string>> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(exceptionSelector) };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(1, delayed.Result.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, delayed.Result.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny3_Faulted3()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Select(exceptionSelector),
                };

            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(1, delayed.Result.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, delayed.Result.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny3_Completed1_Canceled1()
        {
            IEnumerable<Task<string>> tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Then(_ => CompletedTask.Canceled<string>())
                };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny3_Completed1_Faulted1()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector)
                };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny3_Canceled1_Faulted1()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector)
                };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(IEnumerable{Task{TResult}})"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny3_MultiFaulted1()
        {
            Exception[] expectedExceptions = { new FormatException(), new NotSupportedException() };
            Func<Task, Task<string>> exceptionSelector =
                task =>
                {
                    TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
                    tcs.SetException(expectedExceptions);
                    return tcs.Task;
                };
            IEnumerable<Task<string>> tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(exceptionSelector) };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(expectedExceptions.Length, delayed.Result.Exception.InnerExceptions.Count);
            for (int i = 0; i < expectedExceptions.Length; i++)
            {
                Assert.AreSame(expectedExceptions[i], delayed.Result.Exception.InnerExceptions[i]);
            }
        }

        #endregion

        #region WhenAny 4

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhenAny4_NullTasks()
        {
            Task<string>[] tasks = null;
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAny4_NullMemberTask()
        {
            Task<string>[] tasks = { CompletedTask.FromResult(string.Empty), null };
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestWhenAny4_EmptyTasks()
        {
            Task<string>[] tasks = new Task<string>[0];
            DelayedTask.WhenAny(tasks);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny4_Completed1()
        {
            Task<string>[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty) };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);

            Assert.IsTrue(tasks.Contains(delayed.Result));
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
            Assert.AreSame(string.Empty, delayed.Result.Result);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny4_Completed3()
        {
            Task<string>[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Select(_ => string.Empty),
                };

            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);
            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);

            Assert.IsTrue(tasks.Contains(delayed.Result));
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
            Assert.AreSame(string.Empty, delayed.Result.Result);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny4_Canceled1()
        {
            Task<string>[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled<string>()) };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny4_Canceled3()
        {
            Task<string>[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Then(_ => CompletedTask.Canceled<string>()),
                };

            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny4_Faulted1()
        {
            Exception expectedException = new FormatException();
            Func<Task, string> exceptionSelector =
                task =>
                {
                    throw expectedException;
                };
            Task<string>[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(exceptionSelector) };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(1, delayed.Result.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, delayed.Result.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny4_Faulted3()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(20)).Select(exceptionSelector),
                };

            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(1, delayed.Result.Exception.InnerExceptions.Count);
            Assert.AreSame(expectedException, delayed.Result.Exception.InnerExceptions[0]);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny4_Completed1_Canceled1()
        {
            Task<string>[] tasks =
                new[]
                {
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Then(_ => CompletedTask.Canceled<string>())
                };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny4_Completed1_Faulted1()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Select(_ => string.Empty),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector)
                };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Result.Status);
            Assert.AreSame(string.Empty, delayed.Result.Result);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny4_Canceled1_Faulted1()
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
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(_ => CompletedTask.Canceled<string>()),
                    DelayedTask.Delay(TimeSpan.FromMilliseconds(30)).Select(exceptionSelector)
                };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            // this one was the first to complete
            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsCanceled);
            Assert.AreEqual(TaskStatus.Canceled, delayed.Result.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.WhenAny{TResult}(Task{TResult}[])"/>
        /// </summary>
        [TestMethod]
        public void TestWhenAny4_MultiFaulted1()
        {
            Exception[] expectedExceptions = { new FormatException(), new NotSupportedException() };
            Func<Task, Task<string>> exceptionSelector =
                task =>
                {
                    TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
                    tcs.SetException(expectedExceptions);
                    return tcs.Task;
                };
            Task<string>[] tasks = new[] { DelayedTask.Delay(TimeSpan.FromMilliseconds(10)).Then(exceptionSelector) };
            Task<Task<string>> delayed = DelayedTask.WhenAny(tasks);
            Assert.IsFalse(delayed.IsCompleted);

            delayed.Wait();
            Assert.IsTrue(delayed.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, delayed.Status);
            Assert.IsNotNull(delayed.Result);
            Assert.IsTrue(tasks.Contains(delayed.Result));

            Assert.IsTrue(delayed.Result.IsCompleted);
            Assert.IsTrue(delayed.Result.IsFaulted);
            Assert.AreEqual(TaskStatus.Faulted, delayed.Result.Status);
            Assert.IsNotNull(delayed.Result.Exception);
            Assert.AreEqual(expectedExceptions.Length, delayed.Result.Exception.InnerExceptions.Count);
            for (int i = 0; i < expectedExceptions.Length; i++)
            {
                Assert.AreSame(expectedExceptions[i], delayed.Result.Exception.InnerExceptions[i]);
            }
        }

        #endregion
    }
}
