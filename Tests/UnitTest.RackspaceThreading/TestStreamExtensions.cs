namespace UnitTest.RackspaceThreading
{
#if !NET40PLUS
    extern alias tpl;
#endif

    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

#if !NET40PLUS
    using tpl::System.Threading;
    using tpl::System.Threading.Tasks;
#else
    using System.Threading;
    using System.Threading.Tasks;
#endif

    [TestClass]
    public class TestStreamExtensions
    {
        #region CopyToAsync 1

        [TestMethod]
        public void TestCopyToAsync1_NullStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = null;
            Stream destination = new MemoryStream();

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.CopyToAsync(stream, destination))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestCopyToAsync1_NullDestination()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            Stream destination = null;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.CopyToAsync(stream, destination))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestCopyToAsync1_EmptyStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            Stream destination = new MemoryStream();

            StreamExtensions.CopyToAsync(stream, destination).Wait();
            Assert.AreEqual(0, stream.Length);
            Assert.AreEqual(0, destination.Length);
        }

        #endregion

        #region CopyToAsync 2

        [TestMethod]
        public void TestCopyToAsync2_NullStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = null;
            Stream destination = new MemoryStream();
            int bufferSize = 81920;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.CopyToAsync(stream, destination, bufferSize))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestCopyToAsync2_NullDestination()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            Stream destination = null;
            int bufferSize = 81920;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.CopyToAsync(stream, destination, bufferSize))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestCopyToAsync2_EmptyStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            Stream destination = new MemoryStream();
            int bufferSize = 81920;

            StreamExtensions.CopyToAsync(stream, destination, bufferSize).Wait();
            Assert.AreEqual(0, stream.Length);
            Assert.AreEqual(0, destination.Length);
        }

        #endregion

        #region CopyToAsync 3

        [TestMethod]
        public void TestCopyToAsync3_NullStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = null;
            Stream destination = new MemoryStream();
            int bufferSize = 81920;
            CancellationToken cancellationToken = CancellationToken.None;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.CopyToAsync(stream, destination, bufferSize, cancellationToken))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestCopyToAsync3_NullDestination()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            Stream destination = null;
            int bufferSize = 81920;
            CancellationToken cancellationToken = CancellationToken.None;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.CopyToAsync(stream, destination, bufferSize, cancellationToken))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestCopyToAsync3_PreCanceled()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            Stream destination = new MemoryStream();
            int bufferSize = 81920;

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            CancellationToken cancellationToken = cts.Token;

            Task task = StreamExtensions.CopyToAsync(stream, destination, bufferSize, cancellationToken);
            Assert.AreEqual(TaskStatus.Canceled, task.Status);
        }

        [TestMethod]
        public void TestCopyToAsync3_EmptyStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            Stream destination = new MemoryStream();
            int bufferSize = 81920;
            CancellationToken cancellationToken = CancellationToken.None;

            StreamExtensions.CopyToAsync(stream, destination, bufferSize, cancellationToken).Wait();
            Assert.AreEqual(0, stream.Length);
            Assert.AreEqual(0, destination.Length);
        }

        #endregion

        #region FlushAsync 1

        [TestMethod]
        public void TestFlushAsync1_NullStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = null;
            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.FlushAsync(stream))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestFlushAsync1_EmptyStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();

            StreamExtensions.FlushAsync(stream).Wait();
            Assert.AreEqual(0, stream.Length);
        }

        #endregion

        #region FlushAsync 2

        [TestMethod]
        public void TestFlushAsync2_NullStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = null;
            CancellationToken cancellationToken = CancellationToken.None;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.FlushAsync(stream, cancellationToken))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestFlushAsync2_PreCanceled()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            CancellationToken cancellationToken = cts.Token;

            Task task = StreamExtensions.FlushAsync(stream, cancellationToken);
            Assert.AreEqual(TaskStatus.Canceled, task.Status);
        }

        [TestMethod]
        public void TestFlushAsync2_EmptyStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            CancellationToken cancellationToken = CancellationToken.None;

            StreamExtensions.FlushAsync(stream, cancellationToken).Wait();
            Assert.AreEqual(0, stream.Length);
        }

        #endregion

        #region ReadAsync 1

        [TestMethod]
        public void TestReadAsync1_NullStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = null;
            byte[] buffer = new byte[81920];
            int offset = 0;
            int count = Buffer.ByteLength(buffer);

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.ReadAsync(stream, buffer, offset, count))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestReadAsync1_NullBuffer()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            byte[] buffer = null;
            int offset = 0;
            int count = 81920;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.ReadAsync(stream, buffer, offset, count))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestReadAsync1_EmptyStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            byte[] buffer = new byte[81920];
            int offset = 0;
            int count = buffer.Length;

            int actual = StreamExtensions.ReadAsync(stream, buffer, offset, count).Result;
            Assert.AreEqual(0, actual);
        }

        #endregion

        #region ReadAsync 2

        [TestMethod]
        public void TestReadAsync2_NullStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = null;
            byte[] buffer = new byte[81920];
            int offset = 0;
            int count = Buffer.ByteLength(buffer);
            CancellationToken cancellationToken = CancellationToken.None;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.ReadAsync(stream, buffer, offset, count, cancellationToken))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestReadAsync2_NullBuffer()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            byte[] buffer = null;
            int offset = 0;
            int count = 81920;
            CancellationToken cancellationToken = CancellationToken.None;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.ReadAsync(stream, buffer, offset, count, cancellationToken))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestReadAsync2_PreCanceled()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            byte[] buffer = new byte[81920];
            int offset = 0;
            int count = buffer.Length;

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            CancellationToken cancellationToken = cts.Token;

            Task task = StreamExtensions.ReadAsync(stream, buffer, offset, count, cancellationToken);
            Assert.AreEqual(TaskStatus.Canceled, task.Status);
        }

        [TestMethod]
        public void TestReadAsync2_EmptyStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            byte[] buffer = new byte[81920];
            int offset = 0;
            int count = buffer.Length;
            CancellationToken cancellationToken = CancellationToken.None;

            int actual = StreamExtensions.ReadAsync(stream, buffer, offset, count, cancellationToken).Result;
            Assert.AreEqual(0, actual);
        }

        #endregion

        #region WriteAsync 1

        [TestMethod]
        public void TestWriteAsync1_NullStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = null;
            byte[] buffer = new byte[81920];
            int offset = 0;
            int count = Buffer.ByteLength(buffer);

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.WriteAsync(stream, buffer, offset, count))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestWriteAsync1_NullBuffer()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            byte[] buffer = null;
            int offset = 0;
            int count = 81920;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.WriteAsync(stream, buffer, offset, count))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestWriteAsync1_EmptyBuffer()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            byte[] buffer = new byte[81920];
            int offset = 0;
            int count = 0;

            StreamExtensions.WriteAsync(stream, buffer, offset, count).Wait();
            Assert.AreEqual(0, stream.Length);
        }

        #endregion

        #region WriteAsync 2

        [TestMethod]
        public void TestWriteAsync2_NullStream()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = null;
            byte[] buffer = new byte[81920];
            int offset = 0;
            int count = Buffer.ByteLength(buffer);
            CancellationToken cancellationToken = CancellationToken.None;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.WriteAsync(stream, buffer, offset, count, cancellationToken))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestWriteAsync2_NullBuffer()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            byte[] buffer = null;
            int offset = 0;
            int count = 81920;
            CancellationToken cancellationToken = CancellationToken.None;

            bool hasException = false;
            CompletedTask.Default.Then(_ => StreamExtensions.WriteAsync(stream, buffer, offset, count, cancellationToken))
                .Catch<ArgumentNullException>((task, e) => hasException = true)
                .Wait();

            Assert.IsTrue(hasException);
        }

        [TestMethod]
        public void TestWriteAsync2_PreCanceled()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            byte[] buffer = new byte[81920];
            int offset = 0;
            int count = 0;

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            CancellationToken cancellationToken = cts.Token;

            Task task = StreamExtensions.WriteAsync(stream, buffer, offset, count, cancellationToken);
            Assert.AreEqual(TaskStatus.Canceled, task.Status);
        }

        [TestMethod]
        public void TestWriteAsync2_EmptyBuffer()
        {
            // declaring these makes it clear we are testing the correct overload
            Stream stream = new MemoryStream();
            byte[] buffer = new byte[81920];
            int offset = 0;
            int count = 0;
            CancellationToken cancellationToken = CancellationToken.None;

            StreamExtensions.WriteAsync(stream, buffer, offset, count, cancellationToken).Wait();
            Assert.AreEqual(0, stream.Length);
        }

        #endregion
    }
}
