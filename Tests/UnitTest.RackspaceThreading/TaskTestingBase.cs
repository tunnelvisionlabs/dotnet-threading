namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public abstract class TaskTestingBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            TaskScheduler.UnobservedTaskException += HandleUnobservedException;
        }

        private void HandleUnobservedException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            TaskScheduler.UnobservedTaskException -= HandleUnobservedException;
        }
    }
}
