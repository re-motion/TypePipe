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
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.BodyBuilding
{
  /// <summary>
  /// Base class for method body context classes.
  /// </summary>
  public abstract class MethodBodyContextBase : MethodBaseBodyContextBase
  {
    private readonly Type _returnType;
    private readonly MethodInfo _baseMethod;

    protected MethodBodyContextBase (
        ProxyType declaringType,
        bool isStatic,
        IEnumerable<ParameterExpression> parameterExpressions,
        Type returnType,
        MethodInfo baseMethod,
        IMemberSelector memberSelector)
        : base (declaringType, isStatic, parameterExpressions, memberSelector)
    {
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      // Base method may be null.

      _returnType = returnType;
      _baseMethod = baseMethod;
    }

    public ReadOnlyCollection<Type> GenericParameters
    {
      get { throw new NotImplementedException(); }
    }

    public Type ReturnType
    {
      get { return _returnType; }
    }

    public bool HasBaseMethod
    {
      get { return _baseMethod != null; }
    }

    public MethodInfo BaseMethod
    {
      get
      {
        if (!HasBaseMethod)
          throw new NotSupportedException ("This method does not override another method.");

        return _baseMethod;
      }
    }
  }
}