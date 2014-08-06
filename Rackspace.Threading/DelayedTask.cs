namespace Rackspace.Threading
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

#if PORTABLE && !NET45PLUS
    using System.Runtime.CompilerServices;
#endif

    /// <summary>
    /// This class provides methods for creating <see cref="Task"/> instances that represent delays
    /// of a fixed duration or operations to wait on one or more other tasks to complete.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public static class DelayedTask
    {
#if PORTABLE && !NET45PLUS
        /// <summary>
        /// This map ensures the <see cref="Timer"/> instances used for implementing
        /// <see cref="Delay(TimeSpan, CancellationToken)"/> in the portable-net40 library
        /// are not finalized prior to callback execution.
        /// </summary>
        private static readonly ConditionalWeakTable<Task, Timer> _delayTimers =
            new ConditionalWeakTable<Task, Timer>();
#endif

        /// <summary>
        /// Creates a task that will complete after a time delay.
        /// </summary>
        /// <remarks>
        /// After the specified time delay, the task is completed in <see cref="TaskStatus.RanToCompletion"/> state.
        /// <para>This method ignores any fractional milliseconds when evaluating <paramref name="delay"/>.</para>
        /// </remarks>
        /// <param name="delay">The time span to wait before completing the returned task</param>
        /// <returns>A task that represents the time delay</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="delay"/> represents a negative time interval.</exception>
        public static Task Delay(TimeSpan delay)
        {
#if NET45PLUS
            return Task.Delay(delay);
#else
            return Delay(delay, CancellationToken.None);
#endif
        }

        /// <summary>
        /// Creates a task that will complete after a time delay.
        /// </summary>
        /// <remarks>
        /// If the cancellation token is signaled before the specified time delay, then the task
        /// is completed in <see cref="TaskStatus.Canceled"/> state. Otherwise, the task is
        /// completed in <see cref="TaskStatus.RanToCompletion"/> state once the specified time
        /// delay has expired.
        /// <para>This method ignores any fractional milliseconds when evaluating <paramref name="delay"/>.</para>
        /// </remarks>
        /// <param name="delay">The time span to wait before completing the returned task</param>
        /// <param name="cancellationToken">The cancellation token that will be checked prior to completing the returned task</param>
        /// <returns>A task that represents the time delay</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="delay"/> represents a negative time interval.</exception>
        /// <exception cref="TaskCanceledException">If the task has been canceled.</exception>
        /// <exception cref="ObjectDisposedException">If the provided <paramref name="cancellationToken"/> has already been disposed.</exception>
        public static Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        {
#if NET45PLUS
            return Task.Delay(delay, cancellationToken);
#else
            long totalMilliseconds = (long)delay.TotalMilliseconds;
            if (totalMilliseconds < 0)
                throw new ArgumentOutOfRangeException("delay");

            if (cancellationToken.IsCancellationRequested)
                return CompletedTask.Canceled();

            if (totalMilliseconds == 0)
                return CompletedTask.Default;

            TaskCompletionSource<VoidResult> result = new TaskCompletionSource<VoidResult>();

#if !PORTABLE
            TaskCompletionSource<VoidResult> intermediateResult = new TaskCompletionSource<VoidResult>();

            RegisteredWaitHandle timerRegisteredWaitHandle = null;
            RegisteredWaitHandle cancellationRegisteredWaitHandle = null;

            WaitOrTimerCallback timedOutCallback =
                (object state, bool timedOut) =>
                {
                    if (timedOut)
                        intermediateResult.TrySetResult(default(VoidResult));
                };

            IAsyncResult asyncResult = intermediateResult.Task;
            timerRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, timedOutCallback, null, delay, true);

            if (cancellationToken.CanBeCanceled)
            {
                WaitOrTimerCallback cancelledCallback =
                    (object state, bool timedOut) =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                            intermediateResult.TrySetCanceled();
                    };

                cancellationRegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(cancellationToken.WaitHandle, cancelledCallback, null, Timeout.Infinite, true);
            }

            intermediateResult.Task
                .ContinueWith(
                    _ =>
                    {
                        if (cancellationRegisteredWaitHandle != null)
                            cancellationRegisteredWaitHandle.Unregister(null);

                        if (timerRegisteredWaitHandle != null)
                            timerRegisteredWaitHandle.Unregister(null);
                    }, TaskContinuationOptions.ExecuteSynchronously)
                .ContinueWith(
                    cleanupTask =>
                    {
                        switch (cleanupTask.Status)
                        {
                        case TaskStatus.RanToCompletion:
                            result.SetFromTask(intermediateResult.Task);
                            break;

                        case TaskStatus.Canceled:
                            result.SetCanceled();
                            break;

                        case TaskStatus.Faulted:
                            result.SetException(cleanupTask.Exception.InnerExceptions);
                            break;

                        default:
                            throw new InvalidOperationException("Unreachable");
                        }
                    });
#else
            // Since portable-net40 doesn't provide Task.Delay and also doesn't provide ThreadPool.RegisterWaitForSingleObject,
            // we need to implement this functionality using timers stored in a ConditionalWeakTable, which are associated with
            // the actual Task instance that gets returned by this method.

            CancellationTokenRegistration cancellationTokenRegistration = default(CancellationTokenRegistration);
            Timer timer = null;

            TimerCallback timerCallback =
                state =>
                {
                    result.TrySetResult(default(VoidResult));
                    timer.Dispose();
                    cancellationTokenRegistration.Dispose();
                };

            timer = new Timer(timerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _delayTimers.Add(result.Task, timer);
            timer.Change(delay, TimeSpan.FromMilliseconds(-1));


            if (cancellationToken.CanBeCanceled)
            {
                Action cancellationCallback =
                    () =>
                    {
                        result.TrySetCanceled();

                        if (timer != null)
                            timer.Dispose();

                        cancellationTokenRegistration.Dispose();
                    };

                cancellationTokenRegistration = cancellationToken.Register(cancellationCallback);
            }
#endif

            return result.Task;
#endif
        }
    }
}
