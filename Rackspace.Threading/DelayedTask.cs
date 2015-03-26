namespace Rackspace.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// <para>After the specified time delay, the task is completed in <see cref="TaskStatus.RanToCompletion"/>
        /// state.</para>
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
        /// <para>If the cancellation token is signaled before the specified time delay, then the task
        /// is completed in <see cref="TaskStatus.Canceled"/> state. Otherwise, the task is
        /// completed in <see cref="TaskStatus.RanToCompletion"/> state once the specified time
        /// delay has expired.</para>
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

        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <remarks>
        /// <para>If any of the supplied tasks completes in a faulted state, the returned task will also
        /// complete in a <see cref="TaskStatus.Faulted"/> state, where its exceptions will contain
        /// the aggregation of the set of unwrapped exceptions from each of the supplied tasks.</para>
        ///
        /// <para>If none of the supplied tasks faulted but at least one of them was canceled, the
        /// returned task will end in the <see cref="TaskStatus.Canceled"/> state.</para>
        ///
        /// <para>If none of the tasks faulted and none of the tasks were canceled, the resulting
        /// task will end in the <see cref="TaskStatus.RanToCompletion"/> state.</para>
        ///
        /// <para>If the supplied array/enumerable contains no tasks, the returned task will
        /// immediately transition to a <see cref="TaskStatus.RanToCompletion"/> state before it's
        /// returned to the caller.</para>
        /// </remarks>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="tasks"/> contains any <see langword="null"/> values.</exception>
        public static Task WhenAll(params Task[] tasks)
        {
            return WhenAll(tasks.AsEnumerable());
        }

        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <remarks>
        /// <para>If any of the supplied tasks completes in a faulted state, the returned task will also
        /// complete in a <see cref="TaskStatus.Faulted"/> state, where its exceptions will contain
        /// the aggregation of the set of unwrapped exceptions from each of the supplied tasks.</para>
        ///
        /// <para>If none of the supplied tasks faulted but at least one of them was canceled, the
        /// returned task will end in the <see cref="TaskStatus.Canceled"/> state.</para>
        ///
        /// <para>If none of the tasks faulted and none of the tasks were canceled, the resulting
        /// task will end in the <see cref="TaskStatus.RanToCompletion"/> state.</para>
        ///
        /// <para>If the supplied array/enumerable contains no tasks, the returned task will
        /// immediately transition to a <see cref="TaskStatus.RanToCompletion"/> state before it's
        /// returned to the caller.</para>
        /// </remarks>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="tasks"/> contains any <see langword="null"/> values.</exception>
        public static Task WhenAll(IEnumerable<Task> tasks)
        {
#if NET45PLUS
            return Task.WhenAll(tasks);
#else
            if (tasks == null)
                throw new ArgumentNullException("tasks");

            Task[] tasksArray = tasks.ToArray();
            if (tasksArray.Length == 0)
                return CompletedTask.Default;

            if (tasksArray.Contains(null))
                throw new ArgumentException("tasks cannot contain any null values", "tasks");

            TaskCompletionSource<VoidResult> taskCompletionSource = new TaskCompletionSource<VoidResult>();
            Action<Task[]> continuationAction =
                completedTasks =>
                {
                    List<Exception> exceptions = new List<Exception>();
                    bool canceled = false;

                    foreach (var completedTask in completedTasks)
                    {
                        switch (completedTask.Status)
                        {
                        case TaskStatus.RanToCompletion:
                            break;

                        case TaskStatus.Canceled:
                            canceled = true;
                            break;

                        case TaskStatus.Faulted:
                            exceptions.AddRange(completedTask.Exception.InnerExceptions);
                            break;

                        default:
                            throw new InvalidOperationException("Unreachable");
                        }
                    }

                    if (exceptions.Count > 0)
                        taskCompletionSource.SetException(exceptions);
                    else if (canceled)
                        taskCompletionSource.SetCanceled();
                    else
                        taskCompletionSource.SetResult(default(VoidResult));
                };
            Task.Factory.ContinueWhenAll(tasksArray, continuationAction);
            return taskCompletionSource.Task;
#endif
        }

        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <remarks>
        /// <para>If any of the supplied tasks completes in a faulted state, the returned task will also
        /// complete in a <see cref="TaskStatus.Faulted"/> state, where its exceptions will contain
        /// the aggregation of the set of unwrapped exceptions from each of the supplied tasks.</para>
        ///
        /// <para>If none of the supplied tasks faulted but at least one of them was canceled, the
        /// returned task will end in the <see cref="TaskStatus.Canceled"/> state.</para>
        ///
        /// <para>If none of the tasks faulted and none of the tasks were canceled, the resulting
        /// task will end in the <see cref="TaskStatus.RanToCompletion"/> state. The
        /// <see cref="Task{TResult}.Result"/> of the returned task will be set to an array
        /// containing all of the results of the supplied tasks in the same order as they were
        /// provided (e.g. if the input tasks array contained t1, t2, t3, the output task's
        /// <see cref="Task{TResult}.Result"/> will return an <typeparamref name="TResult"/>[]
        /// where <c>arr[0] == t1.Result, arr[1] == t2.Result, and arr[2] == t3.Result</c>).</para>
        ///
        /// <para>If the supplied array/enumerable contains no tasks, the returned task will
        /// immediately transition to a <see cref="TaskStatus.RanToCompletion"/> state before it's
        /// returned to the caller. The returned <typeparamref name="TResult"/>[] will be an array
        /// of 0 elements.</para>
        /// </remarks>
        /// <typeparam name="TResult">The type of the completed task.</typeparam>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="tasks"/> contains any <see langword="null"/> values.</exception>
        public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks)
        {
            return WhenAll(tasks.AsEnumerable());
        }

        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <remarks>
        /// <para>If any of the supplied tasks completes in a faulted state, the returned task will also
        /// complete in a <see cref="TaskStatus.Faulted"/> state, where its exceptions will contain
        /// the aggregation of the set of unwrapped exceptions from each of the supplied tasks.</para>
        ///
        /// <para>If none of the supplied tasks faulted but at least one of them was canceled, the
        /// returned task will end in the <see cref="TaskStatus.Canceled"/> state.</para>
        ///
        /// <para>If none of the tasks faulted and none of the tasks were canceled, the resulting
        /// task will end in the <see cref="TaskStatus.RanToCompletion"/> state. The
        /// <see cref="Task{TResult}.Result"/> of the returned task will be set to an array
        /// containing all of the results of the supplied tasks in the same order as they were
        /// provided (e.g. if the input tasks array contained t1, t2, t3, the output task's
        /// <see cref="Task{TResult}.Result"/> will return an <typeparamref name="TResult"/>[]
        /// where <c>arr[0] == t1.Result, arr[1] == t2.Result, and arr[2] == t3.Result</c>).</para>
        ///
        /// <para>If the supplied array/enumerable contains no tasks, the returned task will
        /// immediately transition to a <see cref="TaskStatus.RanToCompletion"/> state before it's
        /// returned to the caller. The returned <typeparamref name="TResult"/>[] will be an array
        /// of 0 elements.</para>
        /// </remarks>
        /// <typeparam name="TResult">The type of the completed task.</typeparam>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of all of the supplied tasks.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="tasks"/> contains any <see langword="null"/> values.</exception>
        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks)
        {
#if NET45PLUS
            return Task.WhenAll(tasks);
#else
            if (tasks == null)
                throw new ArgumentNullException("tasks");

            Task<TResult>[] tasksArray = tasks.ToArray();
            if (tasksArray.Length == 0)
                return CompletedTask.FromResult(new TResult[0]);

            if (tasksArray.Contains(null))
                throw new ArgumentException("tasks cannot contain any null values", "tasks");

            TaskCompletionSource<TResult[]> taskCompletionSource = new TaskCompletionSource<TResult[]>();
            Action<Task<TResult>[]> continuationAction =
                completedTasks =>
                {
                    List<TResult> results = new List<TResult>();
                    List<Exception> exceptions = new List<Exception>();
                    bool canceled = false;

                    foreach (var completedTask in completedTasks)
                    {
                        switch (completedTask.Status)
                        {
                        case TaskStatus.RanToCompletion:
                            results.Add(completedTask.Result);
                            break;

                        case TaskStatus.Canceled:
                            canceled = true;
                            break;

                        case TaskStatus.Faulted:
                            exceptions.AddRange(completedTask.Exception.InnerExceptions);
                            break;

                        default:
                            throw new InvalidOperationException("Unreachable");
                        }
                    }

                    if (exceptions.Count > 0)
                        taskCompletionSource.SetException(exceptions);
                    else if (canceled)
                        taskCompletionSource.SetCanceled();
                    else
                        taskCompletionSource.SetResult(results.ToArray());
                };
            Task.Factory.ContinueWhenAll(tasksArray, continuationAction);
            return taskCompletionSource.Task;
#endif
        }

        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// </summary>
        /// <remarks>
        /// <para>The returned task will complete when any of the supplied tasks has completed. The returned
        /// task will always end in the <see cref="TaskStatus.RanToCompletion"/> state with its
        /// <see cref="Task{TResult}.Result"/> set to the first task to complete. This is true even if
        /// the first task to complete ended in the <see cref="TaskStatus.Canceled"/> or
        /// <see cref="TaskStatus.Faulted"/> state.</para>
        /// </remarks>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of one of the supplied tasks. The return task's <see cref="Task{TResult}.Result"/> is the task that completed.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <para>If <paramref name="tasks"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="tasks"/> was empty.</para>
        /// </exception>
        public static Task<Task> WhenAny(params Task[] tasks)
        {
            return WhenAny(tasks.AsEnumerable());
        }

        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// </summary>
        /// <remarks>
        /// <para>The returned task will complete when any of the supplied tasks has completed. The returned
        /// task will always end in the <see cref="TaskStatus.RanToCompletion"/> state with its
        /// <see cref="Task{TResult}.Result"/> set to the first task to complete. This is true even if
        /// the first task to complete ended in the <see cref="TaskStatus.Canceled"/> or
        /// <see cref="TaskStatus.Faulted"/> state.</para>
        /// </remarks>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of one of the supplied tasks. The return task's <see cref="Task{TResult}.Result"/> is the task that completed.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <para>If <paramref name="tasks"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="tasks"/> was empty.</para>
        /// </exception>
        public static Task<Task> WhenAny(IEnumerable<Task> tasks)
        {
#if NET45PLUS
            return Task.WhenAny(tasks);
#else
            if (tasks == null)
                throw new ArgumentNullException("tasks");

            Task[] tasksArray = tasks.ToArray();
            if (tasksArray.Length == 0)
                throw new ArgumentException("tasks cannot be empty", "tasks");

            if (tasksArray.Contains(null))
                throw new ArgumentException("tasks cannot contain any null values", "tasks");

            TaskCompletionSource<Task> taskCompletionSource = new TaskCompletionSource<Task>();
            Action<Task> continuationAction =
                completedTask =>
                {
                    switch (completedTask.Status)
                    {
                    case TaskStatus.RanToCompletion:
                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        taskCompletionSource.SetResult(completedTask);
                        break;

                    default:
                        throw new InvalidOperationException("Unreachable");
                    }
                };
            Task.Factory.ContinueWhenAny(tasksArray, continuationAction);
            return taskCompletionSource.Task;
#endif
        }

        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// </summary>
        /// <remarks>
        /// <para>The returned task will complete when any of the supplied tasks has completed. The returned
        /// task will always end in the <see cref="TaskStatus.RanToCompletion"/> state with its
        /// <see cref="Task{TResult}.Result"/> set to the first task to complete. This is true even if
        /// the first task to complete ended in the <see cref="TaskStatus.Canceled"/> or
        /// <see cref="TaskStatus.Faulted"/> state.</para>
        /// </remarks>
        /// <typeparam name="TResult">The type of the completed task.</typeparam>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of one of the supplied tasks. The return task's <see cref="Task{TResult}.Result"/> is the task that completed.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <para>If <paramref name="tasks"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="tasks"/> was empty.</para>
        /// </exception>
        public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks)
        {
            return WhenAny(tasks.AsEnumerable());
        }

        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// </summary>
        /// <remarks>
        /// <para>The returned task will complete when any of the supplied tasks has completed. The returned
        /// task will always end in the <see cref="TaskStatus.RanToCompletion"/> state with its
        /// <see cref="Task{TResult}.Result"/> set to the first task to complete. This is true even if
        /// the first task to complete ended in the <see cref="TaskStatus.Canceled"/> or
        /// <see cref="TaskStatus.Faulted"/> state.</para>
        /// </remarks>
        /// <typeparam name="TResult">The type of the completed task.</typeparam>
        /// <param name="tasks">The tasks to wait on for completion.</param>
        /// <returns>A task that represents the completion of one of the supplied tasks. The return task's <see cref="Task{TResult}.Result"/> is the task that completed.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="tasks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// <para>If <paramref name="tasks"/> contains any <see langword="null"/> values.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="tasks"/> was empty.</para>
        /// </exception>
        public static Task<Task<TResult>> WhenAny<TResult>(IEnumerable<Task<TResult>> tasks)
        {
#if NET45PLUS
            return Task.WhenAny(tasks);
#else
            if (tasks == null)
                throw new ArgumentNullException("tasks");

            Task<TResult>[] tasksArray = tasks.ToArray();
            if (tasksArray.Length == 0)
                throw new ArgumentException("tasks cannot be empty", "tasks");

            if (tasksArray.Contains(null))
                throw new ArgumentException("tasks cannot contain any null values", "tasks");

            TaskCompletionSource<Task<TResult>> taskCompletionSource = new TaskCompletionSource<Task<TResult>>();
            Action<Task<TResult>> continuationAction =
                completedTask =>
                {
                    switch (completedTask.Status)
                    {
                    case TaskStatus.RanToCompletion:
                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                        taskCompletionSource.SetResult(completedTask);
                        break;

                    default:
                        throw new InvalidOperationException("Unreachable");
                    }
                };
            Task.Factory.ContinueWhenAny(tasksArray, continuationAction);
            return taskCompletionSource.Task;
#endif
        }
    }
}
