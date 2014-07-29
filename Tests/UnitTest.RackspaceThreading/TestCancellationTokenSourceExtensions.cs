// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if !NET45PLUS

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
    using tpl::System.Threading;
    using tpl::System.Threading.Tasks;
#else
    using System.Threading;
    using System.Threading.Tasks;
#endif

    [TestClass]
    public class TestCancellationTokenSourceExtensions
    {
        [TestMethod]
        [Timeout(2000)]
        public void TestCancelAfter()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(0.25);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.003);

            CancellationTokenSource cts = new CancellationTokenSource();
            Stopwatch timer = Stopwatch.StartNew();
            CancellationTokenSourceExtensions.CancelAfter(cts, timeout);

            // a task which never completes
            Task[] tasks = { new TaskCompletionSource<object>().Task };

            try
            {
                Task.WaitAll(tasks, cts.Token);
                Assert.Fail("The CancellationTokenSource failed to cancel.");
            }
            catch (OperationCanceledException)
            {
                TimeSpan elapsed = timer.Elapsed;
                Assert.IsTrue(elapsed >= timeout - tolerance, "The CancellationTokenSource cancelled too soon ({0} sec < {1} sec).", elapsed.TotalSeconds, (timeout - tolerance).TotalSeconds);
                Assert.IsTrue(elapsed <= timeout + tolerance, "The CancellationTokenSource cancelled too late ({0} sec > {1} sec).", elapsed.TotalSeconds, (timeout + tolerance).TotalSeconds);
            }
        }

        [TestMethod]
        [Timeout(2000)]
        public void TestCancelAfter_TimerReset()
        {
            TimeSpan initialTimeout = TimeSpan.FromSeconds(0.10);
            TimeSpan updatedTimeout = TimeSpan.FromSeconds(0.35);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.003);

            CancellationTokenSource cts = new CancellationTokenSource();
            Stopwatch timer = Stopwatch.StartNew();
            CancellationTokenSourceExtensions.CancelAfter(cts, initialTimeout);
            CancellationTokenSourceExtensions.CancelAfter(cts, updatedTimeout);

            // a task which never completes
            Task[] tasks = { new TaskCompletionSource<object>().Task };

            try
            {
                Task.WaitAll(tasks, cts.Token);
                Assert.Fail("The CancellationTokenSource failed to cancel.");
            }
            catch (OperationCanceledException)
            {
                TimeSpan elapsed = timer.Elapsed;
                Assert.IsTrue(elapsed >= updatedTimeout - tolerance, "The CancellationTokenSource cancelled too soon ({0} sec < {1} sec).", elapsed.TotalSeconds, (updatedTimeout - tolerance).TotalSeconds);
                Assert.IsTrue(elapsed <= updatedTimeout + tolerance, "The CancellationTokenSource cancelled too late ({0} sec > {1} sec).", elapsed.TotalSeconds, (updatedTimeout + tolerance).TotalSeconds);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCancelAfter_ArgumentNull()
        {
            CancellationTokenSourceExtensions.CancelAfter(null, TimeSpan.FromSeconds(1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestCancelAfter_ArgumentOutOfRange()
        {
            CancellationTokenSourceExtensions.CancelAfter(new CancellationTokenSource(), TimeSpan.FromSeconds(-1));
        }

        [TestMethod]
        public void TestCancelAfter_GCEligible()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            WeakReference weakReference = new WeakReference(cts);
            CancellationTokenSourceExtensions.CancelAfter(cts, TimeSpan.FromHours(1));

            Assert.IsNotNull(weakReference.Target);

            GC.KeepAlive(cts);
            cts = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            Assert.IsNull(weakReference.Target);
        }
    }
}

#endif
