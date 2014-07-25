// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace CSharpSamples
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

    /// <summary>
    /// This class contains unit-tested example code for the <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>
    /// building block method.
    /// </summary>
    [TestClass]
    public class TaskBlockWhileAsync
    {
        [TestMethod]
        public async Task TestWhileAsyncAwait()
        {
            await WhileAsyncAwait();
        }

        [TestMethod]
        public async Task TestWhile()
        {
            await While();
        }

#pragma warning disable 1998 // This async method lacks 'await' operators and will run synchronously....
        #region WhileAsyncAwait
        public async Task WhileAsyncAwait()
        {
            Stack<int> workList = new Stack<int>(new[] { 3, 5, 7 });
            while (workList.Count > 0)
            {
                await ProcessWorkItemAsyncAwait(workList.Pop());
            }
        }

        private async Task ProcessWorkItemAsyncAwait(int item)
        {
            // this would typically perform an asynchronous operation on the
            // work item
        }
        #endregion
#pragma warning restore 1998

        #region WhileAsyncBuildingBlock
        public Task While()
        {
            Stack<int> workList = new Stack<int>(new[] { 3, 5, 7 });
            return TaskBlocks.While(
                () => workList.Count > 0,
                () => ProcessWorkItemAsync(workList.Pop()));
        }

        private Task ProcessWorkItemAsync(int item)
        {
            // this would typically perform an asynchronous operation on the
            // work item
            return CompletedTask.Default;
        }
        #endregion
    }
}
