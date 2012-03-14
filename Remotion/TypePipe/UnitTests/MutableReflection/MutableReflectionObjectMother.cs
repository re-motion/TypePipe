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
using Remotion.FunctionalProgramming;
using Remotion.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableTypeObjectMother
  {
    public static MutableType Create (
      ITypeInfo typeInfo = null,
      IEqualityComparer<MemberInfo> memberInfoEqualityComparer = null,
      IBindingFlagsEvaluator bindingFlagsEvaluator = null)
    {
      return new MutableType(
        typeInfo ?? NewTypeInfoObjectMother.Create(),
        memberInfoEqualityComparer ?? new MemberSignatureEqualityComparer(),
        bindingFlagsEvaluator ?? new BindingFlagsEvaluator());
    }
  }

  public static class ExistingTypeInfoObjectMother
  {
    public static ExistingTypeInfo Create (Type originalType = null)
    {
      return new ExistingTypeInfo (
          originalType ?? typeof (UnspecifiedType));
    }
  }

  public static class NewTypeInfoObjectMother
  {
    public static NewTypeInfo Create (
      Type baseType = null,
      TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit,
      Type[] interfaces = null,
      FieldInfo[] fields = null,
      ConstructorInfo[] constructors = null)
    {
      return new NewTypeInfo (
          baseType ?? typeof(UnspecifiedType),
          attributes,
          interfaces ?? Type.EmptyTypes,
          fields ?? new FieldInfo[0],
          constructors ?? new ConstructorInfo[0]);
    }
  }

  public static class MutableConstructorInfoObjectMother
  {
    public static MutableConstructorInfo Create (
      Type declaringType = null,
      MethodAttributes attributes = MethodAttributes.Public,
      ParameterDeclaration[] parameterDeclarations = null)
    {
      return new MutableConstructorInfo (
          declaringType ?? typeof (UnspecifiedType),
          attributes,
          parameterDeclarations ?? new ParameterDeclaration[0]);
    }
  }

  public static class FutureMethodInfoObjectMother
  {
    public static FutureMethodInfo Create (
        Type declaringType = null,
        MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig,
        Type returnType = null,
        ParameterDeclaration[] parameterDeclarations = null)
    {
      return new FutureMethodInfo (
          declaringType ?? typeof (UnspecifiedType), 
          methodAttributes,
          returnType ?? typeof(UnspecifiedType),
          parameterDeclarations ?? new ParameterDeclaration[0]);
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
        FieldAttributes attributes = FieldAttributes.Private)
    {
      return new FutureFieldInfo (
          declaringType ?? typeof (UnspecifiedType),
          name,
          fieldType ?? typeof (UnspecifiedType),
          attributes);
    }
  }

  public class UnspecifiedType 
  { 
  }
}