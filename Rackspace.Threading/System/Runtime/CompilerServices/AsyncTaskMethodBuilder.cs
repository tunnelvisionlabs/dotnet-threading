// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(AsyncTaskMethodBuilder))]

#else

namespace System.Runtime.CompilerServices
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Rackspace.Threading;

    /// <summary>
    /// Provides a builder for asynchronous methods that return <see cref="Threading.Tasks.Task"/>. This type is
    /// intended for compiler use only.
    /// </summary>
    /// <remarks>
    /// <see cref="AsyncTaskMethodBuilder"/> is a value type, and thus it is copied by value. Prior to being copied,
    /// one of its <see cref="Task"/>, <see cref="SetResult"/>, or <see cref="SetException"/> members must be accessed,
    /// or else the copies may end up building distinct <see cref="Threading.Tasks.Task"/> instances.
    /// </remarks>
    public struct AsyncTaskMethodBuilder : IAsyncMethodBuilder
    {
        /// <summary>A cached <see cref="VoidResult"/> task used for builders that complete synchronously.</summary>
        private static readonly TaskCompletionSource<VoidResult> _cachedCompleted = AsyncTaskMethodBuilder<VoidResult>._defaultResultTask;

#pragma warning disable 0649
        /// <summary>The generic builder object to which this non-generic instance delegates.</summary>
        private AsyncTaskMethodBuilder<VoidResult> _builder; // mutable struct: must not be readonly
#pragma warning restore 0649

        /// <summary>Gets the <see cref="Threading.Tasks.Task"/> for this builder.</summary>
        /// <returns>The <see cref="Threading.Tasks.Task"/> representing the builder's asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
        /// <value>The <see cref="Threading.Tasks.Task"/> for this builder.</value>
        public Task Task
        {
            get
            {
                return _builder.Task;
            }
        }

        /// <summary>
        /// Gets an object that may be used to uniquely identify this builder to the debugger.
        /// </summary>
        /// <remarks>
        /// This property lazily instantiates the ID in a non-thread-safe manner. It must only be used by the debugger,
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
        public static AsyncTaskMethodBuilder Create()
        {
            // Note: If ATMB<T>.Create is modified to do any initialization, this
            //       method needs to be updated to do m_builder = ATMB<T>.Create().
            return default(AsyncTaskMethodBuilder);
        }

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        [DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            // argument validation handled by AsyncMethodBuilderCore
            _builder.Start(ref stateMachine);
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
            _builder.SetStateMachine(stateMachine);
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
            _builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            _builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
        }

        /// <summary>
        /// Completes the <see cref="Threading.Tasks.Task"/> in the <see cref="TaskStatus.RanToCompletion"/> state.
        /// </summary>
        /// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
        /// <exception cref="InvalidOperationException">The task has already completed.</exception>
        public void SetResult()
        {
            // Accessing AsyncTaskMethodBuilder._cachedCompleted is faster than accessing
            // AsyncTaskMethodBuilder<T>._defaultResultTask.
            _builder.SetResult(_cachedCompleted);
        }

        /// <summary>
        /// Completes the <see cref="Threading.Tasks.Task"/> in the <see cref="TaskStatus.Faulted"/> state with the
        /// specified exception.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to use to fault the task.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="exception"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
        /// <exception cref="InvalidOperationException">The task has already completed.</exception>
        public void SetException(Exception exception)
        {
            _builder.SetException(exception);
        }

        /// <summary>
        /// Called by the debugger to request notification when the first wait operation (await, Wait, Result, etc.) on
        /// this builder's task completes.
        /// </summary>
        /// <param name="enabled">
        /// <see langword="true"/> to enable notification; <see langword="false"/> to disable a previously set
        /// notification.
        /// </param>
        internal void SetNotificationForWaitCompletion(bool enabled)
        {
            _builder.SetNotificationForWaitCompletion(enabled);
        }
    }
}

#endif
