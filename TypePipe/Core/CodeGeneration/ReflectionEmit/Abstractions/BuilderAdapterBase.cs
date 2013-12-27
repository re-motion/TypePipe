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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions
{
  /// <summary>
  /// A base class for adapters adapting Reflection.Emit builders to <see cref="ICustomAttributeTargetBuilder"/>.
  /// </summary>
  public abstract class BuilderAdapterBase : ICustomAttributeTargetBuilder
  {
    private readonly Action<CustomAttributeBuilder> _setCustomAttributeMethod;

    protected BuilderAdapterBase (Action<CustomAttributeBuilder> setCustomAttributeMethod)
    {
      ArgumentUtility.CheckNotNull ("setCustomAttributeMethod", setCustomAttributeMethod);

      _setCustomAttributeMethod = setCustomAttributeMethod;
    }

    public void SetCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDeclaration", customAttributeDeclaration);

      var propertyArguments = customAttributeDeclaration.NamedArguments.Where (na => na.MemberInfo.MemberType == MemberTypes.Property).ToArray();
      var fieldArguments = customAttributeDeclaration.NamedArguments.Where (na => na.MemberInfo.MemberType == MemberTypes.Field).ToArray();

      var customAttributeBuilder = new CustomAttributeBuilder (
          customAttributeDeclaration.Constructor,
          customAttributeDeclaration.ConstructorArguments.ToArray(),
          propertyArguments.Select (namedArg => (PropertyInfo) namedArg.MemberInfo).ToArray(),
          propertyArguments.Select (namedArg => namedArg.Value).ToArray(),
          fieldArguments.Select (namedArg => (FieldInfo) namedArg.MemberInfo).ToArray(),
          fieldArguments.Select (namedArg => namedArg.Value).ToArray());

      _setCustomAttributeMethod (customAttributeBuilder);
    }
  }
}