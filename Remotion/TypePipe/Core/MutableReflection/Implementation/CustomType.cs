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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Remotion.Utilities;
using Remotion.FunctionalProgramming;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A custom <see cref="Type"/> that re-implements parts of the reflection API. Other classes may derive from this class to inherit the implementation.
  /// Note that the equality members <see cref="Equals(object)"/>, <see cref="Equals(System.Type)"/> and <see cref="GetHashCode"/> are implemented for
  /// reference equality.
  /// </summary>
  /// <remarks>
  /// Avoid using the members <see cref="UnderlyingSystemType"/> and <see cref="Type.IsAssignableFrom"/>.
  /// Use <see cref="TypeExtensions.IsAssignableFromFast"/> instead.
  /// </remarks>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public abstract class CustomType : Type, ICustomAttributeDataProvider
  {
    private readonly IMemberSelector _memberSelector;

    private readonly string _name;
    private readonly string _namespace;
    private readonly string _fullName;
    private readonly TypeAttributes _attributes;
    private readonly bool _isGenericType;
    private readonly Type _genericTypeDefinition;
    private readonly ReadOnlyCollection<Type> _typeArguments;

    private Type _declaringType;
    private Type _baseType;

    protected CustomType (
        IMemberSelector memberSelector,
        string name,
        string @namespace,
        string fullName,
        TypeAttributes attributes,
        bool isGenericType,
        Type genericTypeDefinition,
        IEnumerable<Type> typeArguments)
    {
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Namespace may be null.
      // Fullname may be null (for generic parameters).
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      // Generic type definition may be null (for non-generic types and generic type definitions).
      ArgumentUtility.CheckNotNull ("typeArguments", typeArguments);

      _memberSelector = memberSelector;
      _name = name;
      _namespace = @namespace;
      _fullName = fullName;
      _attributes = attributes;
      _isGenericType = isGenericType;
      _genericTypeDefinition = genericTypeDefinition;
      _typeArguments = typeArguments.ToList().AsReadOnly();

      Assertion.IsTrue ((isGenericType && _typeArguments.Count > 0) || (!isGenericType && _typeArguments.Count == 0));
      Assertion.IsTrue ((genericTypeDefinition != null && isGenericType) || (genericTypeDefinition == null));
    }

    public abstract IEnumerable<ICustomAttributeData> GetCustomAttributeData ();
    public abstract override InterfaceMapping GetInterfaceMap (Type interfaceType);

    protected abstract IEnumerable<Type> GetAllInterfaces ();
    protected abstract IEnumerable<FieldInfo> GetAllFields ();
    protected abstract IEnumerable<ConstructorInfo> GetAllConstructors ();
    protected abstract IEnumerable<MethodInfo> GetAllMethods ();
    protected abstract IEnumerable<PropertyInfo> GetAllProperties ();
    protected abstract IEnumerable<EventInfo> GetAllEvents ();

    protected void SetDeclaringType (Type declaringType)
    {
      _declaringType = declaringType;
    }

    protected void SetBaseType (Type baseType)
    {
      Assertion.IsTrue (baseType != null || _attributes.IsSet (TypeAttributes.Interface));
      Assertion.IsTrue (baseType == null || _attributes.IsSet (TypeAttributes.ClassSemanticsMask, TypeAttributes.Class) || baseType.IsClass);

      _baseType = baseType;
    }

    public override Assembly Assembly
    {
      get { return null; }
    }

    public override Module Module
    {
      get { return null; }
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

    public override string AssemblyQualifiedName
    {
      get { return _fullName + ", TypePipe_GeneratedAssembly"; }
    }

    public override bool IsGenericType
    {
      get { return _isGenericType; }
    }

    public override bool IsGenericTypeDefinition
    {
      get { return _isGenericType && _genericTypeDefinition == null; }
    }

    public override Type GetGenericTypeDefinition ()
    {
      if (!_isGenericType)
        throw new InvalidOperationException ("GetGenericTypeDefinition can only be called on generic types (IsGenericType must be true).");

      return _genericTypeDefinition ?? this;
    }

    public override Type[] GetGenericArguments ()
    {
      return _typeArguments.ToArray ();
    }

    public override Type MakeGenericType (params Type[] typeArguments)
    {
      ArgumentUtility.CheckNotNullOrItemsNull ("typeArguments", typeArguments);

      if (!IsGenericTypeDefinition)
        throw new InvalidOperationException ("MakeGenericType can only be called on generic type definitions (IsGenericTypeDefinition must be true).");

      return this.MakeTypePipeGenericType (typeArguments);
    }

    /// <summary>
    /// Implements reference equality for <see cref="CustomType"/> derivatives.
    /// </summary>
    public override bool Equals (object other)
    {
      return this == other;
    }

    /// <summary>
    /// Implements reference equality for <see cref="CustomType"/> derivatives. The method which is hidden by this method,
    /// i.e., <see cref="Type.Equals(System.Type)"/> in class <see cref="Type"/>, still works as intended but is slower as it accesses
    /// the <see cref="UnderlyingSystemType"/> property.
    /// </summary>
    public new virtual bool Equals (Type type)
    {
      // ReSharper disable PossibleUnintendedReferenceComparison
      return this == type;
      // ReSharper restore PossibleUnintendedReferenceComparison
    }

    /// <summary>
    /// Returns a hash code based on reference equality.
    /// </summary>
    public override int GetHashCode ()
    {
      return RuntimeHelpers.GetHashCode (this);
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
      return _memberSelector.SelectFields (GetAllFields(), bindingAttr, this).ToArray();
    }

    public override FieldInfo GetField (string name, BindingFlags bindingAttr)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      return _memberSelector.SelectSingleField (GetAllFields(), bindingAttr, name, this);
    }

    public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      return _memberSelector.SelectMethods (GetAllConstructors(), bindingAttr, this).ToArray();
    }

    public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
    {
      return _memberSelector.SelectMethods (GetAllMethods(), bindingAttr, this).ToArray();
    }

    public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
    {
      return _memberSelector.SelectProperties (GetAllProperties(), bindingAttr, this).ToArray();
    }

    public override EventInfo[] GetEvents (BindingFlags bindingAttr)
    {
      return _memberSelector.SelectEvents (GetAllEvents(), bindingAttr, this).ToArray();
    }

    public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      return _memberSelector.SelectSingleEvent (GetAllEvents(), bindingAttr, name, this);
    }

    protected override TypeAttributes GetAttributeFlagsImpl ()
    {
      return _attributes;
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
      // TODO 4836: Consider CallingConventions.
      var binder = binderOrNull ?? DefaultBinder;
      return _memberSelector.SelectSingleMethod (GetAllMethods(), binder, bindingAttr, name, this, typesOrNull, modifiersOrNull);
    }

    protected override PropertyInfo GetPropertyImpl (
        string name, BindingFlags bindingAttr, Binder binderOrNull, Type returnTypeOrNull, Type[] typesOrNull, ParameterModifier[] modifiersOrNull)
    {
      var binder = binderOrNull ?? DefaultBinder;
      return _memberSelector.SelectSingleProperty (GetAllProperties(), binder, bindingAttr, name, this, returnTypeOrNull, typesOrNull, modifiersOrNull);
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

    protected override bool IsContextfulImpl ()
    {
      return false;
    }

    protected override bool IsMarshalByRefImpl ()
    {
      return false;
    }

    public override string ToString ()
    {
      return SignatureDebugStringGenerator.GetTypeSignature (this);
    }

    public string ToDebugString ()
    {
      return string.Format ("{0} = \"{1}\"", GetType ().Name, ToString ());
    }

    #region Members supported only by generic parameters

    private const string c_noGenericParameterMessage = " may only be called on a type for which Type.IsGenericParameter is true.";

    public override bool IsGenericParameter
    {
      get { return false; }
    }

    public override MethodBase DeclaringMethod
    {
      get { throw new InvalidOperationException ("Property DeclaringMethod" + c_noGenericParameterMessage); }
    }

    public override int GenericParameterPosition
    {
      get { throw new InvalidOperationException ("Property GenericParameterPosition" + c_noGenericParameterMessage); }
    }

    public override GenericParameterAttributes GenericParameterAttributes
    {
      get { throw new InvalidOperationException ("Property GenericParameterAttributes" + c_noGenericParameterMessage); }
    }

    public override Type[] GetGenericParameterConstraints ()
    {
      throw new InvalidOperationException ("Method GetGenericParameterConstraints" + c_noGenericParameterMessage);
    }

    #endregion

    #region Not YET implemented abstract members of Type class

    public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
    {
      throw new NotImplementedException ();
    }

    public override MemberInfo[] GetMember (string name, MemberTypes type, BindingFlags bindingAttr)
    {
      return new MemberInfo[0]; // Needed for GetMember(..) - virtual method check
    }

    public override Type[] GetNestedTypes (BindingFlags bindingAttr)
    {
      return new Type[0]; // Needed for virtual method check
    }

    public override Type GetNestedType (string name, BindingFlags bindingAttr)
    {
      throw new NotImplementedException ();
    }

    #endregion

    #region Unsupported Members

    /// <summary>
    /// This property is not supported due to limitations of the <see cref="System.Reflection"/> implementation.
    /// It always throws a <see cref="NotSupportedException"/>.
    /// </summary>
    /// <remarks>
    /// Avoid calling members that access this property, e.g., <see cref="Type.IsAssignableFrom"/>.
    /// See the <see cref="TypeExtensions"/> class for replacements for those members.
    /// </remarks>
    /// <exception cref="NotSupportedException">Always thrown.</exception>
    /// <seealso cref="TypeExtensions.IsAssignableFromFast"/>
    /// <seealso cref="TypeExtensions.IsSerializableFast"/>
    // TODO remove sealded
    public sealed override Type UnderlyingSystemType
    {
      get
      {
        throw new NotSupportedException (
            "Property UnderlyingSystemType is not supported. "
            + "Use a replacement method from class TypeExtensions (e.g. IsAssignableFromFast) to avoid accessing the property.");
      }
    }

    public override Type ReflectedType
    {
      get { throw new NotSupportedException ("Property ReflectedType is not supported."); }
    }

    public override int MetadataToken
    {
      get { throw new NotSupportedException ("Property MetadataToken is not supported."); }
    }

    public override Guid GUID
    {
      get { throw new NotSupportedException ("Property GUID is not supported."); }
    }

    public override StructLayoutAttribute StructLayoutAttribute
    {
      get { throw new NotSupportedException ("Property StructLayoutAttribute is not supported."); }
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

    #endregion
  }
}