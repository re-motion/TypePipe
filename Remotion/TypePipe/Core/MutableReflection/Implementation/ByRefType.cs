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
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// Represents a <see cref="Type"/> that is passed by reference.
  /// </summary>
  public class ByRefType : CustomType
  {
    private readonly CustomType _elementType;

    public ByRefType (CustomType elementType, IMemberSelector memberSelector)
        : base (
            memberSelector,
            ArgumentUtility.CheckNotNull ("elementType", elementType).Name + "&",
            elementType.Namespace,
            elementType.FullName + "&",
            TypeAttributes.NotPublic,
            null,
            EmptyTypes)
    {
      _elementType = elementType;
    }

    public override Type GetElementType ()
    {
      return _elementType;
    }

    protected override bool IsByRefImpl ()
    {
      return true;
    }

    protected override IEnumerable<Type> GetAllInterfaces ()
    {
      return Type.EmptyTypes;
    }

    protected override IEnumerable<FieldInfo> GetAllFields ()
    {
      return Enumerable.Empty<FieldInfo> ();
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      return Enumerable.Empty<ConstructorInfo>();
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      return Enumerable.Empty<MethodInfo> ();
    }

    protected override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      return Enumerable.Empty<PropertyInfo> ();
    }

    protected override IEnumerable<EventInfo> GetAllEvents ()
    {
      return Enumerable.Empty<EventInfo>();
    }

    public override IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      throw new NotImplementedException ();
    }

    public override InterfaceMapping GetInterfaceMap (Type interfaceType)
    {
      // tODO not supported in customType 
      // MutableType is the only type which implements this.

      throw new NotImplementedException ();
    }
  }
}