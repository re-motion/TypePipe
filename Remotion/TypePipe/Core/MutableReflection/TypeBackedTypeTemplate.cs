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
  // TODO Docs
  public class TypeBackedTypeTemplate : ITypeTemplate
  {
    private readonly Type _originalType;

    public TypeBackedTypeTemplate (Type originalType)
    {
      ArgumentUtility.CheckNotNull ("originalType", originalType);

      if (CanNotBeSubclassed (originalType))
        throw new ArgumentException ("Original type must not be sealed, an interface, a value type, an enum, a delegate, contain generic"
          + " parameters and must have an accessible constructor.", "originalType");

      _originalType = originalType;
    }

    public Type OriginalType
    {
      get { return _originalType; }
    }

    public Type GetBaseType ()
    {
      return _originalType.BaseType;
    }

    public TypeAttributes GetAttributeFlags ()
    {
      return _originalType.Attributes;
    }

    public Type[] GetInterfaces ()
    {
      return _originalType.GetInterfaces();
    }

    public FieldInfo[] GetFields (BindingFlags bindingAttr)
    {
      return _originalType.GetFields (bindingAttr);
    }

    public ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      return _originalType.GetConstructors (bindingAttr);
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