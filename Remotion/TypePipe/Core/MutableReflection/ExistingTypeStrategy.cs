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
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Defines the characteristics of an existing type.
  /// </summary>
  public class ExistingTypeStrategy : IUnderlyingTypeStrategy
  {
    private readonly Type _originalType;
    private readonly IMemberFilter _memberFilter;

    public ExistingTypeStrategy (Type originalType, IMemberFilter memberFilter)
    {
      ArgumentUtility.CheckNotNull ("originalType", originalType);
      ArgumentUtility.CheckNotNull ("memberFilter", memberFilter);

      // TODO 4695
      if (CanNotBeSubclassed (originalType))
        throw new ArgumentException ("Original type must not be sealed, an interface, a value type, an enum, a delegate, contain generic"
          + " parameters and must have an accessible constructor.", "originalType");

      _originalType = originalType;
      _memberFilter = memberFilter;
    }

    public Type UnderlyingSystemType
    {
      get { return _originalType; }
    }

    public Type BaseType
    {
      get { return _originalType.BaseType; }
    }

    public string Name
    {
      get { return _originalType.Name; }
    }

    public string Namespace
    {
      get { return _originalType.Namespace; }
    }

    public string FullName
    {
      get { return _originalType.FullName; }
    }

    public string StringRepresentation
    {
      get { return _originalType.ToString(); }
    }

    public TypeAttributes Attributes
    {
      get { return _originalType.Attributes; }
    }

    public Type[] Interfaces
    {
      get { return _originalType.GetInterfaces(); }
    }

    public FieldInfo[] Fields
    {
      get
      {
        var bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        var fieldInfos = _originalType.GetFields (bindingAttr);
        var filteredFields = _memberFilter.FilterFields (fieldInfos);

        return filteredFields;
      }
    }

    public ConstructorInfo[] Constructors
    {
      get
      {
        var bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var constructorInfos = _originalType.GetConstructors (bindingAttr);
        return _memberFilter.FilterConstructors (constructorInfos);
      }
    }

    private bool CanNotBeSubclassed (Type type)
    {
      return type.IsSealed
          || type.IsInterface
          || typeof (Delegate).IsAssignableFrom (type)
          || type.ContainsGenericParameters
          || !HasAccessibleConstructor (type);
    }

    private bool HasAccessibleConstructor (Type type)
    {
      return type.GetConstructors (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
          .Any (ctor => ctor.IsPublic || ctor.IsFamily || ctor.IsFamilyOrAssembly);
    }
  }
}