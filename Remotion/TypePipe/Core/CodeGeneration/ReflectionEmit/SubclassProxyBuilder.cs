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
using Remotion.Collections;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
  /// <summary>
  /// Implements <see cref="ISubclassProxyBuilder"/> by building a subclass proxy using <see cref="ITypeBuilder"/> and related interfaces.
  /// Implements forward declarations of method and constructor bodies by deferring emission of code to the <see cref="Build"/> method.
  /// </summary>
  public class SubclassProxyBuilder : ISubclassProxyBuilder
  {
    private readonly ICodeGenerationContextFactory _codeGenerationContextFactory;
    private readonly IInitializationBuilder _initializationBuilder;
    private readonly IProxySerializationEnabler _proxySerializationEnabler;

    public SubclassProxyBuilder (
        ICodeGenerationContextFactory codeGenerationContextFactory,
        IInitializationBuilder initializationBuilder,
        IProxySerializationEnabler proxySerializationEnabler)
    {
      ArgumentUtility.CheckNotNull ("codeGenerationContextFactory", codeGenerationContextFactory);
      ArgumentUtility.CheckNotNull ("initializationBuilder", initializationBuilder);
      ArgumentUtility.CheckNotNull ("proxySerializationEnabler", proxySerializationEnabler);

      _codeGenerationContextFactory = codeGenerationContextFactory;
      _initializationBuilder = initializationBuilder;
      _proxySerializationEnabler = proxySerializationEnabler;
    }

    public ICodeGenerator CodeGenerator
    {
      get { return _codeGenerationContextFactory.CodeGenerator; }
    }

    public Type Build (ProxyType proxyType)
    {
      ArgumentUtility.CheckNotNull ("proxyType", proxyType);

      var context = _codeGenerationContextFactory.CreateContext (proxyType);

      if (proxyType.MutableTypeInitializer != null)
        context.MemberEmitter.AddConstructor (context, proxyType.MutableTypeInitializer);

      var initializationMembers = _initializationBuilder.CreateInitializationMembers (proxyType);
      var initializationMethod = initializationMembers != null ? initializationMembers.Item2 : null;

      _proxySerializationEnabler.MakeSerializable (proxyType, initializationMethod);

      foreach (var customAttribute in proxyType.AddedCustomAttributes)
        context.TypeBuilder.SetCustomAttribute (customAttribute);

      foreach (var ifc in proxyType.AddedInterfaces)
        context.TypeBuilder.AddInterfaceImplementation (ifc);

      foreach (var field in proxyType.AddedFields)
        context.MemberEmitter.AddField (context, field);
      foreach (var ctor in proxyType.AddedConstructors)
        WireAndAddConstructor (context, ctor, initializationMembers);
      foreach (var method in proxyType.AddedMethods)
        context.MemberEmitter.AddMethod (context, method, method.Attributes);

      context.PostDeclarationsActionManager.ExecuteAllActions();

      return context.TypeBuilder.CreateType();
    }

    private void WireAndAddConstructor (
        CodeGenerationContext context, MutableConstructorInfo constructor, Tuple<FieldInfo, MethodInfo> initializationMembers)
    {
      _initializationBuilder.WireConstructorWithInitialization (constructor, initializationMembers, _proxySerializationEnabler);
      context.MemberEmitter.AddConstructor (context, constructor);
    }
  }
}