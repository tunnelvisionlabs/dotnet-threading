// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

﻿#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(AsyncTaskMethodBuilder<>))]

#else

namespace System.Runtime.CompilerServices
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a builder for asynchronous methods that return <see cref="Task{TResult}"/>. This type is intended for
    /// compiler use only.
    /// </summary>
    /// <remarks>
    /// <see cref="AsyncTaskMethodBuilder{TResult}"/> is a value type, and thus it is copied by value. Prior to being
    /// copied, one of its <see cref="Task"/>, <see cref="SetResult(TResult)"/>, or
    /// <see cref="SetException(Exception)"/> members must be accessed, or else the copies may end up building distinct
    /// <see cref="Threading.Tasks.Task"/> instances.
    /// </remarks>
    /// <typeparam name="TResult">The return type of the asynchronous method.</typeparam>
    /// <preliminary/>
    public struct AsyncTaskMethodBuilder<TResult> : IAsyncMethodBuilder
    {
        /// <summary>A cached task for default(<typeparamref name="TResult"/>).</summary>
        internal static readonly TaskCompletionSource<TResult> _defaultResultTask = AsyncMethodTaskCache<TResult>.CreateCompleted(default(TResult));

        // WARNING: For performance reasons, the m_task field is lazily initialized.
        //          For correct results, the struct AsyncTaskMethodBuilder<TResult> must
        //          always be used from the same location/copy, at least until m_task is
        //          initialized.  If that guarantee is broken, the field could end up being
        //          initialized on the wrong copy.

        /// <summary>State related to the <see cref="IAsyncStateMachine"/>.</summary>
        private AsyncMethodBuilderCore _coreState; // mutable struct: must not be readonly

        /// <summary>The lazily-initialized task.</summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1308:Variable names must not be prefixed", Justification = "Must be named m_task for debugger step-over to work correctly.")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SX1309:Field names must begin with underscore", Justification = "Must be named m_task for debugger step-over to work correctly.")]
        private Task<TResult> m_task; // lazily-initialized: must not be readonly

        /// <summary>The lazily-initialized task completion source.</summary>
        private TaskCompletionSource<TResult> _taskCompletionSource; // lazily-initialized: must not be readonly

        static AsyncTaskMethodBuilder()
        {
            try
            {
                // Temporary support for disabling crashing if tasks go unobserved.
                AsyncVoidMethodBuilder.PreventUnobservedTaskExceptions();
            }
            catch
            {
            }
        }

        /// <summary>Gets the lazily-initialized <see cref="TaskCompletionSource{TResult}"/>.</summary>
        /// <value>
        /// The lazily-initialized <see cref="TaskCompletionSource{TResult}"/>.
        /// </value>
        internal TaskCompletionSource<TResult> CompletionSource
        {
            get
            {
                // Get and return the task. If there isn't one, first create one and store it.
                var tcs = _taskCompletionSource;
                if (tcs == null)
                {
                    Debug.Assert(m_task == null, "Task should be null if TCS is null");
                    _taskCompletionSource = tcs = new TaskCompletionSource<TResult>();
                    m_task = tcs.Task;
                }

                return tcs;
            }
        }

        /// <summary>Gets the <see cref="Task{TResult}"/> for this builder.</summary>
        /// <returns>The <see cref="Task{TResult}"/> representing the builder's asynchronous operation.</returns>
        /// <value>
        /// The <see cref="Task{TResult}"/> for this builder.
        /// </value>
        public Task<TResult> Task
        {
            get
            {
                var tcs = CompletionSource;
                Debug.Assert(tcs != null && m_task != null, "Task should have been initialized.");
                return tcs.Task;
            }
        }

        /// <summary>
        /// Gets an object that may be used to uniquely identify this builder to the debugger.
        /// </summary>
        /// <remarks>
        /// This property lazily instantiates the ID in a non-thread-safe manner.  It must only be used by the debugger,
        /// and only in a single-threaded manner when no other threads are in the middle of accessing this property or
        /// <see cref="Task"/>.
        /// </remarks>
        /// <value>
        /// An object that may be used to uniquely identify this builder to the debugger.
        /// </value>
        private object ObjectIdForDebugger
        {
            get
            {
                return Task;
            }
        }

        /// <summary>Initializes a new <see cref="AsyncTaskMethodBuilder"/>.</summary>
        /// <returns>The initialized <see cref="AsyncTaskMethodBuilder"/>.</returns>
        public static AsyncTaskMethodBuilder<TResult> Create()
        {
            // NOTE:  If this method is ever updated to perform more initialization,
            //        ATMB.Create must also be updated to call this Create method.
            return default(AsyncTaskMethodBuilder<TResult>);
        }

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        [DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            // argument validation handled by AsyncMethodBuilderCore
            _coreState.Start(ref stateMachine);
        }

        /// <summary>Associates the builder with the state machine it represents.</summary>
        /// <param name="stateMachine">The heap-allocated state machine object.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stateMachine"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The builder is incorrectly initialized.</exception>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // argument validation handled by AsyncMethodBuilderCore
            _coreState.SetStateMachine(stateMachine);
        }

        /// <summary>Perform any initialization necessary prior to lifting the builder to the heap.</summary>
        void IAsyncMethodBuilder.PreBoxInitialization()
        {
            // Force the Task to be initialized prior to the first suspending await so that the original stack-based
            // builder has a reference to the right Task.
            var ignored = Task;
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                var continuation = _coreState.GetCompletionAction(ref this, ref stateMachine);
                Debug.Assert(continuation != null, "GetCompletionAction should always return a valid action.");
                awaiter.OnCompleted(continuation);
            }
            catch (Exception e)
            {
                AsyncServices.ThrowAsync(e, targetContext: null);
            }
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
#if !SILVERLIGHT
        // [SecuritySafeCritical]
