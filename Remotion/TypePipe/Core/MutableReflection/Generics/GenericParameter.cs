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
  /// <summary>
  /// Represents a generic type parameter on a generic type or method definition.
  /// </summary>
  public class GenericParameter : CustomType
  {
    private const BindingFlags c_allMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    private readonly GenericParameterAttributes _genericParameterAttributes;
    private readonly Type _baseTypeConstraint;
    private readonly ReadOnlyCollection<Type> _interfaceConstraints;

    public GenericParameter (
        IMemberSelector memberSelector,
        string name,
        string @namespace,
        GenericParameterAttributes genericParameterAttributes,
        Type baseTypeConstraint,
        IEnumerable<Type> interfaceConstraints)
        : base (
            memberSelector,
            name,
            @namespace,
            fullName: null,
            attributes: TypeAttributes.Public,
            isGenericType: false,
            isGenericTypeDefinition: false,
            typeArguments: EmptyTypes)
    {
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Namespace may be null.
      ArgumentUtility.CheckNotNull ("baseTypeConstraint", baseTypeConstraint);
      ArgumentUtility.CheckNotNull ("interfaceConstraints", interfaceConstraints);

      _genericParameterAttributes = genericParameterAttributes;
      _baseTypeConstraint = baseTypeConstraint;
      _interfaceConstraints = interfaceConstraints.ToList().AsReadOnly();

      SetBaseType (_baseTypeConstraint);
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
      return _baseTypeConstraint.GetInterfaces().Concat (_interfaceConstraints);
    }

    protected override IEnumerable<FieldInfo> GetAllFields ()
    {
      return _baseTypeConstraint.GetFields(c_allMembers);
    }

    protected override IEnumerable<ConstructorInfo> GetAllConstructors ()
    {
      if ((_genericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint)
          == GenericParameterAttributes.DefaultConstructorConstraint)
        yield return new GenericParameterDefaultConstructor (this);
    }

    protected override IEnumerable<MethodInfo> GetAllMethods ()
    {
      return _baseTypeConstraint.GetMethods (c_allMembers);
    }

    protected override IEnumerable<PropertyInfo> GetAllProperties ()
    {
      return _baseTypeConstraint.GetProperties (c_allMembers);
    }

    protected override IEnumerable<EventInfo> GetAllEvents ()
    {
      return _baseTypeConstraint.GetEvents (c_allMembers);
    }
  }
}