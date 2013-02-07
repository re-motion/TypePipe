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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Remotion.Collections;
using Remotion.FunctionalProgramming;
using Remotion.Text;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Represents a constructed generic type, i.e., a generic type definition that was instantiated with type arguments.
  /// This class is needed because the the original reflection classes do not work in combination with <see cref="CustomType"/> instances.
  /// </summary>
  /// <remarks>Instances of this class are returned by <see cref="TypeExtensions.MakeTypePipeGenericType"/>.</remarks>
  public class TypeInstantiation : CustomType, ITypeAdjuster
  {
    private const BindingFlags c_allMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    private readonly ReadOnlyCollection<Type> _interfaces;
    private readonly ReadOnlyCollection<FieldInfo> _fields;
    private readonly ReadOnlyCollection<ConstructorInfo> _constructors;
    private readonly ReadOnlyCollection<MethodInfo> _methods;
    private readonly ReadOnlyCollection<PropertyInfo> _properties;
    private readonly ReadOnlyCollection<EventInfo> _events;

    public static TypeInstantiation Create (
        Type genericTypeDefinition, Type[] typeArguments, IMemberSelector memberSelector, IUnderlyingTypeFactory underlyingTypeFactory)
    {
      ArgumentUtility.CheckNotNull ("genericTypeDefinition", genericTypeDefinition);
      ArgumentUtility.CheckNotNull ("typeArguments", typeArguments);
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNull ("underlyingTypeFactory", underlyingTypeFactory);
      Assertion.IsTrue (genericTypeDefinition.IsGenericTypeDefinition);
      Assertion.IsTrue (genericTypeDefinition.GetGenericArguments().Length == typeArguments.Length);

      var parametersToArguments = genericTypeDefinition.GetGenericArguments().Zip (typeArguments).ToDictionary (t => t.Item1, t => t.Item2);
      var baseType = SubstituteGenericParameters (genericTypeDefinition.BaseType, parametersToArguments, memberSelector, underlyingTypeFactory);
      var fullName = GetFullName (genericTypeDefinition, typeArguments);

      return new TypeInstantiation (
          memberSelector, underlyingTypeFactory, genericTypeDefinition, parametersToArguments, baseType, fullName, typeArguments);
    }

    private static string GetFullName (Type genericTypeDefinition, Type[] typeArguments)
    {
      var typeArgumentString = SeparatedStringBuilder.Build (",", typeArguments, t => "[" + t.AssemblyQualifiedName + "]");
      return string.Format ("{0}[{1}]", genericTypeDefinition.FullName, typeArgumentString);
    }

    private static Type SubstituteGenericParameters (
        Type type, Dictionary<Type, Type> parametersToArguments, IMemberSelector memberSelector, IUnderlyingTypeFactory underlyingTypeFactory)
    {
      if (type == null)
        return null;

      var typeArgument = parametersToArguments.GetValueOrDefault (type);
      if (typeArgument != null)
        return typeArgument;

      if (!type.IsGenericType)
        return type;

      var oldTypeArguments = type.GetGenericArguments ();
      // TODO: This should be a List 'newTypeArguments', not a Dictionary.
      var mapping = oldTypeArguments.ToDictionary (
          a => a, a => SubstituteGenericParameters (a, parametersToArguments, memberSelector, underlyingTypeFactory));

      // No substitution necessary (this is an optimization only).
      // TODO: Either remove, or change to compare oldTypeArguments.SequenceEqual (newTypeArguments).
      if (mapping.All (pair => pair.Key == pair.Value))
        return type;

      var genericTypeDefinition = type.GetGenericTypeDefinition();
      Assertion.IsNotNull (genericTypeDefinition);

      // TODO Later: return typeDefinition.MakeTypePipeGenericType (mapping); - and move the code below to that API

      var newTypeArguments = mapping.Values.ToArray ();

      // Make RuntimeType if all type arguments are RuntimeTypes.
      // if (newTypeArguments.All (typeArg => typeArg.IsRuntimeType()))
      if (mapping.Values.All (typeArg => typeArg.IsRuntimeType()))
        return genericTypeDefinition.MakeGenericType (newTypeArguments);
      else
        return Create (genericTypeDefinition, newTypeArguments, memberSelector, underlyingTypeFactory);
    }

    private readonly IMemberSelector _memberSelector;
    private readonly IUnderlyingTypeFactory _underlyingTypeFactory;
    private readonly Type _genericTypeDefinition;
    private readonly Dictionary<Type, Type> _parametersToArguments;

    private TypeInstantiation (
        IMemberSelector memberSelector,
        IUnderlyingTypeFactory underlyingTypeFactory,
        Type genericTypeDefinition,
        Dictionary<Type, Type> parametersToArguments,
        Type baseType,
        string fullName,
        Type[] typeArguments)
        : base (
            memberSelector,
            underlyingTypeFactory,
            null,
            baseType,
            genericTypeDefinition.Name,
            genericTypeDefinition.Namespace,
            fullName,
            genericTypeDefinition.Attributes,
            isGenericType: true,
            isGenericTypeDefinition: false,
            typeArguments: typeArguments)
    {
      _memberSelector = memberSelector;
      _underlyingTypeFactory = underlyingTypeFactory;
      _genericTypeDefinition = genericTypeDefinition;
      _parametersToArguments = parametersToArguments;

      _interfaces = genericTypeDefinition.GetInterfaces().Select (SubstituteGenericParameters).ToList().AsReadOnly();
      _fields = genericTypeDefinition
          .GetFields (c_allMembers)
          .Select (f => new FieldOnTypeInstantiation (this, this, f)).Cast<FieldInfo>().ToList().AsReadOnly();
      _constructors = genericTypeDefinition
          .GetConstructors (c_allMembers)
          .Select (c => new ConstructorOnTypeInstantiation (this, this, c)).Cast<ConstructorInfo>().ToList().AsReadOnly();
      _methods = genericTypeDefinition
          .GetMethods (c_allMembers)
          .Select (m => new MethodOnTypeInstantiation (this, this, m)).Cast<MethodInfo>().ToList().AsReadOnly();
      _properties = null;
      _events = null;
    }

    public override Type GetGenericTypeDefinition ()
    {
      return _genericTypeDefinition;
    }

    public Type SubstituteGenericParameters (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return SubstituteGenericParameters (type, _parametersToArguments, _memberSelector, _underlyingTypeFactory);
    }

    // TODO: override declaringType with throwing ex

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      throw new NotImplementedException();
    }

    public override InterfaceMapping GetInterfaceMap (Type interfaceType)
    {
      throw new NotImplementedException();
    }

    protected override IEnumerable<Type> GetAllInterfaces ()
    {
      return _interfaces;
    }

    protected override IEnumerable<FieldInfo> GetAllFields ()
    {
      return _fields;
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      return _constructors;
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      return _methods;
    }

    protected override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      return _properties;
    }

    protected override IEnumerable<EventInfo> GetAllEvents ()
    {
      return _events;
    }
  }
}