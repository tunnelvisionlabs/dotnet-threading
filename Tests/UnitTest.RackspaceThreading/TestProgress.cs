namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

    [TestClass]
    public class TestProgress
    {
        [TestMethod]
        [Timeout(2000)]
        public void TestCreateProgressT_Event()
        {
            int progressValue = 0;
            ManualResetEventSlim mre = new ManualResetEventSlim();

            var progressObject = new Rackspace.Threading.Progress<int>();
            progressObject.ProgressChanged +=
                (sender, e) =>
                {
                    progressValue = e.Progress;
                    mre.Set();
                };

            IProgress<int> progress = progressObject;
            progress.Report(3);
            mre.Wait();

            Assert.AreEqual(3, progressValue);
        }

        [TestMethod]
        [Timeout(2000)]
        public void TestCreateProgressT_Handler()
        {
            int progressValue = 0;
            ManualResetEventSlim mre = new ManualResetEventSlim();

            var progressObject = new Rackspace.Threading.Progress<int>(
                value =>
                {
                    progressValue = value;
                    mre.Set();
                });

            IProgress<int> progress = progressObject;
            progress.Report(3);
            mre.Wait();

            Assert.AreEqual(3, progressValue);
        }

        [TestMethod]
        [Timeout(2000)]
        public void TestCreateProgressT_HandlerAndEvent()
        {
            int handlerProgressValue = 0;
            int eventProgressValue = 0;
            ManualResetEventSlim handlerMre = new ManualResetEventSlim();
            ManualResetEventSlim eventMre = new ManualResetEventSlim();

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
            handlerMre.Wait();
            eventMre.Wait();

            Assert.AreEqual(3, handlerProgressValue);
            Assert.AreEqual(3, eventProgressValue);
        }
    }
}
