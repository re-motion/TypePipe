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
      Assertion.IsFalse (mutableType.IsGenericType, "TODO: adapt code");

      // TODO Review: This has a problem when the mutableType reoccurs within a member signature or expression.
      // Fix by adding the mutableType to the cache with "true" first, and in the end replacing the cache entry with 
      // the real result. (This requires a method SetStrongNamed (Type, bool) on IStrongNamedTypeVerifier.)
      // TODO Review: Change the cache to use reference equality (ReferenceEqualityComparer), otherwise MutableType can't
      // have a separate entry from the underlying type.

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
      if (!mutableType.AddedFields.All (IsStrongNamed))
        return false;
      if (!mutableType.AddedConstructors.All (IsStrongNamed))
        return false;
      if (!mutableType.AddedMethods.All (IsStrongNamed))
        return false;

      // TODO 4791: properties, events

      // TODO added and modified bodies (ctors, methods)

      return true;
    }

    private bool IsStrongNamed (MutableFieldInfo field)
    {
      // TODO Review: AddedAttributes
      //if (TypePipeCustomAttributeData.GetCustomAttributes (field).Select (x => x.Type).Any (IsSigned))
      //  return true;

      if (!_typeAnalyzer.IsStrongNamed (field.FieldType))
        return false;

      return true;
    }

    private bool IsStrongNamed (IMutableMethodBase methodBase)
    {
      // TODO Review: AddedAttributes
      //if (!TypePipeCustomAttributeData.GetCustomAttributes (mutableMethod).Select (x => x.Type).All (IsSigned))
      //  return false;

      // TODO Review: Add .MutableParameters to IMutableMethodBase, move parameter checks from MethodInfo overload to here.
      if (!methodBase.MutableParameters.All (IsStrongNamed))
        return false;

      if (!_expressionAnalyzer.IsStrongNameCompatible (methodBase.Body))
        return false;

      return true;
    }

    private bool IsStrongNamed (MutableMethodInfo method)
    {
      Assertion.IsFalse (method.IsGenericMethod, "TODO: adapt code");

      if (!IsStrongNamed ((IMutableMethodBase) method))
        return false;

      if (!IsStrongNamed (method.MutableReturnParameter))
        return false;

      return true;
    }

    private bool IsStrongNamed (MutableParameterInfo parameter)
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