// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

﻿#if NET40PLUS && !NET45PLUS

[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(Microsoft.Runtime.CompilerServices.ConfiguredTaskAwaitable<>))]

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
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an awaitable object that allows for configured awaits on <see cref="Task{TResult}"/>.
    /// </summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    /// <typeparam name="TResult">The result type of the <see cref="Task{TResult}"/>.</typeparam>
    public struct ConfiguredTaskAwaitable<TResult>
    {
        /// <summary>The underlying awaitable on whose logic this awaitable relies.</summary>
        private readonly ConfiguredTaskAwaiter _configuredTaskAwaiter;

        /// <summary>Initializes a new instance of the <see cref="ConfiguredTaskAwaitable{TResult}"/> struct.</summary>
        /// <param name="task">The awaitable <see cref="Task{TResult}"/>.</param>
        /// <param name="continueOnCapturedContext">
        /// <see langword="true"/> to attempt to marshal the continuation back to the original context captured;
        /// otherwise, <see langword="false"/>.
        /// </param>
        internal ConfiguredTaskAwaitable(Task<TResult> task, bool continueOnCapturedContext)
        {
            _configuredTaskAwaiter = new ConfiguredTaskAwaiter(task, continueOnCapturedContext);
        }

        /// <summary>Gets an awaiter for this awaitable.</summary>
        /// <returns>The awaiter.</returns>
        public ConfiguredTaskAwaiter GetAwaiter()
        {
            return _configuredTaskAwaiter;
        }

        /// <summary>Provides an awaiter for a <see cref="ConfiguredTaskAwaitable{TResult}"/>.</summary>
        /// <remarks>This type is intended for compiler use only.</remarks>
        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion
        {
            /// <summary>The task being awaited.</summary>
            private readonly Task<TResult> _task;

            /// <summary>Whether to attempt marshaling back to the original context.</summary>
            private readonly bool _continueOnCapturedContext;

            /// <summary>Initializes a new instance of the <see cref="ConfiguredTaskAwaiter"/> struct.</summary>
            /// <param name="task">The awaitable <see cref="Task{TResult}"/>.</param>
            /// <param name="continueOnCapturedContext">
            /// <see langword="true"/> to attempt to marshal the continuation back to the original context captured;
            /// otherwise, <see langword="false"/>.
            /// </param>
            internal ConfiguredTaskAwaiter(Task<TResult> task, bool continueOnCapturedContext)
            {
                Debug.Assert(task != null, "Assertion failed: task != null");
                _task = task;
                _continueOnCapturedContext = continueOnCapturedContext;
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
            /// Schedules the continuation onto the <see cref="Task"/> associated with this <see cref="TaskAwaiter"/>.
            /// </summary>
            /// <param name="continuation">The action to invoke when the await operation completes.</param>
            /// <exception cref="ArgumentNullException">
            /// If <paramref name="continuation"/> is <see langword="null"/>.
            /// </exception>
            /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
            public void OnCompleted(Action continuation)
            {
                TaskAwaiter.OnCompletedInternal(_task, continuation, _continueOnCapturedContext);
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
                TaskAwaiter.OnCompletedInternal(_task, continuation, _continueOnCapturedContext);
            }

            /// <summary>Ends the await on the completed <see cref="Task{TResult}"/>.</summary>
            /// <returns>The result of the completed <see cref="Task{TResult}"/>.</returns>
            /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
            /// <exception cref="InvalidOperationException">The task was not yet completed.</exception>
            /// <exception cref="TaskCanceledException">The task was canceled.</exception>
            /// <exception cref="Exception">The task completed in a <see cref="TaskStatus.Faulted"/> state.</exception>
            public TResult GetResult()
            {
                TaskAwaiter.ValidateEnd(_task);
                return _task.Result;
            }
        }
    }
}

#endif
