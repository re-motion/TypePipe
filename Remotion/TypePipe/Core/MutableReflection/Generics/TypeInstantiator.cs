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
using Remotion.Text;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;
using Remotion.Collections;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Provides methods for substituting generic type parameters with type arguments by holding onto the type arguments
  /// </summary>
  public class TypeInstantiator : ITypeInstantiator
  {
    private readonly IMemberSelector _memberSelector;
    private readonly IUnderlyingTypeFactory _underlyingTypeFactory;
    private readonly IDictionary<Type, Type> _parametersToArguments;

    public TypeInstantiator (
        IMemberSelector memberSelector, IUnderlyingTypeFactory underlyingTypeFactory, IDictionary<Type, Type> parametersToArguments)
    {
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNull ("underlyingTypeFactory", underlyingTypeFactory);
      ArgumentUtility.CheckNotNull ("parametersToArguments", parametersToArguments);

      _memberSelector = memberSelector;
      _underlyingTypeFactory = underlyingTypeFactory;
      _parametersToArguments = parametersToArguments;
    }

    public IEnumerable<Type> TypeArguments
    {
      get { return _parametersToArguments.Values; }
    }

    // TODO yyy: what about ToString?
    public string GetFullName (Type genericTypeDefinition)
    {
      ArgumentUtility.CheckNotNull ("genericTypeDefinition", genericTypeDefinition);
      Assertion.IsTrue (genericTypeDefinition.IsGenericTypeDefinition);

      var typeArguments = SeparatedStringBuilder.Build (",", TypeArguments, t => "[" + t.AssemblyQualifiedName + "]");
      return string.Format ("{0}[{1}]", genericTypeDefinition.FullName, typeArguments);
    }

    public Type SubstituteGenericParameters (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      var typeArgument = _parametersToArguments.GetValueOrDefault (type);
      if (typeArgument != null)
        return typeArgument;

      if (!type.IsGenericType)
        return type;

      var oldTypeArguments = type.GetGenericArguments();
      // TODO: This should be a List 'newTypeArguments', not a Dictionary.
      var mapping = oldTypeArguments.ToDictionary (a => a, SubstituteGenericParameters);

      // No substitution necessary (this is an optimization only).
      // TODO: Either remove, or change to compare oldTypeArguments.SequenceEqual (newTypeArguments).
      if (mapping.All (pair => pair.Key == pair.Value))
        return type;

      // TODO: var typeDefinition = type.GetGenericTypeDefinition();
      
      // TODO Later: return typeDefinition.MakeTypePipeGenericType (mapping); - and move the code below to that API

      // Make RuntimeType if all type arguments are RuntimeTypes.
      // if (newTypeArguments.All (typeArg => typeArg.IsRuntimeType()))
      if (mapping.Values.All (typeArg => typeArg.IsRuntimeType()))
      {
        // TODO: Remove this.
        // Do not simply use mapping.Values (because order matters).
        //var typeArguments = typeParameters.Select (typeParam => mapping[typeParam]).ToArray ();
        var typeArguments = mapping.Values.ToArray();

        // TODO: return typeDefinition.MakeGenericType (newTypeArguments)
        return type.MakeGenericType (typeArguments);
      }
      else
      {
        var typeInstantiator = new TypeInstantiator (_memberSelector, _underlyingTypeFactory, mapping);
        
        // TODO: return new TypeInstantiation (_memberSelector, _underlyingTypeFactory, typeDefinition, newTypeArguments);
        return new TypeInstantiation (_memberSelector, _underlyingTypeFactory, typeInstantiator, type);
      }
    }

    public FieldInfo SubstituteGenericParameters (TypeInstantiation declaringType, FieldInfo field)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("field", field);

      return new FieldOnTypeInstantiation (declaringType, this, field);
    }

    public ConstructorInfo SubstituteGenericParameters (TypeInstantiation declaringType, ConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("constructor", constructor);

      return new ConstructorOnTypeInstantiation (declaringType, this, constructor);
    }

    public MethodInfo SubstituteGenericParameters (TypeInstantiation declaringType, MethodInfo method)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("method", method);

      return new MethodOnTypeInstantiation (declaringType, this, method);
    }

    public ParameterInfo SubstituteGenericParameters (MemberInfo member, ParameterInfo parameter)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      ArgumentUtility.CheckNotNull ("parameter", parameter);

      return parameter;
    }

    public PropertyInfo SubstituteGenericParameters (TypeInstantiation declaringType, PropertyInfo property)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("property", property);

      return property;
    }

    public EventInfo SubstituteGenericParameters (TypeInstantiation declaringType, EventInfo event_)
    {
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);
      ArgumentUtility.CheckNotNull ("event_", event_);

      return event_;
    }
  }
}