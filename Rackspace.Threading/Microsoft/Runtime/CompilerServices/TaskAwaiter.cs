// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if NET40PLUS && !NET45PLUS

[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(Microsoft.Runtime.CompilerServices.TaskAwaiter))]

#else

// NOTE: The reason this type does exist in the BCL System.Threading.Tasks contract is because we need to be able to
// construct one of these in the AwaitExtensions class. The equivalent type in the current platform does not have an
// accessible constructor, hence the AwaitExtensions would fail when run on platforms where System.Threading.Tasks gets
// unified.
namespace Microsoft.Runtime.CompilerServices
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Provides an awaiter for awaiting a <see cref="Task"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public struct TaskAwaiter : ICriticalNotifyCompletion
    {
        /// <summary>The default value to use for continueOnCapturedContext.</summary>
        internal const bool ContinueOnCapturedContextDefault = true; // marshal by default

        /// <summary>Error message for GetAwaiter.</summary>
        private const string InvalidOperationExceptionTaskNotCompleted = "The task has not yet completed.";

        /// <summary>The task being awaited.</summary>
        private readonly Task _task;

        /// <summary>Initializes a new instance of the <see cref="TaskAwaiter"/> struct.</summary>
        /// <param name="task">The <see cref="Task"/> to be awaited.</param>
        internal TaskAwaiter(Task task)
        {
            Debug.Assert(task != null, "Assertion failed: task != null");
            _task = task;
        }

        /// <summary>Gets a value indicating whether the task being awaited is completed.</summary>
        /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
        /// <value>
        /// <see langword="true"/> if the task being awaited is completed; otherwise, <see langword="false"/>.
        /// </value>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        public bool IsCompleted
        {
            get
            {
                return _task.IsCompleted;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current thread is appropriate for inlining the await continuation.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the current thread is appropriate for inlining the await continuation; otherwise,
        /// <see langword="false"/>.
        /// </value>
        private static bool IsValidLocationForInlining
        {
            get
            {
                var currentCtx = SynchronizationContext.Current;
                if (currentCtx != null && currentCtx.GetType() != typeof(SynchronizationContext))
                    return false;
                else
                    return TaskScheduler.Current == TaskScheduler.Default;
            }
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public void OnCompleted(Action continuation)
        {
            OnCompletedInternal(_task, continuation, ContinueOnCapturedContextDefault);
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
#if !SILVERLIGHT
        //// [SecurityCritical]
#endif
        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompletedInternal(_task, continuation, ContinueOnCapturedContextDefault);
        }

        /// <summary>Ends the await on the completed <see cref="Task"/>.</summary>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <exception cref="InvalidOperationException">The task was not yet completed.</exception>
        /// <exception cref="TaskCanceledException">The task was canceled.</exception>
        /// <exception cref="Exception">The task completed in a Faulted state.</exception>
        public void GetResult()
        {
            ValidateEnd(_task);
        }

        /// <summary>
        /// Fast checks for the end of an await operation to determine whether more needs to be done prior to completing
        /// the await.
        /// </summary>
        /// <param name="task">The awaited task.</param>
        internal static void ValidateEnd(Task task)
        {
            if (task.Status != TaskStatus.RanToCompletion)
                HandleNonSuccess(task);
        }

        /// <summary>Handles validations on tasks that aren't successfully completed.</summary>
        /// <param name="task">The awaited task.</param>
        private static void HandleNonSuccess(Task task)
        {
            if (!task.IsCompleted)
            {
                try
                {
                    task.Wait();
                }
                catch
                {
                }
            }

            if (task.Status != TaskStatus.RanToCompletion)
            {
                ThrowForNonSuccess(task);
            }
        }

        /// <summary>
        /// Throws an exception to handle a task that completed in a state other than
        /// <see cref="TaskStatus.RanToCompletion"/>.
        /// </summary>
        /// <param name="task">The task.</param>
        private static void ThrowForNonSuccess(Task task)
        {
            Debug.Assert(task.Status != TaskStatus.RanToCompletion, "Assertion failed: task.Status != TaskStatus.RanToCompletion");

            // Handle whether the task has been canceled or faulted
            switch (task.Status)
            {
            // If the task completed in a canceled state, throw an OperationCanceledException. TaskCanceledException
            // derives from OCE, and by throwing it we automatically pick up the completed task's CancellationToken if
            // it has one, including that CT in the OCE.
            case TaskStatus.Canceled:
                throw new TaskCanceledException(task);

            // If the task faulted, throw its first exception, even if it contained more than one.
            case TaskStatus.Faulted:
                throw AsyncServices.PrepareExceptionForRethrow(task.Exception.InnerException);

            // This should not happen on valid usage.
            default:
                throw new InvalidOperationException(InvalidOperationExceptionTaskNotCompleted);
            }
        }

        /// <summary>Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.</summary>
        /// <param name="task">The awaited task.</param>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <param name="continueOnCapturedContext">Whether to capture and marshal back to the current context.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        internal static void OnCompletedInternal(Task task, Action continuation, bool continueOnCapturedContext)
        {
            if (continuation == null)
                throw new ArgumentNullException("continuation");

            SynchronizationContext sc = continueOnCapturedContext ? SynchronizationContext.Current : null;
            if (sc != null && sc.GetType() != typeof(SynchronizationContext))
            {
                // When the task completes, post to the synchronization context, or run it inline if we're already in
                // the right place
                task.ContinueWith(
                    delegate
                    {
                        try
                        {
                            sc.Post(state => ((Action)state)(), continuation);
                        }
                        catch (Exception exc)
                        {
                            AsyncServices.ThrowAsync(exc, null);
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
            else
            {
                var scheduler = continueOnCapturedContext ? TaskScheduler.Current : TaskScheduler.Default;
                if (task.IsCompleted)
                {
                    Task.Factory.StartNew(
                        s => ((Action)s)(), continuation, CancellationToken.None, TaskCreationOptions.None, scheduler);
                }
                else
                {
                    // NOTE: There is a known rare race here.  For performance reasons, we want this continuation to
                    // execute synchronously when the task completes, but if the task is already completed by the time
                    // we call ContinueWith, we don't want it executing synchronously as part of the ContinueWith call.
                    // If the race occurs, and if the unbelievable happens and it occurs frequently enough to
                    // stack dive, ContinueWith's support for depth checking helps to mitigate this.
                    if (scheduler != TaskScheduler.Default)
                    {
                        // When the task completes, run the continuation in a callback using the correct task scheduler.
                        task.ContinueWith(
                            _ => RunNoException(continuation),
                            CancellationToken.None,
                            TaskContinuationOptions.ExecuteSynchronously,
                            scheduler);
                    }
                    else
                    {
                        // When the task completes, run the continuation in a callback using the correct task scheduler.
                        task.ContinueWith(
                            delegate
                            {
                                if (IsValidLocationForInlining)
                                {
                                    RunNoException(continuation);
                                }
                                else
                                {
                                    Task.Factory.StartNew(
                                        s => RunNoException((Action)s),
                                        continuation,
                                        CancellationToken.None,
                                        TaskCreationOptions.None,
                                        TaskScheduler.Default);
                                }
                            },
                            CancellationToken.None,
                            TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Default);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the delegate in a try/catch that will propagate the exception asynchronously on the thread pool.
        /// </summary>
        /// <param name="continuation">The <see cref="Action"/> to execute.</param>
        private static void RunNoException(Action continuation)
        {
            try
            {
                continuation();
            }
            catch (Exception exc)
            {
                AsyncServices.ThrowAsync(exc, null);
            }
        }
    }
}

#endif
