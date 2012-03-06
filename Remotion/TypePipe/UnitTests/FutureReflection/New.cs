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
using Remotion.FunctionalProgramming;
using Remotion.TypePipe.FutureReflection;

namespace Remotion.TypePipe.UnitTests.FutureReflection
{
  public static class New
  {
    private static readonly ParameterInfo[] EmptyParameters = new ParameterInfo[0];

    public static FutureType FutureType (TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit)
    {
      return new FutureType (typeAttributes);
    }

    public static FutureConstructorInfo FutureConstructorInfo (Type declaringType = null, ParameterInfo[] parameters = null)
    {
      declaringType = declaringType ?? FutureType();
      parameters = parameters ?? EmptyParameters;

      return new FutureConstructorInfo (declaringType, parameters);
    }

    public static FutureMethodInfo FutureMethodInfo (
      Type declaringType = null,
      MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig,
      ParameterInfo[] parameters = null)
    {
      declaringType = declaringType ?? FutureType ();
      parameters = parameters ?? EmptyParameters;

      return new FutureMethodInfo (declaringType, methodAttributes, parameters);
    }

    public static FuturePropertyInfo FuturePropertyInfo (
      Type declaringType = null,
      Type propertyType = null,
      MethodInfo getMethod = null,
      MethodInfo setMethod = null)
    {
      declaringType = declaringType ?? FutureType ();
      propertyType = propertyType ?? FutureType ();
      if (getMethod == null && setMethod == null)
        getMethod = FutureMethodInfo ();

      return new FuturePropertyInfo (declaringType, propertyType, Maybe.ForValue(getMethod), Maybe.ForValue(setMethod));
    }

    public static FutureParameterInfo FutureParameterInfo (Type parameterType = null)
    {
      parameterType = parameterType ?? typeof (ExampleType);

      return new FutureParameterInfo (parameterType);
    }
  }

  public class ExampleType { }
}