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
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Scripting.Ast;
using Remotion.Collections;
using Remotion.FunctionalProgramming;
using Remotion.Reflection.MemberSignatures;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.MutableReflection
{
  /// <summary>
  /// Represents a <see cref="Type"/> that can be changed. Changes are recorded and, depending on the concrete <see cref="MutableType"/>, applied
  /// to an existing type or to a newly created type.
  /// </summary>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public class MutableType : Type
  {
    private readonly UnderlyingTypeDescriptor _underlyingTypeDescriptor;
    private readonly IBindingFlagsEvaluator _bindingFlagsEvaluator;

    private readonly MutableTypeMemberCollection<FieldInfo, MutableFieldInfo> _fields;
    private readonly MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo> _constructors;
    private readonly MutableTypeMemberCollection<MethodInfo, MutableMethodInfo> _methods;

    private readonly List<Type> _addedInterfaces = new List<Type>();

    public MutableType (
      UnderlyingTypeDescriptor underlyingTypeDescriptor,
      IBindingFlagsEvaluator bindingFlagsEvaluator)
    {
      ArgumentUtility.CheckNotNull ("underlyingTypeDescriptor", underlyingTypeDescriptor);
      ArgumentUtility.CheckNotNull ("bindingFlagsEvaluator", bindingFlagsEvaluator);

      _underlyingTypeDescriptor = underlyingTypeDescriptor;
      _bindingFlagsEvaluator = bindingFlagsEvaluator;

      _fields = new MutableTypeMemberCollection<FieldInfo, MutableFieldInfo> (this, _underlyingTypeDescriptor.Fields, CreateExistingField);
      _constructors = new MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo> (
          this, _underlyingTypeDescriptor.Constructors, CreateExistingMutableConstructor);
      _methods = new MutableTypeMemberCollection<MethodInfo, MutableMethodInfo> (this, _underlyingTypeDescriptor.Methods, CreateExistingMutableMethod);
    }

    public ReadOnlyCollection<Type> AddedInterfaces
    {
      get { return _addedInterfaces.AsReadOnly(); }
    }

    public ReadOnlyCollection<MutableFieldInfo> AddedFields
    {
      get { return _fields.AddedMembers; }
    }

    public ReadOnlyCollection<MutableConstructorInfo> AddedConstructors
    {
      get { return _constructors.AddedMembers; }
    }

    public ReadOnlyCollection<MutableMethodInfo> AddedMethods
    {
      get { return _methods.AddedMembers; }
    }

    public ReadOnlyCollectionDecorator<MutableFieldInfo> ExistingMutableFields
    {
      get { return _fields.ExistingDeclaredMembers; }
    }

    public ReadOnlyCollectionDecorator<MutableConstructorInfo> ExistingMutableConstructors
    {
      get { return _constructors.ExistingDeclaredMembers; }
    }

    public ReadOnlyCollectionDecorator<MutableMethodInfo> ExistingMutableMethods
    {
      get { return _methods.ExistingDeclaredMembers; }
    }

    public IEnumerable<MutableFieldInfo> AllMutableFields
    {
      get { return _fields.AllMutableMembers; }
    }

    public IEnumerable<MutableConstructorInfo> AllMutableConstructors
    {
      get { return _constructors.AllMutableMembers; }
    }

    public IEnumerable<MutableMethodInfo> AllMutableMethods
    {
      get { return _methods.AllMutableMembers; }
    }

    public override Type UnderlyingSystemType
    {
      get { return _underlyingTypeDescriptor.UnderlyingSystemType; }
    }

    public bool IsNewType
    {
      get { return false; }
    }

    public override Assembly Assembly
    {
      get { return null; }
    }

    public override Module Module
    {
      get { return null; }
    }

    public override Type BaseType
    {
      get { return _underlyingTypeDescriptor.BaseType; }
    }

    public override string Name
    {
      get { return _underlyingTypeDescriptor.Name; }
    }

    public override string Namespace
    {
      get { return _underlyingTypeDescriptor.Namespace; }
    }

    public override string FullName
    {
      get { return _underlyingTypeDescriptor.FullName; }
    }

    public override string ToString ()
    {
      return _underlyingTypeDescriptor.StringRepresentation;
    }

    public string ToDebugString ()
    {
      return string.Format ("MutableType = \"{0}\"", Name);
    }

    public bool IsEquivalentTo (Type type)
    {
      return type == this || type == UnderlyingSystemType;
    }

    public void AddInterface (Type interfaceType)
    {
      ArgumentUtility.CheckNotNull ("interfaceType", interfaceType);

      if (!interfaceType.IsInterface)
        throw new ArgumentException ("Type must be an interface.", "interfaceType");

      if (GetInterfaces().Contains (interfaceType))
      {
        var message = string.Format ("Interface '{0}' is already implemented.", interfaceType.Name);
        throw new ArgumentException (message, "interfaceType");
      }

      _addedInterfaces.Add (interfaceType);
    }

    public override Type[] GetInterfaces ()
    {
      return _underlyingTypeDescriptor.Interfaces.Concat (_addedInterfaces).ToArray();
    }

    public override Type GetInterface (string name, bool ignoreCase)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      var comparisonMode = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
      var interfaces = GetInterfaces().Where (iface => iface.Name.Equals (name, comparisonMode)).ToArray();

      if (interfaces.Length == 0)
        return null;
      if (interfaces.Length > 1)
        throw new AmbiguousMatchException (string.Format ("Ambiguous interface name '{0}'.", name));

      return interfaces[0];
    }

    public MutableFieldInfo AddField (Type type, string name, FieldAttributes attributes = FieldAttributes.Private)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      var signature = new FieldSignature (type);
      if (AllMutableFields.Any (field => field.Name == name && FieldSignature.Create (field).Equals (signature)))
        throw new ArgumentException ("Field with equal name and signature already exists.", "name");

      var descriptor = UnderlyingFieldInfoDescriptor.Create (type, name, attributes);
      var fieldInfo = new MutableFieldInfo (this, descriptor);

      _fields.Add (fieldInfo);

      return fieldInfo;
    }

    public override FieldInfo GetField (string name, BindingFlags bindingAttr)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      var fields = GetFields (bindingAttr).Where (field => field.Name == name).ToArray ();

      if (fields.Length == 0)
        return null;
      if (fields.Length > 1)
        throw new AmbiguousMatchException (string.Format ("Ambiguous field name '{0}'.", name));

      return fields[0];
    }

    public override FieldInfo[] GetFields (BindingFlags bindingAttr)
    {
      return _fields.Where (field => _bindingFlagsEvaluator.HasRightAttributes (field.Attributes, bindingAttr)).ToArray();
    }

    public MutableConstructorInfo AddConstructor (
        MethodAttributes attributes,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        Func<ConstructorBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      if ((attributes & MethodAttributes.Static) != 0)
        throw new ArgumentException ("Adding static constructors is not (yet) supported.", "attributes");

      var parameterDeclarationCollection = parameterDeclarations.ConvertToCollection();

      var signature = new MethodSignature (typeof (void), parameterDeclarationCollection.Select (pd => pd.Type), 0);
      if (AllMutableConstructors.Any (ctor => signature.Equals (MethodSignature.Create (ctor))))
        throw new ArgumentException ("Constructor with equal signature already exists.", "parameterDeclarations");
      
      var parameterExpressions = parameterDeclarationCollection.Select (pd => pd.Expression);
      var context = new ConstructorBodyCreationContext (this, parameterExpressions);
      var body = BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, context);
      
      var descriptor = UnderlyingConstructorInfoDescriptor.Create (attributes, parameterDeclarationCollection, body);
      var constructorInfo = new MutableConstructorInfo (this, descriptor);

      _constructors.Add (constructorInfo);

      return constructorInfo;
    }
    
    public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      return _constructors.Where (ctor => _bindingFlagsEvaluator.HasRightAttributes (ctor.Attributes, bindingAttr)).ToArray();
    }

    public MutableConstructorInfo GetMutableConstructor (ConstructorInfo constructorInfo)
    {
      ArgumentUtility.CheckNotNull ("constructorInfo", constructorInfo);

      return _constructors.GetMutableMember(constructorInfo);
    }

    public MutableMethodInfo AddMethod (
        string name,
        MethodAttributes attributes,
        Type returnType,
        IEnumerable<ParameterDeclaration> parameterDeclarations,
        Func<MethodBodyCreationContext, Expression> bodyProvider)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("returnType", returnType);
      ArgumentUtility.CheckNotNull ("parameterDeclarations", parameterDeclarations);
      ArgumentUtility.CheckNotNull ("bodyProvider", bodyProvider);

      var parameterDeclarationCollection = parameterDeclarations.ConvertToCollection ();

      var signature = new MethodSignature (returnType, parameterDeclarationCollection.Select (pd => pd.Type), 0);
      if (AllMutableMethods.Where (m => m.Name == name).Any (method => signature.Equals (MethodSignature.Create (method))))
      {
        var message = string.Format ("Method '{0}' with equal signature already exists.", name);
        throw new ArgumentException (message, "name");
      }

      var parameterExpressions = parameterDeclarationCollection.Select (pd => pd.Expression);
      var isStatic = (attributes & MethodAttributes.Static) == MethodAttributes.Static;
      var context = new MethodBodyCreationContext (this, parameterExpressions, isStatic);
      var body = BodyProviderUtility.GetTypedBody (returnType, bodyProvider, context);

      var descriptor = UnderlyingMethodInfoDescriptor.Create (name, attributes, returnType, parameterDeclarationCollection, false, false, false, body);
      var methodInfo = new MutableMethodInfo (this, descriptor);

      _methods.Add (methodInfo);

      return methodInfo;
    }

    public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
    {
      return _methods.Where (method => _bindingFlagsEvaluator.HasRightAttributes (method.Attributes, bindingAttr)).ToArray();
    }

    public MutableMethodInfo GetMutableMethod (MethodInfo methodInfo)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);

      return _methods.GetMutableMember (methodInfo);
    }

    public virtual void Accept (IMutableTypeMemberHandler memberHandler)
    {
      ArgumentUtility.CheckNotNull ("memberHandler", memberHandler);

      // Unmodified
      foreach (var field in ExistingMutableFields.Where (f => !f.IsModified))
        memberHandler.HandleUnmodifiedField (field);
      foreach (var ctor in ExistingMutableConstructors.Where (c => !c.IsModified))
        memberHandler.HandleUnmodifiedConstructor (ctor);
      foreach (var method in ExistingMutableMethods.Where (m => !m.IsModified))
        memberHandler.HandleUnmodifiedMethod (method);

      // Added
      foreach (var field in _fields.AddedMembers)
        memberHandler.HandleAddedField (field);
      foreach (var ctor in _constructors.AddedMembers)
        memberHandler.HandleAddedConstructor (ctor);
      foreach (var method in _methods.AddedMembers)
        memberHandler.HandleAddedMethod (method);

      // Modfied
      foreach (var ctor in ExistingMutableConstructors.Where (c => c.IsModified))
        memberHandler.HandleModifiedConstructor (ctor);
      foreach (var method in ExistingMutableMethods.Where (m => m.IsModified))
        memberHandler.HandleModifiedMethod (method);
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
      return _underlyingTypeDescriptor.Attributes;
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

    protected override ConstructorInfo GetConstructorImpl (
        BindingFlags bindingAttr, Binder binderOrNull, CallingConventions callConvention, Type[] typesOrNull, ParameterModifier[] modifiersOrNull)
    {
      var candidates = GetConstructors (bindingAttr);
      return (ConstructorInfo) SafeSelectMethod (binderOrNull, bindingAttr, candidates, typesOrNull, modifiersOrNull);
    }

    protected override MethodInfo GetMethodImpl (
        string name,
        BindingFlags bindingAttr,
        Binder binderOrNull,
        CallingConventions callConvention,
        Type[] typesOrNull,
        ParameterModifier[] modifiersOrNull)
    {
      var candidates = GetMethods (bindingAttr).Where (m => m.Name == name).ToArray();
      return (MethodInfo) SafeSelectMethod (binderOrNull, bindingAttr, candidates, typesOrNull, modifiersOrNull);
    }

    private MethodBase SafeSelectMethod (
        Binder binderOrNull, BindingFlags bindingAttr, MethodBase[] candidates, Type[] typesOrNull, ParameterModifier[] modifiersOrNull)
    {
      if (candidates.Length == 0)
        return null;

      if (candidates.Length == 1)
        return candidates[0];

      Assertion.IsTrue (typesOrNull != null || modifiersOrNull == null, "Cannot check modifiers if types are null.");

      if (typesOrNull == null)
        throw new AmbiguousMatchException (string.Format ("Ambiguous method name '{0}'.", candidates[0].Name));

      var binder = binderOrNull ?? DefaultBinder;
      Assertion.IsNotNull (binder);

      return binder.SelectMethod (bindingAttr, candidates, typesOrNull, modifiersOrNull);
    }

    private MutableFieldInfo CreateExistingField (FieldInfo originalField)
    {
      var descriptor = UnderlyingFieldInfoDescriptor.Create (originalField);
      return new MutableFieldInfo (this, descriptor);
    }

    private MutableConstructorInfo CreateExistingMutableConstructor (ConstructorInfo originalConstructor)
    {
      var descriptor = UnderlyingConstructorInfoDescriptor.Create (originalConstructor);
      return new MutableConstructorInfo (this, descriptor);
    }

    private MutableMethodInfo CreateExistingMutableMethod (MethodInfo originalMethod)
    {
      var descriptor = UnderlyingMethodInfoDescriptor.Create (originalMethod);
      return new MutableMethodInfo (this, descriptor);
    }

    #region Not YET implemented abstract members of Type class

    public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override MemberInfo[] GetMember (string name, MemberTypes type, BindingFlags bindingAttr)
    {
      return new MemberInfo[0]; // Needed for GetMember(..) - virtual method check
    }

    public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
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
      throw new NotImplementedException();
    }

    protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binderOrNull, Type returnType, Type[] types, ParameterModifier[] modifiers)
    {
      //types = types ?? Type.EmptyTypes;
      throw new NotImplementedException();
    }

    public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
    {
      return new PropertyInfo[0]; // Needed for virtual method check
    }
    
    public override object[] GetCustomAttributes (bool inherit)
    {
      throw new NotImplementedException();
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      throw new NotImplementedException();
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region Unsupported Members

    public override int MetadataToken
    {
      get { throw new NotSupportedException ("Property MutableType.MetadataToken is not supported."); }
    }

    public override Guid GUID
    {
      get { throw new NotSupportedException ("Property MutableType.GUID is not supported."); }
    }

    public override string AssemblyQualifiedName
    {
      get { throw new NotSupportedException ("Property MutableType.AssemblyQualifiedName is not supported."); }
    }

    public override StructLayoutAttribute StructLayoutAttribute
    {
      get { throw new NotSupportedException ("Property MutableType.StructLayoutAttribute is not supported."); }
    }

    public override GenericParameterAttributes GenericParameterAttributes
    {
      get { throw new NotSupportedException ("Property MutableType.GenericParameterAttributes is not supported."); }
    }

    public override int GenericParameterPosition
    {
      get { throw new NotSupportedException ("Property MutableType.GenericParameterPosition is not supported."); }
    }

    public override RuntimeTypeHandle TypeHandle
    {
      get { throw new NotSupportedException ("Property MutableType.TypeHandle is not supported."); }
    }

    public override MemberInfo[] GetDefaultMembers ()
    {
      throw new NotSupportedException ("Method MutableType.GetDefaultMembers is not supported.");
    }

    public override InterfaceMapping GetInterfaceMap (Type interfaceType)
    {
      throw new NotSupportedException ("Method MutableType.GetInterfaceMap is not supported.");
    }

    public override object InvokeMember (string name, BindingFlags invokeAttr, Binder binderOrNull, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
    {
      throw new NotSupportedException ("Method MutableType.InvokeMember is not supported.");
    }

    public override Type MakePointerType ()
    {
      throw new NotSupportedException ("Method MutableType.MakePointerType is not supported.");
    }

    public override Type MakeByRefType ()
    {
      throw new NotSupportedException ("Method MutableType.MakeByRefType is not supported.");
    }

    public override Type MakeArrayType ()
    {
      throw new NotSupportedException ("Method MutableType.MakeArrayType is not supported.");
    }

    public override Type MakeArrayType (int rank)
    {
      throw new NotSupportedException ("Method MutableType.MakeArrayType is not supported.");
    }

    public override int GetArrayRank ()
    {
      throw new NotSupportedException ("Method MutableType.GetArrayRank is not supported.");
    }

    public override Type[] GetGenericParameterConstraints ()
    {
      throw new NotSupportedException ("Method MutableType.GetGenericParameterConstraints is not supported.");
    }

    public override Type MakeGenericType (params Type[] typeArguments)
    {
      throw new NotSupportedException ("Method MutableType.MakeGenericType is not supported.");
    }

    public override Type[] GetGenericArguments ()
    {
      throw new NotSupportedException ("Method MutableType.GetGenericArguments is not supported.");
    }

    public override Type GetGenericTypeDefinition ()
    {
      throw new NotSupportedException ("Method MutableType.GetGenericTypeDefinition is not supported.");
    }

    #endregion
  }
}