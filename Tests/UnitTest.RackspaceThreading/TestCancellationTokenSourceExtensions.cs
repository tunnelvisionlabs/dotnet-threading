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
    }
}

#endif
