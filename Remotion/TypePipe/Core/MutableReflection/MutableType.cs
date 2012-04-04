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
using Microsoft.Scripting.Ast;
using Remotion.Collections;
using Remotion.FunctionalProgramming;
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
    private readonly IEqualityComparer<MemberInfo> _memberInfoEqualityComparer;
    private readonly IBindingFlagsEvaluator _bindingFlagsEvaluator;

    private readonly List<Type> _addedInterfaces = new List<Type>();
    private readonly List<MutableFieldInfo> _addedFields = new List<MutableFieldInfo>();
    private readonly List<MutableConstructorInfo> _addedConstructors = new List<MutableConstructorInfo>();

    private readonly ReadOnlyDictionary<ConstructorInfo, MutableConstructorInfo> _existingConstructors;

    public MutableType (
      UnderlyingTypeDescriptor underlyingTypeDescriptor,
      IEqualityComparer<MemberInfo> memberInfoEqualityComparer,
      IBindingFlagsEvaluator bindingFlagsEvaluator)
    {
      ArgumentUtility.CheckNotNull ("underlyingTypeDescriptor", underlyingTypeDescriptor);
      ArgumentUtility.CheckNotNull ("memberInfoEqualityComparer", memberInfoEqualityComparer);
      ArgumentUtility.CheckNotNull ("bindingFlagsEvaluator", bindingFlagsEvaluator);

      _underlyingTypeDescriptor = underlyingTypeDescriptor;
      _memberInfoEqualityComparer = memberInfoEqualityComparer;
      _bindingFlagsEvaluator = bindingFlagsEvaluator;

      _existingConstructors = _underlyingTypeDescriptor.Constructors.ToDictionary (ctor => ctor, CreateExistingMutableConstructor).AsReadOnly();
    }

    public ReadOnlyCollection<Type> AddedInterfaces
    {
      get { return _addedInterfaces.AsReadOnly(); }
    }

    public ReadOnlyCollection<MutableFieldInfo> AddedFields
    {
      get { return _addedFields.AsReadOnly(); }
    }

    public ReadOnlyCollection<MutableConstructorInfo> AddedConstructors
    {
      get { return _addedConstructors.AsReadOnly(); }
    }

    public ReadOnlyCollection<Type> ExistingInterfaces
    {
      get { return _underlyingTypeDescriptor.Interfaces; }
    }

    public ReadOnlyCollection<FieldInfo> ExistingFields
    {
      get { return _underlyingTypeDescriptor.Fields; }
    }

    public ReadOnlyCollectionDecorator<MutableConstructorInfo> ExistingConstructors
    {
      get { return _existingConstructors.Values.AsReadOnly(); }
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

      if (GetInterfaces ().Contains (interfaceType))
      {
        var message = string.Format ("Interface '{0}' is already implemented.", interfaceType.Name);
        throw new ArgumentException (message, "interfaceType");
      }

      _addedInterfaces.Add (interfaceType);
    }

    public override Type[] GetInterfaces ()
    {
      return _underlyingTypeDescriptor.Interfaces.Concat (AddedInterfaces).ToArray();
    }

    public MutableFieldInfo AddField (Type type, string name, FieldAttributes attributes = FieldAttributes.Private)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);
      ArgumentUtility.CheckNotNull ("type", type);

      var fieldInfo = new MutableFieldInfo (this, type, name, attributes);

      if (GetAllFields ().Any (field => field.Name == name && _memberInfoEqualityComparer.Equals(field, fieldInfo)))
        throw new ArgumentException ("Field with equal name and signature already exists.", "name");

      _addedFields.Add (fieldInfo);

      return fieldInfo;
    }

    public override FieldInfo GetField (string name, BindingFlags bindingAttr)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      var fieldInfos = GetFields (bindingAttr).Where (field => field.Name == name).ToArray ();
      if (fieldInfos.Length == 0)
        return null;
      if (fieldInfos.Length > 1)
        throw new AmbiguousMatchException (string.Format ("Ambiguous field name '{0}'.", name));

      return fieldInfos[0];
    }

    public override FieldInfo[] GetFields (BindingFlags bindingAttr)
    {
      return ExistingFields.Concat (_addedFields.Cast<FieldInfo>())
          .Where (field => _bindingFlagsEvaluator.HasRightAttributes (field.Attributes, bindingAttr))
          .ToArray();
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
      var parameterExpressions = parameterDeclarationCollection.Select (pd => pd.Expression);

      var context = new ConstructorBodyCreationContext (this, parameterExpressions);
      var body = BodyProviderUtility.GetVoidBody (bodyProvider, context);
      
      var descriptor = UnderlyingConstructorInfoDescriptor.Create (attributes, parameterDeclarationCollection, body);
      var constructorInfo = new MutableConstructorInfo (this, descriptor);

      if (GetAllConstructors ().Any (ctor => _memberInfoEqualityComparer.Equals(ctor, constructorInfo)))
        throw new ArgumentException ("Constructor with equal signature already exists.", "parameterDeclarations");

      _addedConstructors.Add (constructorInfo);

      return constructorInfo;
    }

    public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      var allConstructors = ExistingConstructors.Concat (AddedConstructors);
      return allConstructors.Where (ctor => _bindingFlagsEvaluator.HasRightAttributes (ctor.Attributes, bindingAttr)).ToArray();
    }

    public MutableConstructorInfo GetMutableConstructor (ConstructorInfo constructor)
    {
      ArgumentUtility.CheckNotNull ("constructor", constructor);

      CheckDeclaringType (constructor, "constructor");

      if (constructor is MutableConstructorInfo)
        return (MutableConstructorInfo) constructor;

      var matchingMutableConstructorInfo = _existingConstructors.GetValueOrDefault (constructor);
      if (matchingMutableConstructorInfo == null)
        throw new NotSupportedException ("The given constructor cannot be mutated.");
      
      return matchingMutableConstructorInfo;
    }

    public virtual void Accept (ITypeModificationHandler modificationHandler)
    {
      ArgumentUtility.CheckNotNull ("modificationHandler", modificationHandler);

      foreach (var addedInterface in _addedInterfaces)
        modificationHandler.HandleAddedInterface (addedInterface);

      foreach (var addedField in _addedFields)
        modificationHandler.HandleAddedField (addedField);

      foreach (var addedConstructor in _addedConstructors)
        modificationHandler.HandleAddedConstructor (addedConstructor);
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

    protected override ConstructorInfo GetConstructorImpl (
        BindingFlags bindingAttr, Binder binderOrNull, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
    {
      var binder = binderOrNull ?? DefaultBinder;
      var candidates = GetConstructors (bindingAttr).ToArray ();

      if (candidates.Length == 0)
        return null;

      Assertion.IsNotNull (binder, "DefaultBinder is never null.");
      return (ConstructorInfo) binder.SelectMethod (bindingAttr, candidates, types, modifiers);
    }

    private FieldInfo[] GetAllFields ()
    {
      return ExistingFields.Concat (AddedFields.Cast<FieldInfo>()).ToArray();
    }

    private ConstructorInfo[] GetAllConstructors ()
    {
      return ExistingConstructors.Concat (AddedConstructors).ToArray();
    }

    private void CheckDeclaringType (MemberInfo member, string parameterName)
    {
      if (!IsEquivalentTo (member.DeclaringType))
      {
        var memberKind = char.ToUpper (parameterName[0]) + parameterName.Substring (1);
        var message = string.Format ("{0} is declared by a different type: '{1}'.", memberKind, member.DeclaringType);
        throw new ArgumentException (message, parameterName);
      }
    }

    private MutableConstructorInfo CreateExistingMutableConstructor (ConstructorInfo originalConstructor)
    {
      return new MutableConstructorInfo (this, UnderlyingConstructorInfoDescriptor.Create (originalConstructor));
    }

    #region Not implemented abstract members of Type class

    public override Type GetElementType ()
    {
      throw new NotImplementedException();
    }

    public override Guid GUID
    {
      get { throw new NotImplementedException(); }
    }

    public override Module Module
    {
      get { throw new NotImplementedException(); }
    }

    public override string AssemblyQualifiedName
    {
      get { throw new NotImplementedException(); }
    }

    protected override bool IsArrayImpl ()
    {
      throw new NotImplementedException();
    }

    protected override bool IsPointerImpl ()
    {
      throw new NotImplementedException();
    }

    protected override bool IsPrimitiveImpl ()
    {
      throw new NotImplementedException();
    }

    protected override bool IsCOMObjectImpl ()
    {
      throw new NotImplementedException();
    }

    public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override object InvokeMember (string name, BindingFlags invokeAttr, Binder binderOrNull, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
    {
      throw new NotImplementedException();
    }

    public override Type GetInterface (string name, bool ignoreCase)
    {
      throw new NotImplementedException();
    }

    public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override EventInfo[] GetEvents (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override Type[] GetNestedTypes (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override Type GetNestedType (string name, BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binderOrNull, Type returnType, Type[] types, ParameterModifier[] modifiers)
    {
      throw new NotImplementedException();
    }

    public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binderOrNull, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
    {
      // TODO TypePipe: Implement using GetMethods, add and use BindingFlagsEvaluator.HasRightName (string actualName, string expectedName, BindingFlags bindingFlags), then apply binder/DefaultBinder
      throw new NotImplementedException ();
    }

    public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
    {
      // TODO TypePipe: Like GetConstructors.
      throw new NotImplementedException ();
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
  }
}