// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;
#if NET45PLUS
    using NullExceptionType = System.NullReferenceException;
#else
    using NullExceptionType = System.ArgumentNullException;
#endif

    [TestClass]
    public class TestAsyncAwait : TaskTestingBase
    {
        [TestMethod]
        [TestCategory("Async")]
        public void TestUnwrapException()
        {
            TestUnwrapExceptionAsync().Wait();
        }

        private async Task TestUnwrapExceptionAsync()
        {
            try
            {
                await CompletedTask.Default.Select(
                    _ =>
                    {
                        throw new ArgumentNullException();
                    });
                Assert.Fail("Expected an exception");
            }
            catch (ArgumentNullException)
            {
            }
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestAwaitNullTask()
        {
            TestAwaitNullTaskAsync().Wait();
        }

        private async Task TestAwaitNullTaskAsync()
        {
            try
            {
#pragma warning disable CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
                await default(Task);
#pragma warning restore CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
                Assert.Fail("Expected an exception");
            }
            catch (NullExceptionType)
            {
            }
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestAwaitNullTaskT()
        {
            TestAwaitNullTaskTAsync().Wait();
        }

        private async Task TestAwaitNullTaskTAsync()
        {
            try
            {
#pragma warning disable CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
                await default(Task<int>);
#pragma warning restore CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
                Assert.Fail("Expected an exception");
            }
            catch (NullExceptionType)
            {
            }
        }

        [TestMethod]
        [TestCategory("Async")]
        [ExpectedException(typeof(NullExceptionType))]
        public void TestConfigureAwaitNullTask()
        {
#pragma warning disable CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
            default(Task).ConfigureAwait(true);
#pragma warning restore CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
        }

        [TestMethod]
        [TestCategory("Async")]
        [ExpectedException(typeof(NullExceptionType))]
        public void TestConfigureAwaitNullTaskT()
        {
#pragma warning disable CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
            default(Task<int>).ConfigureAwait(true);
#pragma warning restore CS1720 // Expression will always cause a System.NullReferenceException because the type's default value is null
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestConfigureAwaitTrue()
        {
            TestConfigureAwaitAsync(true).Wait();
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestConfigureAwaitFalse()
        {
            TestConfigureAwaitAsync(false).Wait();
        }

        private async Task TestConfigureAwaitAsync(bool continueOnCapturedContext)
        {
            await DelayedTask.Delay(TimingGranularity).ConfigureAwait(continueOnCapturedContext);
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestConfigureAwaitTrueTaskT()
        {
            TestConfigureAwaitTaskTAsync(true).Wait();
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestConfigureAwaitFalseTaskT()
        {
            TestConfigureAwaitTaskTAsync(false).Wait();
        }

        private async Task TestConfigureAwaitTaskTAsync(bool continueOnCapturedContext)
        {
            await DelayedTask.Delay(TimingGranularity).Select(_ => 0).ConfigureAwait(continueOnCapturedContext);
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestYield()
        {
            TestYieldAsync().Wait();
        }

        private async Task TestYieldAsync()
        {
            await DelayedTask.Yield();
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestReturnTrue()
        {
            Assert.IsTrue(TestReturnBooleanAsync(true).Result);
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestReturnFalse()
        {
            Assert.IsFalse(TestReturnBooleanAsync(false).Result);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task<bool> TestReturnBooleanAsync(bool value)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return value;
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestReturnZero()
        {
            Assert.AreEqual(0, TestReturnZeroAsync().Result);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        private async Task<int> TestReturnZeroAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return 0;
        }

        [TestMethod]
        [TestCategory("Async")]
        public void TestReturnZeroAfterDelay()
        {
            Assert.AreEqual(0, TestReturnZeroAfterDelayAsync().Result);
        }

        private async Task<int> TestReturnZeroAfterDelayAsync()
        {
            await DelayedTask.Delay(TimingGranularity);
            return 0;
        }
    }
}
