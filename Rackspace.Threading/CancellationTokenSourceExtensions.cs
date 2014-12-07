// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Rackspace.Threading
{
    using System;
    using System.Threading;

#if !NET45PLUS
#if NET40PLUS
    using System.Runtime.CompilerServices;
#else
    using System.Collections.Generic;
    using System.Collections.Concurrent;
#endif
#endif

    /// <summary>
    /// Provides extension methods for the <see cref="CancellationTokenSource"/> class.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public static class CancellationTokenSourceExtensions
    {
#if !NET45PLUS
#if NET40PLUS
        /// <summary>
        /// This map prevents <see cref="Timer"/> instances from being garbage collected prior to the
        /// <see cref="CancellationTokenSource"/> with which they are associated.
        /// </summary>
        private static readonly ConditionalWeakTable<CancellationTokenSource, TimerHolder> _timers =
            new ConditionalWeakTable<CancellationTokenSource, TimerHolder>();
#else
        /// <summary>
        /// This set prevents <see cref="Timer"/> instances from being garbage collected prior to the
        /// <see cref="CancellationTokenSource"/> with which they are associated.
        /// </summary>
        private static readonly ConcurrentDictionary<HashedWeakReference<CancellationTokenSource>, Timer> _timers =
            new ConcurrentDictionary<HashedWeakReference<CancellationTokenSource>, Timer>();
#endif
#endif

        /// <summary>
        /// Schedules a <see cref="CancellationTokenSource.Cancel()"/> operation on a <see cref="CancellationTokenSource"/>
        /// after the specified time span.
        /// </summary>
        /// <remarks>
        /// If a previous call to this method scheduled a cancellation, the cancellation time is
        /// reset to the new <paramref name="delay"/> value. This method has no effect if the
        /// <see cref="CancellationTokenSource"/> has already been cancelled (i.e. the
        /// <see cref="CancellationTokenSource.IsCancellationRequested"/> property returns
        /// <see langword="true"/>.
        /// <para>
        /// In all versions of .NET, requesting cancellation of a <see cref="CancellationTokenSource"/> will
        /// not prevent the instance from becoming eligible for garbage collection prior to the timer expiring.
        /// In .NET 4 and newer, any associated <see cref="T:System.Threading.Timer"/> instance will become eligible for
        /// garbage collection at the same time as the associated <see cref="CancellationTokenSource"/>.
        /// </para>
        /// </remarks>
        /// <param name="cts">The <see cref="CancellationTokenSource"/> to cancel after a delay.</param>
        /// <param name="delay">The time span to wait before canceling the <see cref="CancellationTokenSource"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="cts"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException">If <paramref name="cts"/> has been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If the <see cref="TimeSpan.TotalMilliseconds"/> property of <paramref name="delay"/> is less than -1 or greater than <see cref="int.MaxValue"/>.</exception>
        public static void CancelAfter(this CancellationTokenSource cts, TimeSpan delay)
        {
            if (cts == null)
                throw new ArgumentNullException("cts");

#if NET45PLUS
            cts.CancelAfter(delay);
#else
            if (delay.TotalMilliseconds < -1 || delay.TotalMilliseconds > int.MaxValue)
                throw new ArgumentOutOfRangeException("delay");

            if (cts.IsCancellationRequested)
                return;

#if NET40PLUS
            TimerHolder holder = _timers.GetOrCreateValue(cts);
            try
            {
                holder.GetOrCreateTimer(cts).Change(delay, TimeSpan.FromMilliseconds(-1));
            }
            catch (ObjectDisposedException)
            {
            }
#else
            Timer timer = _timers.GetOrAdd(new HashedWeakReference<CancellationTokenSource>(cts), CreateTimer);
            timer.Change(delay, TimeSpan.FromMilliseconds(-1));
#endif
#endif
        }

#if !NET45PLUS
#if !NET40PLUS
        private static Timer CreateTimer(HashedWeakReference<CancellationTokenSource> key)
        {
            CancellationTokenSource cts = key.Target;
            if (cts == null)
                throw new InvalidOperationException();

            TimerState state = new TimerState(cts);
            Timer timer = new Timer(TimeElapsed, state, Timeout.Infinite, Timeout.Infinite);
            state.Timer = timer;
            return timer;
        }
#endif

        private static void TimeElapsed(object state)
        {
            TimerState timerState = (TimerState)state;
#if !NET40PLUS
            foreach (HashedWeakReference<CancellationTokenSource> key in _timers.Keys)
            {
                if (key.IsAlive)
                    continue;

                Timer timer;
                if (_timers.TryRemove(key, out timer))
                    timer.Dispose();
            }
#endif

            try
            {
                CancellationTokenSource cts = timerState.CancellationTokenSource;
                if (cts != null)
                    cts.Cancel();

                timerState.Timer.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }

#if NET40PLUS
        private sealed class TimerHolder
        {
            private readonly object _lock = new object();
            private Timer _timer;

            public Timer GetOrCreateTimer(CancellationTokenSource cts)
            {
                if (cts == null)
                    throw new ArgumentNullException("cts");

                if (_timer != null)
                    return _timer;

                lock (_lock)
                {
                    if (_timer == null)
                    {
                        TimerState state = new TimerState(cts);
                        _timer = new Timer(TimeElapsed, state, Timeout.Infinite, Timeout.Infinite);
                        state.Timer = _timer;
                    }

                    return _timer;
                }
            }
        }
#else
        private class HashedWeakReference : WeakReference
        {
            private readonly int _hashCode;

            public HashedWeakReference(object target)
                : base(target)
            {
                _hashCode = EqualityComparer<object>.Default.GetHashCode(target);
            }

            public override object Target
            {
                get
                {
                    return base.Target;
                }

                set
                {
                    throw new NotSupportedException();
                }
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                    return true;
                if (obj == null)
                    return false;

                HashedWeakReference other = obj as HashedWeakReference;
                if (other == null)
                    return false;

                if (GetHashCode() != other.GetHashCode())
                    return false;

                return EqualityComparer<object>.Default.Equals(Target, other.Target);
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }

        private class HashedWeakReference<T> : HashedWeakReference
            where T : class
        {
            public HashedWeakReference(T target)
                : base(target)
            {
            }

            public virtual new T Target
            {
                get
                {
                    return (T)base.Target;
                }
            }

            public override bool Equals(object obj)
            {
                if (obj == this)
                    return true;
                if (obj == null)
                    return false;

                HashedWeakReference<T> other = obj as HashedWeakReference<T>;
                if (other == null)
                    return false;

                if (GetHashCode() != other.GetHashCode())
                    return false;

                return EqualityComparer<T>.Default.Equals(Target, other.Target);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
#endif

        private sealed class TimerState
        {
            private readonly WeakReference _cts = new WeakReference(null);

            public TimerState(CancellationTokenSource cancellationTokenSource)
            {
                if (cancellationTokenSource == null)
                    throw new ArgumentNullException("cancellationTokenSource");

                CancellationTokenSource = cancellationTokenSource;
            }

            public CancellationTokenSource CancellationTokenSource
            {
                get
                {
                    return (CancellationTokenSource)_cts.Target;
                }

                private set
                {
                    _cts.Target = value;
                }
            }

            public Timer Timer
            {
                get;
                set;
            }
        }
#endif
    }
}
