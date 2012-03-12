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
  // TODO Docs
  public class FutureTypeTemplate : ITypeTemplate
  {
    private readonly Type _baseType;
    private readonly TypeAttributes _attributes;
    private readonly Type[] _interfaces;
    private readonly FieldInfo[] _fields;

    public FutureTypeTemplate (Type baseType, TypeAttributes attributes, Type[] interfaces, FieldInfo[] fields)
    {
      ArgumentUtility.CheckNotNull ("baseType", baseType);
      ArgumentUtility.CheckNotNull ("interfaces", interfaces);
      ArgumentUtility.CheckNotNull ("fields", fields);

      _baseType = baseType;
      _attributes = attributes;
      _interfaces = interfaces;
      _fields = fields; 
    }

    public Type GetBaseType ()
    {
      return _baseType;
    }

    public TypeAttributes GetAttributeFlags ()
    {
      return _attributes;
    }

    public Type[] GetInterfaces ()
    {
      return _interfaces;
    }

    public FieldInfo[] GetFields (BindingFlags bindingAttr)
    {
      return _fields;
    }

    public ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
    {
      throw new NotImplementedException();
    }
  }
}