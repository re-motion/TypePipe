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
using System.Linq;
using System.Reflection;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.TypePipe.MutableReflection.Generics
{
  /// <summary>
  /// Provides methods for substituting generic type parameters with type arguments by holding onto the type arguments
  /// </summary>
  public class TypeInstantiator : ITypeInstantiator
  {
    private readonly ReadOnlyCollection<Type> _typeArguments;

    public TypeInstantiator (IEnumerable<Type> typeArguments)
    {
      ArgumentUtility.CheckNotNull ("typeArguments", typeArguments);

      _typeArguments = typeArguments.ToList().AsReadOnly();
    }

    public IEnumerable<Type> TypeArguments
    {
      get { return _typeArguments; }
    }

    // TODO yyy: what about ToString?
    public string GetFullName (Type genericTypeDefinition)
    {
      ArgumentUtility.CheckNotNull ("genericTypeDefinition", genericTypeDefinition);
      Assertion.IsTrue (genericTypeDefinition.IsGenericTypeDefinition);

      var typeArguments = SeparatedStringBuilder.Build (",", _typeArguments, t => "[" + t.AssemblyQualifiedName + "]");
      return string.Format ("{0}[{1}]", genericTypeDefinition.FullName, typeArguments);
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