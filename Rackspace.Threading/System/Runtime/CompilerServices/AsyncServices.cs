// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if NET45PLUS || !NET40PLUS

// NOTE: If you change this copy, please also change the copy under the Async partition
namespace System.Runtime.CompilerServices
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

#if NET45PLUS
    using System.Threading.Tasks;
#endif

#if EXCEPTION_STACK_PRESERVE
    using System.Diagnostics;
    using System.Reflection;
#endif

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements must be documented", Justification = "The reference source is missing documentation")]
    internal static class AsyncServices
    {
#if EXCEPTION_STACK_PRESERVE
        /// <summary>
        /// A <see cref="MethodInfo"/> for the internal <c>PrepForRemoting</c> method on <see cref="Exception"/>.
        /// </summary>
        private static readonly MethodInfo _prepForRemoting = GetPrepForRemotingMethodInfo();

        /// <summary>An empty array to use with <see cref="MethodBase.Invoke(object, object[])"/>.</summary>
        private static readonly object[] _emptyParams = new object[0];
#endif

        /// <summary>Throws the exception on the thread pool.</summary>
        /// <param name="exception">The exception to propagate.</param>
        /// <param name="targetContext">
        /// The target context on which to propagate the exception; otherwise, <see langword="null"/> to use the thread
        /// pool.
        /// </param>
        internal static void ThrowAsync(Exception exception, SynchronizationContext targetContext)
        {
            // If the user supplied a SynchronizationContext...
            if (targetContext != null)
            {
                try
                {
                    // Post the throwing of the exception to that context, and return.
                    targetContext.Post(state => { throw PrepareExceptionForRethrow((Exception)state); }, exception);
                    return;
                }
                catch (Exception postException)
                {
                    // If something goes horribly wrong in the Post, we'll propagate both exceptions on the thread pool
                    exception = new AggregateException(exception, postException);
                }
            }

#if NET45PLUS
            Task.Run(() =>
            {
                throw PrepareExceptionForRethrow(exception);
            });
#else
            // Propagate the exception(s) on the ThreadPool
            ThreadPool.QueueUserWorkItem(state => { throw PrepareExceptionForRethrow((Exception)state); }, exception);
#endif
        }

        /// <summary>Copies the exception's stack trace so its stack trace isn't overwritten.</summary>
        /// <param name="exc">The exception to prepare.</param>
        /// <returns>The input exception, <paramref name="exc"/>.</returns>
        internal static Exception PrepareExceptionForRethrow(Exception exc)
        {
#if EXCEPTION_STACK_PRESERVE
            Debug.Assert(exc != null, "Assertion failed: exc != null");
            if (_prepForRemoting != null)
            {
                try
                {
                    _prepForRemoting.Invoke(exc, _emptyParams);
                }
                catch
                {
                }
            }
#endif

            return exc;
        }

#if EXCEPTION_STACK_PRESERVE
        /// <summary>
        /// Gets the <see cref="MethodInfo"/> for the internal <c>PrepForRemoting</c> method on <see cref="Exception"/>.
        /// </summary>
        /// <returns>The <see cref="MethodInfo"/> if it could be retrieved; otherwise, <see langword="null"/>.</returns>
        private static MethodInfo GetPrepForRemotingMethodInfo()
        {
            try
            {
                return typeof(Exception).GetMethod("PrepForRemoting", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            catch
            {
                return null;
            }
        }
#endif
    }
}

#endif
