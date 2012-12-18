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
using Microsoft.Scripting.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Descriptors;
using Remotion.TypePipe.UnitTests.MutableReflection.Descriptors;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableConstructorInfoObjectMother
  {
    public static MutableConstructorInfo Create (
        MutableType declaringType = null,
        MethodAttributes attributes = MethodAttributes.Family,
        IEnumerable<ParameterDeclaration> parameterDeclarations = null,
        Expression body = null)
    {
      return CreateForNew (declaringType, attributes, parameterDeclarations, body);
    }

    public static MutableConstructorInfo CreateForExisting (ConstructorInfo underlyingConstructor = null, MutableType declaringType = null)
    {
      underlyingConstructor = underlyingConstructor ?? ReflectionObjectMother.GetSomeConstructor();
      declaringType = declaringType ??  MutableTypeObjectMother.CreateForExisting (underlyingConstructor.DeclaringType);
      var descriptor = ConstructorDescriptorObjectMother.CreateForExisting (underlyingConstructor);

      return new MutableConstructorInfo (declaringType, descriptor);
    }

    public static MutableConstructorInfo CreateForExistingAndModify (ConstructorInfo underlyingConstructor = null, MutableType declaringType = null)
    {
      var ctor = CreateForExisting (underlyingConstructor, declaringType);
      MutableConstructorInfoTestHelper.ModifyConstructor (ctor);
      return ctor;
    }

    public static MutableConstructorInfo CreateForNew (
        MutableType declaringType = null,
        MethodAttributes attributes = MethodAttributes.Family,
        IEnumerable<ParameterDeclaration> parameterDeclarations = null,
        Expression body = null)
    {
      declaringType = declaringType ?? MutableTypeObjectMother.Create();
      var descriptor = ConstructorDescriptorObjectMother.CreateForNew (attributes, parameterDeclarations, body);

      return new MutableConstructorInfo (declaringType, descriptor);
    }

    public static MutableConstructorInfo CreateForNewWithParameters (MutableType declaringType, params ParameterDeclaration[] parameterDeclarations)
    {
      return CreateForNew (declaringType, parameterDeclarations: parameterDeclarations);
    }

    public static MutableConstructorInfo CreateForNewWithParameters (params ParameterDeclaration[] parameterDeclarations)
    {
      return CreateForNewWithParameters (null, parameterDeclarations);
    }
  }
}