// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace CSharpSamples
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;
    using StringBuilder = System.Text.StringBuilder;
    using StringWriter = System.IO.StringWriter;

    /// <summary>
    /// This class contains unit-tested example code for the <see cref="TaskBlocks.Using{TResource}(Func{Task{TResource}}, Func{Task{TResource}, Task})"/>
    /// building block method.
    /// </summary>
    [TestClass]
    public class TaskBlockUsing
    {
        private const string SampleText = "Text to read";

        [TestMethod]
        public async Task TestUsingAsyncAwait()
        {
            StringBuilder builder = new StringBuilder();
            await UsingAsyncAwait(builder);
            Assert.AreEqual(SampleText, builder.ToString());
        }

        [TestMethod]
        public async Task TestUsingAsyncBuildingBlock()
        {
            StringBuilder builder = new StringBuilder();
            await Using(builder);
            Assert.AreEqual(SampleText, builder.ToString());
        }

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously....
        #region UsingAsyncAwait
        public async Task UsingAsyncAwait(StringBuilder builder)
        {
            using (StringWriter resource = await AcquireResourceAsyncAwait(builder))
            {
                await resource.WriteAsync(SampleText);
            }
        }

        private async Task<StringWriter> AcquireResourceAsyncAwait(StringBuilder builder)
        {
            // this would generally contain an asynchronous call
            return new StringWriter(builder);
        }
        #endregion
#pragma warning restore 1998

        #region UsingAsyncBuildingBlock
        public Task Using(StringBuilder builder)
        {
            return TaskBlocks.Using(
                () => AcquireResourceAsync(builder),
                task => task.Result.WriteAsync(SampleText));
        }

        private Task<StringWriter> AcquireResourceAsync(StringBuilder builder)
        {
            // this would generally contain an asynchronous call
            return CompletedTask.FromResult(new StringWriter(builder));
        }
        #endregion
    }
}
