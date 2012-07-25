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
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// A custom type that re-implements parts of the reflection API.
  /// Other classes may derive from this class to inherit this implementation.
  /// </summary>
  public abstract class CustomType : Type
  {

    #region Unsupported Members

    public override int MetadataToken
    {
      get { throw new NotSupportedException ("Property MetadataToken is not supported."); }
    }

    public override Guid GUID
    {
      get { throw new NotSupportedException ("Property GUID is not supported."); }
    }

    public override string AssemblyQualifiedName
    {
      get { throw new NotSupportedException ("Property AssemblyQualifiedName is not supported."); }
    }

    public override StructLayoutAttribute StructLayoutAttribute
    {
      get { throw new NotSupportedException ("Property StructLayoutAttribute is not supported."); }
    }

    public override GenericParameterAttributes GenericParameterAttributes
    {
      get { throw new NotSupportedException ("Property GenericParameterAttributes is not supported."); }
    }

    public override int GenericParameterPosition
    {
      get { throw new NotSupportedException ("Property GenericParameterPosition is not supported."); }
    }

    public override RuntimeTypeHandle TypeHandle
    {
      get { throw new NotSupportedException ("Property TypeHandle is not supported."); }
    }

    public override MemberInfo[] GetDefaultMembers ()
    {
      throw new NotSupportedException ("Method GetDefaultMembers is not supported.");
    }

    public override InterfaceMapping GetInterfaceMap (Type interfaceType)
    {
      throw new NotSupportedException ("Method GetInterfaceMap is not supported.");
    }

    public override object InvokeMember (string name, BindingFlags invokeAttr, Binder binderOrNull, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
    {
      throw new NotSupportedException ("Method InvokeMember is not supported.");
    }

    public override Type MakePointerType ()
    {
      throw new NotSupportedException ("Method MakePointerType is not supported.");
    }

    public override Type MakeByRefType ()
    {
      throw new NotSupportedException ("Method MakeByRefType is not supported.");
    }

    public override Type MakeArrayType ()
    {
      throw new NotSupportedException ("Method MakeArrayType is not supported.");
    }

    public override Type MakeArrayType (int rank)
    {
      throw new NotSupportedException ("Method MakeArrayType is not supported.");
    }

    public override int GetArrayRank ()
    {
      throw new NotSupportedException ("Method GetArrayRank is not supported.");
    }

    public override Type[] GetGenericParameterConstraints ()
    {
      throw new NotSupportedException ("Method GetGenericParameterConstraints is not supported.");
    }

    public override Type MakeGenericType (params Type[] typeArguments)
    {
      throw new NotSupportedException ("Method MakeGenericType is not supported.");
    }

    public override Type[] GetGenericArguments ()
    {
      throw new NotSupportedException ("Method GetGenericArguments is not supported.");
    }

    public override Type GetGenericTypeDefinition ()
    {
      throw new NotSupportedException ("Method GetGenericTypeDefinition is not supported.");
    }

    #endregion
  }
}