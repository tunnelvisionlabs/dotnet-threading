// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
#if !NET40PLUS
    extern alias tpl;
#endif

    using System;
    using System.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

#if !NET40PLUS
    using tpl::System.Threading.Tasks;
    using AggregateException = tpl::System.AggregateException;
#else
    using System.Threading.Tasks;
#endif

    [TestClass]
    public class TestCompletedTask
    {
        [TestMethod]
        public void TestCancelled()
        {
            Task cancelledTask = CompletedTask.Canceled();
            Assert.AreEqual(TaskStatus.Canceled, cancelledTask.Status);
            Assert.IsTrue(cancelledTask.IsCanceled);

            try
            {
                cancelledTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }
        }

        [TestMethod]
        public void TestCancelledT()
        {
            Task<int> cancelledTask = CompletedTask.Canceled<int>();
            Assert.AreEqual(TaskStatus.Canceled, cancelledTask.Status);
            Assert.IsTrue(cancelledTask.IsCanceled);

            try
            {
                cancelledTask.Wait();
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }

            try
            {
                int result = cancelledTask.Result;
                Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
            }
        }

        [TestMethod]
        public void TestDefault()
        {
            Task completedTask = CompletedTask.Default;
            Assert.AreEqual(TaskStatus.RanToCompletion, completedTask.Status);
            Assert.IsTrue(completedTask.IsCompleted);

            Stopwatch timer = Stopwatch.StartNew();
            Task.WaitAll(new[] { completedTask });
            Assert.IsTrue(timer.Elapsed <= TimeSpan.FromMilliseconds(5), "Waiting on CompletedTask.Default resulted in an unexpected delay ({0}ms > 5ms)", timer.Elapsed.TotalMilliseconds);
        }

        [TestMethod]
        public void TestDefaultT()
        {
            int expected = 3;

            Task<int> completedTask = CompletedTask.FromResult(expected);
            Assert.AreEqual(TaskStatus.RanToCompletion, completedTask.Status);
            Assert.IsTrue(completedTask.IsCompleted);
            Assert.AreEqual(expected, completedTask.Result);

            Stopwatch timer = Stopwatch.StartNew();
            Task.WaitAll(new[] { completedTask });
            Assert.IsTrue(timer.Elapsed <= TimeSpan.FromMilliseconds(5), "Waiting on CompletedTask.Default resulted in an unexpected delay ({0}ms > 5ms)", timer.Elapsed.TotalMilliseconds);
        }

        [TestMethod]
        public void TestDefaultTObject()
        {
            object expected = new object();

            Task<object> completedTask = CompletedTask.FromResult(expected);
            Assert.AreEqual(TaskStatus.RanToCompletion, completedTask.Status);
            Assert.IsTrue(completedTask.IsCompleted);
            Assert.AreSame(expected, completedTask.Result);

            Stopwatch timer = Stopwatch.StartNew();
            Task.WaitAll(new[] { completedTask });
            Assert.IsTrue(timer.Elapsed <= TimeSpan.FromMilliseconds(5), "Waiting on CompletedTask.Default resulted in an unexpected delay ({0}ms > 5ms)", timer.Elapsed.TotalMilliseconds);
        }
    }
}
