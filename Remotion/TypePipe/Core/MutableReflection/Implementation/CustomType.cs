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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A custom type that re-implements parts of the reflection API.
  /// Other classes may derive from this class to inherit this implementation.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public abstract class CustomType : Type, ICustomAttributeDataProvider
  {
    private readonly IMemberSelector _memberSelector;

    private readonly Type _declaringType;
    private readonly Type _baseType;
    private readonly string _name;
    private readonly string _namespace;
    private readonly string _fullName;

    protected CustomType (
        IMemberSelector memberSelector,
        Type declaringType,
        Type baseType,
        string name,
        string @namespace,
        string fullName)
    {
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      // Declaring type may be null (for non-nested types).
      ArgumentUtility.CheckNotNull ("baseType", baseType);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Namespace may be null.
      ArgumentUtility.CheckNotNullOrEmpty ("fullName", fullName);
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);

      _memberSelector = memberSelector;
      _declaringType = declaringType;
      _baseType = baseType;
      _name = name;
      _namespace = @namespace;
      _fullName = fullName;
    }

    public abstract IEnumerable<ICustomAttributeData> GetCustomAttributeData ();
    public abstract override InterfaceMapping GetInterfaceMap (Type interfaceType);

    protected abstract override TypeAttributes GetAttributeFlagsImpl ();
    protected abstract IEnumerable<Type> GetAllInterfaces ();
    protected abstract IEnumerable<FieldInfo> GetAllFields ();
    protected abstract IEnumerable<ConstructorInfo> GetAllConstructors ();
    protected abstract IEnumerable<MethodInfo> GetAllMethods ();

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
      // TODO HACK!
      get { return _baseType; }
    }

    public override Type DeclaringType
    {
      get { return _declaringType; }
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

    public override Type GetElementType ()
    {
      return null;
    }

    public IEnumerable<ICustomAttributeData> GetCustomAttributeData (bool inherit)
    {
      return TypePipeCustomAttributeData.GetCustomAttributes (this, inherit);
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

    public override Type[] GetInterfaces ()
    {
      return GetAllInterfaces().ToArray();
    }

    public override Type GetInterface (string name, bool ignoreCase)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      var comparisonMode = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
      return GetAllInterfaces()
          .SingleOrDefault (
              iface => iface.Name.Equals (name, comparisonMode),
              () => new AmbiguousMatchException (string.Format ("Ambiguous interface name '{0}'.", name)));
    }

    public override FieldInfo[] GetFields (BindingFlags bindingAttr)
    {
      return _memberSelector.SelectFields (GetAllFields(), bindingAttr).ToArray ();
    }

    public override FieldInfo GetField (string name, BindingFlags bindingAttr)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      return _memberSelector.SelectSingleField (GetAllFields(), bindingAttr, name);
    }

    public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      return _memberSelector.SelectMethods (GetAllConstructors(), bindingAttr, this).ToArray();
    }

    public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
    {
      return _memberSelector.SelectMethods (GetAllMethods(), bindingAttr, this).ToArray();
    }

    protected override ConstructorInfo GetConstructorImpl (
        BindingFlags bindingAttr, Binder binderOrNull, CallingConventions callConvention, Type[] typesOrNull, ParameterModifier[] modifiersOrNull)
    {
      var binder = binderOrNull ?? DefaultBinder;
      return _memberSelector.SelectSingleMethod (GetAllConstructors(), binder, bindingAttr, null, this, typesOrNull, modifiersOrNull);
    }

    protected override MethodInfo GetMethodImpl (
        string name,
        BindingFlags bindingAttr,
        Binder binderOrNull,
        CallingConventions callConvention,
        Type[] typesOrNull,
        ParameterModifier[] modifiersOrNull)
    {
      var binder = binderOrNull ?? DefaultBinder;
      return _memberSelector.SelectSingleMethod (GetAllMethods(), binder, bindingAttr, name, this, typesOrNull, modifiersOrNull);
    }

    protected override bool HasElementTypeImpl ()
    {
      return false;
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

    public override string ToString ()
    {
      return SignatureDebugStringGenerator.GetTypeSignature (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("{0} = \"{1}\"", GetType().Name, ToString());
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