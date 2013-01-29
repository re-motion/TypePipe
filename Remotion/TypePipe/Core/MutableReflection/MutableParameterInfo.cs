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
using System.Linq;
using System.Reflection;
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
    private readonly int _position;
    private readonly string _name;
    private readonly Type _type;
    private readonly ParameterAttributes _attributes;

    private readonly CustomAttributeContainer _customAttributeContainer = new CustomAttributeContainer();

    public MutableParameterInfo (MemberInfo member, int position, string name, Type type, ParameterAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      // Name may be null.
      ArgumentUtility.CheckNotNull ("type", type);
      Assertion.IsTrue (type != typeof (void) || position == -1);
      Assertion.IsTrue (position >= -1);

      _member = member;
      _position = position;
      _name = name;
      _type = type;
      _attributes = attributes;
    }

    public override MemberInfo Member
    {
      get { return _member; }
    }

    public override int Position
    {
      get { return _position; }
    }

    public override string Name
    {
      get { return _name; }
    }

    public override Type ParameterType
    {
      get { return _type; }
    }

    public override ParameterAttributes Attributes
    {
      get { return _attributes; }
    }

    public ReadOnlyCollection<CustomAttributeDeclaration> AddedCustomAttributes
    {
      get { return _customAttributeContainer.AddedCustomAttributes; }
    }

    public void AddCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDeclaration", customAttributeDeclaration);

      _customAttributeContainer.AddCustomAttribute (customAttributeDeclaration);
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData ()
    {
      return _customAttributeContainer.AddedCustomAttributes.Cast<ICustomAttributeData>();
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