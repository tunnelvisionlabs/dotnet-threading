// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

﻿#if NET40PLUS && !NET45PLUS

[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(Microsoft.Runtime.CompilerServices.TaskAwaiter<>))]

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

    /// <summary>Provides an awaiter for awaiting a <see cref="Task{TResult}"/>.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    /// <typeparam name="TResult">The return type of the <see cref="Task{TResult}"/>.</typeparam>
    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion, INotifyCompletion
    {
        /// <summary>The task being awaited.</summary>
        private readonly Task<TResult> _task;

        /// <summary>Initializes a new instance of the <see cref="TaskAwaiter{TResult}"/> struct.</summary>
        /// <param name="task">The <see cref="Task{TResult}"/> to be awaited.</param>
        internal TaskAwaiter(Task<TResult> task)
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
        /// Schedules the continuation onto the <see cref="Task{TResult}"/> associated with this
        /// <see cref="TaskAwaiter{TResult}"/>.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NullReferenceException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public void OnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(_task, continuation, TaskAwaiter.ContinueOnCapturedContextDefault);
        }

        /// <summary>
        /// Schedules the continuation onto the <see cref="Task{TResult}"/> associated with this
        /// <see cref="TaskAwaiter{TResult}"/>.
        /// </summary>
        /// <param name="continuation">The action to invoke when the await operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
#if !SILVERLIGHT
        ////[SecurityCritical]
#endif
        public void UnsafeOnCompleted(Action continuation)
        {
            TaskAwaiter.OnCompletedInternal(_task, continuation, TaskAwaiter.ContinueOnCapturedContextDefault);
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

#endif
