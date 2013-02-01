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
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  // TODO docs
  public class TypeInstantiation : CustomType
  {
    private const BindingFlags c_allBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

    private readonly ReadOnlyCollection<Type> _interfaces;
    private readonly IEnumerable<FieldInfo> _fields;

    public TypeInstantiation (
        IMemberSelector memberSelector,
        IUnderlyingTypeFactory underlyingTypeFactory,
        ITypeInstantiator typeInstantiator,
        Type genericTypeDefinition)
        : base (
            memberSelector,
            underlyingTypeFactory,
            null,
            typeInstantiator.SubstituteGenericParameters (genericTypeDefinition.BaseType),
            typeInstantiator.GetSimpleName (genericTypeDefinition),
            genericTypeDefinition.Namespace,
            typeInstantiator.GetFullName (genericTypeDefinition),
            genericTypeDefinition.Attributes,
            isGenericType: true,
            isGenericTypeDefinition: true)
    {
      Assertion.IsTrue (genericTypeDefinition.IsGenericTypeDefinition);

      _interfaces = genericTypeDefinition.GetInterfaces().Select (typeInstantiator.SubstituteGenericParameters).ToList().AsReadOnly();
      _fields = genericTypeDefinition.GetFields (c_allBindingFlags).Select (typeInstantiator.SubstituteGenericParameters).ToList().AsReadOnly();
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
      throw new NotImplementedException();
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      throw new NotImplementedException();
    }
  }
}