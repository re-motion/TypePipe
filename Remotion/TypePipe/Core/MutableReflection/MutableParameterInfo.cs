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
  /// Represents a parameter for a member that does not exist yet. This is used to represent the signature of a member that has yet to be generated
  /// within an expression tree.
  /// </summary>
  public class MutableParameterInfo : ParameterInfo
  {
    public static MutableParameterInfo CreateFromDeclaration (MemberInfo member, int position, ParameterDeclaration parameterDeclaration)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      ArgumentUtility.CheckNotNull ("parameterDeclaration", parameterDeclaration);

      return new MutableParameterInfo (member, position, parameterDeclaration.Type, parameterDeclaration.Name, parameterDeclaration.Attributes);
    }

    private readonly MemberInfo _member;
    private readonly int _position;
    private readonly Type _parameterType;
    private readonly string _name;
    private readonly ParameterAttributes _attributes;

    public MutableParameterInfo (MemberInfo member, int position, Type parameterType, string name, ParameterAttributes attributes)
    {
      ArgumentUtility.CheckNotNull ("member", member);
      ArgumentUtility.CheckNotNull ("parameterType", parameterType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      _member = member;
      _position = position;
      _parameterType = parameterType;
      _name = name;
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

    public override Type ParameterType
    {
      get { return _parameterType; }
    }

    public override string Name
    {
      get { return _name; }
    }

    public override ParameterAttributes Attributes
    {
      get { return _attributes; }
    }
  }
}