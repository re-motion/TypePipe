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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class FutureTypeObjectMother
  {
    //public static FutureType Create (TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit)
    //{
    //  return new FutureType (typeAttributes);
    //}
  }

  public static class FutureConstructorInfoObjectMother
  {
    public static FutureConstructorInfo Create (Type declaringType = null, ParameterInfo[] parameters = null)
    {
      return new FutureConstructorInfo (
          declaringType ?? typeof (UnspecifiedType),
          parameters ?? new ParameterInfo[0]);
    }
  }

  public static class FutureMethodInfoObjectMother
  {
    public static FutureMethodInfo Create (
        Type declaringType = null,
        MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig,
        ParameterInfo[] parameters = null)
    {
      return new FutureMethodInfo (
          declaringType ?? typeof (UnspecifiedType), 
          methodAttributes, 
          parameters ?? new ParameterInfo[0]);
    }
  }

  public static class FuturePropertyInfoObjectMother
  {
    public static FuturePropertyInfo Create (
        Type declaringType = null,
        Type propertyType = null,
        MethodInfo getMethod = null,
        MethodInfo setMethod = null)
    {
      if (getMethod == null && setMethod == null)
        getMethod = FutureMethodInfoObjectMother.Create();

      return new FuturePropertyInfo (
          declaringType ?? typeof (UnspecifiedType),
          propertyType ?? typeof (UnspecifiedType),
          Maybe.ForValue (getMethod),
          Maybe.ForValue (setMethod));
    }
  }

  public static class FutureFieldInfoObjectMother
  {
    public static FutureFieldInfo Create (
        Type declaringType = null,
        string name = "_newField",
        Type fieldType = null,
        FieldAttributes fieldAttributes = FieldAttributes.Private)
    {
      return new FutureFieldInfo (
          declaringType ?? typeof (UnspecifiedType),
          name,
          fieldType ?? typeof (UnspecifiedType),
          fieldAttributes);
    }
  }

  public static class FutureParameterInfoObjectMother
  {
    public static FutureParameterInfo Create (Type parameterType = null)
    {
      return new FutureParameterInfo (parameterType ?? typeof (UnspecifiedType));
    }
  }

  public class UnspecifiedType 
  { 
  }
}