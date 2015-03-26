namespace UnitTest.RackspaceThreading
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public abstract class TaskTestingBase
    {
        /// <summary>
        /// During testing, timed events are assumed to be ordered if their expected times differ by more than
        /// <see cref="TimingGranularity"/>. Reducing this value increases confidence that events occur at the requested
        /// time, but may increase the number of false test failures.
        /// </summary>
        /// <remarks>
        /// <para>This value is greater than <see cref="TimeSpan.Zero"/>.</para>
        /// </remarks>
        public TimeSpan TimingGranularity
        {
            get
            {
                string granularity = Environment.GetEnvironmentVariable("TIMING_GRANULARITY_MS");
                int result;
                if (!int.TryParse(granularity, out result))
                    return TimeSpan.FromMilliseconds(10);

                return TimeSpan.FromMilliseconds(result);
            }
        }

        /// <summary>
        /// During testing, timed events should occur no more than <see cref="AdvanceTolerance"/> in advance of the
        /// requested time. Reducing this value increases confidence that events occur at the requested time, but may
        /// increase the number of false test failures.
        /// </summary>
        /// <remarks>
        /// <para>This value is greater than or equal to <see cref="TimeSpan.Zero"/>.</para>
        /// </remarks>
        public TimeSpan AdvanceTolerance
        {
            get
            {
                string tolerance = Environment.GetEnvironmentVariable("ADVANCE_TOLERANCE_MS");
                int result;
                if (!int.TryParse(tolerance, out result))
                    return TimeSpan.FromMilliseconds(25);

                return TimeSpan.FromMilliseconds(result);
            }
        }

        /// <summary>
        /// During testing, timed events should occur no more than <see cref="DelayTolerance"/> following the requested
        /// time. Reducing this value increases confidence that events occur at the requested time, but may increase the
        /// number of false test failures.
        /// </summary>
        /// <remarks>
        /// <para>This value is greater than or equal to <see cref="TimeSpan.Zero"/>.</para>
        /// </remarks>
        public TimeSpan DelayTolerance
        {
            get
            {
                string tolerance = Environment.GetEnvironmentVariable("DELAY_TOLERANCE_MS");
                int result;
                if (!int.TryParse(tolerance, out result))
                    return TimeSpan.FromMilliseconds(25);

                return TimeSpan.FromMilliseconds(result);
            }
        }

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
