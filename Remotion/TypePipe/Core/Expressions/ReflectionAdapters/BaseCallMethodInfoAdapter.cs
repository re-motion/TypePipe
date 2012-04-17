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

namespace Remotion.TypePipe.Expressions.ReflectionAdapters
{
  /// <summary>
  /// Wraps another <see cref="MethodInfo"/> in order to represent it as a base call.
  /// This can be used to construct a <see cref="MethodCallExpression"/> with the additional information that the method should be invoked
  /// as a non-virtual base call.
  /// </summary>
  public class BaseCallMethodInfoAdapter : DelegatingMethodInfoBase<MethodInfo>
  {
    public BaseCallMethodInfoAdapter (MethodInfo adaptedMethod)
        : base (adaptedMethod)
    {
    }

    public MethodInfo AdaptedMethodInfo
    {
      get { return InnerMethod; }
    }

    public override ICustomAttributeProvider ReturnTypeCustomAttributes
    {
      get { return InnerMethod.ReturnTypeCustomAttributes; }
    }

    public override MethodInfo GetBaseDefinition ()
    {
      return InnerMethod.GetBaseDefinition();
    }

    protected override Type GetReturnType ()
    {
      return InnerMethod.ReturnType;
    }
  }
}