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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableConstructorInfoObjectMother
  {
    public static MutableConstructorInfo Create (
        MutableType declaringType = null,
        UnderlyingConstructorInfoDescriptor underlyingConstructorInfoDescriptor = null)
    {
      return new MutableConstructorInfo (
          declaringType ?? MutableTypeObjectMother.Create(),
          underlyingConstructorInfoDescriptor ?? UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew());
    }

    public static MutableConstructorInfo CreateForExisting (ConstructorInfo originalConstructorInfo = null)
    {
      var descriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForExisting (originalConstructorInfo);
      return Create (underlyingConstructorInfoDescriptor: descriptor);
    }

    public static MutableConstructorInfo CreateForExistingAndModify (ConstructorInfo originalConstructorInfo = null)
    {
      var ctor = CreateForExisting (originalConstructorInfo);
      MutableConstructorInfoTestHelper.ModifyConstructor (ctor);
      return ctor;
    }

    public static MutableConstructorInfo CreateForNew (MutableType declaringType = null)
    {
      var descriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew();
      return Create (declaringType: declaringType, underlyingConstructorInfoDescriptor: descriptor);
    }

    public static MutableConstructorInfo CreateForNewWithParameters (
        MutableType declaringType,
        params ParameterDeclaration[] parameterDeclarations)
    {
      var descriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew (parameterDeclarations: parameterDeclarations);
      return Create (declaringType: declaringType, underlyingConstructorInfoDescriptor: descriptor);
    }

    public static MutableConstructorInfo CreateForNewWithParameters (params ParameterDeclaration[] parameterDeclarations)
    {
      return CreateForNewWithParameters (null, parameterDeclarations);
    }
  }
}