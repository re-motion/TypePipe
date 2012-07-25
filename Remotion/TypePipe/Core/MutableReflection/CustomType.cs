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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// A custom type that re-implements parts of the reflection API.
  /// Other classes may derive from this class to inherit this implementation.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public abstract class CustomType : Type
  {
    private readonly IMemberSelector _memberSelector;

    private readonly Type _underlyingSystemType;
    private readonly Type _baseType;
    private readonly TypeAttributes _typeAttributes;
    private readonly string _name;
    private readonly string _namespace;
    private readonly string _fullName;

    protected CustomType (
        IMemberSelector memberSelector,
        Type underlyingSystemType,
        Type baseType,
        TypeAttributes typeAttributes,
        string name,
        string @namespace,
        string fullName)
    {
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNull ("underlyingSystemType", underlyingSystemType);
      // Base type may be null (for type object)
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNullOrEmpty ("namespace", @namespace);
      ArgumentUtility.CheckNotNullOrEmpty ("fullName", fullName);
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);

      _memberSelector = memberSelector;
      _underlyingSystemType = underlyingSystemType;
      _baseType = baseType;
      _typeAttributes = typeAttributes;
      _name = name;
      _namespace = @namespace;
      _fullName = fullName;
    }

    public override Assembly Assembly
    {
      get { return null; }
    }

    public override Module Module
    {
      get { return null; }
    }

    public override Type UnderlyingSystemType
    {
      get { return _underlyingSystemType; }
    }

    public override Type BaseType
    {
      get { return _baseType; }
    }

    public override string Name
    {
      get { return _name; }
    }

    public override string Namespace
    {
      get { return _namespace; }
    }

    public override string FullName
    {
      get { return _fullName; }
    }

    public override string ToString ()
    {
      return SignatureDebugStringGenerator.GetTypeSignature (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("{0} = \"{1}\"", GetType().Name, ToString ());
    }

    public override Type GetElementType ()
    {
      return null;
    }

    protected override bool HasElementTypeImpl ()
    {
      return false;
    }

    protected override TypeAttributes GetAttributeFlagsImpl ()
    {
      return _typeAttributes;
    }

    protected override bool IsByRefImpl ()
    {
      return false;
    }

    protected override bool IsArrayImpl ()
    {
      return false;
    }

    protected override bool IsPointerImpl ()
    {
      return false;
    }

    protected override bool IsPrimitiveImpl ()
    {
      return false;
    }

    protected override bool IsCOMObjectImpl ()
    {
      return false;
    }

    #region Not YET implemented abstract members of Type class

    public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
    {
      throw new NotImplementedException ();
    }

    public override MemberInfo[] GetMember (string name, MemberTypes type, BindingFlags bindingAttr)
    {
      return new MemberInfo[0]; // Needed for GetMember(..) - virtual method check
    }

    public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
    {
      throw new NotImplementedException ();
    }

    public override EventInfo[] GetEvents (BindingFlags bindingAttr)
    {
      return new EventInfo[0]; // Needed for GetEvents() - virtual method check
    }

    public override Type[] GetNestedTypes (BindingFlags bindingAttr)
    {
      return new Type[0]; // Needed for virtual method check
    }

    public override Type GetNestedType (string name, BindingFlags bindingAttr)
    {
      throw new NotImplementedException ();
    }

    protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binderOrNull, Type returnType, Type[] types, ParameterModifier[] modifiers)
    {
      //types = types ?? Type.EmptyTypes;
      throw new NotImplementedException ();
    }

    public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
    {
      return new PropertyInfo[0]; // Needed for virtual method check
    }

    public override object[] GetCustomAttributes (bool inherit)
    {
      throw new NotImplementedException ();
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      throw new NotImplementedException ();
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      throw new NotImplementedException ();
    }

    #endregion

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