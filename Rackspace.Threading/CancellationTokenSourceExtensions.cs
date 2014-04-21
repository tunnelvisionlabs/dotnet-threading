// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if !NET45PLUS

namespace Rackspace.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Provides extension methods for the <see cref="CancellationTokenSource"/> class.
    /// </summary>
    public static class CancellationTokenSourceExtensions
    {
        /// <summary>
        /// This static set prevents <see cref="Timer"/> instances from being prematurely garbage collected.
        /// </summary>
        private static readonly HashSet<Timer> _timers = new HashSet<Timer>();

        /// <summary>
        /// Schedules a <see cref="CancellationTokenSource.Cancel()"/> operation on a <see cref="CancellationTokenSource"/>
        /// after the specified time span.
        /// </summary>
        /// <param name="cts">The <see cref="CancellationTokenSource"/> to cancel after a delay.</param>
        /// <param name="delay">The time span to wait before canceling the <see cref="CancellationTokenSource"/>.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="cts"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException">If <paramref name="cts"/> has been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="delay"/> is less than -1 (milliseconds) or greater than <see cref="int.MaxValue"/>.</exception>
        public static void CancelAfter(this CancellationTokenSource cts, TimeSpan delay)
        {
            if (cts == null)
                throw new ArgumentNullException("cts");

            TimerState state = new TimerState(cts);
            Timer timer = new Timer(TimeElapsed, state, delay, TimeSpan.FromMilliseconds(-1));
            state.Timer = timer;
            lock (_timers)
            {
                _timers.Add(timer);
            }
        }

        private static void TimeElapsed(object state)
        {
            TimerState timerState = (TimerState)state;
            lock (_timers)
            {
                _timers.Remove(timerState.Timer);
            }

            timerState.Timer.Dispose();
            try
            {
                timerState.CancellationTokenSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private sealed class TimerState
        {
            public TimerState(CancellationTokenSource cancellationTokenSource)
            {
                if (cancellationTokenSource == null)
                    throw new ArgumentNullException("cancellationTokenSource");

                CancellationTokenSource = cancellationTokenSource;
            }

            public CancellationTokenSource CancellationTokenSource
            {
                get;
                private set;
            }

            public Timer Timer
            {
                get;
                set;
            }
        }
    }
}

#endif
