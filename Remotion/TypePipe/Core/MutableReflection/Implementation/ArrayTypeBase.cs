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
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A base class for <see cref="Type"/>s representing arrays, that is, vectors and multidimensional arrays.
  /// </summary>
  public abstract class ArrayTypeBase : CustomType
  {
    private static string GetArrayTypeName (string elementTypeName, int rank)
    {
      var rankNotation = new string (',', rank - 1);
      return string.Format ("{0}[{1}]", elementTypeName, rankNotation);
    }

    private const BindingFlags c_all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private readonly CustomType _elementType;
    private readonly int _rank;
    private readonly ReadOnlyCollection<Type> _interfaces;
    private readonly ReadOnlyCollection<ConstructorInfo> _constructors;
    private readonly ReadOnlyCollection<MethodInfo> _methods;
    private readonly ReadOnlyCollection<PropertyInfo> _properties;

    protected ArrayTypeBase (CustomType elementType, int rank, IMemberSelector memberSelector)
        : base (
            memberSelector,
            GetArrayTypeName (ArgumentUtility.CheckNotNull ("elementType", elementType).Name, rank),
            elementType.Namespace,
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable,
            null,
            EmptyTypes)
    {
      Assertion.IsTrue (rank > 0);
      _elementType = elementType;
      _rank = rank;

      SetBaseType (typeof (Array));

      // ReSharper disable DoNotCallOverridableMethodsInConstructor
      _interfaces = CreateInterfaces (elementType).ToList().AsReadOnly();
      _constructors = CreateConstructors (rank).ToList().AsReadOnly();
      // ReSharper restore DoNotCallOverridableMethodsInConstructor
      _methods = CreateMethods (elementType, rank).ToList().AsReadOnly();
      _properties = CreateProperties().ToList().AsReadOnly();
    }

    protected abstract IEnumerable<Type> CreateInterfaces (CustomType elementType);
    protected abstract IEnumerable<ConstructorInfo> CreateConstructors (int rank);

    public override Type GetElementType ()
    {
      return _elementType;
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return Enumerable.Empty<ICustomAttributeData>();
    }

    public override int GetArrayRank ()
    {
      return _rank;
    }

    public override bool Equals (object obj)
    {
      return Equals (obj as Type);
    }

    public override bool Equals (Type type)
    {
      var other = type as ArrayTypeBase;
      if (other == null)
        return false;

      return Equals (_elementType, other._elementType) && _rank == other._rank;
    }

    public override int GetHashCode ()
    {
      return EqualityUtility.GetRotatedHashCode (_elementType, _rank);
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
      return _methods;
    }

    protected override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      return _properties;
    }

    protected override IEnumerable<EventInfo> GetAllEvents ()
    {
      return Enumerable.Empty<EventInfo>();
    }

    private IEnumerable<MethodInfo> CreateMethods (CustomType elementType, int rank)
    {
      var attributes = MethodAttributes.Public;
      var indexParameters = Enumerable.Range (0, rank).Select (i => new ParameterDeclaration (typeof (int), "index" + i)).ToList();
      var valueParameter = new ParameterDeclaration (elementType, "value");

      yield return new MethodOnCustomType (this, "Address", attributes, EmptyTypes, elementType.MakeByRefType(), indexParameters);
      yield return new MethodOnCustomType (this, "Get", attributes, EmptyTypes, elementType, indexParameters);
      yield return new MethodOnCustomType (this, "Set", attributes, EmptyTypes, typeof (void), indexParameters.Concat (valueParameter));

      foreach (var baseMethods in typeof (Array).GetMethods (c_all))
        yield return baseMethods;
    }

    private IEnumerable<PropertyInfo> CreateProperties ()
    {
      return typeof (Array).GetProperties (c_all);
    }
  }
}