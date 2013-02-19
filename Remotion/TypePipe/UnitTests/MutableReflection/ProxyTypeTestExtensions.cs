// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class ProxyTypeTestExtensions
  {
    private static int s_counter;

    public static MutableFieldInfo AddField2 (
        this ProxyType proxyType, string name = null, Type type = null, FieldAttributes attributes = FieldAttributes.Private)
    {
      name = name ?? "Field_" + ++s_counter;
      type = type ?? typeof (int);

      return proxyType.AddField (name, type, attributes);
    }

    public static MutableMethodInfo AddMethod2 (
        this ProxyType proxyType,
        string name = null,
        MethodAttributes attributes = MethodAttributes.Public,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameters = null,
        Func<MethodBodyCreationContext, Expression> bodyProvider = null)
    {
      name = name ?? "Method_" + ++s_counter;
      bodyProvider = bodyProvider == null && !attributes.IsSet (MethodAttributes.Abstract)
                         ? (ctx => Expression.Default (ctx.ReturnType))
                         : (Func<MethodBodyCreationContext, Expression>) null;

      return proxyType.AddMethod (name, attributes, returnType, parameters, bodyProvider);
    }

    public static MutablePropertyInfo AddProperty2 (
        this ProxyType proxyType,
        string name = null,
        Type type = null,
        IEnumerable<ParameterDeclaration> indexParameters = null,
        MethodAttributes accessorAttributes = MethodAttributes.Public,
        Func<MethodBodyCreationContext, Expression> getBodyProvider = null,
        Func<MethodBodyCreationContext, Expression> setBodyProvider = null)
    {
      name = name ?? "Property_" + ++s_counter;
      type = type ?? typeof (int);
      if (getBodyProvider == null && setBodyProvider == null)
        getBodyProvider = ctx => Expression.Default (ctx.ReturnType);

      return proxyType.AddProperty (name, type, indexParameters, accessorAttributes, getBodyProvider, setBodyProvider);
    }
  }
}