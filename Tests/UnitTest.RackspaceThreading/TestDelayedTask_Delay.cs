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
    using tpl::System.Threading;
    using tpl::System.Threading.Tasks;
    using AggregateException = tpl::System.AggregateException;
#else
    using System.Threading;
    using System.Threading.Tasks;
#endif

    [TestClass]
    public class TestDelayedTask_Delay
    {
        #region Delay 1

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan)"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestDelay1_Negative()
        {
            TimeSpan delay = TimeSpan.FromSeconds(-1);
            DelayedTask.Delay(delay);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan)"/>.
        /// </summary>
        [TestMethod]
        public void TestDelay1_Negative_Fractional()
        {
            TimeSpan delay = TimeSpan.FromTicks(-100);
            Task task = DelayedTask.Delay(delay);
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan)"/>.
        /// </summary>
        [TestMethod]
        public void TestDelay1_Zero()
        {
            Task task = DelayedTask.Delay(TimeSpan.Zero);
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan)"/>.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestDelay1_Timing()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(0.25);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.025);

            Stopwatch timer = Stopwatch.StartNew();
            DelayedTask.Delay(timeout).Wait();

            TimeSpan elapsed = timer.Elapsed;
            Assert.IsTrue(elapsed >= timeout - tolerance, "The Delay expired too soon ({0} sec < {1} sec).", elapsed.TotalSeconds, (timeout - tolerance).TotalSeconds);
            Assert.IsTrue(elapsed <= timeout + tolerance, "The Delay expired too late ({0} sec > {1} sec).", elapsed.TotalSeconds, (timeout + tolerance).TotalSeconds);
        }

        /// <summary>
        /// This test verifies that the behavior of <see cref="DelayedTask.Delay(TimeSpan)"/>
        /// is not affected by a forced garbage collection.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestDelay1_GCBehavior()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(0.40);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.025);

            Stopwatch timer = Stopwatch.StartNew();
            Task delayTask = DelayedTask.Delay(timeout);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            delayTask.Wait();

            TimeSpan elapsed = timer.Elapsed;
            Assert.IsTrue(elapsed >= timeout - tolerance, "The Delay expired too soon ({0} sec < {1} sec).", elapsed.TotalSeconds, (timeout - tolerance).TotalSeconds);
            Assert.IsTrue(elapsed <= timeout + tolerance, "The Delay expired too late ({0} sec > {1} sec).", elapsed.TotalSeconds, (timeout + tolerance).TotalSeconds);
        }

        #endregion

        #region Delay 2

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestDelay2_NoCancel_Negative()
        {
            TimeSpan delay = TimeSpan.FromSeconds(-1);
            DelayedTask.Delay(delay, CancellationToken.None);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        public void TestDelay2_NoCancel_Negative_Fractional()
        {
            TimeSpan delay = TimeSpan.FromTicks(-100);
            Task task = DelayedTask.Delay(delay, CancellationToken.None);
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        public void TestDelay2_NoCancel_Zero()
        {
            Task task = DelayedTask.Delay(TimeSpan.Zero, CancellationToken.None);
            Assert.IsTrue(task.IsCompleted);
            Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestDelay2_NoCancel_Timing()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(0.25);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.025);

            Stopwatch timer = Stopwatch.StartNew();
            DelayedTask.Delay(timeout, CancellationToken.None).Wait();

            TimeSpan elapsed = timer.Elapsed;
            Assert.IsTrue(elapsed >= timeout - tolerance, "The Delay expired too soon ({0} sec < {1} sec).", elapsed.TotalSeconds, (timeout - tolerance).TotalSeconds);
            Assert.IsTrue(elapsed <= timeout + tolerance, "The Delay expired too late ({0} sec > {1} sec).", elapsed.TotalSeconds, (timeout + tolerance).TotalSeconds);
        }

        /// <summary>
        /// This test verifies that the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>
        /// is not affected by a forced garbage collection.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestDelay2_NoCancel_GCBehavior()
        {
            TimeSpan timeout = TimeSpan.FromSeconds(0.40);
            TimeSpan tolerance = TimeSpan.FromSeconds(0.025);

            Stopwatch timer = Stopwatch.StartNew();
            Task delayTask = DelayedTask.Delay(timeout, CancellationToken.None);

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            delayTask.Wait();

            TimeSpan elapsed = timer.Elapsed;
            Assert.IsTrue(elapsed >= timeout - tolerance, "The Delay expired too soon ({0} sec < {1} sec).", elapsed.TotalSeconds, (timeout - tolerance).TotalSeconds);
            Assert.IsTrue(elapsed <= timeout + tolerance, "The Delay expired too late ({0} sec > {1} sec).", elapsed.TotalSeconds, (timeout + tolerance).TotalSeconds);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestDelay2_Uncancel_Negative()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                TimeSpan delay = TimeSpan.FromSeconds(-1);
                DelayedTask.Delay(delay, cts.Token);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        public void TestDelay2_Uncancel_Negative_Fractional()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                TimeSpan delay = TimeSpan.FromTicks(-100);
                Task task = DelayedTask.Delay(delay, cts.Token);
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        public void TestDelay2_Uncancel_Zero()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Task task = DelayedTask.Delay(TimeSpan.Zero, cts.Token);
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestDelay2_Uncancel_Timing()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                TimeSpan timeout = TimeSpan.FromSeconds(0.25);
                TimeSpan tolerance = TimeSpan.FromSeconds(0.025);

                Stopwatch timer = Stopwatch.StartNew();
                DelayedTask.Delay(timeout, cts.Token).Wait();

                TimeSpan elapsed = timer.Elapsed;
                Assert.IsTrue(elapsed >= timeout - tolerance, "The Delay expired too soon ({0} sec < {1} sec).", elapsed.TotalSeconds, (timeout - tolerance).TotalSeconds);
                Assert.IsTrue(elapsed <= timeout + tolerance, "The Delay expired too late ({0} sec > {1} sec).", elapsed.TotalSeconds, (timeout + tolerance).TotalSeconds);
            }
        }

        /// <summary>
        /// This test verifies that the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>
        /// is not affected by a forced garbage collection.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestDelay2_Uncancel_GCBehavior()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                TimeSpan timeout = TimeSpan.FromSeconds(0.40);
                TimeSpan tolerance = TimeSpan.FromSeconds(0.025);

                Stopwatch timer = Stopwatch.StartNew();
                Task delayTask = DelayedTask.Delay(timeout, cts.Token);

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

                delayTask.Wait();

                TimeSpan elapsed = timer.Elapsed;
                Assert.IsTrue(elapsed >= timeout - tolerance, "The Delay expired too soon ({0} sec < {1} sec).", elapsed.TotalSeconds, (timeout - tolerance).TotalSeconds);
                Assert.IsTrue(elapsed <= timeout + tolerance, "The Delay expired too late ({0} sec > {1} sec).", elapsed.TotalSeconds, (timeout + tolerance).TotalSeconds);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestDelay2_PreCancel_Negative()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                TimeSpan delay = TimeSpan.FromSeconds(-1);
                DelayedTask.Delay(delay, cts.Token);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        public void TestDelay2_PreCancel_Negative_Fractional()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                TimeSpan delay = TimeSpan.FromTicks(-100);
                Task task = DelayedTask.Delay(delay, cts.Token);
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        public void TestDelay2_PreCancel_Zero()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                Task task = DelayedTask.Delay(TimeSpan.Zero, cts.Token);
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestDelay2_PreCancel_Timing()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.Cancel();

                TimeSpan timeout = TimeSpan.FromSeconds(0.25);

                Task task = DelayedTask.Delay(timeout, cts.Token);
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.Canceled, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestDelay2_CancelAfter_Negative()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                TimeSpan cancelAfter = TimeSpan.FromSeconds(0.25);
                cts.CancelAfter(cancelAfter);

                TimeSpan delay = TimeSpan.FromSeconds(-1);
                DelayedTask.Delay(delay, cts.Token);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        public void TestDelay2_CancelAfter_Negative_Fractional()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                TimeSpan cancelAfter = TimeSpan.FromSeconds(0.25);
                cts.CancelAfter(cancelAfter);

                TimeSpan delay = TimeSpan.FromTicks(-100);
                Task task = DelayedTask.Delay(delay, cts.Token);
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        public void TestDelay2_CancelAfter_Zero()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                TimeSpan cancelAfter = TimeSpan.FromSeconds(0.25);
                cts.CancelAfter(cancelAfter);

                Task task = DelayedTask.Delay(TimeSpan.Zero, cts.Token);
                Assert.IsTrue(task.IsCompleted);
                Assert.AreEqual(TaskStatus.RanToCompletion, task.Status);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestDelay2_CancelAfter_Timing()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Stopwatch timer = Stopwatch.StartNew();

                TimeSpan cancelAfter = TimeSpan.FromSeconds(0.25);
                TimeSpan tolerance = TimeSpan.FromSeconds(0.025);
                cts.CancelAfter(cancelAfter);

                TimeSpan timeout = TimeSpan.FromSeconds(1);
                Task delayTask = DelayedTask.Delay(timeout, cts.Token);

                try
                {
                    delayTask.Wait();
                    Assert.Fail("Expected an exception.");
                }
                catch (AggregateException ex)
                {
                    Assert.AreEqual(TaskStatus.Canceled, delayTask.Status);
                    Assert.AreEqual(1, ex.InnerExceptions.Count);
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                }

                TimeSpan elapsed = timer.Elapsed;
                Assert.IsTrue(elapsed >= cancelAfter - tolerance, "The Delay expired too soon ({0} sec < {1} sec).", elapsed.TotalSeconds, (cancelAfter - tolerance).TotalSeconds);
                Assert.IsTrue(elapsed <= cancelAfter + tolerance, "The Delay expired too late ({0} sec > {1} sec).", elapsed.TotalSeconds, (cancelAfter + tolerance).TotalSeconds);
            }
        }

        /// <summary>
        /// This test verifies that the behavior of <see cref="DelayedTask.Delay(TimeSpan, CancellationToken)"/>
        /// is not affected by a forced garbage collection.
        /// </summary>
        [TestMethod]
        [Timeout(2000)]
        public void TestDelay2_CancelAfter_GCBehavior()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Stopwatch timer = Stopwatch.StartNew();

                TimeSpan cancelAfter = TimeSpan.FromSeconds(0.25);
                TimeSpan tolerance = TimeSpan.FromSeconds(0.025);
                cts.CancelAfter(cancelAfter);

                TimeSpan timeout = TimeSpan.FromSeconds(1);
                Task delayTask = DelayedTask.Delay(timeout, cts.Token);

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

                try
                {
                    delayTask.Wait();
                    Assert.Fail("Expected an exception.");
                }
                catch (AggregateException ex)
                {
                    Assert.AreEqual(TaskStatus.Canceled, delayTask.Status);
                    Assert.AreEqual(1, ex.InnerExceptions.Count);
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                }

                TimeSpan elapsed = timer.Elapsed;
                Assert.IsTrue(elapsed >= cancelAfter - tolerance, "The Delay expired too soon ({0} sec < {1} sec).", elapsed.TotalSeconds, (cancelAfter - tolerance).TotalSeconds);
                Assert.IsTrue(elapsed <= cancelAfter + tolerance, "The Delay expired too late ({0} sec > {1} sec).", elapsed.TotalSeconds, (cancelAfter + tolerance).TotalSeconds);
            }
        }

        #endregion
    }
}
