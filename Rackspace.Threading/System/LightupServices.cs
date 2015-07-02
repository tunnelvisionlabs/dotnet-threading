// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if !NET40PLUS

// The reference source is missing documentation
#pragma warning disable SA1600 // Elements must be documented

namespace System
{
    using System.Linq;
    using System.Reflection;

    internal static class LightupServices
    {
        internal static readonly Delegate NotFound = new Action(() => { });

        public static Delegate ReplaceWith(Delegate d, Type delegateType)
        {
            return Delegate.CreateDelegate(delegateType, d.Target, d.Method);
        }

        public static Type[] GetMethodArgumentTypes(Type actionOrFuncType, bool bindInstance = true)
        {
            Type[] arguments = actionOrFuncType.GetGenericArguments();

            if (!bindInstance)
            {
                // We aren't binding the instance, remove "this" argument
                arguments = arguments.Skip(1).ToArray();
            }

            if (IsActionType(actionOrFuncType))
                return arguments;

            // We have a Func, remove it's trailing return type
            return arguments.Take(arguments.Length - 1).ToArray();
        }

        public static bool IsActionType(Type type)
        {
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();

            return type == typeof(Action)
                || type == typeof(Action<>)
                || type == typeof(Action<,>)
                || type == typeof(Action<,,>)
                || type == typeof(Action<,,,>);
        }

        public static Delegate CreateDelegate(Type type, object instance, MethodInfo method)
        {
            if (method.IsStatic)
                instance = null;

            try
            {
                return Delegate.CreateDelegate(type, instance, method);
            }
            catch (InvalidOperationException)
            {
                // Exists, but not callable
            }
            catch (MemberAccessException)
            {
                // Exists, but don't have required access
            }

            return NotFound;
        }
    }
}

#endif
