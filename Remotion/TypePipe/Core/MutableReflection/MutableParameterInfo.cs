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
using System.Diagnostics;
using System.Reflection;
using Remotion.TypePipe.MutableReflection.Descriptors;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="ParameterInfo"/> that can be modified.
  /// This allows to represent parameters for <see cref="MutableMethodInfo"/> or <see cref="MutableConstructorInfo"/> instances.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public class MutableParameterInfo : ParameterInfo, IMutableInfo
  {
    private readonly MemberInfo _member;
    private readonly ParameterDescriptor _descriptor;

    private readonly MutableInfoCustomAttributeContainer _customAttributeContainer;

    public MutableParameterInfo (MemberInfo member, ParameterDescriptor descriptor)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      ArgumentUtility.CheckNotNull ("descriptor", descriptor);

      _member = member;
      _descriptor = descriptor;

      _customAttributeContainer = new MutableInfoCustomAttributeContainer (descriptor.CustomAttributeDataProvider, () => CanAddCustomAttributes);
    }

    public override MemberInfo Member
    {
      get { return _member; }
    }

    public override int Position
    {
      get { return _descriptor.Position; }
    }

    public ParameterInfo UnderlyingSystemParameterInfo
    {
      get { return _descriptor.UnderlyingSystemInfo ?? this; }
    }

    public bool IsNew
    {
      get { return _descriptor.UnderlyingSystemInfo == null; }
    }

    public bool IsModified
    {
      get { return AddedCustomAttributeDeclarations.Count != 0; }
    }

    public override Type ParameterType
    {
      get { return _descriptor.Type; }
    }

    public override string Name
    {
      get { return _descriptor.Name; }
    }

    public override ParameterAttributes Attributes
    {
      get { return _descriptor.Attributes; }
    }

    public bool CanAddCustomAttributes
    {
      // TODO 4695
      get { return IsNew; }
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributeDeclarations
    {
      get { return _customAttributeContainer.AddedCustomAttributeDeclarations; }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDeclaration", customAttributeDeclaration);

      _customAttributeContainer.AddCustomAttribute (customAttributeDeclaration);
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeContainer.GetCustomAttributeData();
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData (bool inherit)
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (this);
    }

    public override object[] GetCustomAttributes (bool inherit)
    {
      return CustomAttributeFinder.GetCustomAttributes (this, inherit);
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return CustomAttributeFinder.GetCustomAttributes (this, attributeType, inherit);
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      ArgumentUtility.CheckNotNull ("attributeType", attributeType);

      return CustomAttributeFinder.IsDefined (this, attributeType, inherit);
    }

    public override string ToString ()
    {
      return SignatureDebugStringGenerator.GetParameterSignature (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("MutableParameter = \"{0}\", DeclaringMember = \"{1}\"", ToString(), Member.Name);
    }
  }
}