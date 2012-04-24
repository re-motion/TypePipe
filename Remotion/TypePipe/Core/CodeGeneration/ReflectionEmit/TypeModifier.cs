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
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements the <see cref="ITypeModifier"/> interface using Reflection.Emit.
  /// </summary>
  /// <remarks>
  /// This class modifies the behavior of types by deriving runtime-generated subclass proxies that add or override members.
  /// </remarks>
  public class TypeModifier : ITypeModifier
  {
    private readonly ISubclassProxyBuilderFactory _builderFactory;

    public TypeModifier (ISubclassProxyBuilderFactory builderFactory)
    {
      ArgumentUtility.CheckNotNull ("builderFactory", builderFactory);

      _builderFactory = builderFactory;
    }

    public Type ApplyModifications (MutableType mutableType)
    {
      ArgumentUtility.CheckNotNull ("mutableType", mutableType);

      var builder = _builderFactory.CreateBuilder (mutableType);

      ExtendedTypeModificationHandlerExtensions.Accept (mutableType, builder);

      return builder.Build();
    }
  }

  public static class ExtendedTypeModificationHandlerExtensions
  {
    public static void Accept (this MutableType mutableType, IExtendedTypeModificationHandler extendedHandler)
    {
      foreach (var field in mutableType.ExistingFields.Where (ctor => !ctor.IsModified))
        extendedHandler.HandleUnmodifiedField (field);

      foreach (var constructor in mutableType.ExistingConstructors.Where (ctor => !ctor.IsModified))
        extendedHandler.HandleUnmodifiedConstructor (constructor);

      foreach (var method in mutableType.ExistingMethods.Where(ctor => !ctor.IsModified))
        extendedHandler.HandleUnmodifiedMethod (method);

      mutableType.Accept (extendedHandler);
    }
  }
}
