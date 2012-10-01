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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="ParameterInfo"/> that can be modified.
  /// This allows to represent parameters for <see cref="MutableMethodInfo"/> or <see cref="MutableConstructorInfo"/> instances.
  /// </summary>
  public class MutableParameterInfo : ParameterInfo, ITypePipeCustomAttributeProvider
  {
    private readonly MemberInfo _member;
    private readonly int _position;
    private readonly UnderlyingParameterInfoDescriptor _underlyingParameterInfoDescriptor;

    private readonly DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> _customAttributeDatas;

    public MutableParameterInfo (MemberInfo member, int position, UnderlyingParameterInfoDescriptor underlyingParameterInfoDescriptor)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      ArgumentUtility.CheckNotNull ("underlyingParameterInfoDescriptor", underlyingParameterInfoDescriptor);

      _member = member;
      _position = position;
      _underlyingParameterInfoDescriptor = underlyingParameterInfoDescriptor;

      _customAttributeDatas =
          new DoubleCheckedLockingContainer<ReadOnlyCollection<ICustomAttributeData>> (underlyingParameterInfoDescriptor.CustomAttributeDataProvider);
    }

    public override MemberInfo Member
    {
      get { return _member; }
    }

    public override int Position
    {
      get { return _position; }
    }

    public ParameterInfo UnderlyingSystemParameterInfo
    {
      get { return _underlyingParameterInfoDescriptor.UnderlyingSystemInfo ?? this; }
    }

    public override Type ParameterType
    {
      get { return _underlyingParameterInfoDescriptor.Type; }
    }

    public override string Name
    {
      get { return _underlyingParameterInfoDescriptor.Name; }
    }

    public override ParameterAttributes Attributes
    {
      get { return _underlyingParameterInfoDescriptor.Attributes; }
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeDatas.Value;
    }
  }
}