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

namespace Remotion.TypePipe.UnitTests.MutableReflection.Descriptors
{
  public static class ConstructorDescriptorObjectMother
  {
    public static ConstructorDescriptor Create (
        MethodAttributes attributes =
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
        IEnumerable<ParameterDeclaration> parameterDeclarations = null,
        Expression body = null)
    {
      return CreateForNew (attributes, parameterDeclarations, body);
    }

    public static ConstructorDescriptor CreateForNew (
        MethodAttributes attributes =
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
        IEnumerable<ParameterDeclaration> parameterDeclartions = null,
        Expression body = null)
    {
      parameterDeclartions = parameterDeclartions ?? ParameterDeclaration.EmptyParameters;
      body = body ?? Expression.Empty();

      return ConstructorDescriptor.Create (attributes, ParameterDescriptor.CreateFromDeclarations (parameterDeclartions), body);
    }

    public static ConstructorDescriptor CreateForExisting (ConstructorInfo underlyingConstructor = null)
    {
      return ConstructorDescriptor.Create (underlyingConstructor ?? ReflectionObjectMother.GetSomeDefaultConstructor());
    }
  }
}