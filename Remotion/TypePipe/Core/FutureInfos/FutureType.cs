// This file is part of the re-motion TypePipe project (typepipe.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-motion TypePipe is free software; you can redistribute it 
// and/or modify it under the terms of the Apache License, Version 2.0
// as published by the Apache Software Foundation.
// 
// re-motion TypePipe is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// Apache License, Version 2.0 for more details.
// 
// You should have received a copy of the Apache License, Version 2.0
// along with re-motion; if not, see http://www.apache.org/licenses.
// 
using System;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using Remotion.TypePipe.Utilities;

namespace Remotion.TypePipe.FutureInfos
{
  public class FutureType : Type
  {
    private readonly Slot<TypeBuilder> _typeBuilder = Slot.New<TypeBuilder> ("TypeBuilder");

    public void SetTypeBuilder (TypeBuilder typeBuilder)
    {
      _typeBuilder.Set (typeBuilder);
    }

    #region NotImplemented
    public override object[] GetCustomAttributes (bool inherit)
    {
      throw new NotImplementedException();
    }

    public override bool IsDefined (Type attributeType, bool inherit)
    {
      throw new NotImplementedException();
    }

    public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override Type GetInterface (string name, bool ignoreCase)
    {
      throw new NotImplementedException();
    }

    public override Type[] GetInterfaces ()
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

    public override Type GetElementType ()
    {
      throw new NotImplementedException();
    }

    protected override bool HasElementTypeImpl ()
    {
      throw new NotImplementedException();
    }

    protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
    {
      throw new NotImplementedException();
    }

    public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
    {
      throw new NotImplementedException();
    }

    public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override FieldInfo GetField (string name, BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override FieldInfo[] GetFields (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    public override MemberInfo[] GetMembers (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }

    protected override TypeAttributes GetAttributeFlagsImpl ()
    {
      throw new NotImplementedException();
    }

    protected override bool IsArrayImpl ()
    {
      throw new NotImplementedException();
    }

    protected override bool IsByRefImpl ()
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

    public override object InvokeMember (string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
    {
      throw new NotImplementedException();
    }

    public override Type UnderlyingSystemType
    {
      get { throw new NotImplementedException(); }
    }

    protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
    {
      throw new NotImplementedException();
    }

    public override string Name
    {
      get { throw new NotImplementedException(); }
    }

    public override Guid GUID
    {
      get { throw new NotImplementedException(); }
    }

    public override Module Module
    {
      get { throw new NotImplementedException(); }
    }

    public override Assembly Assembly
    {
      get { throw new NotImplementedException(); }
    }

    public override string FullName
    {
      get { throw new NotImplementedException(); }
    }

    public override string Namespace
    {
      get { throw new NotImplementedException(); }
    }

    public override string AssemblyQualifiedName
    {
      get { throw new NotImplementedException(); }
    }

    public override Type BaseType
    {
      get { throw new NotImplementedException(); }
    }

    public override object[] GetCustomAttributes (Type attributeType, bool inherit)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}