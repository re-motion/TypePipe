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
using System.Linq;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.StrongNaming
{
  public class MutableTypeAnalyzer : IMutableTypeAnalyzer
  {
    private readonly ITypeAnalyzer _typeAnalyzer;
    private readonly IExpressionAnalyzer _expressionAnalyzer;

    public MutableTypeAnalyzer (ITypeAnalyzer typeAnalyzer, IExpressionAnalyzer expressionAnalyzer)
    {
      ArgumentUtility.CheckNotNull ("typeAnalyzer", typeAnalyzer);
      ArgumentUtility.CheckNotNull ("expressionAnalyzer", expressionAnalyzer);

      _typeAnalyzer = typeAnalyzer;
      _expressionAnalyzer = expressionAnalyzer;
    }

    public bool IsStrongNameCompatible (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      // Temporarily assume the that the mutable type is compatible in case it is used in signatures or bodies.
      _typeAnalyzer.SetStrongNamed (mutableType, true);

      var isCompatible = IsCompatible (mutableType);

      // Correct the temporary assumption with the actual result.
      _typeAnalyzer.SetStrongNamed (mutableType, isCompatible);

      return isCompatible;
    }

    // TODO 4740: Adapt for new types
    // TODO 4791: properties, events
    private bool IsCompatible (MutableType mutableType)
    {
      Assertion.IsFalse (mutableType.IsGenericType, "TODO: adapt code");

      return _typeAnalyzer.IsStrongNamed (mutableType.UnderlyingSystemType)
             && IsCompatible (attributeTarget: mutableType)
             && mutableType.TypeInitializations.All (_expressionAnalyzer.IsStrongNameCompatible)
             && mutableType.InstanceInitializations.All (_expressionAnalyzer.IsStrongNameCompatible)
             && mutableType.AddedInterfaces.All (_typeAnalyzer.IsStrongNamed)
             && mutableType.AddedFields.All (IsCompatible)
             && mutableType.AddedConstructors.All (IsCompatible)
             && mutableType.AddedMethods.All (IsCompatible);
      // TODO added and modified bodies (ctors, methods)
    }

    private bool IsCompatible (IMutableInfo attributeTarget)
    {
      return attributeTarget.AddedCustomAttributes.All (a => _typeAnalyzer.IsStrongNamed (a.Type));
    }

    private bool IsCompatible (MutableFieldInfo field)
    {
      return IsCompatible (attributeTarget: field) && _typeAnalyzer.IsStrongNamed (field.FieldType);
    }

    private bool IsCompatible (IMutableMethodBase methodBase)
    {
      return IsCompatible (attributeTarget: methodBase)
             && methodBase.MutableParameters.All (IsCompatible)
             && _expressionAnalyzer.IsStrongNameCompatible (methodBase.Body);
    }

    private bool IsCompatible (MutableMethodInfo method)
    {
      Assertion.IsFalse (method.IsGenericMethod, "TODO: adapt code");

      return IsCompatible (methodBase: method) && IsCompatible (method.MutableReturnParameter);
    }

    private bool IsCompatible (MutableParameterInfo parameter)
    {
      Assertion.IsTrue (parameter.GetRequiredCustomModifiers().Length == 0 && parameter.GetOptionalCustomModifiers().Length == 0);

      return IsCompatible (attributeTarget: parameter) && _typeAnalyzer.IsStrongNamed (parameter.ParameterType);
    }
  }
}