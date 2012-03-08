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
using System.Linq;
using System.Reflection;
using Microsoft.Scripting.Ast;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public class Arguments
  {
    private readonly object[] _instances;
    private readonly ParameterInfo[] _parameters;
    private readonly IEnumerable<Expression> _expressions;

    public Arguments (params object[] instances)
    {
      ArgumentUtility.CheckNotNull ("instances", instances);

      _instances = instances;
      _parameters = instances.Select (i => FutureParameterInfoObjectMother.Create (i.GetType())).ToArray();
      _expressions = instances.Select (Expression.Constant).Cast<Expression>();
    }

    public object[] Instances
    {
      get { return _instances; }
    }

    public ParameterInfo[] Parameters
    {
      get { return _parameters; }
    }

    public IEnumerable<Expression> Expressions
    {
      get { return _expressions; }
    }
  }
}