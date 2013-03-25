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
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Represents an array <see cref="Type"/>.
  /// </summary>
  public class ArrayType : CustomType
  {
    private static string GetArrayTypeName (string elementTypeName, int rank)
    {
      var rankNotation = new string (',', rank - 1);
      return string.Format ("{0}[{1}]", elementTypeName, rankNotation);
    }

    private const BindingFlags c_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private readonly CustomType _elementType;
    private readonly ReadOnlyCollection<Type> _interfaces;
    private readonly ReadOnlyCollection<ConstructorInfo> _constructors;

    public ArrayType (CustomType elementType, int rank, IMemberSelector memberSelector)
        : base (
            memberSelector,
            GetArrayTypeName (ArgumentUtility.CheckNotNull ("elementType", elementType).Name, rank),
            elementType.Namespace,
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable,
            null,
            EmptyTypes)
    {
      _elementType = elementType;

      SetBaseType (typeof (Array));

      _interfaces = CreateInterfaces (elementType).ToList().AsReadOnly();
      _constructors = CreateConstructors().ToList().AsReadOnly();
    }

    public override Type GetElementType ()
    {
      return _elementType;
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return Enumerable.Empty<ICustomAttributeData>();
    }

    protected override bool IsArrayImpl ()
    {
      return true;
    }

    protected override IEnumerable<Type> GetAllInterfaces ()
    {
      return _interfaces;
    }

    protected override IEnumerable<FieldInfo> GetAllFields ()
    {
      return Enumerable.Empty<FieldInfo>();
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      return _constructors;
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      var attributes = MethodAttributes.Public;

      var indexParameter = new ParameterDeclaration (typeof (int), "index");
      var valueParameter = new ParameterDeclaration (_elementType, "value");

      yield return new MethodOnCustomType (this, "Address", attributes, EmptyTypes, _elementType.MakeByRefType(), new[] { indexParameter });
      yield return new MethodOnCustomType (this, "Get", attributes, EmptyTypes, _elementType, new[] { indexParameter });
      yield return new MethodOnCustomType (this, "Set", attributes, EmptyTypes, typeof (void), new[] { indexParameter, valueParameter });


      foreach (var baseMethods in typeof (Array).GetMethods(c_all))
        yield return baseMethods;
    }

    protected override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      return Enumerable.Empty<PropertyInfo>();
    }

    protected override IEnumerable<EventInfo> GetAllEvents ()
    {
      return Enumerable.Empty<EventInfo>();
    }

    private IEnumerable<Type> CreateInterfaces (Type elementType)
    {
      yield return typeof (IEnumerable<>).MakeTypePipeGenericType (elementType);
      yield return typeof (ICollection<>).MakeTypePipeGenericType (elementType);
      yield return typeof (IList<>).MakeTypePipeGenericType (elementType);

      foreach (var baseInterface in typeof (Array).GetInterfaces ())
        yield return baseInterface;
    }

    private IEnumerable<ConstructorInfo> CreateConstructors ()
    {
      var parameters = new[] { new ParameterDeclaration (typeof (int), "rank") };
      var attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;

      yield return new ConstructorOnCustomType (this, attributes, parameters);
    }
  }
}