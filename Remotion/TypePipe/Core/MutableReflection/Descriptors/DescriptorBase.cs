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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Descriptors
{
  /// <summary>
  /// Serves as a base for all descriptor classes.
  /// </summary>
  /// <typeparam name="TInfo">The type of the member (or <see cref="ParameterInfo"/> which is not derived from <see cref="MemberInfo"/>).</typeparam>
  public abstract class DescriptorBase<TInfo>
      where TInfo: class
  {
// ReSharper disable StaticFieldInGenericType
    protected static readonly Func<ReadOnlyCollection<ICustomAttributeData>> EmptyCustomAttributeDataProvider =
        () => new ICustomAttributeData[0].ToList().AsReadOnly();
// ReSharper restore StaticFieldInGenericType

    protected static Func<ReadOnlyCollection<ICustomAttributeData>> GetCustomAttributeProvider (MemberInfo member)
    {
      return () => TypePipeCustomAttributeData.GetCustomAttributes (member).ToList().AsReadOnly();
    }

    protected static Func<ReadOnlyCollection<ICustomAttributeData>> GetCustomAttributeProvider (ParameterInfo parameter)
    {
      return () => TypePipeCustomAttributeData.GetCustomAttributes (parameter).ToList().AsReadOnly();
    }

    private readonly TInfo _underlyingSystemInfo;
    private readonly string _name;
    private readonly Func<ReadOnlyCollection<ICustomAttributeData>> _customAttributeDataProvider;

    protected DescriptorBase (
        TInfo underlyingInfo, string name, Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider)
    {
      Assertion.IsTrue (underlyingInfo == null || underlyingInfo is MemberInfo || underlyingInfo is ParameterInfo);
      Assertion.IsNotNull (name);
      Assertion.IsNotNull (customAttributeDataProvider);

      _underlyingSystemInfo = underlyingInfo;
      _name = name;
      _customAttributeDataProvider = customAttributeDataProvider;
    }

    public TInfo UnderlyingSystemInfo
    {
      get { return _underlyingSystemInfo; }
    }

    public string Name
    {
      get { return _name; }
    }

    public Func<ReadOnlyCollection<ICustomAttributeData>> CustomAttributeDataProvider
    {
      get { return _customAttributeDataProvider; }
    }
  }
}