// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

﻿#if !NET40PLUS

namespace System.Runtime.CompilerServices
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>Holds state related to the builder's <see cref="IAsyncStateMachine"/>.</summary>
    /// <remarks>This is a mutable struct. Be very delicate with it.</remarks>
    internal struct AsyncMethodBuilderCore
    {
        /// <summary>A reference to the heap-allocated state machine object associated with this builder.</summary>
        internal IAsyncStateMachine _stateMachine;

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stateMachine"/> is <see langword="null"/>.
        /// </exception>
#if !SILVERLIGHT
        // [SecuritySafeCritical]
#endif
        [DebuggerStepThrough]
        internal void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            if (stateMachine == null)
                throw new ArgumentNullException("stateMachine");

            stateMachine.MoveNext();
        }

        /// <summary>Associates the builder with the state machine it represents.</summary>
        /// <param name="stateMachine">The heap-allocated state machine object.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stateMachine"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The builder is incorrectly initialized.</exception>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            if (stateMachine == null)
                throw new ArgumentNullException("stateMachine");
            if (_stateMachine != null)
                throw new InvalidOperationException("The builder was not properly initialized.");

            _stateMachine = stateMachine;
        }

        /// <summary>
        /// Gets the <see cref="Action"/> to use with an awaiter's <see cref="INotifyCompletion.OnCompleted"/> or
        /// <see cref="ICriticalNotifyCompletion.UnsafeOnCompleted"/> method. On first invocation, the supplied state
        /// machine will be boxed.
        /// </summary>
        /// <typeparam name="TMethodBuilder">Specifies the type of the method builder used.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine used.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="stateMachine">The state machine.</param>
        /// <returns>An <see cref="Action"/> to provide to the awaiter.</returns>
#if !SILVERLIGHT
        //// [SecuritySafeCritical]
#endif
        internal Action GetCompletionAction<TMethodBuilder, TStateMachine>(ref TMethodBuilder builder, ref TStateMachine stateMachine)
            where TMethodBuilder : IAsyncMethodBuilder
            where TStateMachine : IAsyncStateMachine
        {
            Debug.Assert(builder != null, "Expected valid builder");
            Debug.Assert(stateMachine != null, "Expected valid state machine reference");

            // The builder needs to flow ExecutionContext, so capture it.
            var capturedContext = ExecutionContextLightup.Instance.Capture();

            MoveNextRunner runner = new MoveNextRunner(capturedContext);
            Action action = new Action(runner.Run);

            // If this is our first await, such that we've not yet boxed the state machine, do so now.
            if (_stateMachine == null)
            {
                // This is our first await, and we're not boxed yet. First performance any work that must affect both
                // the non-boxed and boxed builders.
                builder.PreBoxInitialization();

                // Box the state machine, then tell the boxed instance to call back into its own builder, so we can
                // cache the boxed reference.
                Debug.Assert(!object.ReferenceEquals((object)stateMachine, (object)stateMachine), "Expected an unboxed state machine reference");
                _stateMachine = stateMachine;
                _stateMachine.SetStateMachine(_stateMachine);
            }

            Debug.Assert(runner._stateMachine == null, "The runner's state machine should not yet have been populated.");
            Debug.Assert(_stateMachine != null, "The builder's state machine field should have been initialized.");

            // Now that we have the state machine, store it into the runner that the action delegate points to. And
            // return the action.
            //
            // Only after this line is the Action delegate usable.
            runner._stateMachine = _stateMachine;
            return action;
        }

        /// <summary>
        /// Provides the ability to invoke a state machine's <see cref="IAsyncStateMachine.MoveNext"/> method under a
        /// supplied <see cref="ExecutionContext"/>.
        /// </summary>
        private sealed class MoveNextRunner
        {
            /// <summary>Cached delegate used with <see cref="ExecutionContext.Run"/>.</summary>
            private static Action<object> _invokeMoveNext; // lazily-initialized due to SecurityCritical attribution

            /// <summary>The context with which to run <see cref="IAsyncStateMachine.MoveNext"/>.</summary>
            private readonly ExecutionContextLightup _context;

            /// <summary>
            /// The state machine whose <see cref="IAsyncStateMachine.MoveNext"/> method should be invoked.
            /// </summary>
            [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields must be private", Justification = "Set by methods in the containing type.")]
            internal IAsyncStateMachine _stateMachine;

            /// <summary>Initializes a new instance of the <see cref="MoveNextRunner"/> class.</summary>
            /// <param name="context">The context with which to run <see cref="IAsyncStateMachine.MoveNext"/>.</param>
            internal MoveNextRunner(ExecutionContextLightup context)
            {
                _context = context;
            }

            /// <summary>Invokes <see cref="IAsyncStateMachine.MoveNext"/> under the provided context.</summary>
            internal void Run()
            {
                Debug.Assert(_stateMachine != null, "The state machine must have been set before calling Run.");

                if (_context != null)
                {
                    try
                    {
                        // Get the callback, lazily initializing it as necessary
                        Action<object> callback = _invokeMoveNext;
                        if (callback == null)
                        {
                            _invokeMoveNext = callback = InvokeMoveNext;
                        }

                        if (_context == null)
                        {
                            callback(_stateMachine);
                        }
                        else
                        {
                            // Use the context and callback to invoke _stateMachine.MoveNext.
                            ExecutionContextLightup.Instance.Run(_context, callback, _stateMachine);
                        }
                    }
                    finally
                    {
                        if (_context != null)
                            _context.Dispose();
                    }
                }
                else
                {
                    _stateMachine.MoveNext();
                }
            }

            /// <summary>
            /// Invokes the <see cref="IAsyncStateMachine.MoveNext"/> method on the supplied
            /// <see cref="IAsyncStateMachine"/>.
            /// </summary>
            /// <param name="stateMachine">The <see cref="IAsyncStateMachine"/> machine instance.</param>
            private static void InvokeMoveNext(object stateMachine)
            {
                ((IAsyncStateMachine)stateMachine).MoveNext();
            }
        }
    }
}

#endif
