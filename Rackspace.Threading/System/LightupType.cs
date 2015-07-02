// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if !NET40PLUS

// The reference source is missing documentation
#pragma warning disable SA1600 // Elements must be documented

namespace System
{
    internal static class LightupType
    {
        public static readonly Type ParameterizedThreadStart = GetExternallyVisibleType("System.Threading.ParameterizedThreadStart, mscorlib");
        public static readonly Type ExecutionContext = GetExternallyVisibleType("System.Threading.ExecutionContext, mscorlib");
        public static readonly Type ContextCallback = GetExternallyVisibleType("System.Threading.ContextCallback, mscorlib");
        public static readonly Type OperatingSystem = GetExternallyVisibleType("System.OperatingSystem, mscorlib");

        private static Type GetExternallyVisibleType(string typeName)
        {
            // Types such as ExecutionContext exist on Phone, but are not visible
            Type type = Type.GetType(typeName);
            if (type != null && type.IsVisible)
                return type;

            return null;
        }
    }
}

#endif
