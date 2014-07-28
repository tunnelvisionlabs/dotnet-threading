namespace Rackspace.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// Provides an <see cref="IProgress{T}"/> that invokes callbacks for each reported progress value.
    /// </summary>
    /// <remarks>
    /// Any handler provided to the constructor or event handlers registered with the
    /// <see cref="ProgressChanged"/> event are invoked through a <see cref="SynchronizationContext"/>
    /// instance captured when the instance is constructed. If there is no current
    /// <see cref="SynchronizationContext"/> at the time of construction, the callbacks will be invoked
    /// on the <see cref="T:System.Threading.ThreadPool"/>.
    /// </remarks>
    /// <typeparam name="T">Specifies the type of the progress report value.</typeparam>
    public class Progress<T> : IProgress<T>
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly SendOrPostCallback _callback;
        private readonly Action<T> _handler;

        /// <summary>
        /// Raised for each reported progress value.
        /// </summary>
        /// <remarks>
        /// Handlers registered with this event will be invoked on the <see cref="SynchronizationContext"/>
        /// captured when the instance was constructed.
        /// </remarks>
        public event EventHandler<ProgressChangedEventArgs<T>> ProgressChanged;

        /// <summary>
        /// Initializes the <see cref="Progress{T}"/> object.
        /// </summary>
        public Progress()
        {
            _synchronizationContext = SynchronizationContext.Current;
            _callback = Callback;
        }

        /// <summary>
        /// Initializes the <see cref="Progress{T}"/> object with the specified callback.
        /// </summary>
        /// <param name="handler">
        /// A handler to invoke for each reported progress value. This handler will be invoked in addition
        /// to any delegates registered with the <see cref="ProgressChanged"/> event. Depending on the
        /// <see cref="SynchronizationContext"/> instance captured by the <see cref="Progress{T}"/> at
        /// construction, it is possible that this handler instance could be invoked concurrently with
        /// itself.
        /// </param>
        public Progress(Action<T> handler)
            : this()
        {
            _handler = handler;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Any handler provided to the constructor or event handlers registered with the
        /// <see cref="ProgressChanged"/> event are invoked through a <see cref="SynchronizationContext"/>
        /// instance captured when the instance is constructed. If there is no current
        /// <see cref="SynchronizationContext"/> at the time of construction, the callbacks will be invoked
        /// on the <see cref="T:System.Threading.ThreadPool"/>.
        /// </remarks>
        void IProgress<T>.Report(T value)
        {
            OnReport(value);
        }

        /// <summary>
        /// Reports a progress change.
        /// </summary>
        /// <param name="value">The value of the updated progress.</param>
        protected virtual void OnReport(T value)
        {
            if (_handler != null || ProgressChanged != null)
                _synchronizationContext.Post(_callback, value);
        }

        private void Callback(object state)
        {
            T value = (T)state;
            if (_handler != null)
                _handler(value);

            EventHandler<ProgressChangedEventArgs<T>> t = ProgressChanged;
            if (t != null)
                t(this, new ProgressChangedEventArgs<T>(value));
        }
    }
}
