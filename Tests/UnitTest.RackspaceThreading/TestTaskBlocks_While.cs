// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace UnitTest.RackspaceThreading
{
#if !NET40PLUS
    extern alias tpl;
#endif

    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rackspace.Threading;

#if !NET40PLUS
    using tpl::System.Threading;
    using tpl::System.Threading.Tasks;
    using AggregateException = tpl::System.AggregateException;
#else
    using System.Threading;
    using System.Threading.Tasks;
#endif

    [TestClass]
    public class TestTaskBlocks_While
    {
        #region While 1

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhile1_NullConditionFunction()
        {
            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = null;
            Func<Task> body = new WhileBody().ExecuteAsync;

            TaskBlocks.While(condition, body);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhile1_NullBodyFunction()
        {
            WhileCondition whileCondition = new WhileCondition(0);

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = null;

            try
            {
                TaskBlocks.While(condition, body);
            }
            catch
            {
                Assert.AreEqual(0, whileCondition.EvaluateCount);
                throw;
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop0_Success()
        {
            WhileCondition whileCondition = new WhileCondition(0);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = TaskBlocks.While(condition, body);
            whileTask.Wait();

            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_Success()
        {
            WhileCondition whileCondition = new WhileCondition(3);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = TaskBlocks.While(condition, body);
            whileTask.Wait();

            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop0_FaultedCondition()
        {
            WhileCondition whileCondition = new WhileCondition(0, DelegateBehavior.Faulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_FaultedCondition()
        {
            WhileCondition whileCondition = new WhileCondition(3, DelegateBehavior.Faulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_CanceledBody()
        {
            WhileCondition whileCondition = WhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.Canceled);

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Canceled, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_FaultedBody()
        {
            WhileCondition whileCondition = WhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.Faulted);

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileBody.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_SyncFaultedBody()
        {
            WhileCondition whileCondition = WhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.SyncFaulted);

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileBody.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions - 1, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{bool}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile1_Loop3_NullTaskBody()
        {
            WhileCondition whileCondition = WhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.NullTask);

            // declaring these makes it clear we are testing the correct overload
            Func<bool> condition = whileCondition.Evaluate;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(InvalidOperationException));

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions - 1, whileBody.ExecutionCount);
            }
        }

        #endregion

        #region While 1

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhile2_NullConditionFunction()
        {
            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = null;
            Func<Task> body = new WhileBody().ExecuteAsync;

            TaskBlocks.While(condition, body);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestWhile2_NullBodyFunction()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0);

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = null;

            try
            {
                TaskBlocks.While(condition, body);
            }
            catch
            {
                Assert.AreEqual(0, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(0, whileCondition.EvaluateAsyncCount);
                throw;
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop0_Success()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = TaskBlocks.While(condition, body);
            whileTask.Wait();

            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_Success()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(3);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = TaskBlocks.While(condition, body);
            whileTask.Wait();

            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
            Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
            Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop0_CanceledCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0, DelegateBehavior.Canceled);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Canceled, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_CanceledCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(3, DelegateBehavior.Canceled);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Canceled, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop0_FaultedCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0, DelegateBehavior.Faulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_FaultedCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(3, DelegateBehavior.Faulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop0_SyncFaultedCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0, DelegateBehavior.SyncFaulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_SyncFaultedCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(3, DelegateBehavior.SyncFaulted);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileCondition.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop0_NullTaskCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(0, DelegateBehavior.NullTask);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(InvalidOperationException));

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_NullTaskCondition()
        {
            AsyncWhileCondition whileCondition = new AsyncWhileCondition(3, DelegateBehavior.NullTask);
            WhileBody whileBody = new WhileBody();

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(InvalidOperationException));

                Assert.AreEqual(whileCondition.TotalIterations + 1, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileCondition.TotalIterations, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_CanceledBody()
        {
            AsyncWhileCondition whileCondition = AsyncWhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.Canceled);

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Canceled, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(TaskCanceledException));

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_FaultedBody()
        {
            AsyncWhileCondition whileCondition = AsyncWhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.Faulted);

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileBody.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_SyncFaultedBody()
        {
            AsyncWhileCondition whileCondition = AsyncWhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.SyncFaulted);

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreSame(whileBody.ExpectedException, ex.InnerExceptions[0]);

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions - 1, whileBody.ExecutionCount);
            }
        }

        /// <summary>
        /// This method tests the behavior of <see cref="TaskBlocks.While(Func{Task{bool}}, Func{Task})"/>.
        /// </summary>
        [TestMethod]
        public void TestWhile2_Loop3_NullTaskBody()
        {
            AsyncWhileCondition whileCondition = AsyncWhileCondition.True();
            WhileBody whileBody = new WhileBody(3, DelegateBehavior.NullTask);

            // declaring these makes it clear we are testing the correct overload
            Func<Task<bool>> condition = whileCondition.EvaluateAsync;
            Func<Task> body = whileBody.ExecuteAsync;

            Task whileTask = null;

            try
            {
                whileTask = TaskBlocks.While(condition, body);
                whileTask.Wait();
                Assert.Fail("Expected an exception");
            }
            catch (AggregateException ex)
            {
                Assert.IsNotNull(whileTask);
                Assert.AreEqual(TaskStatus.Faulted, whileTask.Status);
                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.IsInstanceOfType(ex.InnerExceptions[0], typeof(InvalidOperationException));

                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.EvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileCondition.SyncPartEvaluateAsyncCount);
                Assert.AreEqual(whileBody.MaxExecutions, whileBody.SyncPartExecutionCount);
                Assert.AreEqual(whileBody.MaxExecutions - 1, whileBody.ExecutionCount);
            }
        }

        #endregion

        private abstract class BaseWhileCondition : IDisposable
        {
            protected BaseWhileCondition(int totalIterations, DelegateBehavior behavior)
            {
                if (totalIterations < 0)
                    throw new ArgumentOutOfRangeException("totalIterations");

                TotalIterations = totalIterations;
                Behavior = behavior;
                if (behavior == DelegateBehavior.Faulted || behavior == DelegateBehavior.SyncFaulted)
                    ExpectedException = new ArithmeticException();
                else if (behavior == DelegateBehavior.Canceled)
                    CancellationTokenSource = new CancellationTokenSource();
            }

            public int TotalIterations
            {
                get;
                private set;
            }

            public DelegateBehavior Behavior
            {
                get;
                private set;
            }

            public int Iteration
            {
                get;
                private set;
            }

            public Exception ExpectedException
            {
                get;
                private set;
            }

            protected int EvaluateCoreCount
            {
                get;
                private set;
            }

            protected CancellationTokenSource CancellationTokenSource
            {
                get;
                private set;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected bool EvaluateCore()
            {
                EvaluateCoreCount++;

                if (Iteration == TotalIterations)
                    return false;

                Iteration++;
                return true;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    CancellationTokenSource cancellationTokenSource = this.CancellationTokenSource;
                    if (cancellationTokenSource != null)
                        cancellationTokenSource.Dispose();
                }
            }
        }

        private class WhileCondition : BaseWhileCondition
        {
            public WhileCondition(int totalIterations)
                : this(totalIterations, DelegateBehavior.Success)
            {
            }

            public WhileCondition(int totalIterations, DelegateBehavior behavior)
                : base(totalIterations, behavior)
            {
                if (behavior != DelegateBehavior.Success && behavior != DelegateBehavior.Faulted)
                    throw new ArgumentException("Unsupported behavior for while condition");
            }

            public int EvaluateCount
            {
                get
                {
                    return EvaluateCoreCount;
                }
            }

            public static WhileCondition True()
            {
                return new WhileCondition(int.MaxValue);
            }

            public static WhileCondition False()
            {
                return new WhileCondition(0);
            }

            public bool Evaluate()
            {
                bool result = EvaluateCore();
                if (!result && Behavior == DelegateBehavior.Faulted)
                    throw ExpectedException;

                return result;
            }
        }

        private class AsyncWhileCondition : BaseWhileCondition
        {
            public AsyncWhileCondition(int totalIterations)
                : this(totalIterations, DelegateBehavior.Success)
            {
            }

            public AsyncWhileCondition(int totalIterations, DelegateBehavior behavior)
                : base(totalIterations, behavior)
            {
            }

            public int SyncPartEvaluateAsyncCount
            {
                get;
                private set;
            }

            public int EvaluateAsyncCount
            {
                get;
                private set;
            }

            public static AsyncWhileCondition True()
            {
                return new AsyncWhileCondition(int.MaxValue);
            }

            public static AsyncWhileCondition False()
            {
                return new AsyncWhileCondition(0);
            }

            public Task<bool> EvaluateAsync()
            {
                SyncPartEvaluateAsyncCount++;

                if (Iteration == TotalIterations)
                {
                    if (Behavior == DelegateBehavior.SyncFaulted)
                        throw ExpectedException;
                    else if (Behavior == DelegateBehavior.NullTask)
                        return null;
                }

                CancellationToken cancellationToken = CancellationTokenSource != null ? CancellationTokenSource.Token : CancellationToken.None;
                return Task.Factory.StartNew(() =>
                    {
                        EvaluateAsyncCount++;

                        bool result = EvaluateCore();
                        if (!result)
                        {
                            if (Behavior == DelegateBehavior.Faulted)
                            {
                                throw ExpectedException;
                            }
                            else if (Behavior == DelegateBehavior.Canceled)
                            {
                                CancellationTokenSource.Cancel();
                                CancellationTokenSource.Token.ThrowIfCancellationRequested();
                            }
                        }

                        return result;
                    }, cancellationToken);
            }
        }

        private class WhileBody : IDisposable
        {
            public WhileBody()
                : this(int.MaxValue, DelegateBehavior.Success)
            {
            }

            public WhileBody(int maxExecutions, DelegateBehavior behavior)
            {
                MaxExecutions = maxExecutions;
                Behavior = behavior;
                if (behavior == DelegateBehavior.Faulted || behavior == DelegateBehavior.SyncFaulted)
                    ExpectedException = new FormatException();
                else if (behavior == DelegateBehavior.Canceled)
                    CancellationTokenSource = new CancellationTokenSource();
            }

            public int MaxExecutions
            {
                get;
                private set;
            }

            public DelegateBehavior Behavior
            {
                get;
                private set;
            }

            public int SyncPartExecutionCount
            {
                get;
                private set;
            }

            public int ExecutionCount
            {
                get;
                private set;
            }

            public CancellationTokenSource CancellationTokenSource
            {
                get;
                private set;
            }

            public Exception ExpectedException
            {
                get;
                private set;
            }

            public Task ExecuteAsync()
            {
                SyncPartExecutionCount++;

                if (ExecutionCount == MaxExecutions - 1)
                {
                    if (Behavior == DelegateBehavior.SyncFaulted)
                        throw ExpectedException;
                    else if (Behavior == DelegateBehavior.NullTask)
                        return null;
                }

                CancellationToken cancellationToken = CancellationTokenSource != null ? CancellationTokenSource.Token : CancellationToken.None;
                return Task.Factory.StartNew(() =>
                    {
                        ExecutionCount++;

                        if (ExecutionCount >= MaxExecutions)
                        {
                            if (Behavior == DelegateBehavior.Faulted)
                            {
                                throw ExpectedException;
                            }
                            else if (Behavior == DelegateBehavior.Canceled)
                            {
                                CancellationTokenSource.Cancel();
                                CancellationTokenSource.Token.ThrowIfCancellationRequested();
                            }
                        }
                    }, cancellationToken);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    CancellationTokenSource cancellationTokenSource = this.CancellationTokenSource;
                    if (cancellationTokenSource != null)
                        cancellationTokenSource.Dispose();
                }
            }
        }

        private enum DelegateBehavior
        {
            /// <summary>
            /// The delegate completes successfully.
            /// </summary>
            Success,

            /// <summary>
            /// For delegates that return a value directly, the delegate throws an exception. For
            /// delegates that return a <see cref="Task"/>, the delegate returns a <see cref="Task"/>
            /// instance which terminates with the <see cref="TaskStatus.Faulted"/> status.
            /// </summary>
            Faulted,

            /// <summary>
            /// The delegate returns a task which terminates with the <see cref="TaskStatus.Canceled"/> status.
            /// </summary>
            /// <remarks>
            /// This behavior only applies to delegates which return a <see cref="Task"/>.
            /// </remarks>
            Canceled,

            /// <summary>
            /// The delegate throws an exception prior to returning a task.
            /// </summary>
            /// <remarks>
            /// This behavior only applies to delegates which return a <see cref="Task"/>.
            /// </remarks>
            SyncFaulted,

            /// <summary>
            /// The delegate returns <see langword="null"/> instead of a <see cref="Task"/> instance.
            /// </summary>
            /// <remarks>
            /// This behavior only applies to delegates which return a <see cref="Task"/>.
            /// </remarks>
            NullTask,
        }
    }
}
