// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
#if !NET40PLUS
    extern alias tpl;
#endif

    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Rackspace.Threading;
    using MemoryStream = System.IO.MemoryStream;
    using Stream = System.IO.Stream;

#if !NET40PLUS
    using tpl::System.Threading;
    using tpl::System.Threading.Tasks;
    using AggregateException = tpl::System.AggregateException;
#else
    using System.Threading;
    using System.Threading.Tasks;
#endif

    [TestClass]
    public class TestWebRequestExtensions
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetRequestStreamAsync_NullRequest()
        {
            WebRequestExtensions.GetRequestStreamAsync(null);
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestGetRequestStreamAsync()
        {
            WebRequest request = WebRequest.CreateHttp("http://httpbin.org/post");
            request.Method = "POST";

            string sampleData = "Sample Data";
            byte[] buffer = Encoding.UTF8.GetBytes(sampleData);
            MemoryStream outputStream = new MemoryStream();

            Task testTask =
                WebRequestExtensions.GetRequestStreamAsync(request)
                .Then(task => StreamExtensions.WriteAsync(task.Result, buffer, 0, buffer.Length, CancellationToken.None))
                .Then(task => WebRequestExtensions.GetResponseAsync(request))
                .Then(task => StreamExtensions.CopyToAsync(task.Result.GetResponseStream(), outputStream));

            testTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, testTask.Status);
            Console.Error.WriteLine(Encoding.UTF8.GetString(outputStream.GetBuffer()));

            PostData postData = JsonConvert.DeserializeObject<PostData>(Encoding.UTF8.GetString(outputStream.GetBuffer()));
            Assert.AreEqual(sampleData, postData.data);
            Assert.AreEqual(request.RequestUri.OriginalString, postData.url);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetResponseAsync1_NullRequest()
        {
            WebRequestExtensions.GetResponseAsync(null);
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestGetResponseAsync1()
        {
            WebRequest request = WebRequest.CreateHttp("http://httpbin.org/get");
            request.Method = "GET";

            MemoryStream outputStream = new MemoryStream();

            Task testTask =
                WebRequestExtensions.GetResponseAsync(request)
                .Then(task => StreamExtensions.CopyToAsync(task.Result.GetResponseStream(), outputStream));

            testTask.Wait();
            Assert.AreEqual(TaskStatus.RanToCompletion, testTask.Status);
            Console.Error.WriteLine(Encoding.UTF8.GetString(outputStream.GetBuffer()));

            GetData getData = JsonConvert.DeserializeObject<GetData>(Encoding.UTF8.GetString(outputStream.GetBuffer()));
            Assert.AreEqual(request.RequestUri.OriginalString, getData.url);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetResponseAsync2_NullRequest()
        {
            WebRequestExtensions.GetResponseAsync(null, CancellationToken.None);
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestGetResponseAsync2()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(2));

                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/get");
                request.Method = "GET";

                MemoryStream outputStream = new MemoryStream();

                Task testTask =
                    WebRequestExtensions.GetResponseAsync(request, cts.Token)
                    .Then(task => CopyToAsync(task.Result.GetResponseStream(), outputStream));

                testTask.Wait();
                Assert.AreEqual(TaskStatus.RanToCompletion, testTask.Status);
                Console.Error.WriteLine(Encoding.UTF8.GetString(outputStream.GetBuffer()));

                GetData getData = JsonConvert.DeserializeObject<GetData>(Encoding.UTF8.GetString(outputStream.GetBuffer()));
                Assert.AreEqual(request.RequestUri.OriginalString, getData.url);
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestGetResponseAsync2_WebRequestTimeout()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/delay/6");
                request.Timeout = 1000;
                request.Method = "GET";

                Task<WebResponse> responseTask = null;

                try
                {
                    responseTask = WebRequestExtensions.GetResponseAsync(request, cts.Token);
                    responseTask.Wait();
                    Assert.Fail("Expected a WebException wrapped in an AggregateException");
                }
                catch (AggregateException ex)
                {
                    Assert.IsNotNull(responseTask);
                    Assert.AreEqual(TaskStatus.Faulted, responseTask.Status);
                    Assert.AreEqual(1, ex.InnerExceptions.Count);
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(WebException));

                    WebException webException = (WebException)ex.InnerExceptions[0];
                    Assert.AreEqual(WebExceptionStatus.Timeout, webException.Status);
                }
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestGetResponseAsync2_Timeout()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(2));

                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/delay/6");
                request.Method = "GET";

                Task<WebResponse> responseTask = null;

                try
                {
                    responseTask = WebRequestExtensions.GetResponseAsync(request, cts.Token);
                    responseTask.Wait();
                    Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
                }
                catch (AggregateException ex)
                {
                    Assert.IsNotNull(responseTask);
                    Assert.AreEqual(TaskStatus.Canceled, responseTask.Status);
                    Assert.AreEqual(1, ex.InnerExceptions.Count);
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetResponseAsync3_NullRequest_NoThrow()
        {
            WebRequestExtensions.GetResponseAsync(null, false, CancellationToken.None);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetResponseAsync3_NullRequest_Throw()
        {
            WebRequestExtensions.GetResponseAsync(null, true, CancellationToken.None);
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestGetResponseAsync3_Success_NoThrow()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(2));

                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/get");
                request.Method = "GET";

                MemoryStream outputStream = new MemoryStream();

                Task testTask =
                    WebRequestExtensions.GetResponseAsync(request, false, cts.Token)
                    .Then(task => CopyToAsync(task.Result.GetResponseStream(), outputStream));

                testTask.Wait();
                Assert.AreEqual(TaskStatus.RanToCompletion, testTask.Status);
                Console.Error.WriteLine(Encoding.UTF8.GetString(outputStream.GetBuffer()));

                GetData getData = JsonConvert.DeserializeObject<GetData>(Encoding.UTF8.GetString(outputStream.GetBuffer()));
                Assert.AreEqual(request.RequestUri.OriginalString, getData.url);
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestGetResponseAsync3_Success_Throw()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(2));

                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/get");
                request.Method = "GET";

                MemoryStream outputStream = new MemoryStream();

                Task testTask =
                    WebRequestExtensions.GetResponseAsync(request, true, cts.Token)
                    .Then(task => CopyToAsync(task.Result.GetResponseStream(), outputStream));

                testTask.Wait();
                Assert.AreEqual(TaskStatus.RanToCompletion, testTask.Status);
                Console.Error.WriteLine(Encoding.UTF8.GetString(outputStream.GetBuffer()));

                GetData getData = JsonConvert.DeserializeObject<GetData>(Encoding.UTF8.GetString(outputStream.GetBuffer()));
                Assert.AreEqual(request.RequestUri.OriginalString, getData.url);
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestGetResponseAsync3_Error_NoThrow()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(2));

                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/status/404");
                request.Method = "GET";

                WebResponse response = null;
                MemoryStream outputStream = new MemoryStream();

                Task testTask =
                    WebRequestExtensions.GetResponseAsync(request, false, cts.Token)
                    .Then(task =>
                        {
                            response = task.Result;
                            return CopyToAsync(task.Result.GetResponseStream(), outputStream);
                        });

                testTask.Wait();
                Assert.AreEqual(TaskStatus.RanToCompletion, testTask.Status);
                Console.Error.WriteLine(Encoding.UTF8.GetString(outputStream.GetBuffer()));

                Assert.IsInstanceOfType(response, typeof(HttpWebResponse));
                Assert.AreEqual(HttpStatusCode.NotFound, ((HttpWebResponse)response).StatusCode);
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestGetResponseAsync3_Error_Throw()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(2));

                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/status/404");
                request.Method = "GET";

                MemoryStream outputStream = new MemoryStream();

                Task testTask = null;

                try
                {
                    testTask =
                        WebRequestExtensions.GetResponseAsync(request, true, cts.Token)
                        .Then(task => CopyToAsync(task.Result.GetResponseStream(), outputStream));

                    testTask.Wait();
                    Assert.Fail("Expected a WebException wrapped in an AggregateException");
                }
                catch (AggregateException ex)
                {
                    Assert.IsNotNull(testTask);
                    Assert.AreEqual(TaskStatus.Faulted, testTask.Status);
                    Assert.AreEqual(1, ex.InnerExceptions.Count);
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(WebException));

                    WebException webException = (WebException)ex.InnerExceptions[0];
                    Assert.IsInstanceOfType(webException.Response, typeof(HttpWebResponse));

                    HttpWebResponse response = (HttpWebResponse)webException.Response;
                    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                }
            }
        }

        [TestMethod]
        public void TestGetResponseAsync3_WebRequestTimeout_NoThrow()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/delay/6");
                request.Timeout = 1000;
                request.Method = "GET";

                Task<WebResponse> responseTask = null;

                try
                {
                    responseTask = WebRequestExtensions.GetResponseAsync(request, false, cts.Token);
                    responseTask.Wait();
                    Assert.Fail("Expected a WebException wrapped in an AggregateException");
                }
                catch (AggregateException ex)
                {
                    Assert.IsNotNull(responseTask);
                    Assert.AreEqual(TaskStatus.Faulted, responseTask.Status);
                    Assert.AreEqual(1, ex.InnerExceptions.Count);
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(WebException));

                    WebException webException = (WebException)ex.InnerExceptions[0];
                    Assert.AreEqual(WebExceptionStatus.Timeout, webException.Status);
                }
            }
        }

        [TestMethod]
        [Timeout(10000)]
        public void TestGetResponseAsync3_WebRequestTimeout_Throw()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/delay/6");
                request.Timeout = 1000;
                request.Method = "GET";

                Task<WebResponse> responseTask = null;

                try
                {
                    responseTask = WebRequestExtensions.GetResponseAsync(request, true, cts.Token);
                    responseTask.Wait();
                    Assert.Fail("Expected a WebException wrapped in an AggregateException");
                }
                catch (AggregateException ex)
                {
                    Assert.IsNotNull(responseTask);
                    Assert.AreEqual(TaskStatus.Faulted, responseTask.Status);
                    Assert.AreEqual(1, ex.InnerExceptions.Count);
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(WebException));

                    WebException webException = (WebException)ex.InnerExceptions[0];
                    Assert.AreEqual(WebExceptionStatus.Timeout, webException.Status);
                }
            }
        }

        [TestMethod]
        public void TestGetResponseAsync3_Timeout_NoThrow()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(2));

                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/delay/6");
                request.Method = "GET";

                Task<WebResponse> responseTask = null;

                try
                {
                    responseTask = WebRequestExtensions.GetResponseAsync(request, false, cts.Token);
                    responseTask.Wait();
                    Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
                }
                catch (AggregateException ex)
                {
                    Assert.IsNotNull(responseTask);
                    Assert.AreEqual(TaskStatus.Canceled, responseTask.Status);
                    Assert.AreEqual(1, ex.InnerExceptions.Count);
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                }
            }
        }

        [TestMethod]
        public void TestGetResponseAsync3_Timeout_Throw()
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(2));

                WebRequest request = WebRequest.CreateHttp("http://httpbin.org/delay/6");
                request.Method = "GET";

                Task<WebResponse> responseTask = null;

                try
                {
                    responseTask = WebRequestExtensions.GetResponseAsync(request, true, cts.Token);
                    responseTask.Wait();
                    Assert.Fail("Expected a TaskCanceledException wrapped in an AggregateException");
                }
                catch (AggregateException ex)
                {
                    Assert.IsNotNull(responseTask);
                    Assert.AreEqual(TaskStatus.Canceled, responseTask.Status);
                    Assert.AreEqual(1, ex.InnerExceptions.Count);
                    Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));
                }
            }
        }

        private static Task CopyToAsync(Stream stream, Stream destination)
        {
#if NET45PLUS
            return stream.CopyToAsync(destination);
#else
            return StreamExtensions.CopyToAsync(stream, destination);
#endif
        }

        private class GetData
        {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value null
            public JToken args;
            public Dictionary<string, string> headers;
            public string origin;
            public string url;
#pragma warning restore 649
        }

        private class PostData
        {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value null
            public JToken args;
            public string data;
            public JToken files;
            public JToken form;
            public Dictionary<string, string> headers;
            public JToken json;
            public string origin;
            public string url;
#pragma warning restore 649
        }
    }
}
