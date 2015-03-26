// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

    [TestClass]
    public class TestCancellationTokenSourceExtensions
    {
        /// <summary>
        /// This test validates the basic behavior of <see cref="CancellationTokenSourceExtensions.CancelAfter"/>.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestCancelAfter()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(0.25);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.025);

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

        /// <summary>
        /// This test validates the behavior of <see cref="CancellationTokenSourceExtensions.CancelAfter"/> when
        /// the <see cref="CancellationTokenSource"/> has already been canceled.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestCancelAfter_AlreadyCanceled()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            Assert.IsTrue(cts.IsCancellationRequested);

            cts.CancelAfter(TimeSpan.FromSeconds(2));
            Assert.IsTrue(cts.IsCancellationRequested);
        }

        /// <summary>
        /// This test validates the behavior of <see cref="CancellationTokenSourceExtensions.CancelAfter"/> when
        /// called multiple times for a single <see cref="CancellationTokenSource"/> instance.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestCancelAfter_TimerReset()
        {
            TimeSpan initialTimeout = TimeSpan.FromSeconds(0.10);
            TimeSpan updatedTimeout = TimeSpan.FromSeconds(0.35);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.025);

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

        /// <summary>
        /// This test ensures the <see cref="CancellationTokenSourceExtensions.CancelAfter"/> method throws
        /// the expected exception for a <see langword="null"/> <see cref="CancellationTokenSource"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestCancelAfter_ArgumentNull()
        {
            CancellationTokenSourceExtensions.CancelAfter(null, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// This test ensures the <see cref="CancellationTokenSourceExtensions.CancelAfter"/> method throws
        /// the expected exception for an invalid negative delay.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestCancelAfter_ArgumentOutOfRange()
        {
            CancellationTokenSourceExtensions.CancelAfter(new CancellationTokenSource(), TimeSpan.FromSeconds(-1));
        }

        /// <summary>
        /// This test ensures the <see cref="CancellationTokenSourceExtensions.CancelAfter"/> does not
        /// affect whether or not a <see cref="CancellationTokenSource"/> is eligible for garbage collection.
        /// </summary>
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

        /// <summary>
        /// This test ensures a <see cref="Timer"/> created on the stack still executes its callback once,
        /// an imperfect measure of GC predictability within the test environment.
        /// </summary>
        [TestMethod]
        public void TestNoForcedGCAllowsTimer()
        {
            bool executed = false;
            new Timer(_ => executed = true, null, TimeSpan.FromSeconds(0.4), TimeSpan.FromMilliseconds(-1));

            global::System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(0.6)).Wait();

            Assert.IsTrue(executed);
        }

        /// <summary>
        /// This test ensures a <see cref="Timer"/> created on the stack is finalized by a forced garbage
        /// collection, and thus does not execute its callback, an imperfect measure of GC predictability
        /// within the test environment.
        /// </summary>
        [TestMethod]
        public void TestForcedGCHaltsTimer()
        {
            bool executed = false;
            new Timer(_ => executed = true, null, TimeSpan.FromSeconds(0.4), TimeSpan.FromMilliseconds(-1));

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            global::System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(0.6)).Wait();

            Assert.IsFalse(executed);
        }

        /// <summary>
        /// This test verifies that the <see cref="Timer"/> instances created and managed by
        /// <see cref="CancellationTokenSourceExtensions"/> are not eligible for garbage collection
        /// prematurely, provided the test environment is predictable with respect to the
        /// <see cref="TestNoForcedGCAllowsTimer"/> and <see cref="TestForcedGCHaltsTimer"/> tests.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestCancelAfter_TimerPinning()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(0.40);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.025);

            CancellationTokenSource cts = new CancellationTokenSource();
            Stopwatch timer = Stopwatch.StartNew();
            CancellationTokenSourceExtensions.CancelAfter(cts, timeout);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

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
    }
}
