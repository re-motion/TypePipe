﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection.BodyBuilding;

namespace Remotion.TypePipe.MutableReflection.Implementation.MemberFactory
{
  /// <summary>
  /// Serves as a factory for mutable members, that is, mutable reflection objects that implement <see cref="IMutableMember"/>.
  /// </summary>
  /// <remarks>
  /// This interface is an implementation detail of <see cref="MutableType"/>.
  /// </remarks>
  public interface IMutableMemberFactory : IMethodFactory
  {
    MutableType CreateNestedType (MutableType declaringType, string name, TypeAttributes attributes, Type baseType);

    Expression CreateInitialization (MutableType declaringType, Func<InitializationBodyContext, Expression> initializationProvider);

    MutableFieldInfo CreateField (MutableType declaringType, string name, Type type, FieldAttributes attributes);

    MutableConstructorInfo CreateConstructor (
        MutableType declaringType,
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameters,
        Func<ConstructorBodyCreationContext, Expression> bodyProvider);

    MutableMethodInfo CreateExplicitOverride (
        MutableType declaringType, MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProvider);

    MutableMethodInfo GetOrCreateOverride (MutableType declaringType, MethodInfo overriddenMethod, out bool isNewlyCreated);

    MutableMethodInfo GetOrCreateImplementation (MutableType declaringType, MethodInfo interfaceMethod, out bool isNewlyCreated);

    MutablePropertyInfo CreateProperty (
        MutableType declaringType,
        string name,
        Type type,
        IEnumerable<ParameterDeclaration> indexParameters,
        MethodAttributes accessorAttributes,
        Func<MethodBodyCreationContext, Expression> getBodyProvider,
        Func<MethodBodyCreationContext, Expression> setBodyProvider);

    MutablePropertyInfo CreateProperty (
        MutableType declaringType, string name, PropertyAttributes attributes, MutableMethodInfo getMethod, MutableMethodInfo setMethod);

    MutableEventInfo CreateEvent (
        MutableType declaringType,
        string name,
        Type handlerType,
        MethodAttributes accessorAttributes,
        Func<MethodBodyCreationContext, Expression> addBodyProvider,
        Func<MethodBodyCreationContext, Expression> removeBodyProvider,
        Func<MethodBodyCreationContext, Expression> raiseBodyProvider);

    MutableEventInfo CreateEvent (
        MutableType declaringType,
        string name,
        EventAttributes attributes,
        MutableMethodInfo addMethod,
        MutableMethodInfo removeMethod,
        MutableMethodInfo raiseMethod);
  }
}