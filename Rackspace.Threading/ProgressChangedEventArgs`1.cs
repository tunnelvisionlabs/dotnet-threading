namespace Rackspace.Threading
{
    using System;

    /// <summary>
    /// This class contains information for the <see cref="Rackspace.Threading.Progress{T}.ProgressChanged"/> event.
    /// </summary>
    /// <remarks>
    /// <para>Prior to .NET 4.5, the <see cref="EventHandler{TEventArgs}"/> delegate included a
    /// constraint that the generic argument be derived from <see cref="EventArgs"/>. This
    /// constraint prevents a direct back-port of the <see cref="T:System.Progress`1"/>
    /// class to earlier frameworks, so the <see cref="Rackspace.Threading.Progress{T}"/> class is provided with
    /// a wrapper around the progress value reported by the
    /// <see cref="Rackspace.Threading.Progress{T}.ProgressChanged"/> event.</para>
    /// </remarks>
    /// <typeparam name="T">The type representing the progress indicator.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public class ProgressChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// This is the backing field for the <see cref="Progress"/> property.
        /// </summary>
        private readonly T _progress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressChangedEventArgs{T}"/> class
        /// with the specified progress.
        /// </summary>
        /// <param name="progress">An instance of <typeparamref name="T"/> representing the current progress.</param>
        public ProgressChangedEventArgs(T progress)
        {
            _progress = progress;
        }

        /// <summary>
        /// Gets the current progress.
        /// </summary>
        /// <value>
        /// An instance of <typeparamref name="T"/> representing the current progress.
        /// </value>
        public T Progress
        {
            get
            {
                return _progress;
            }
        }
    }
}
