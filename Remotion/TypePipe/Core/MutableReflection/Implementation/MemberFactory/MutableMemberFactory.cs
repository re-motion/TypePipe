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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation.MemberFactory
{
  /// <summary>
  /// This class acts as a facade for creating mutable members, i.e., it implements <see cref="IMutableMemberFactory"/> by delegating to the
  /// focused member factories.
  /// </summary>
  public class MutableMemberFactory : IMutableMemberFactory
  {
    private readonly InitializationFactory _initializationFactory;
    private readonly FieldFactory _fieldFactory;
    private readonly ConstructorFactory _constructorFactory;
    private readonly MethodFactory _methodFactory;
    private readonly MethodOverrideFactory _methodOverrideFactory;
    private readonly PropertyFactory _propertyFactory;
    private readonly EventFactory _eventFactory;

    public MutableMemberFactory (IRelatedMethodFinder relatedMethodFinder)
    {
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);

      _initializationFactory = new InitializationFactory();
      _fieldFactory = new FieldFactory();
      _constructorFactory = new ConstructorFactory();
      _methodFactory = new MethodFactory (relatedMethodFinder);
      _methodOverrideFactory = new MethodOverrideFactory (relatedMethodFinder, _methodFactory);
      _propertyFactory = new PropertyFactory (_methodFactory);
      _eventFactory = new EventFactory (_methodFactory);
    }

    public Expression CreateInitialization (MutableType declaringType, Func<InitializationBodyContext, Expression> initializationProvider)
    {
      return _initializationFactory.CreateInitialization (declaringType, initializationProvider);
    }

    public MutableFieldInfo CreateField (MutableType declaringType, string name, Type type, FieldAttributes attributes)
    {
      return _fieldFactory.CreateField (declaringType, name, type, attributes);
    }

    public MutableConstructorInfo CreateConstructor (
        MutableType declaringType,
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameters,
        Func<ConstructorBodyCreationContext, Expression> bodyProvider)
    {
      return _constructorFactory.CreateConstructor (declaringType, attributes, parameters, bodyProvider);
    }

    public MutableMethodInfo CreateMethod (
        MutableType declaringType,
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameters,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      return _methodFactory.CreateMethod (declaringType, name, attributes, returnType, parameters, bodyProvider);
    }

    public MutableMethodInfo CreateMethod (
        MutableType declaringType,
        string name,
        MethodAttributes attributes,
        IEnumerable<GenericParameterDeclaration> genericParameters,
        Func<GenericParameterContext, Type> returnTypeProvider,
        Func<GenericParameterContext, IEnumerable<ParameterDeclaration>> parameterProvider,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      return _methodFactory.CreateMethod (declaringType, name, attributes, genericParameters, returnTypeProvider, parameterProvider, bodyProvider);
    }

    public MutableMethodInfo CreateExplicitOverride (
        MutableType declaringType, MethodInfo overriddenMethodBaseDefinition, Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      return _methodOverrideFactory.CreateExplicitOverride (declaringType, overriddenMethodBaseDefinition, bodyProvider);
    }

    public MutableMethodInfo GetOrCreateOverride (MutableType declaringType, MethodInfo overriddenMethod, out bool isNewlyCreated)
    {
      return _methodOverrideFactory.GetOrCreateOverride (declaringType, overriddenMethod, out isNewlyCreated);
    }

    public MutablePropertyInfo CreateProperty (
        MutableType declaringType,
        string name,
        Type type,
        IEnumerable<ParameterDeclaration> indexParameters,
        MethodAttributes accessorAttributes,
        Func<MethodBodyCreationContext, Expression> getBodyProvider,
        Func<MethodBodyCreationContext, Expression> setBodyProvider)
    {
      return _propertyFactory.CreateProperty (declaringType, name, type, indexParameters, accessorAttributes, getBodyProvider, setBodyProvider);
    }

    public MutablePropertyInfo CreateProperty (
        MutableType declaringType, string name, PropertyAttributes attributes, MutableMethodInfo getMethod, MutableMethodInfo setMethod)
    {
      return _propertyFactory.CreateProperty (declaringType, name, attributes, getMethod, setMethod);
    }

    public MutableEventInfo CreateEvent (
        MutableType declaringType,
        string name,
        Type handlerType,
        MethodAttributes accessorAttributes,
        Func<MethodBodyCreationContext, Expression> addBodyProvider,
        Func<MethodBodyCreationContext, Expression> removeBodyProvider,
        Func<MethodBodyCreationContext, Expression> raiseBodyProvider)
    {
      return _eventFactory.CreateEvent (declaringType, name, handlerType, accessorAttributes, addBodyProvider, removeBodyProvider, raiseBodyProvider);
    }

    public MutableEventInfo CreateEvent (
        MutableType declaringType,
        string name,
        EventAttributes attributes,
        MutableMethodInfo addMethod,
        MutableMethodInfo removeMethod,
        MutableMethodInfo raiseMethod)
    {
      return _eventFactory.CreateEvent (declaringType, name, attributes, addMethod, removeMethod, raiseMethod);
    }
  }
}