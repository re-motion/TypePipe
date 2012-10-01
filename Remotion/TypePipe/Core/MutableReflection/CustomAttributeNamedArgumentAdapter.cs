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
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Adapts <see cref="CustomAttributeNamedArgument"/> to the interface <see cref="ICustomAttributeNamedArgument"/>.
  /// </summary>
  public class CustomAttributeNamedArgumentAdapter : ICustomAttributeNamedArgument
  {
    private readonly MemberInfo _member;
    private readonly Type _memberType;
    private readonly object _value;

    public CustomAttributeNamedArgumentAdapter (CustomAttributeNamedArgument customAttributeNamedArgument)
    {
      // customAttributeNamedArgument is struct
      _member = customAttributeNamedArgument.MemberInfo;
      _memberType = ReflectionUtility.GetFieldOrPropertyType (customAttributeNamedArgument.MemberInfo);
      _value = CustomAttributeTypedArgumentUtility.Unwrap (customAttributeNamedArgument.TypedValue);
    }

    public MemberInfo MemberInfo
    {
      get { return _member; }
    }

    public Type MemberType
    {
      get { return _memberType; }
    }

    public object Value
    {
      get { return _value; }
    }
  }
}