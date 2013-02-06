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
using System.Reflection;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Provides methods for substituting generic type parameters with type arguments by holding onto the type arguments
  /// </summary>
  public class TypeInstantiator : ITypeInstantiator
  {
    public IEnumerable<Type> TypeArguments
    {
      get { throw new NotImplementedException(); }
    }

    public string GetSimpleName (Type genericTypeDefinition)
    {
      throw new NotImplementedException();
    }

    public string GetFullName (Type genericTypeDefinition)
    {
      throw new NotImplementedException();
    }

    public Type SubstituteGenericParameters (Type type)
    {
      throw new NotImplementedException();
    }

    public FieldInfo SubstituteGenericParameters (FieldInfo field)
    {
      throw new NotImplementedException();
    }

    public ConstructorInfo SubstituteGenericParameters (ConstructorInfo constructor)
    {
      throw new NotImplementedException();
    }

    public MethodInfo SubstituteGenericParameters (MethodInfo method)
    {
      throw new NotImplementedException();
    }

    public PropertyInfo SubstituteGenericParameters (PropertyInfo property)
    {
      throw new NotImplementedException();
    }

    public EventInfo SubstituteGenericParameters (EventInfo event_)
    {
      throw new NotImplementedException();
    }
  }
}