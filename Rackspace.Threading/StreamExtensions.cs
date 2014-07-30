// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if !NET45PLUS

namespace Rackspace.Threading
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides extension methods for the <see cref="Stream"/> class.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public static class StreamExtensions
    {
        /// <summary>
        /// Asynchronously reads the bytes from a source stream and writes them to a destination stream.
        /// </summary>
        /// <remarks>
        /// Copying begins at the current position in <paramref name="stream"/>.
        /// </remarks>
        /// <param name="stream">The source stream.</param>
        /// <param name="destination">The stream to which the contents of the source stream will be copied.</param>
        /// <returns>A task that represents the asynchronous copy operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stream"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="destination"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// If <paramref name="stream"/> is disposed.
        /// <para>-or-</para>
        /// <para>If <paramref name="destination"/> is disposed.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If <paramref name="stream"/> does not support reading.
        /// <para>-or-</para>
        /// <para>If <paramref name="destination"/> does not support writing.</para>
        /// </exception>
        public static Task CopyToAsync(this Stream stream, Stream destination)
        {
            return CopyToAsync(stream, destination, 16 * 1024, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously reads the bytes from a source stream and writes them to a destination stream,
        /// using a specified buffer size.
        /// </summary>
        /// <remarks>
        /// Copying begins at the current position in <paramref name="stream"/>.
        /// </remarks>
        /// <param name="stream">The source stream.</param>
        /// <param name="destination">The stream to which the contents of the source stream will be copied.</param>
        /// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The default size is 81920.</param>
        /// <returns>A task that represents the asynchronous copy operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stream"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="destination"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="bufferSize"/> is negative or zero.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// If <paramref name="stream"/> is disposed.
        /// <para>-or-</para>
        /// <para>If <paramref name="destination"/> is disposed.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If <paramref name="stream"/> does not support reading.
        /// <para>-or-</para>
        /// <para>If <paramref name="destination"/> does not support writing.</para>
        /// </exception>
        public static Task CopyToAsync(this Stream stream, Stream destination, int bufferSize)
        {
            return CopyToAsync(stream, destination, bufferSize, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously reads the bytes from a source stream and writes them to a destination stream,
        /// using a specified buffer size and cancellation token.
        /// </summary>
        /// <remarks>
        /// If the operation is canceled before it completes, the returned task contains the <see cref="TaskStatus.Canceled"/>
        /// value for the <see cref="Task.Status"/> property.
        /// <para>
        /// Copying begins at the current position in <paramref name="stream"/>.
        /// </para>
        /// </remarks>
        /// <param name="stream">The source stream.</param>
        /// <param name="destination">The stream to which the contents of the source stream will be copied.</param>
        /// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The default size is 81920.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous copy operation.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stream"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="destination"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="bufferSize"/> is negative or zero.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// If <paramref name="stream"/> is disposed.
        /// <para>-or-</para>
        /// <para>If <paramref name="destination"/> is disposed.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// If <paramref name="stream"/> does not support reading.
        /// <para>-or-</para>
        /// <para>If <paramref name="destination"/> does not support writing.</para>
        /// </exception>
        public static Task CopyToAsync(this Stream stream, Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (!stream.CanRead)
                throw new NotSupportedException("The stream does not support reading");
            if (!destination.CanWrite)
                throw new NotSupportedException("The destination does not support writing");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize");

            if (cancellationToken.IsCancellationRequested)
                return CompletedTask.Canceled();

            return CopyToAsync(stream, destination, new byte[bufferSize], cancellationToken);
        }

        private static Task CopyToAsync(Stream stream, Stream destination, byte[] buffer, CancellationToken cancellationToken)
        {
            int nread = 0;
            Func<Task<bool>> condition =
                () => stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
                    .Select(task => (nread = task.Result) != 0);
            Func<Task> body = () => destination.WriteAsync(buffer, 0, nread, cancellationToken);

            return TaskBlocks.While(condition, body);
        }

        /// <summary>
        /// Asynchronously clears all buffers for a stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <remarks>
        /// If a derived class does not flush the buffer in its implementation of the <see cref="Stream.Flush()"/> method,
        /// the <see cref="FlushAsync(Stream)"/> method will not flush the buffer.
        /// </remarks>
        /// <param name="stream">The stream to flush.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException">If <paramref name="stream"/> has been disposed.</exception>
        public static Task FlushAsync(this Stream stream)
        {
            return FlushAsync(stream, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously clears all buffers for a stream and causes any buffered data to be written to the underlying device,
        /// and monitors cancellation requests.
        /// </summary>
        /// <remarks>
        /// If the operation is canceled before it completes, the returned task contains the <see cref="TaskStatus.Canceled"/>
        /// value for the <see cref="Task.Status"/> property.
        /// <para>
        /// If a derived class does not flush the buffer in its implementation of the <see cref="Stream.Flush()"/> method,
        /// the <see cref="FlushAsync(Stream)"/> method will not flush the buffer.
        /// </para>
        /// </remarks>
        /// <param name="stream">The stream to flush.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException">If <paramref name="stream"/> has been disposed.</exception>
        public static Task FlushAsync(this Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (cancellationToken.IsCancellationRequested)
                return CompletedTask.Canceled();

            return Task.Factory.StartNew(state => ((Stream)state).Flush(), stream, cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from a stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="Stream.CanRead"/> property to determine whether the stream instance supports reading.
        /// </remarks>
        /// <param name="stream">The stream to read data from.</param>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/>
        /// property contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if
        /// the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stream"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="buffer"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="offset"/> is negative.
        /// <para>-or-</para>
        /// <para>If <paramref name="count"/> is negative.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
        /// </exception>
        /// <exception cref="NotSupportedException">If <paramref name="stream"/> does not support reading.</exception>
        /// <exception cref="ObjectDisposedException">If <paramref name="stream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="stream"/> is currently in use by a previous read operation.</exception>
        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            return ReadAsync(stream, buffer, offset, count, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from a stream, advances the position within the stream by the number of bytes read,
        /// and monitors cancellation requests.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="Stream.CanRead"/> property to determine whether the stream instance supports reading.
        /// <para>
        /// If the operation is canceled before it completes, the returned task contains the <see cref="TaskStatus.Canceled"/>
        /// value for the <see cref="Task.Status"/> property.
        /// </para>
        /// </remarks>
        /// <param name="stream">The stream to read data from.</param>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer"/> at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. When the task completes successfully, the <see cref="Task{TResult}.Result"/>
        /// property contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if
        /// the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stream"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="buffer"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="offset"/> is negative.
        /// <para>-or-</para>
        /// <para>If <paramref name="count"/> is negative.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
        /// </exception>
        /// <exception cref="NotSupportedException">If <paramref name="stream"/> does not support reading.</exception>
        /// <exception cref="ObjectDisposedException">If <paramref name="stream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="stream"/> is currently in use by a previous read operation.</exception>
        public static Task<int> ReadAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (cancellationToken.IsCancellationRequested)
                return CompletedTask.Canceled<int>();

            return Task<int>.Factory.FromAsync(stream.BeginRead, stream.EndRead, buffer, offset, count, null);
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to a stream and advances the position within the stream by the number of bytes written.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="Stream.CanWrite"/> property to determine whether the stream instance supports writing.
        /// </remarks>
        /// <param name="stream">The stream to write data to.</param>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The zero-based byte offset in buffer from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <returns>
        /// A task that represents the asynchronous write operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stream"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="buffer"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="offset"/> is negative.
        /// <para>-or-</para>
        /// <para>If <paramref name="count"/> is negative.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
        /// </exception>
        /// <exception cref="NotSupportedException">If <paramref name="stream"/> does not support writing.</exception>
        /// <exception cref="ObjectDisposedException">If <paramref name="stream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="stream"/> is currently in use by a previous write operation.</exception>
        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count)
        {
            return WriteAsync(stream, buffer, offset, count, CancellationToken.None);
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to a stream, advances the position within the stream by the number of bytes written,
        /// and monitors cancellation requests.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="Stream.CanWrite"/> property to determine whether the stream instance supports writing.
        /// <para>
        /// If the operation is canceled before it completes, the returned task contains the <see cref="TaskStatus.Canceled"/>
        /// value for the <see cref="Task.Status"/> property.
        /// </para>
        /// </remarks>
        /// <param name="stream">The stream to write data to.</param>
        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="offset">The zero-based byte offset in buffer from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>
        /// A task that represents the asynchronous write operation.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stream"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="buffer"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="offset"/> is negative.
        /// <para>-or-</para>
        /// <para>If <paramref name="count"/> is negative.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
        /// </exception>
        /// <exception cref="NotSupportedException">If <paramref name="stream"/> does not support writing.</exception>
        /// <exception cref="ObjectDisposedException">If <paramref name="stream"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">If <paramref name="stream"/> is currently in use by a previous write operation.</exception>
        public static Task WriteAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.Factory.FromAsync(stream.BeginWrite, stream.EndWrite, buffer, offset, count, null);
        }
    }
}

#endif
