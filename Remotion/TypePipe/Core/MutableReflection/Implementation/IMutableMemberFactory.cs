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
using Remotion.TypePipe.MutableReflection.BodyBuilding;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Serves as a factory for mutable members.
  /// </summary>
  /// <remarks>
  /// This interface is an implementation detail of <see cref="MutableType"/>.
  /// </remarks>
  public interface IMutableMemberFactory
  {
    Expression CreateInitialization (MutableType declaringType, bool isStatic, Func<InitializationBodyContext, Expression> initializationProvider);

    MutableFieldInfo CreateMutableField (MutableType declaringType, string name, Type type, FieldAttributes attributes);

    MutableConstructorInfo CreateMutableConstructor (
        MutableType declaringType,
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        Func<ConstructorBodyCreationContext, Expression> bodyProvider);

    MutableMethodInfo CreateMutableMethod (
        MutableType declaringType,
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        Func<MethodBodyCreationContext, Expression> bodyProvider);

    MutableMethodInfo GetOrCreateMutableMethodOverride (MutableType declaringType, MethodInfo method, out bool isNewlyCreated);
  }
}