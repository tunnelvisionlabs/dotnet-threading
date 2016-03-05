// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if !NET40PLUS

namespace System.Runtime.CompilerServices
{
    /// <summary>Represents an asynchronous method builder.</summary>
    internal interface IAsyncMethodBuilder
    {
#pragma warning disable SA1600 // Elements must be documented
        void PreBoxInitialization();
#pragma warning restore SA1600 // Elements must be documented
    }
}

#endif
