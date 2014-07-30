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
        /// <para>
        /// This method expands on the <c>using</c> statement provided by C# by implementing support for
        /// <see cref="IAsyncDisposable"/> as described in
        /// <see href="http://roslyn.codeplex.com/discussions/546377">IAsyncDisposable, using statements, and async/await</see>.
        /// </para>
        ///
        /// <note type="caller">
        /// If the <paramref name="resource"/> function throws an exception, or if it returns <see langword="null"/>,
        /// or if the <see cref="Task{TResult}"/> it returns does not complete successfully, the resource will not be
        /// acquired by this method. In either of these situations the caller is responsible for ensuring the
        /// <paramref name="resource"/> function cleans up any resources it creates.
        /// </note>
        /// </remarks>
        /// <typeparam name="TResource">The type of resource used within the task and disposed of afterwards.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by the continuation <see cref="Task{TResult}"/>.</typeparam>
        /// <param name="resource">A function which acquires the resource used during the execution of the task.</param>
        /// <param name="body">The continuation function which provides the <see cref="Task{TResult}"/> which acts as the body of the <c>using</c> block.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. When the task completes successfully,
        /// the <see cref="Task{TResult}.Result"/> property will contain the result provided by the
        /// <see cref="Task{TResult}.Result"/> property of the task returned from <paramref name="body"/>.</returns>
        /// <example>
        /// The following example asynchronously acquires a resource by calling the user method <c>AcquireResourceAsync</c>.
        /// The resource will be disposed after the body executes, prior to returning the result of the body.
        /// <code source="..\Samples\CSharpSamples\TaskBlockUsingWithResult.cs" region="UsingWithResultAsyncBuildingBlock" language="cs"/>
        /// <para>
        /// For reference, the following example demonstrates a (nearly) equivalent implementation of this behavior using
        /// the <see langword="async/await"/> operators.
        /// </para>
        /// <code source="..\Samples\CSharpSamples\TaskBlockUsingWithResult.cs" region="UsingWithResultAsyncAwait" language="cs"/>
        /// </example>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="resource"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="resource"/> returns <see langword="null"/>.
        /// </exception>
        public static Task<TResult> Using<TResource, TResult>(Func<Task<TResource>> resource, Func<Task<TResource>, Task<TResult>> body)
            where TResource : IDisposable
        {
            if (resource == null)
                throw new ArgumentNullException("resource");
            if (body == null)
                throw new ArgumentNullException("body");

            Task<TResource> resourceTask = resource();
            if (resourceTask == null)
                throw new InvalidOperationException("The resource acquisition Task provided by the 'resource' delegate cannot be null.");

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
        /// <para>
        /// This method expands on the <c>using</c> statement provided by C# by implementing support for
        /// <see cref="IAsyncDisposable"/> as described in
        /// <see href="http://roslyn.codeplex.com/discussions/546377">IAsyncDisposable, using statements, and async/await</see>.
        /// </para>
        ///
        /// <note type="caller">
        /// If the <paramref name="resource"/> function throws an exception, or if it returns <see langword="null"/>,
        /// or if the <see cref="Task{TResult}"/> it returns does not complete successfully, the resource will not be
        /// acquired by this method. In either of these situations the caller is responsible for ensuring the
        /// <paramref name="resource"/> function cleans up any resources it creates.
        /// </note>
        /// </remarks>
        /// <typeparam name="TResource">The type of resource used within the task and disposed of afterwards.</typeparam>
        /// <param name="resource">A function which acquires the resource used during the execution of the task.</param>
        /// <param name="body">The continuation function which provides the <see cref="Task"/> which acts as the body of the <c>using</c> block.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <example>
        /// The following example asynchronously acquires a resource by calling the user method <c>AcquireResourceAsync</c>.
        /// The resource will be disposed after the body executes. No result is return from this operation, as the body of
        /// the task block represents an asynchronous operation that does not return a result.
        /// <code source="..\Samples\CSharpSamples\TaskBlockUsing.cs" region="UsingAsyncBuildingBlock" language="cs"/>
        /// <para>
        /// For reference, the following example demonstrates a (nearly) equivalent implementation of this behavior using
        /// the <see langword="async/await"/> operators.
        /// </para>
        /// <code source="..\Samples\CSharpSamples\TaskBlockUsing.cs" region="UsingAsyncAwait" language="cs"/>
        /// </example>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="resource"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="resource"/> returns <see langword="null"/>.
        /// </exception>
        public static Task Using<TResource>(Func<Task<TResource>> resource, Func<Task<TResource>, Task> body)
            where TResource : IDisposable
        {
            if (resource == null)
                throw new ArgumentNullException("resource");
            if (body == null)
                throw new ArgumentNullException("body");

            Task<TResource> resourceTask = resource();
            if (resourceTask == null)
                throw new InvalidOperationException("The resource acquisition Task provided by the 'resource' delegate cannot be null.");

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
        /// <example>
        /// The following example shows a basic "while" loop implemented using this building block.
        /// <code source="..\Samples\CSharpSamples\TaskBlockWhileAsync.cs" region="WhileAsyncBuildingBlock" language="cs"/>
        /// <para>
        /// For reference, the following example demonstrates a (nearly) equivalent implementation of this behavior using
        /// the <see langword="async/await"/> operators.
        /// </para>
        /// <code source="..\Samples\CSharpSamples\TaskBlockWhileAsync.cs" region="WhileAsyncAwait" language="cs"/>
        /// </example>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="condition"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="body"/> returns <see langword="null"/>.
        /// </exception>
        public static Task While(Func<bool> condition, Func<Task> body)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (body == null)
                throw new ArgumentNullException("body");

            TaskCompletionSource<VoidResult> taskCompletionSource = new TaskCompletionSource<VoidResult>();
            Task currentTask;

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
                    Task bodyTask = body();
                    if (bodyTask == null)
                        throw new InvalidOperationException("The Task provided by the 'body' delegate cannot be null.");

                    currentTask = bodyTask.Select(continuation).Finally(handleErrors);
                };

            currentTask = CompletedTask.Default.Select(continuation).Finally(handleErrors);
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
        /// <example>
        /// The following example shows a basic "while" loop implemented using this building block.
        /// <code source="..\Samples\CSharpSamples\TaskBlockWhileAsyncCondition.cs" region="WhileAsyncBuildingBlock" language="cs"/>
        /// <para>
        /// For reference, the following example demonstrates a (nearly) equivalent implementation of this behavior using
        /// the <see langword="async/await"/> operators.
        /// </para>
        /// <code source="..\Samples\CSharpSamples\TaskBlockWhileAsyncCondition.cs" region="WhileAsyncAwait" language="cs"/>
        /// </example>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="condition"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If <paramref name="condition"/> returns <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="body"/> returns <see langword="null"/>.</para>
        /// </exception>
        public static Task While(Func<Task<bool>> condition, Func<Task> body)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (body == null)
                throw new ArgumentNullException("body");

            TaskCompletionSource<VoidResult> taskCompletionSource = new TaskCompletionSource<VoidResult>();
            Task currentTask;

            Action<Task> statusCheck =
                previousTask =>
                {
                    if (previousTask.Status != TaskStatus.RanToCompletion)
                        taskCompletionSource.SetFromTask(previousTask);
                };

            Func<Task, Task<bool>> conditionContinuation =
                previousTask =>
                {
                    Task<bool> conditionTask = condition();
                    if (conditionTask == null)
                        throw new InvalidOperationException("The Task provided by the 'condition' delegate cannot be null.");

                    return conditionTask;
                };

            Action<Task<bool>> continuation = null;
            continuation =
                previousTask =>
                {
                    if (!previousTask.Result)
                    {
                        taskCompletionSource.TrySetResult(null);
                        return;
                    }

                    // reschedule
                    Task bodyTask = body();
                    if (bodyTask == null)
                        throw new InvalidOperationException("The Task provided by the 'body' delegate cannot be null.");

                    currentTask = bodyTask.Then(conditionContinuation).Select(continuation).Finally(statusCheck);
                };

            currentTask = CompletedTask.Default.Then(conditionContinuation).Select(continuation).Finally(statusCheck);
            return taskCompletionSource.Task;
        }
    }
}
