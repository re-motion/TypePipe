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

namespace Remotion.TypePipe.MutableReflection.Generics
{
  public class GenericParameter : CustomType
  {
    private readonly GenericParameterAttributes _genericParameterAttributes;
    private readonly Type _baseTypeConstraint;
    private readonly ReadOnlyCollection<Type> _interfaceConstraints;

    public GenericParameter (
        IMemberSelector memberSelector,
        string name,
        GenericParameterAttributes genericParameterAttributes,
        Type baseTypeConstraint,
        IEnumerable<Type> interfaceConstraints)
        : base (memberSelector, name, "namespace", "fullname", TypeAttributes.ReservedMask, false, false, EmptyTypes)
    {
      _genericParameterAttributes = genericParameterAttributes;
      _baseTypeConstraint = baseTypeConstraint;
      _interfaceConstraints = interfaceConstraints.ToList().AsReadOnly();

      SetBaseType (_baseTypeConstraint);
    }

    public override Type UnderlyingSystemType
    {
      get { throw new NotImplementedException(); }
    }

    public override bool IsGenericParameter
    {
      get { return true; }
    }

    public override GenericParameterAttributes GenericParameterAttributes
    {
      get { return _genericParameterAttributes; }
    }

    public override Type[] GetGenericParameterConstraints ()
    {
      return new[] { _baseTypeConstraint }.Where (c => c != typeof (object))
                                          .Concat (_interfaceConstraints).ToArray();
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
      return _interfaceConstraints;
    }

    protected override IEnumerable<FieldInfo> GetAllFields ()
    {
      throw new NotImplementedException();
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      throw new NotImplementedException();
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      throw new NotImplementedException();
    }

    protected override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      throw new NotImplementedException();
    }

    protected override IEnumerable<EventInfo> GetAllEvents ()
    {
      throw new NotImplementedException();
    }
  }
}