// This file is part of the re-motion TypePipe project (typepipe.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-motion TypePipe is free software; you can redistribute it 
// and/or modify it under the terms of the Apache License, Version 2.0
// as published by the Apache Software Foundation.
// 
// re-motion TypePipe is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// Apache License, Version 2.0 for more details.
// 
// You should have received a copy of the Apache License, Version 2.0
// along with re-motion; if not, see http://www.apache.org/licenses.
// 
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace System.Dynamic.Utils {

    // Extensions on System.Type and friends
    internal static class TypeExtensions {

        /// <summary>
        /// Creates a closed delegate for the given (dynamic)method.
        /// </summary>
        internal static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType, object target) {
            Debug.Assert(methodInfo != null && delegateType != null);

            var dm = methodInfo as DynamicMethod;
            if (dm != null) {
                return dm.CreateDelegate(delegateType, target);
            } else {
                return Delegate.CreateDelegate(delegateType, target, methodInfo);
            }
        }

        internal static Type GetReturnType(this MethodBase mi) {
            return (mi.IsConstructor) ? mi.DeclaringType : ((MethodInfo)mi).ReturnType;
        }

        private static readonly CacheDict<MethodBase, ParameterInfo[]> _ParamInfoCache = new CacheDict<MethodBase, ParameterInfo[]>(75);
        
        internal static ParameterInfo[] GetParametersCached(this MethodBase method) {
            ParameterInfo[] pis;
            lock (_ParamInfoCache) {
                if (!_ParamInfoCache.TryGetValue(method, out pis)) {
                    pis = method.GetParameters();

                    Type t = method.DeclaringType;
                    if (t != null && TypeUtils.CanCache(t)) {
                        _ParamInfoCache[method] = pis;
                    }
                }
            }
            return pis;
        }

        // Expression trees/compiler just use IsByRef, why do we need this?
        // (see LambdaCompiler.EmitArguments for usage in the compiler)
        internal static bool IsByRefParameter(this ParameterInfo pi) {
            // not using IsIn/IsOut properties as they are not available in Silverlight:
            if (pi.ParameterType.IsByRef) return true;

            return (pi.Attributes & (ParameterAttributes.Out)) == ParameterAttributes.Out;
        }

        // Returns the matching method if the parameter types are reference
        // assignable from the provided type arguments, otherwise null. 
        internal static MethodInfo GetMethodValidated(
            this Type type,
            string name,
            BindingFlags bindingAttr,
            Binder binder,
            Type[] types,
            ParameterModifier[] modifiers) {
            
            var method = type.GetMethod(name, bindingAttr, binder, types, modifiers);

            return method.MatchesArgumentTypes(types) ? method : null;
        }

        /// <summary>
        /// Returns true if the method's parameter types are reference assignable from
        /// the argument types, otherwise false.
        /// 
        /// An example that can make the method return false is that 
        /// typeof(double).GetMethod("op_Equality", ..., new[] { typeof(double), typeof(int) })
        /// returns a method with two double parameters, which doesn't match the provided
        /// argument types.
        /// </summary>
        /// <returns></returns>
        private static bool MatchesArgumentTypes(this MethodInfo mi, Type[] argTypes) {
            if (mi == null || argTypes == null) {
                return false;
            }
            var ps = mi.GetParameters();

            if (ps.Length != argTypes.Length) {
                return false;
            }

            for (int i = 0; i < ps.Length; i++) {
                if (!TypeUtils.AreReferenceAssignable(ps[i].ParameterType, argTypes[i])) {
                    return false;
                }
            }
            return true;
        }
    }
}
