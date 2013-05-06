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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.CodeGeneration.Implementation.ReflectionEmit.Abstractions
{
  /// <summary>
  /// A base class for decorators that decorate <see cref="ICustomAttributeTargetBuilder"/> instances.
  /// </summary>
  public abstract class BuilderDecoratorBase : ICustomAttributeTargetBuilder
  {
    private readonly ICustomAttributeTargetBuilder _customAttributeTargetBuilder;
    protected readonly IEmittableOperandProvider EmittableOperandProvider;

    protected BuilderDecoratorBase (ICustomAttributeTargetBuilder customAttributeTargetBuilder, IEmittableOperandProvider emittableOperandProvider)
    {
      ArgumentUtility.CheckNotNull ("customAttributeTargetBuilder", customAttributeTargetBuilder);
      ArgumentUtility.CheckNotNull ("emittableOperandProvider", emittableOperandProvider);

      _customAttributeTargetBuilder = customAttributeTargetBuilder;
      EmittableOperandProvider = emittableOperandProvider;
    }

    public void SetCustomAttribute (CustomAttributeDeclaration customAttributeDeclaration)
    {
      ArgumentUtility.CheckNotNull ("customAttributeDeclaration", customAttributeDeclaration);

      var emittableConstructorArguments = customAttributeDeclaration.ConstructorArguments.Select (MakeAttributeArgumentEmittable).ToArray();
      var emittableNamedArguments = customAttributeDeclaration.NamedArguments.Select (MakeEmittable).ToArray();
      var emittableDeclaration = new CustomAttributeDeclaration (
          customAttributeDeclaration.Constructor, emittableConstructorArguments, emittableNamedArguments);

      _customAttributeTargetBuilder.SetCustomAttribute (emittableDeclaration);
    }

    private object MakeAttributeArgumentEmittable (object argumentValue)
    {
      var type = argumentValue as Type;
      if (type != null)
        return EmittableOperandProvider.GetEmittableType (type);

      var array = argumentValue as Array;
      if (array != null)
      {
        for (int i = 0; i < array.Length; i++)
          array.SetValue (MakeAttributeArgumentEmittable (array.GetValue (i)), i);
      }

      return argumentValue;
    }

    private NamedArgumentDeclaration MakeEmittable (ICustomAttributeNamedArgument customAttributeNamedArgument)
    {
      var member = customAttributeNamedArgument.MemberInfo;
      var emittableValue = MakeAttributeArgumentEmittable (customAttributeNamedArgument.Value);

      return member is FieldInfo
                 ? new NamedArgumentDeclaration ((FieldInfo) member, emittableValue)
                 : new NamedArgumentDeclaration ((PropertyInfo) member, emittableValue);
    }
  }
}