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

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Serves as a base for all descriptor classes.
  /// </summary>
  /// <typeparam name="TMember">The type of the member.</typeparam>
  public abstract class UnderlyingDescriptorBase<TMember>
      where TMember: class
  {
    protected static Func<ReadOnlyCollection<ICustomAttributeData>> GetCustomAttributeProvider (MemberInfo member)
    {
      return () => CustomAttributeData.GetCustomAttributes (member)
                       .Select (x => new CustomAttributeDataAdapter (x))
                       .Cast<ICustomAttributeData>().ToList().AsReadOnly();
    }

    protected static Func<ReadOnlyCollection<ICustomAttributeData>> GetCustomAttributeProvider (ParameterInfo parameter)
    {
      return () => CustomAttributeData.GetCustomAttributes (parameter)
                       .Select (x => new CustomAttributeDataAdapter (x))
                       .Cast<ICustomAttributeData>().ToList().AsReadOnly();
    }

    private readonly TMember _underlyingSystemMember;
    private readonly string _name;
    private readonly Func<ReadOnlyCollection<ICustomAttributeData>> _customAttributeDataProvider;

    protected UnderlyingDescriptorBase (
        TMember underlyingSystemMember, string name, Func<ReadOnlyCollection<ICustomAttributeData>> customAttributeDataProvider)
    {
      Assertion.IsTrue (underlyingSystemMember == null || underlyingSystemMember is MemberInfo || underlyingSystemMember is ParameterInfo);
      Assertion.IsNotNull (name);
      Assertion.IsNotNull (customAttributeDataProvider);

      _underlyingSystemMember = underlyingSystemMember;
      _name = name;
      _customAttributeDataProvider = customAttributeDataProvider;
    }

    public TMember UnderlyingSystemMember
    {
      get { return _underlyingSystemMember; }
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