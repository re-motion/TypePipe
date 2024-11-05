﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Remotion.FunctionalProgramming;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Implementation
{
  /// <summary>
  /// A custom <see cref="Type"/> that re-implements parts of the reflection API. Other classes may derive from this class to inherit the implementation.
  /// Note that the equality members <see cref="Equals(object)"/>, <see cref="Equals(System.Type)"/> and <see cref="GetHashCode"/> are implemented for
  /// reference equality.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public abstract class CustomType : Type, ICustomAttributeDataProvider
  {
    private readonly IMemberSelector _memberSelector = new MemberSelector (new BindingFlagsEvaluator());

    private readonly string _name;
    private readonly string _namespace;
    private readonly TypeAttributes _attributes;
    private readonly Type _genericTypeDefinition;
    private readonly IReadOnlyList<Type> _typeArguments;

    private Type _declaringType;
    private Type _baseType;

    protected CustomType (
        string name,
        string @namespace,
        TypeAttributes attributes,
        Type genericTypeDefinition,
        IEnumerable<Type> typeArguments)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      // Namespace may be null.
      // Generic type definition may be null (for non-generic types and generic type definitions).
      ArgumentUtility.CheckNotNull ("typeArguments", typeArguments);

      _name = name;
      _namespace = @namespace;
      _attributes = attributes;
      _genericTypeDefinition = genericTypeDefinition;
      _typeArguments = typeArguments.ToList().AsReadOnly();

      Assertion.IsTrue (genericTypeDefinition == null || genericTypeDefinition.GetGenericArguments ().Length == _typeArguments.Count);
    }

    public abstract IEnumerable<ICustomAttributeData> GetCustomAttributeData ();

    public abstract IEnumerable<Type> GetAllNestedTypes ();
    public abstract IEnumerable<Type> GetAllInterfaces ();
    public abstract IEnumerable<FieldInfo> GetAllFields ();
    public abstract IEnumerable<ConstructorInfo> GetAllConstructors ();
    public abstract IEnumerable<MethodInfo> GetAllMethods ();
    public abstract IEnumerable<PropertyInfo> GetAllProperties ();
    public abstract IEnumerable<EventInfo> GetAllEvents ();

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

    // ReSharper disable AssignNullToNotNullAttribute
    public override Assembly Assembly
    {
      get { return null; }
    }
    // ReSharper restore AssignNullToNotNullAttribute

    // ReSharper disable AssignNullToNotNullAttribute
    public override Module Module
    {
      get { return null; }
    }
    // ReSharper restore AssignNullToNotNullAttribute

    public override Type DeclaringType
    {
      get { return _declaringType; }
    }

    public override MemberTypes MemberType
    {
      get { return _declaringType == null ? MemberTypes.TypeInfo : MemberTypes.NestedType; }
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
      get
      {
        var name = new StringBuilder();

        if (!string.IsNullOrEmpty (_namespace))
        {
          name.Append (_namespace);
          name.Append ('.');
        }

        var declaringTypes = EnumerableExtensions.CreateSequence<Type> (this, x => x.DeclaringType).Reverse();
        name.Append (string.Join ("+", declaringTypes.Select (t => t.Name)));

        if (IsGenericType)
        {
          name.Append ('[');
          name.Append (string.Join (",", _typeArguments.Select (t => '[' + t.AssemblyQualifiedName + ']')));
          name.Append (']');
        }

        return name.ToString();
      }
    }

    public override string AssemblyQualifiedName
    {
      get { return FullName + ", TypePipe_GeneratedAssembly"; }
    }

    public override bool IsGenericType
    {
      get { return _typeArguments.Count > 0; }
    }

    public override bool IsGenericTypeDefinition
    {
      get { return IsGenericType && _genericTypeDefinition == null; }
    }

    public override Type GetGenericTypeDefinition ()
    {
      if (!IsGenericType)
        throw new InvalidOperationException ("GetGenericTypeDefinition can only be called on generic types (IsGenericType must be true).");

      return _genericTypeDefinition ?? this;
    }

    public override Type[] GetGenericArguments ()
    {
      return _typeArguments.ToArray();
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
      return ReferenceEquals (this, other);
    }

    /// <summary>
    /// Implements reference equality for <see cref="CustomType"/> derivatives.
    /// </summary>
    public override bool Equals (Type other)
    {
      return object.ReferenceEquals (this, other);
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

    public override Type MakeByRefType ()
    {
      return new ByRefType (this);
    }

    public override Type MakeArrayType ()
    {
      return new VectorType (this);
    }

    public override Type MakeArrayType (int rank)
    {
      if (rank <= 0)
        throw new ArgumentOutOfRangeException ("rank", "Array rank must be greater than zero.");

      return new MultiDimensionalArrayType (this, rank);
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

    public override Type[] GetNestedTypes(BindingFlags bindingAttr)
    {
      return _memberSelector.SelectTypes (GetAllNestedTypes(), bindingAttr).ToArray();
    }

    public override Type GetNestedType(string name, BindingFlags bindingAttr)
    {
      // TODO 4744
      // When we implement this we need to use a "NestedTypeOnTypeInstantiation" similiar to othe other MemberXXXOnTypeInstantiation.
      // Note that a generic type definition is not instantiated (at least not fully) and should "stay" a generic type definition.
      // See MethodOnTypeInstantiation constructor and GetGenericMethodDefinition. (Should work similiar for NestedTypeOnTypeInstantiation).
      // Create an integration test for this!
      ArgumentUtility.CheckNotNullOrEmpty("name", name);

      return _memberSelector.SelectSingleType (GetAllNestedTypes(), bindingAttr, name);
    }

    public override Type[] GetInterfaces ()
    {
      return GetAllInterfaces().ToArray();
    }

    public override Type GetInterface (string name, bool ignoreCase)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      var comparisonMode = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
      var interfaces = GetAllInterfaces().Where (iface => iface.Name.Equals (name, comparisonMode)).ToArray();
      if (interfaces.Length > 1)
        throw new AmbiguousMatchException (string.Format ("Ambiguous interface name '{0}'.", name));
      return interfaces.SingleOrDefault();
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
      // Performance optimization.
      var allBindingFlags = (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      if (bindingAttr == allBindingFlags)
        return GetAllMethods().ToArray();

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
    /// <seealso cref="TypeExtensions.IsTypePipeAssignableFrom"/>
    public override Type UnderlyingSystemType
    {
      get
      {
        throw new NotSupportedException (
            "Property UnderlyingSystemType is not supported. "
            + "Use a replacement method from class TypeExtensions (e.g. IsTypePipeAssignableFrom) to avoid accessing the property.");
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

    public override InterfaceMapping GetInterfaceMap (Type interfaceType)
    {
      throw new NotSupportedException ("Method GetInterfaceMap is not supported.");
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

    public override int GetArrayRank ()
    {
      throw new NotSupportedException ("Method GetArrayRank is not supported.");
    }

    #endregion
  }
}