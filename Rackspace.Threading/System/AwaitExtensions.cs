// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

﻿#if NET40PLUS && !NET45PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(AwaitExtensions))]

#else

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Runtime.CompilerServices;

/// <summary>
/// Provides extension methods for threading-related types.
/// </summary>
/// <preliminary/>
public static class AwaitExtensions
{
    /// <summary>Gets an awaiter used to await this <see cref="Task"/>.</summary>
    /// <param name="task">The task to await.</param>
    /// <returns>An awaiter instance.</returns>
    public static TaskAwaiter GetAwaiter(this Task task)
    {
        if (task == null)
            throw new ArgumentNullException("task");

        return new TaskAwaiter(task);
    }

    /// <summary>Gets an awaiter used to await this <see cref="Task"/>.</summary>
    /// <typeparam name="TResult">Specifies the type of data returned by the task.</typeparam>
    /// <param name="task">The task to await.</param>
    /// <returns>An awaiter instance.</returns>
    public static TaskAwaiter<TResult> GetAwaiter<TResult>(this Task<TResult> task)
    {
        if (task == null)
            throw new ArgumentNullException("task");

        return new TaskAwaiter<TResult>(task);
    }

    /// <summary>Creates and configures an awaitable object for awaiting the specified task.</summary>
    /// <param name="task">The task to be awaited.</param>
    /// <param name="continueOnCapturedContext">
    /// <see langword="true"/> to automatically marshal back to the original call site's current
    /// <see cref="SynchronizationContext"/> or <see cref="TaskScheduler"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The instance to be awaited.</returns>
    public static ConfiguredTaskAwaitable ConfigureAwait(this Task task, bool continueOnCapturedContext)
    {
        if (task == null)
            throw new ArgumentNullException("task");

        return new ConfiguredTaskAwaitable(task, continueOnCapturedContext);
    }

    /// <summary>Creates and configures an awaitable object for awaiting the specified task.</summary>
    /// <typeparam name="TResult">The return type of the <see cref="Task{TResult}"/>.</typeparam>
    /// <param name="task">The task to be awaited.</param>
    /// <param name="continueOnCapturedContext">
    /// <see langword="true"/> to automatically marshal back to the original call site's current
    /// <see cref="SynchronizationContext"/> or <see cref="TaskScheduler"/>; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The instance to be awaited.</returns>
    public static ConfiguredTaskAwaitable<TResult> ConfigureAwait<TResult>(this Task<TResult> task, bool continueOnCapturedContext)
    {
        if (task == null)
            throw new ArgumentNullException("task");

        return new ConfiguredTaskAwaitable<TResult>(task, continueOnCapturedContext);
    }
}

#endif
