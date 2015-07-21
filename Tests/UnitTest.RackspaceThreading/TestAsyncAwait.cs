// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

    [TestClass]
    public class TestAsyncAwait
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
    }
}
