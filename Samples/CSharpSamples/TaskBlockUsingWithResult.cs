// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace CSharpSamples
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;
    using StringReader = System.IO.StringReader;

    /// <summary>
    /// This class contains unit-tested example code for the <see cref="TaskBlocks.Using{TResource, TResult}(Func{Task{TResource}}, Func{Task{TResource}, Task{TResult}})"/>
    /// building block method.
    /// </summary>
    [TestClass]
    public class TaskBlockUsingWithResult
    {
        private const string SampleText = "Text to read";

        [TestMethod]
        public async Task TestUsingWithResultAsyncAwait()
        {
            string text = await UsingWithResultAsyncAwait();
            Assert.AreEqual(SampleText, text);
        }

        [TestMethod]
        public async Task TestUsingWithResultAsyncBuildingBlock()
        {
            string text = await UsingWithResult();
            Assert.AreEqual(SampleText, text);
        }

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously....
        #region UsingWithResultAsyncAwait
        public async Task<string> UsingWithResultAsyncAwait()
        {
            using (StringReader resource = await AcquireResourceAsyncAwait())
            {
                return await resource.ReadToEndAsync();
            }
        }

        private async Task<StringReader> AcquireResourceAsyncAwait()
        {
            // this would generally contain an asynchronous call
            return new StringReader("Text to read");
        }
        #endregion
#pragma warning restore 1998

        #region UsingWithResultAsyncBuildingBlock
        public Task<string> UsingWithResult()
        {
            return TaskBlocks.Using(
                () => AcquireResourceAsync(),
                task => task.Result.ReadToEndAsync());
        }

        private Task<StringReader> AcquireResourceAsync()
        {
            // this would generally contain an asynchronous call
            return CompletedTask.FromResult(new StringReader("Text to read"));
        }
        #endregion
    }
}