#endif
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                var continuation = _coreState.GetCompletionAction(ref this, ref stateMachine);
                Debug.Assert(continuation != null, "GetCompletionAction should always return a valid action.");
                awaiter.UnsafeOnCompleted(continuation);
            }
            catch (Exception e)
            {
                AsyncServices.ThrowAsync(e, targetContext: null);
            }
        }

        /// <summary>
        /// Completes the <see cref="Task{TResult}"/> in the <see cref="TaskStatus.RanToCompletion"/> state with the
        /// specified result.
        /// </summary>
        /// <param name="result">The result to use to complete the task.</param>
        /// <exception cref="InvalidOperationException">The task has already completed.</exception>
        public void SetResult(TResult result)
        {
            // Get the currently stored task, which will be non-null if get_Task has already been accessed. If there
            // isn't one, get a task and store it.
            var tcs = _taskCompletionSource;
            if (tcs == null)
            {
                Debug.Assert(m_task == null, "Task should be null if TCS is null");
                _taskCompletionSource = GetTaskForResult(result);
                Debug.Assert(_taskCompletionSource != null, "GetTaskForResult should never return null");
                m_task = _taskCompletionSource.Task;
            }
            else if (!tcs.TrySetResult(result))
            {
                // Slow path: complete the existing task.
                throw new InvalidOperationException("The Task was already completed.");
            }
        }

        /// <summary>
        /// Completes the builder by using either the supplied completed task, or by completing
        /// the builder's previously accessed task using default(<typeparamref name="TResult"/>).
        /// </summary>
        /// <param name="completedTask">
        /// A task already completed with the value default(<typeparamref name="TResult"/>).
        /// </param>
        /// <exception cref="System.InvalidOperationException">The task has already completed.</exception>
        internal void SetResult(TaskCompletionSource<TResult> completedTask)
        {
            Debug.Assert(completedTask != null, "Expected non-null task");
            Debug.Assert(completedTask.Task.Status == TaskStatus.RanToCompletion, "Expected a successfully completed task");

            // Get the currently stored task, which will be non-null if get_Task has already been accessed. If there
            // isn't one, store the supplied completed task.
            var tcs = _taskCompletionSource;
            if (tcs == null)
            {
                Debug.Assert(m_task == null, "Task should be null if TCS is null");
                _taskCompletionSource = completedTask;
                m_task = _taskCompletionSource.Task;
            }
            else
            {
                // Otherwise, complete the task that's there.
                SetResult(default(TResult));
            }
        }

        /// <summary>
        /// Completes the <see cref="Task{TResult}"/> in the <see cref="TaskStatus.Faulted"/> state with the specified
        /// exception.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to use to fault the task.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="exception"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The task has already completed.</exception>
        public void SetException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            // Get the task, forcing initialization if it hasn't already been initialized.
            var task = CompletionSource;

            // If the exception represents cancellation, cancel the task. Otherwise, fault the task.
            var oce = exception as OperationCanceledException;
            bool successfullySet = oce != null ?
                task.TrySetCanceled() :
                task.TrySetException(exception);

            // Unlike with TaskCompletionSource, we do not need to spin here until m_task is completed, since
            // AsyncTaskMethodBuilder.SetException should not be immediately followed by any code that depends on the
            // task having completely completed.  Moreover, with correct usage,  SetResult or SetException should only
            // be called once, so the Try* methods should always return true, so no spinning would be necessary anyway
            // (the spinning in TCS is only relevant if another thread won the race to complete the task).
            if (!successfullySet)
            {
                throw new InvalidOperationException("The Task was already completed.");
            }
        }

        /// <summary>
        /// Called by the debugger to request notification when the first wait operation (await, Wait, Result, etc.) on
        /// this builder's task completes.
        /// </summary>
        /// <param name="enabled">
        /// <see langword="true"/> to enable notification; <see langword="false"/> to disable a previously set
        /// notification.
        /// </param>
        /// <remarks>
        /// This should only be invoked from within an asynchronous method, and only by the debugger.
        /// </remarks>
        internal void SetNotificationForWaitCompletion(bool enabled)
        {
            // NOP in the compatibility lib
        }

        /// <summary>
        /// Gets a task for the specified result. This will either be a cached or new task, never
        /// <see langword="null"/>.
        /// </summary>
        /// <param name="result">The result for which we need a task.</param>
        /// <returns>The completed task containing the result.</returns>
        private TaskCompletionSource<TResult> GetTaskForResult(TResult result)
        {
            ////Contract.Ensures(
            ////   EqualityComparer<TResult>.Default.Equals(result, Contract.Result<Task<TResult>>().Result),
            ////    "The returned task's Result must return the same value as the specified result value.");
            var cache = AsyncMethodTaskCache<TResult>.Singleton;
            return cache != null ?
                cache.FromResult(result) :
                AsyncMethodTaskCache<TResult>.CreateCompleted(result);
        }
    }
}

#endif
