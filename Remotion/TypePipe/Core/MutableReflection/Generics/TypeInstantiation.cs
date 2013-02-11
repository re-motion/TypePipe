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

    private static string GetFullName (InstantiationInfo info)
    {
      var typeArgumentString = SeparatedStringBuilder.Build (",", info.TypeArguments, t => "[" + t.AssemblyQualifiedName + "]");
      return string.Format ("{0}[{1}]", info.GenericTypeDefinition.FullName, typeArgumentString);
    }

    private static Type SubstituteGenericParameters (
        Type type, Dictionary<Type, Type> parametersToArguments, Dictionary<InstantiationInfo, TypeInstantiation> instantiations)
    {
      var typeArgument = parametersToArguments.GetValueOrDefault (type);
      if (typeArgument != null)
        return typeArgument;

      if (!type.IsGenericType)
        return type;

      var oldTypeArguments = type.GetGenericArguments();
      var newTypeArguments = oldTypeArguments.Select (a => SubstituteGenericParameters (a, parametersToArguments, instantiations)).ToArray();

      // No substitution necessary (this is an optimization only).
      if (oldTypeArguments.SequenceEqual (newTypeArguments))
        return type;

      var genericTypeDefinition = type.GetGenericTypeDefinition();
      var instantiationInfo = new InstantiationInfo (genericTypeDefinition, newTypeArguments);

      return instantiationInfo.MakeGenericType (instantiations);
    }

    private readonly Type _genericTypeDefinition;
    private readonly Dictionary<InstantiationInfo, TypeInstantiation> _instantiations;
    private readonly Dictionary<Type, Type> _parametersToArguments;

    private readonly ReadOnlyCollection<Type> _interfaces;
    private readonly ReadOnlyCollection<FieldInfo> _fields;
    private readonly ReadOnlyCollection<ConstructorInfo> _constructors;
    private readonly ReadOnlyCollection<MethodInfo> _methods;
    private readonly ReadOnlyCollection<PropertyInfo> _properties;
    private readonly ReadOnlyCollection<EventInfo> _events;

    public TypeInstantiation (
        IMemberSelector memberSelector,
        IUnderlyingTypeFactory underlyingTypeFactory,
        InstantiationInfo instantiationInfo,
        Dictionary<InstantiationInfo, TypeInstantiation> instantiations)
        : base (
            memberSelector,
            underlyingTypeFactory,
            null,
            instantiationInfo.GenericTypeDefinition.Name,
            instantiationInfo.GenericTypeDefinition.Namespace,
            GetFullName (instantiationInfo),
            instantiationInfo.GenericTypeDefinition.Attributes,
            isGenericType: true,
            isGenericTypeDefinition: false,
            typeArguments: instantiationInfo.TypeArguments)
    {
      _genericTypeDefinition = instantiationInfo.GenericTypeDefinition;
      _instantiations = instantiations;

      _parametersToArguments = _genericTypeDefinition
          .GetGenericArguments().Zip (instantiationInfo.TypeArguments).ToDictionary (t => t.Item1, t => t.Item2);

      // Add own instantation to context before substituting any generic parameters. 
      instantiations.Add (instantiationInfo, this); 

      if (_genericTypeDefinition.BaseType != null)
        SetBaseType (SubstituteGenericParameters (_genericTypeDefinition.BaseType));

      _interfaces = _genericTypeDefinition
          .GetInterfaces()
          .Select (SubstituteGenericParameters).ToList().AsReadOnly();
      _fields = _genericTypeDefinition
          .GetFields (c_allMembers)
          .Select (f => new FieldOnTypeInstantiation (this, f)).Cast<FieldInfo>().ToList().AsReadOnly();
      _constructors = _genericTypeDefinition
          .GetConstructors (c_allMembers)
          .Select (c => new ConstructorOnTypeInstantiation (this, this, c)).Cast<ConstructorInfo>().ToList().AsReadOnly();
      _methods = _genericTypeDefinition
          .GetMethods (c_allMembers)
          .Select (m => new MethodOnTypeInstantiation (this, this, m)).Cast<MethodInfo>().ToList().AsReadOnly();
      _properties = null;
      _events = null;
    }

    // TODO Review: better option.
    public override Type DeclaringType
    {
      get { throw new NotSupportedException ("Property DeclaringType is not supported."); }
    }

    public override Type GetGenericTypeDefinition ()
    {
      return _genericTypeDefinition;
    }

    public Type SubstituteGenericParameters (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);

      return SubstituteGenericParameters (type, _parametersToArguments, _instantiations);
    }

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