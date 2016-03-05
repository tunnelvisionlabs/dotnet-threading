// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(ICriticalNotifyCompletion))]

#else

namespace System.Runtime.CompilerServices
{
    using System.Security;
    using System.Threading;

    /// <summary>
    /// Represents an awaiter used to schedule continuations when an await operation completes.
    /// </summary>
    /// <preliminary/>
    public interface ICriticalNotifyCompletion : INotifyCompletion
    {
        /// <summary>Schedules the continuation action to be invoked when the instance completes.</summary>
        /// <param name="continuation">The action to invoke when the operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        /// Unlike <see cref="INotifyCompletion.OnCompleted"/>, <see cref="UnsafeOnCompleted"/> need not propagate
        /// <see cref="ExecutionContext"/> information.
        [SecurityCritical]
        void UnsafeOnCompleted(Action continuation);
    }
}

#endif
