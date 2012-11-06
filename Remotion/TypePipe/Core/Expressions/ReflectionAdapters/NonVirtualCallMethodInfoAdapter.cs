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
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.Utilities;

namespace Remotion.TypePipe.Expressions.ReflectionAdapters
{
  /// <summary>
  /// Wraps another <see cref="MethodInfo"/> in order to represent it as non-virtual method call.
  /// This can be used to construct a <see cref="MethodCallExpression"/> with the additional information that a virtual method should be invoked
  /// with a non-virtual method call (e.g. for calling an overridden base method).
  /// </summary>
  public class NonVirtualCallMethodInfoAdapter : DelegatingMethodInfoBase<MethodInfo>
  {
    public static NonVirtualCallMethodInfoAdapter Adapt (MethodBase methodBase)
    {
      ArgumentUtility.CheckNotNull ("methodBase", methodBase);
      Assertion.IsTrue (methodBase is MethodInfo || methodBase is ConstructorInfo);

      var method = methodBase as MethodInfo ?? new ConstructorAsMethodInfoAdapter ((ConstructorInfo) methodBase);
      return new NonVirtualCallMethodInfoAdapter (method);
    }

    public NonVirtualCallMethodInfoAdapter (MethodInfo adaptedMethod)
        : base (adaptedMethod)
    {
    }

    public MethodInfo AdaptedMethod
    {
      get { return InnerMethod; }
    }

    public override Type ReturnType
    {
      get { return InnerMethod.ReturnType; }
    }

    public override ICustomAttributeProvider ReturnTypeCustomAttributes
    {
      get { return InnerMethod.ReturnTypeCustomAttributes; }
    }

    public override MethodInfo GetBaseDefinition ()
    {
      return InnerMethod.GetBaseDefinition();
    }
  }
}