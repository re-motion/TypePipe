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
  /// Represents an array <see cref="Type"/>.
  /// </summary>
  public class MultiDimensionalArrayType : CustomType
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

    public MultiDimensionalArrayType (CustomType elementType, int rank, IMemberSelector memberSelector)
        : base (
            memberSelector,
            GetArrayTypeName (ArgumentUtility.CheckNotNull ("elementType", elementType).Name, rank),
            elementType.Namespace,
            TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable,
            null,
            EmptyTypes)
    {
      _elementType = elementType;
      _rank = rank;

      SetBaseType (typeof (Array));

      _interfaces = CreateInterfaces (elementType).ToList().AsReadOnly();
      _constructors = CreateConstructors().ToList().AsReadOnly();
      _methods = CreateMethods (elementType).ToList().AsReadOnly();
    }

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

      foreach (var baseInterface in typeof (Array).GetInterfaces())
        yield return baseInterface;
    }

    private IEnumerable<ConstructorInfo> CreateConstructors ()
    {
      var attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      var lengthParameters = Enumerable.Range (0, _rank).Select (i => new ParameterDeclaration (typeof (int), "length" + i)).ToList();
      var lowerBoundParameters = Enumerable.Range (0, _rank).Select (i => new ParameterDeclaration (typeof (int), "lowerBound" + i));

      yield return new ConstructorOnCustomType (this, attributes, lengthParameters);
      yield return new ConstructorOnCustomType (this, attributes, Interleave (lowerBoundParameters, lengthParameters));
    }

    private IEnumerable<MethodInfo> CreateMethods (CustomType elementType)
    {
      var attributes = MethodAttributes.Public;
      var indexParameters = Enumerable.Range (0, _rank).Select (i => new ParameterDeclaration (typeof (int), "index" + i)).ToList();
      var valueParameter = new ParameterDeclaration (elementType, "value");

      yield return new MethodOnCustomType (this, "Address", attributes, EmptyTypes, elementType.MakeByRefType(), indexParameters);
      yield return new MethodOnCustomType (this, "Get", attributes, EmptyTypes, elementType, indexParameters);
      yield return new MethodOnCustomType (this, "Set", attributes, EmptyTypes, typeof (void), indexParameters.Concat (valueParameter));

      foreach (var baseMethods in typeof (Array).GetMethods (c_all))
        yield return baseMethods;
    }

    private static IEnumerable<T> Interleave<T> (
        IEnumerable<T> first,
        IEnumerable<T> second)
    {
      using (IEnumerator<T>
                 enumerator1 = first.GetEnumerator(),
                 enumerator2 = second.GetEnumerator())
      {
        while (enumerator1.MoveNext())
        {
          yield return enumerator1.Current;
          if (enumerator2.MoveNext())
            yield return enumerator2.Current;
        }
      }
    }
  }
}