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

    private bool IsCompatible (MutableType mutableType)
    {
      Assertion.IsFalse (mutableType.IsGenericType, "TODO: adapt code");

      // TODO 4740: Adapt for new types
      if (!_typeAnalyzer.IsStrongNamed (mutableType.UnderlyingSystemType))
        return false;

      // TODO Review: AddedAttributes missing

      if (!mutableType.TypeInitializations.All (_expressionAnalyzer.IsStrongNameCompatible))
        return false;
      if (!mutableType.InstanceInitializations.All (_expressionAnalyzer.IsStrongNameCompatible))
        return false;

      if (!mutableType.AddedInterfaces.All (_typeAnalyzer.IsStrongNamed))
        return false;
      if (!mutableType.AddedFields.All (IsCompatible))
        return false;
      if (!mutableType.AddedConstructors.All (IsCompatible))
        return false;
      if (!mutableType.AddedMethods.All (IsCompatible))
        return false;

      // TODO 4791: properties, events

      // TODO added and modified bodies (ctors, methods)

      return true;
    }

    private bool IsCompatible (MutableFieldInfo field)
    {
      // TODO Review: AddedAttributes
      //if (TypePipeCustomAttributeData.GetCustomAttributes (field).Select (x => x.Type).Any (IsSigned))
      //  return true;

      if (!_typeAnalyzer.IsStrongNamed (field.FieldType))
        return false;

      return true;
    }

    private bool IsCompatible (IMutableMethodBase methodBase)
    {
      // TODO Review: AddedAttributes
      //if (!TypePipeCustomAttributeData.GetCustomAttributes (mutableMethod).Select (x => x.Type).All (IsSigned))
      //  return false;

      // TODO Review: Add .MutableParameters to IMutableMethodBase, move parameter checks from MethodInfo overload to here.
      if (!methodBase.MutableParameters.All (IsCompatible))
        return false;

      if (!_expressionAnalyzer.IsStrongNameCompatible (methodBase.Body))
        return false;

      return true;
    }

    private bool IsCompatible (MutableMethodInfo method)
    {
      Assertion.IsFalse (method.IsGenericMethod, "TODO: adapt code");

      if (!IsCompatible ((IMutableMethodBase) method))
        return false;

      if (!IsCompatible (method.MutableReturnParameter))
        return false;

      return true;
    }

    private bool IsCompatible (MutableParameterInfo parameter)
    {
      Assertion.IsTrue (parameter.GetRequiredCustomModifiers().Length == 0 && parameter.GetOptionalCustomModifiers().Length == 0);

      // TODO Review: AddedAttributes
      //if (!TypePipeCustomAttributeData.GetCustomAttributes (parameter).Select (x => x.Type).All (IsSigned))
      //  return false;

      if (!_typeAnalyzer.IsStrongNamed (parameter.ParameterType))
        return false;

      return true;
    }
  }
}