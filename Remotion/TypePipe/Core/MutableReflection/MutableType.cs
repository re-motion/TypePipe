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
  /// <remarks>
  ///   <para>
  ///     When an instance of a <see cref="MutableType"/> is to be compared for equality with another <see cref="Type"/> instance, the
  ///     <see cref="IsEquivalentTo"/> method should be used rather than comparing via <see cref="object.Equals(object)"/>.
  ///   </para>
  /// </remarks>
  [DebuggerDisplay ("{ToDebugString(),nq}")]
  public class MutableType : Type
  {
    private readonly UnderlyingTypeDescriptor _underlyingTypeDescriptor;
    private readonly IMemberSelector _memberSelector;
    private readonly IRelatedMethodFinder _relatedMethodFinder;

    private readonly MutableTypeMemberCollection<FieldInfo, MutableFieldInfo> _fields;
    private readonly MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo> _constructors;
    private readonly MutableTypeMemberCollection<MethodInfo, MutableMethodInfo> _methods;

    private readonly List<Type> _addedInterfaces = new List<Type>();

    public MutableType (
      UnderlyingTypeDescriptor underlyingTypeDescriptor,
      IMemberSelector memberSelector,
      IRelatedMethodFinder relatedMethodFinder)
    {
      ArgumentUtility.CheckNotNull ("underlyingTypeDescriptor", underlyingTypeDescriptor);
      ArgumentUtility.CheckNotNull ("memberSelector", memberSelector);
      ArgumentUtility.CheckNotNull ("relatedMethodFinder", relatedMethodFinder);

      _underlyingTypeDescriptor = underlyingTypeDescriptor;
      _memberSelector = memberSelector;
      _relatedMethodFinder = relatedMethodFinder;

      _fields = new MutableTypeMemberCollection<FieldInfo, MutableFieldInfo> (this, _underlyingTypeDescriptor.Fields, CreateExistingField, true);
      _constructors = new MutableTypeMemberCollection<ConstructorInfo, MutableConstructorInfo> (
        this, _underlyingTypeDescriptor.Constructors, CreateExistingMutableConstructor, true);
      _methods = new MutableTypeMethodCollection (this, _underlyingTypeDescriptor.Methods, CreateExistingMutableMethod);
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

    public bool IsAssignableTo (Type other)
    {
      ArgumentUtility.CheckNotNull ("other", other);

      return IsEquivalentTo (other)
             || other.IsAssignableFrom (BaseType)
             || GetInterfaces ().Any (other.IsAssignableFrom);
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
      return GetInterfaces()
          .SingleOrDefault (
              iface => iface.Name.Equals (name, comparisonMode),
              () => new AmbiguousMatchException (string.Format ("Ambiguous interface name '{0}'.", name)));
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

        public override FieldInfo[] GetFields (BindingFlags bindingAttr)
    {
      return _memberSelector.SelectFields (_fields, bindingAttr).ToArray();
    }

    public override FieldInfo GetField (string name, BindingFlags bindingAttr)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      return _memberSelector.SelectSingleField (_fields, bindingAttr, name);
    }

    public MutableFieldInfo GetMutableField (FieldInfo fieldInfo)
    {
      ArgumentUtility.CheckNotNull ("fieldInfo", fieldInfo);

      return _fields.GetMutableMember (fieldInfo);
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
      var context = new ConstructorBodyCreationContext (this, parameterExpressions, _memberSelector);
      var body = BodyProviderUtility.GetTypedBody (typeof (void), bodyProvider, context);
      
      var descriptor = UnderlyingConstructorInfoDescriptor.Create (attributes, parameterDeclarationCollection, body);
      var constructorInfo = new MutableConstructorInfo (this, descriptor);

      _constructors.Add (constructorInfo);

      return constructorInfo;
    }
    
    public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      return _memberSelector.SelectMethods (_constructors, bindingAttr, this).ToArray();
    }

    public MutableConstructorInfo GetMutableConstructor (ConstructorInfo constructorInfo)
    {
      ArgumentUtility.CheckNotNull ("constructorInfo", constructorInfo);

      return _constructors.GetMutableMember (constructorInfo);
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

      var isVirtual = MethodAttributeUtility.IsSet (attributes, MethodAttributes.Virtual);
      var isNewSlot = MethodAttributeUtility.IsSet (attributes, MethodAttributes.NewSlot);
      if (!isVirtual && isNewSlot)
        throw new ArgumentException ("Virtual and NewSlot are not a valid combination for method attributes.", "attributes");

      var parameterDeclarationCollection = parameterDeclarations.ConvertToCollection ();

      var signature = new MethodSignature (returnType, parameterDeclarationCollection.Select (pd => pd.Type), 0);
      // Fix code duplication?
      if (AllMutableMethods.Any (m => m.Name == name && signature.Equals (MethodSignature.Create (m))))
      {
        var message = string.Format ("Method '{0}' with equal signature already exists.", name);
        throw new ArgumentException (message, "name");
      }

      var parameterExpressions = parameterDeclarationCollection.Select (pd => pd.Expression);
      var isStatic = MethodAttributeUtility.IsSet (attributes, MethodAttributes.Static);
      var baseMethod = isVirtual && !isNewSlot ? GetBaseMethod (name, signature) : null;
      var context = new MethodBodyCreationContext (this, parameterExpressions, isStatic, baseMethod, _memberSelector);
      var body = BodyProviderUtility.GetTypedBody (returnType, bodyProvider, context);

      var descriptor = UnderlyingMethodInfoDescriptor.Create (
          name, attributes, returnType, parameterDeclarationCollection, baseMethod, false, false, false, body);
      var methodInfo = new MutableMethodInfo (this, descriptor);

      _methods.Add (methodInfo);

      return methodInfo;
    }

    public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
    {
      return _memberSelector.SelectMethods (_methods, bindingAttr, this).ToArray();
    }

    /// <summary>
    /// Returns a <see cref="MutableMethodInfo"/> that can be used to modify the behavior of the given <paramref name="methodInfo"/>. If the method
    /// is declared on the modified type, it returns the corresponding <see cref="MutableMethodInfo"/> from the <see cref="ExistingMutableMethods"/>
    /// collection. If it is declared on a base type, this method returns an override for it, creating one if necessary.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> to get a <see cref="MutableMethodInfo"/> for.</param>
    /// <returns>
    /// The <see cref="MutableMethodInfo"/> corresponding to <paramref name="methodInfo"/> or an override of the method.
    /// </returns>
    public MutableMethodInfo GetOrAddMutableMethod (MethodInfo methodInfo)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);

      return _methods.GetMutableMember (methodInfo);
    }

    public virtual void Accept (IMutableTypeUnmodifiedMutableMemberHandler handler)
    {
      ArgumentUtility.CheckNotNull ("handler", handler);

      foreach (var field in ExistingMutableFields.Where (f => !f.IsModified))
        handler.HandleUnmodifiedField (field);
      foreach (var ctor in ExistingMutableConstructors.Where (c => !c.IsModified))
        handler.HandleUnmodifiedConstructor (ctor);
      foreach (var method in ExistingMutableMethods.Where (m => !m.IsModified))
        handler.HandleUnmodifiedMethod (method);
    }

    public virtual void Accept (IMutableTypeModificationHandler handler)
    {
      ArgumentUtility.CheckNotNull ("handler", handler);

      foreach (var ifc in AddedInterfaces)
        handler.HandleAddedInterface (ifc);

      foreach (var field in _fields.AddedMembers)
        handler.HandleAddedField (field);
      foreach (var ctor in _constructors.AddedMembers)
        handler.HandleAddedConstructor (ctor);
      foreach (var method in _methods.AddedMembers)
        handler.HandleAddedMethod (method);

      foreach (var ctor in ExistingMutableConstructors.Where (c => c.IsModified))
        handler.HandleModifiedConstructor (ctor);
      foreach (var method in ExistingMutableMethods.Where (m => m.IsModified))
        handler.HandleModifiedMethod (method);
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
      var candidates = _constructors;
      var binder = binderOrNull ?? DefaultBinder;
      return _memberSelector.SelectSingleMethod (candidates, binder, bindingAttr, ".ctor", this, typesOrNull, modifiersOrNull);
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
      return _memberSelector.SelectSingleMethod (_methods, binder, bindingAttr, name, this, typesOrNull, modifiersOrNull);
    }

    private MethodInfo GetBaseMethod (string name, MethodSignature signature)
    {
      var baseMethod = _relatedMethodFinder.GetMostDerivedVirtualMethod (name, signature, BaseType);
      if (baseMethod != null && baseMethod.IsFinal)
      {
        var message = string.Format ("Cannot override final method '{0}'.", name);
        throw new NotSupportedException (message);
      }

      return baseMethod;
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
      var descriptor = UnderlyingMethodInfoDescriptor.Create (originalMethod, _relatedMethodFinder);
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