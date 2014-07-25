// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Rackspace.Threading
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods for creating tasks which emulate the behavior of <see langword="async/await"/>
    /// without requiring the use of those keywords.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public static class TaskBlocks
    {
        /// <summary>
        /// Provides support for resource cleanup in asynchronous code where the <see langword="async/await"/>
        /// keywords are not available.
        /// </summary>
        /// <remarks>
        /// This code implements support for the following construct without requiring the use of <see langword="async/await"/>.
        ///
        /// <code language="cs">
        /// using (IDisposable disposable = await resource().ConfigureAwait(false))
        /// {
        ///     return await body(disposable).ConfigureAwait(false);
        /// }
        /// </code>
        ///
        /// <note type="caller">
        /// If the <paramref name="resource"/> function throws an exception, or if the <see cref="Task{TResult}"/> it returns
        /// does not complete successfully, the resource will not be acquired by this method. In either of these
        /// situations the caller is responsible for ensuring the <paramref name="resource"/> function cleans up any
        /// resources it creates.
        /// </note>
        /// </remarks>
        /// <typeparam name="TResource">The type of resource used within the task and disposed of afterwards.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the continuation <see cref="Task{TResult}"/>.</typeparam>
        /// <param name="resource">A function which acquires the resource used during the execution of the task.</param>
        /// <param name="body">The continuation function which provides the <see cref="Task{TResult}"/> which acts as the body of the <c>using</c> block.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. When the task completes successfully,
        /// the <see cref="Task{TResult}.Result"/> property will contain the result provided by the
        /// <see cref="Task{TResult}.Result"/> property of the task returned from <paramref name="body"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="resource"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> is <see langword="null"/>.</para>
        /// </exception>
        public static Task<TResult> Using<TResource, TResult>(Func<Task<TResource>> resource, Func<Task<TResource>, Task<TResult>> body)
            where TResource : IDisposable
        {
            if (resource == null)
                throw new ArgumentNullException("resource");
            if (body == null)
                throw new ArgumentNullException("body");

            Task<TResource> resourceTask = resource();
            return resourceTask
                .Then(body)
                .Finally(
                    task =>
                    {
                        if (resourceTask.Status == TaskStatus.RanToCompletion)
                        {
                            IDisposable disposable = resourceTask.Result;
                            if (disposable != null)
                            {
                                IAsyncDisposable asyncDisposable = disposable as IAsyncDisposable;
                                if (asyncDisposable != null)
                                {
                                    return asyncDisposable.DisposeAsync();
                                }
                                else
                                {
                                    disposable.Dispose();
                                    return CompletedTask.Default;
                                }
                            }
                        }

                        return CompletedTask.Default;
                    });
        }

        /// <summary>
        /// Provides support for resource cleanup in asynchronous code where the <see langword="async/await"/>
        /// keywords are not available.
        /// </summary>
        /// <remarks>
        /// This code implements support for the following construct without requiring the use of <see langword="async/await"/>.
        ///
        /// <code language="cs">
        /// using (IDisposable disposable = await resource().ConfigureAwait(false))
        /// {
        ///     await body(disposable).ConfigureAwait(false);
        /// }
        /// </code>
        ///
        /// <note type="caller">
        /// If the <paramref name="resource"/> function throws an exception, or if the <see cref="Task{TResult}"/> it returns
        /// does not complete successfully, the resource will not be acquired by this method. In either of these
        /// situations the caller is responsible for ensuring the <paramref name="resource"/> function cleans up any
        /// resources it creates.
        /// </note>
        /// </remarks>
        /// <typeparam name="TResource">The type of resource used within the task and disposed of afterwards.</typeparam>
        /// <param name="resource">A function which acquires the resource used during the execution of the task.</param>
        /// <param name="body">The continuation function which provides the <see cref="Task"/> which acts as the body of the <c>using</c> block.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="resource"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> is <see langword="null"/>.</para>
        /// </exception>
        public static Task Using<TResource>(Func<Task<TResource>> resource, Func<Task<TResource>, Task> body)
            where TResource : IDisposable
        {
            if (resource == null)
                throw new ArgumentNullException("resource");
            if (body == null)
                throw new ArgumentNullException("body");

            Task<TResource> resourceTask = resource();
            return resourceTask
                .Then(body)
                .Finally(
                    task =>
                    {
                        if (resourceTask.Status == TaskStatus.RanToCompletion)
                        {
                            IDisposable disposable = resourceTask.Result;
                            if (disposable != null)
                            {
                                IAsyncDisposable asyncDisposable = disposable as IAsyncDisposable;
                                if (asyncDisposable != null)
                                {
                                    return asyncDisposable.DisposeAsync();
                                }
                                else
                                {
                                    disposable.Dispose();
                                    return CompletedTask.Default;
                                }
                            }
                        }

                        return CompletedTask.Default;
                    });
        }

        /// <summary>
        /// Provides support for a conditional "while" loop in asynchronous code, without requiring the use of <see langword="async/await"/>.
        /// </summary>
        /// <remarks>
        /// This code implements support for the following construct without requiring the use of <see langword="async/await"/>.
        ///
        /// <code language="cs">
        /// while (condition())
        /// {
        ///     await body().ConfigureAwait(false);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="condition">A function which evaluates the condition of the asynchronous <c>while</c> loop.</param>
        /// <param name="body">A function which returns a <see cref="Task"/> representing one iteration of the body of the <c>while</c> loop.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="condition"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> is <see langword="null"/>.</para>
        /// </exception>
        public static Task While(Func<bool> condition, Func<Task> body)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (body == null)
                throw new ArgumentNullException("body");

            TaskCompletionSource<VoidResult> taskCompletionSource = new TaskCompletionSource<VoidResult>();
            Task currentTask = CompletedTask.Default;

            // This action specifically handles cases where evaluating condition or body
            // results in an exception.
            Action<Task> handleErrors =
                previousTask =>
                {
                    if (previousTask.Status != TaskStatus.RanToCompletion)
                        taskCompletionSource.SetFromTask(previousTask);
                };

            Action<Task> continuation = null;
            continuation =
                previousTask =>
                {
                    if (previousTask.Status != TaskStatus.RanToCompletion)
                    {
                        taskCompletionSource.SetFromTask(previousTask);
                        return;
                    }

                    if (!condition())
                    {
                        taskCompletionSource.SetResult(null);
                        return;
                    }

                    // reschedule
                    currentTask = body().Finally(continuation).Finally(handleErrors);
                };

            currentTask.Finally(continuation).Finally(handleErrors);
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Provides support for a conditional "while" loop in asynchronous code, without requiring the use of <see langword="async/await"/>.
        /// </summary>
        /// <remarks>
        /// This code implements support for the following construct without requiring the use of <see langword="async/await"/>.
        ///
        /// <code language="cs">
        /// while (await condition().ConfigureAwait(false))
        /// {
        ///     await body().ConfigureAwait(false);
        /// }
        /// </code>
        /// </remarks>
        /// <param name="condition">A function which returns a <see cref="Task"/> representing the asynchronous evaluation of the <c>while</c> condition.</param>
        /// <param name="body">A function which returns a <see cref="Task"/> representing one iteration of the body of the <c>while</c> loop.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="condition"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> is <see langword="null"/>.</para>
        /// </exception>
        public static Task While(Func<Task<bool>> condition, Func<Task> body)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (body == null)
                throw new ArgumentNullException("body");

            TaskCompletionSource<VoidResult> taskCompletionSource = new TaskCompletionSource<VoidResult>();
            Task currentTask = CompletedTask.Default;

            Action<Task> statusCheck =
                previousTask =>
                {
                    if (previousTask.Status != TaskStatus.RanToCompletion)
                        taskCompletionSource.SetFromTask(previousTask);
                };

            Func<Task, Task<bool>> conditionContinuation =
                previousTask =>
                {
                    if (taskCompletionSource.Task.IsCompleted)
                        return CompletedTask.FromResult(false);

                    return condition();
                };

            Action<Task<bool>> continuation = null;
            continuation =
                previousTask =>
                {
                    if (taskCompletionSource.Task.IsCompleted)
                    {
                        return;
                    }

                    if (!previousTask.Result)
                    {
                        taskCompletionSource.TrySetResult(null);
                        return;
                    }

                    // reschedule
                    currentTask = body().Finally(statusCheck).Then(conditionContinuation).Select(continuation).Finally(statusCheck);
                };

            currentTask = currentTask.Finally(statusCheck).Then(conditionContinuation).Select(continuation).Finally(statusCheck);
            return taskCompletionSource.Task;
        }
    }
}
