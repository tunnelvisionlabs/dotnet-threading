// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

﻿#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(AsyncVoidMethodBuilder))]

#else

namespace System.Runtime.CompilerServices
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a builder for asynchronous methods that return void. This type is intended for compiler use only.
    /// </summary>
    public struct AsyncVoidMethodBuilder : IAsyncMethodBuilder
    {
        /// <summary>Non-zero if <see cref="PreventUnobservedTaskExceptions"/> has already been invoked.</summary>
        private static int _preventUnobservedTaskExceptionsInvoked;

        /// <summary>The synchronization context associated with this operation.</summary>
        private readonly SynchronizationContext _synchronizationContext;

        /// <summary>State related to the <see cref="IAsyncStateMachine"/>.</summary>
        private AsyncMethodBuilderCore _coreState; // mutable struct: must not be readonly

        /// <summary>An object used by the debugger to uniquely identify this builder. Lazily initialized.</summary>
        private object _objectIdForDebugger;

        /// <summary>
        /// Initializes static members of the <see cref="AsyncVoidMethodBuilder"/> struct.
        /// </summary>
        /// <remarks>
        /// <para>Temporary support for disabling crashing if tasks go unobserved.</para>
        /// </remarks>
        static AsyncVoidMethodBuilder()
        {
            try
            {
                PreventUnobservedTaskExceptions();
            }
            catch
            {
            }
        }

        /// <summary>Initializes a new instance of the <see cref="AsyncVoidMethodBuilder"/> struct.</summary>
        /// <param name="synchronizationContext">
        /// The synchronization context associated with this operation. This may be <see langword="null"/>.
        /// </param>
        private AsyncVoidMethodBuilder(SynchronizationContext synchronizationContext)
        {
            _synchronizationContext = synchronizationContext;
            if (synchronizationContext != null)
                synchronizationContext.OperationStarted();

            _coreState = default(AsyncMethodBuilderCore);
            _objectIdForDebugger = null;
        }

        /// <summary>
        /// Gets an object that may be used to uniquely identify this builder to the debugger.
        /// </summary>
        /// <remarks>
        /// This property lazily instantiates the ID in a non-thread-safe manner. It must only be used by the debugger
        /// and only in a single-threaded manner.
        /// </remarks>
        /// <value>
        /// An object that may be used to uniquely identify this builder to the debugger.
        /// </value>
        private object ObjectIdForDebugger
        {
            get
            {
                if (_objectIdForDebugger == null)
                    _objectIdForDebugger = new object();

                return _objectIdForDebugger;
            }
        }

        /// <summary>
        /// Registers with <see cref="TaskScheduler.UnobservedTaskException"/> to suppress exception crashing.
        /// </summary>
        internal static void PreventUnobservedTaskExceptions()
        {
            if (Interlocked.CompareExchange(ref _preventUnobservedTaskExceptionsInvoked, 1, 0) == 0)
            {
                TaskScheduler.UnobservedTaskException += (s, e) => e.SetObserved();
            }
        }

        /// <summary>Initializes a new <see cref="AsyncVoidMethodBuilder"/>.</summary>
        /// <returns>The initialized <see cref="AsyncVoidMethodBuilder"/>.</returns>
        public static AsyncVoidMethodBuilder Create()
        {
            // Capture the current sync context.  If there isn't one, use the dummy s_noContextCaptured
            // instance; this allows us to tell the state of no captured context apart from the state
            // of an improperly constructed builder instance.
            return new AsyncVoidMethodBuilder(SynchronizationContext.Current);
        }

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stateMachine"/> is <see langword="null"/>.
        /// </exception>
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
            // no initialization is necessary for AsyncVoidMethodBuilder
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
            catch (Exception exc)
            {
                // Prevent exceptions from leaking to the call site, which could
                // then allow multiple flows of execution through the same async method
                // if the awaiter had already scheduled the continuation by the time
                // the exception was thrown.  We propagate the exception on the
                // ThreadPool because we can trust it to not throw, unlike
                // if we were to go to a user-supplied SynchronizationContext,
                // whose Post method could easily throw.
                AsyncServices.ThrowAsync(exc, targetContext: null);
            }
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

        /// <summary>Completes the method builder successfully.</summary>
        public void SetResult()
        {
            if (_synchronizationContext != null)
            {
                NotifySynchronizationContextOfCompletion();
            }
        }

        /// <summary>Faults the method builder with an exception.</summary>
        /// <param name="exception">The exception that is the cause of this fault.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="exception"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
        public void SetException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            if (_synchronizationContext != null)
            {
                // If we captured a synchronization context, post the throwing of the exception to it and decrement its
                // outstanding operation count.
                try
                {
                    AsyncServices.ThrowAsync(exception, targetContext: _synchronizationContext);
                }
                finally
                {
                    NotifySynchronizationContextOfCompletion();
                }
            }
            else
            {
                // Otherwise, queue the exception to be thrown on the ThreadPool. This will result in a crash unless
                // legacy exception behavior is enabled by a configuration file or a CLR host.
                AsyncServices.ThrowAsync(exception, targetContext: null);
            }
        }

        /// <summary>Notifies the current synchronization context that the operation completed.</summary>
        private void NotifySynchronizationContextOfCompletion()
        {
            Debug.Assert(_synchronizationContext != null, "Must only be used with a non-null context.");
            try
            {
                _synchronizationContext.OperationCompleted();
            }
            catch (Exception exc)
            {
                // If the interaction with the SynchronizationContext goes awry, fall back to propagating on the
                // thread pool.
                AsyncServices.ThrowAsync(exc, targetContext: null);
            }
        }
    }
}

#endif
