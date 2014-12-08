// This is intentionally placed in a different scope to ensure Progress<T> resolves to Rackspace.Threading.Progress<T>
using System;

namespace UnitTest.RackspaceThreading
{
#if NET40PLUS
    using IProgressInt = System.IProgress<int>;
#else
    extern alias rax;
    using IProgressInt = rax::System.IProgress<int>;
#endif

    using System.Threading;
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

            Progress<int> progressObject = new Progress<int>();
            progressObject.ProgressChanged +=
                (sender, e) =>
                {
                    progressValue = e.Progress;
                    mre.Set();
                };

            IProgressInt progress = progressObject;
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

            Progress<int> progressObject = new Progress<int>(
                value =>
                {
                    progressValue = value;
                    mre.Set();
                });

            IProgressInt progress = progressObject;
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

            Progress<int> progressObject = new Progress<int>(
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

            IProgressInt progress = progressObject;
            progress.Report(3);
            handlerMre.Wait();
            eventMre.Wait();

            Assert.AreEqual(3, handlerProgressValue);
            Assert.AreEqual(3, eventProgressValue);
        }
    }
}
