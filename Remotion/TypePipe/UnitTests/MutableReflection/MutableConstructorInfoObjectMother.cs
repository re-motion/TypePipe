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

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableConstructorInfoObjectMother
  {
    public static MutableConstructorInfo Create (
        ProxyType declaringType = null,
        MethodAttributes attributes = (MethodAttributes) 7,
        IEnumerable<ParameterDeclaration> parameters = null,
        Expression body = null)
    {
      declaringType = declaringType ?? MutableTypeObjectMother.Create();
      parameters = parameters ?? ParameterDeclaration.EmptyParameters;
      body = body ?? Expression.Empty();

      return new MutableConstructorInfo (declaringType, attributes, parameters, body);
    }

    // todo remove
    public static MutableConstructorInfo CreateForExisting (ConstructorInfo underlyingConstructor = null, ProxyType declaringType = null)
    {
      return null;
    }

    public static MutableConstructorInfo CreateForExistingAndModify (ConstructorInfo underlyingConstructor = null, ProxyType declaringType = null)
    {
      return null;
    }

    public static MutableConstructorInfo CreateForNew (
        ProxyType declaringType = null,
        MethodAttributes attributes = MethodAttributes.Family,
        IEnumerable<ParameterDeclaration> parameterDeclarations = null,
        Expression body = null)
    {
      return null;
    }
  }
}