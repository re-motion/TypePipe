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
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection.BodyBuilding
{
  /// <summary>
  /// Base class for method and constructor body context classes.
  /// </summary>
  public abstract class MethodBaseBodyContextBase : BodyContextBase
  {
    private readonly ReadOnlyCollection<ParameterExpression> _parameters;

    protected MethodBaseBodyContextBase (
        ProxyType declaringType, IEnumerable<ParameterExpression> parameterExpressions, bool isStatic, IMemberSelector memberSelector)
        : base (declaringType, isStatic, memberSelector)
    {
      ArgumentUtility.CheckNotNull ("parameterExpressions", parameterExpressions);

      _parameters = parameterExpressions.ToList().AsReadOnly();
    }

    public ReadOnlyCollection<ParameterExpression> Parameters
    {
      get { return _parameters; }
    }
  }
}