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
using System.Linq;
using System.Reflection;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.Development.TypePipe.UnitTesting.ObjectMothers.MutableReflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  public static class MutableTypeTestExtensions
  {
    private static int s_counter;

    public static void AddCustomAttribute (this MutableType mutableType, CustomAttributeDeclaration customAttributeDeclaration = null)
    {
      customAttributeDeclaration = customAttributeDeclaration ?? CustomAttributeDeclarationObjectMother.Create();

      mutableType.AddCustomAttribute (customAttributeDeclaration);
    }

    public static MutableType AddNestedType (
        this MutableType mutableType, string name = null, TypeAttributes attributes = TypeAttributes.NestedPublic, Type baseType = null)
    {
      name = name ?? "NestedType_" + ++s_counter;
      baseType = baseType ?? typeof (object);

      return mutableType.AddNestedType (name, attributes, baseType);
    }

    public static void AddInterface (this MutableType mutableType, Type interfaceType = null)
    {
      interfaceType = interfaceType ?? ReflectionObjectMother.GetSomeInterfaceType();

      mutableType.AddInterface (interfaceType);
    }

    public static MutableFieldInfo AddField (
        this MutableType mutableType, string name = null, FieldAttributes attributes = FieldAttributes.Private, Type type = null)
    {
      name = name ?? "Field_" + ++s_counter;
      type = type ?? typeof (int);

      return mutableType.AddField (name, attributes, type);
    }

    public static MutableConstructorInfo AddConstructor (
        this MutableType mutableType,
        MethodAttributes attributes = MethodAttributes.Public,
        IEnumerable<ParameterDeclaration> parameters = null,
        Func<ConstructorBodyCreationContext, Expression> bodyProvider = null)
    {
      parameters = parameters ?? ParameterDeclaration.None;
      bodyProvider = bodyProvider ?? (ctx => Expression.Empty());

      return mutableType.AddConstructor (attributes, parameters, bodyProvider);
    }

    public static MutableMethodInfo AddMethod (
        this MutableType mutableType,
        string name = null,
        MethodAttributes attributes = MethodAttributes.Public,
        Type returnType = null,
        IEnumerable<ParameterDeclaration> parameters = null,
        Func<MethodBodyCreationContext, Expression> bodyProvider = null)
    {
      name = name ?? "Method_" + ++s_counter;
      returnType = returnType ?? typeof (void);
      parameters = parameters ?? ParameterDeclaration.None;
      bodyProvider = bodyProvider == null && !attributes.IsSet (MethodAttributes.Abstract)
                         ? (ctx => Expression.Default (ctx.ReturnType))
                         : bodyProvider;

      return MutableTypeExtensions.AddMethod (mutableType, name, attributes, returnType, parameters, bodyProvider);
    }

    public static MutablePropertyInfo AddProperty (
        this MutableType mutableType,
        string name = null,
        Type type = null,
        IEnumerable<ParameterDeclaration> indexParameters = null,
        MethodAttributes accessorAttributes = MethodAttributes.Public,
        Func<MethodBodyCreationContext, Expression> getBodyProvider = null,
        Func<MethodBodyCreationContext, Expression> setBodyProvider = null)
    {
      name = name ?? "Property_" + ++s_counter;
      type = type ?? typeof (int);
      indexParameters = indexParameters ?? ParameterDeclaration.None;
      if (getBodyProvider == null && setBodyProvider == null)
        getBodyProvider = ctx => Expression.Default (ctx.ReturnType);

      return mutableType.AddProperty (name, type, indexParameters, accessorAttributes, getBodyProvider, setBodyProvider);
    }

    public static MutablePropertyInfo AddProperty2 (
        this MutableType mutableType,
        string name = null,
        PropertyAttributes attributes = PropertyAttributes.None,
        MutableMethodInfo getMethod = null,
        MutableMethodInfo setMethod = null)
    {
      name = name ?? "Property_" + ++s_counter;
      if (getMethod == null && setMethod == null)
        getMethod = MutableMethodInfoObjectMother.Create (mutableType, "Getter", returnType: typeof (int));

      return mutableType.AddProperty (name, attributes, getMethod, setMethod);
    }

    public static MutableEventInfo AddEvent (
        this MutableType mutableType,
        string name = null,
        Type handlerType = null,
        MethodAttributes accessorAttributes = MethodAttributes.Public,
        Func<MethodBodyCreationContext, Expression> addBodyProvider = null,
        Func<MethodBodyCreationContext, Expression> removeBodyProvider = null,
        Func<MethodBodyCreationContext, Expression> raiseBodyProvider = null)
    {
      name = name ?? "Event_" + ++s_counter;
      handlerType = handlerType ?? typeof (Action);

      if (addBodyProvider == null)
        addBodyProvider = ctx => Expression.Empty();
      if (removeBodyProvider == null)
        removeBodyProvider = ctx => Expression.Empty();

      return mutableType.AddEvent (name, handlerType, accessorAttributes, addBodyProvider, removeBodyProvider, raiseBodyProvider);
    }

    public static MutableEventInfo AddEvent2 (
        this MutableType mutableType,
        string name = null,
        EventAttributes attributes = EventAttributes.None,
        MutableMethodInfo addMethod = null,
        MutableMethodInfo removeMethod = null,
        MutableMethodInfo raiseMethod = null)
    {
      name = name ?? "Event_" + ++s_counter;
      var handlerMethod = addMethod ?? removeMethod;
      var handlerType = handlerMethod != null ? handlerMethod.GetParameters().Single().ParameterType : typeof (Action);
      addMethod = addMethod ?? MutableMethodInfoObjectMother.Create (mutableType, "Adder", parameters: new[] { ParameterDeclarationObjectMother.Create(handlerType) });
      removeMethod = removeMethod ?? MutableMethodInfoObjectMother.Create (mutableType, "Adder", parameters: new[] { ParameterDeclarationObjectMother.Create (handlerType) });

      return mutableType.AddEvent (name, attributes, addMethod, removeMethod, raiseMethod);
    }
  }
}