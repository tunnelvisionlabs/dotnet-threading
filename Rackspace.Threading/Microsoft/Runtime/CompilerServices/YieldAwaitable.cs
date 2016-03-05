// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if NET40PLUS && !NET45PLUS

[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(Microsoft.Runtime.CompilerServices.YieldAwaitable))]

#else

// NOTE: The reason this type does exist in the BCL System.Threading.Tasks contract is because we need to be able to
// construct one of these in the AwaitExtensions class. The equivalent type in the current platform does not have an
// accessible constructor, hence the AwaitExtensions would fail when run on platforms where System.Threading.Tasks gets
// unified.
namespace Microsoft.Runtime.CompilerServices
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Rackspace.Threading;

    /// <summary>Provides an awaitable context for switching into a target environment.</summary>
    /// <remarks>This type is intended for compiler use only.</remarks>
    public struct YieldAwaitable
    {
        /// <summary>Gets an awaiter for this <see cref="YieldAwaitable"/>.</summary>
        /// <returns>An awaiter for this awaitable.</returns>
        /// <remarks>This method is intended for compiler user rather than use directly in code.</remarks>
        public YieldAwaiter GetAwaiter()
        {
            // if not initialized properly, m_target will just be null
            return default(YieldAwaiter);
        }

        /// <summary>Provides an awaiter that switches into a target environment.</summary>
        /// <remarks>This type is intended for compiler use only.</remarks>
        public struct YieldAwaiter : ICriticalNotifyCompletion, INotifyCompletion
        {
            /// <summary>A completed task.</summary>
            private static readonly Task _completed = CompletedTask.FromResult(0);

            /// <summary>Gets a value indicating whether a yield is not required.</summary>
            /// <remarks>This property is intended for compiler user rather than use directly in code.</remarks>
            /// <value>A value indicating whether a yield is not required.</value>
            public bool IsCompleted
            {
                get
                {
                    // yielding is always required for YieldAwaiter, hence false
                    return false;
                }
            }

            /// <summary>Posts the <paramref name="continuation"/> back to the current context.</summary>
            /// <param name="continuation">The action to invoke asynchronously.</param>
            /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
            public void OnCompleted(Action continuation)
            {
                _completed.GetAwaiter().OnCompleted(continuation);
            }

            /// <summary>Posts the <paramref name="continuation"/> back to the current context.</summary>
            /// <param name="continuation">The action to invoke asynchronously.</param>
            /// <exception cref="InvalidOperationException">The awaiter was not properly initialized.</exception>
            ////[SecurityCritical]
            public void UnsafeOnCompleted(Action continuation)
            {
                _completed.GetAwaiter().UnsafeOnCompleted(continuation);
            }

            /// <summary>Ends the await operation.</summary>
            public void GetResult()
            {
                // NOP. It exists purely because the compiler pattern demands it.
            }
        }
    }
}

#endif
