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
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.MutableReflection.BodyBuilding
{
  /// <summary>
  /// Provides access to expressions needed for building the bodies of added methods. 
  /// </summary>
  /// <seealso cref="ProxyType.AddMethod"/>
  public class MethodBodyCreationContext : MethodBodyContextBase
  {
    public MethodBodyCreationContext (
        ProxyType declaringType,
        bool isStatic,
        IEnumerable<ParameterExpression> parameterExpressions,
        IEnumerable<Type> genericParameters,
        Type returnType,
        MethodInfo baseMethod,
        IMemberSelector memberSelector)
        : base (declaringType, isStatic, parameterExpressions, genericParameters, returnType, baseMethod, memberSelector)
    {
    }
  }
}