// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

    [TestClass]
    public class TestProgress : TaskTestingBase
    {
        [TestMethod]
        [Timeout(2000)]
        public void TestCreateProgressT_Event()
        {
            int progressValue = 0;
            ManualResetEvent mre = new ManualResetEvent(false);

            var progressObject = new Rackspace.Threading.Progress<int>();
            progressObject.ProgressChanged +=
                (sender, e) =>
                {
                    progressValue = e.Progress;
                    mre.Set();
                };

            IProgress<int> progress = progressObject;
            progress.Report(3);
            mre.WaitOne();

            Assert.AreEqual(3, progressValue);
        }

        [TestMethod]
        [Timeout(2000)]
        public void TestCreateProgressT_Handler()
        {
            int progressValue = 0;
            ManualResetEvent mre = new ManualResetEvent(false);

            var progressObject = new Rackspace.Threading.Progress<int>(
                value =>
                {
                    progressValue = value;
                    mre.Set();
                });

            IProgress<int> progress = progressObject;
            progress.Report(3);
            mre.WaitOne();

            Assert.AreEqual(3, progressValue);
        }

        [TestMethod]
        [Timeout(2000)]
        public void TestCreateProgressT_HandlerAndEvent()
        {
            int handlerProgressValue = 0;
            int eventProgressValue = 0;
            ManualResetEvent handlerMre = new ManualResetEvent(false);
            ManualResetEvent eventMre = new ManualResetEvent(false);

            var progressObject = new Rackspace.Threading.Progress<int>(
                value =>
                {
                    handlerProgressValue = value;
                    handlerMre.Set();
                });
            progressObject.ProgressChanged +=
                (sender, e) =>
                {
                    eventProgressValue = e.Progress;
                    eventMre.Set();
                };

            IProgress<int> progress = progressObject;
            progress.Report(3);
            handlerMre.WaitOne();
            eventMre.WaitOne();

            Assert.AreEqual(3, handlerProgressValue);
            Assert.AreEqual(3, eventProgressValue);
        }
    }
}
