// Copyright (c) Rackspace, US Inc. All Rights Reserved. Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Rackspace.Threading
{
    using System.Threading.Tasks;

    /// <summary>
    /// This class is used as the task type for instances of <see cref="TaskCompletionSource{TResult}"/> intended to
    /// represent a <see cref="Task"/> which is not a <see cref="Task{TResult}"/>. Since this type is not externally
    /// visible, users will not be able to cast the task to a <see cref="Task{TResult}"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    internal sealed class VoidResult
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="VoidResult"/> class from being created.
        /// </summary>
        private VoidResult()
        {
        }
    }
}
