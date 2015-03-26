#if NET40PLUS

using System;
using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(IProgress<>))]

#else

namespace System
{
    /// <summary>
    /// Defines a provider for progress updates.
    /// </summary>
    /// <typeparam name="T">The type of progress update value.</typeparam>
    public interface IProgress<T>
    {
        /// <summary>
        /// Reports a progress update.
        /// </summary>
        /// <param name="value">The value of the updated progress.</param>
        void Report(T value);
    }
}

#endif
