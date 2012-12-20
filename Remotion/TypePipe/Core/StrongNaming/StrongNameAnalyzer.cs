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
using Remotion.TypePipe.Expressions.ReflectionAdapters;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.StrongNaming
{
  public class StrongNameAnalyzer : IStrongNameAnalyzer
  {
    private readonly IStrongNameTypeVerifier _strongNameTypeVerifier;
    private readonly IStrongNameExpressionVerifier _strongNameExpressionVerifier;

    private readonly Dictionary<Type, bool> _cache = new Dictionary<Type, bool> ();

    public StrongNameAnalyzer (IStrongNameTypeVerifier strongNameTypeVerifier, IStrongNameExpressionVerifier strongNameExpressionVerifier)
    {
      ArgumentUtility.CheckNotNull ("strongNameTypeVerifier", strongNameTypeVerifier);
      ArgumentUtility.CheckNotNull ("strongNameExpressionVerifier", strongNameExpressionVerifier);

      _strongNameTypeVerifier = strongNameTypeVerifier;
      _strongNameExpressionVerifier = strongNameExpressionVerifier;
    }

    public bool IsStrongNameCompatible (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      // TODO Review: This has a problem when the mutableType reoccurs within a member signature or expression.
      // Fix by adding the mutableType to the cache with "true" first, and in the end replacing the cache entry with 
      // the real result. (This requires a method SetStrongNamed (Type, bool) on IStrongNamedTypeVerifier.)
      // TODO Review: Change the cache to use reference equality (ReferenceEqualityComparer), otherwise MutableType can't
      // have a separate entry from the underlying type.

      if (!IsStrongNamed (mutableType.UnderlyingSystemType))
        return false;

      // TODO Review: AddedAttributes missing

      // TODO Review: AddedInterfaces
      if (!mutableType.GetInterfaces().All (IsStrongNamed))
        return false;

      if (!mutableType.AddedFields.All (IsStrongNamed))
        return false;

      // TODO Review: Just call the overload for MutableMethodInfo, which itself should delegate to the overload for IMutableMethodBase
      if (!mutableType.AddedMethods.Cast<MethodInfo>().All (IsStrongNamed) ||
          !mutableType.AddedMethods.Cast<IMutableMethodBase>().All (IsStrongNamed))
        return false;

      // TODO Review: Just calling the overload for IMutableMethodBase should be enough
      if (!mutableType.AddedConstructors.Select (x => new ConstructorAsMethodInfoAdapter (x)).All (IsStrongNamed) ||
          !mutableType.AddedConstructors.All (IsStrongNamed))
        return false;

      return true;
    }

    // TODO Review: Move this implementation to StrongNamedTypeVerifier.IsStrongNamed
    private bool IsStrongNamed (Type type)
    {
      bool signed;

      if (!_cache.TryGetValue (type, out signed))
      {
        // TODO Review: Is there a test for a non-generic type?
        signed = _strongNameTypeVerifier.IsStrongNamed (type) && type.GetGenericArguments().All (IsStrongNamed);
        _cache.Add (type, signed);
      }

      return signed;
    }

    private bool IsStrongNamed (MutableFieldInfo mutableField)
    {
      // TODO Review: AddedAttributes
      //if (TypePipeCustomAttributeData.GetCustomAttributes (mutableField).Select (x => x.Type).Any (IsSigned))
      //  return true;

      if (!IsStrongNamed (mutableField.FieldType))
        return false;

      return true;
    }


    private bool IsStrongNamed (IMutableMethodBase mutable)
    {
      // TODO Review: AddedAttributes
      //if (!TypePipeCustomAttributeData.GetCustomAttributes (mutableMethod).Select (x => x.Type).All (IsSigned))
      //  return false;

      // TODO Review: Add .MutableParameters to IMutableMethodBase, move parameter checks from MethodInfo overload to here.

      if (!_strongNameExpressionVerifier.IsStrongNamed (mutable.Body))
        return false;

      return true;
    }

    // TODO Review: MutableMethodInfo
    private bool IsStrongNamed (MethodInfo method)
    {
      // TODO Review: AddedAttributes need not be analyzed, is handled by IMutableMethodBase overload
      //if (!TypePipeCustomAttributeData.GetCustomAttributes (mutableMethod).Select (x => x.Type).All (IsSigned))
      //  return false;

      // TODO Review: Call IMutableMethodBase overload

      // TODO Review: .MutableParameters
      if (!method.GetParameters ().All (IsStrongNamed))
        return false;

      // TODO Review: Use .MutableReturnParameter, not just the return type
      if (!IsStrongNamed ((method).ReturnType))
        return false;

      // TODO Review: Assertion: Method is not generic

      return true;
    }

    // TODO Review: MutableParameterInfo
    private bool IsStrongNamed (ParameterInfo parameterInfo)
    {
      // TODO Review: AddedAttributes
      //if (!TypePipeCustomAttributeData.GetCustomAttributes (parameterInfo).Select (x => x.Type).All (IsSigned))
      //  return false;

      // TODO Review: Assertion: No required or optional custom modifiers

      if (!IsStrongNamed (parameterInfo.ParameterType))
        return false;

      return true;
    }
  }
}