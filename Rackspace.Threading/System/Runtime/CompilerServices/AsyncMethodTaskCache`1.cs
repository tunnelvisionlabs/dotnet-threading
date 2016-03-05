// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

﻿#if !NET40PLUS

namespace System.Runtime.CompilerServices
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>Provides a base class used to cache tasks of a specific return type.</summary>
    /// <typeparam name="TResult">Specifies the type of results the cached tasks return.</typeparam>
    internal class AsyncMethodTaskCache<TResult>
    {
        /// <summary>
        /// A singleton cache for this result type. This may be <see langword="null"/> if there are no cached tasks for
        /// this <typeparamref name="TResult"/>.
        /// </summary>
        internal static readonly AsyncMethodTaskCache<TResult> Singleton = CreateCache();

        /// <summary>Creates a non-disposable task.</summary>
        /// <param name="result">The result for the task.</param>
        /// <returns>The cacheable task.</returns>
        internal static TaskCompletionSource<TResult> CreateCompleted(TResult result)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.TrySetResult(result);
            return tcs;
        }

        /// <summary>Creates a cache.</summary>
        /// <returns>A task cache for this result type.</returns>
        private static AsyncMethodTaskCache<TResult> CreateCache()
        {
            // Get the result type
            var resultType = typeof(TResult);

            // Return a new cache for this particular kind of task. If we don't have a specialized cache for this type,
            // return null.
            if (resultType == typeof(bool))
            {
                return (AsyncMethodTaskCache<TResult>)(object)new AsyncMethodBooleanTaskCache();
            }
            else if (resultType == typeof(int))
            {
                return (AsyncMethodTaskCache<TResult>)(object)new AsyncMethodInt32TaskCache();
            }
            else
            {
                return null;
            }
        }

        /// <summary>Gets a cached task if one exists.</summary>
        /// <param name="result">The result for which we want a cached task.</param>
        /// <returns>A cached task if one exists; otherwise, <see langword="null"/>.</returns>
        internal virtual TaskCompletionSource<TResult> FromResult(TResult result)
        {
            return CreateCompleted(result);
        }

        /// <summary>Provides a cache for <see cref="bool"/> tasks.</summary>
        private sealed class AsyncMethodBooleanTaskCache : AsyncMethodTaskCache<bool>
        {
            /// <summary>A <see langword="true"/> task.</summary>
            private readonly TaskCompletionSource<bool> _true = CreateCompleted(true);

            /// <summary>A <see langword="false"/> task.</summary>
            private readonly TaskCompletionSource<bool> _false = CreateCompleted(false);

            /// <summary>Gets a cached task for the <see cref="bool"/> result.</summary>
            /// <param name="result"><see langword="true"/> or <see langword="false"/></param>
            /// <returns>A cached task for the <see cref="bool"/> result.</returns>
            internal sealed override TaskCompletionSource<bool> FromResult(bool result)
            {
                return result ? _true : _false;
            }
        }

        /// <summary>Provides a cache for <see cref="int"/> tasks.</summary>
        private sealed class AsyncMethodInt32TaskCache : AsyncMethodTaskCache<int>
        {
            /// <summary>The minimum value, inclusive, for which we want a cached task.</summary>
            internal const int InclusiveInt32Min = -1;

            /// <summary>The maximum value, exclusive, for which we want a cached task.</summary>
            internal const int ExclusiveInt32Max = 9;

            /// <summary>The cache of <see cref="int"/>-returning <see cref="Task{TResult}"/> instances.</summary>
            internal static readonly TaskCompletionSource<int>[] Int32Tasks = CreateInt32Tasks();

            /// <summary>
            /// Creates an array of cached tasks for the values in the range [<see cref="InclusiveInt32Min"/>,
            /// <see cref="ExclusiveInt32Max"/>).
            /// </summary>
            /// <returns>An array of completion sources for cached integer values.</returns>
            private static TaskCompletionSource<int>[] CreateInt32Tasks()
            {
                Debug.Assert(ExclusiveInt32Max >= InclusiveInt32Min, "Expected max to be at least min");
                var tasks = new TaskCompletionSource<int>[ExclusiveInt32Max - InclusiveInt32Min];
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = CreateCompleted(i + InclusiveInt32Min);
                }

                return tasks;
            }

            /// <summary>Gets a cached task for the <see cref="int"/> result.</summary>
            /// <param name="result">The integer value.</param>
            /// <returns>
            /// A cached task for the <see cref="int"/> result; otherwise, <see langword="null"/> if not cached.
            /// </returns>
            internal sealed override TaskCompletionSource<int> FromResult(int result)
            {
                return (result >= InclusiveInt32Min && result < ExclusiveInt32Max) ?
                    Int32Tasks[result - InclusiveInt32Min] :
                    CreateCompleted(result);
            }
        }
    }
}

#endif
