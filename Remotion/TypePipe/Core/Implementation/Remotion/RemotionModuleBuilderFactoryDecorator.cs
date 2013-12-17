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
using Remotion.Reflection.TypeDiscovery;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.Implementation.Remotion
{
  // TODO 5545: Move this to Remotion.Core.
  /// <summary>
  /// Decorates an instance of <see cref="IModuleBuilderFactory"/> and adds the <see cref="NonApplicationAssemblyAttribute"/> to the
  /// <see cref="IAssemblyBuilder"/> whenever a <see cref="IModuleBuilder"/> is created.
  /// </summary>
  /// <threadsafety static="true" instance="true"/>
  public class RemotionModuleBuilderFactoryDecorator : IModuleBuilderFactory
  {
    private static readonly ConstructorInfo s_nonApplicationAssemblyAttributeConstructor =
        MemberInfoFromExpressionUtility.GetConstructor (() => new NonApplicationAssemblyAttribute());

    private readonly IModuleBuilderFactory _moduleBuilderFactory;

    [CLSCompliant (false)]
    public RemotionModuleBuilderFactoryDecorator (IModuleBuilderFactory moduleBuilderFactory)
    {
      ArgumentUtility.CheckNotNull ("moduleBuilderFactory", moduleBuilderFactory);

      _moduleBuilderFactory = moduleBuilderFactory;
    }

    [CLSCompliant (false)]
    public IModuleBuilder CreateModuleBuilder (string assemblyName, string assemblyDirectoryOrNull, bool strongNamed, string keyFilePathOrNull)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyName", assemblyName);

      var moduleBuilder = _moduleBuilderFactory.CreateModuleBuilder (assemblyName, assemblyDirectoryOrNull, strongNamed, keyFilePathOrNull);

      var attribute = new CustomAttributeDeclaration (s_nonApplicationAssemblyAttributeConstructor, new object[0]);
      moduleBuilder.AssemblyBuilder.SetCustomAttribute (attribute);

      return moduleBuilder;
    }
  }
}