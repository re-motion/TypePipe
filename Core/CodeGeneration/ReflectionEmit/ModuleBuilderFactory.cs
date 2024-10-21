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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using JetBrains.Annotations;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.StrongNaming;
using Remotion.Utilities;

namespace Remotion.TypePipe.CodeGeneration.ReflectionEmit
{
#if NETFRAMEWORK
  /// <summary>
  /// This class creates instances of <see cref="IModuleBuilder"/>.
  /// </summary>
  /// <remarks> The module will be created with <see cref="AssemblyBuilderAccess.RunAndSave"/> and the <c>emitSymbolInfo</c> flag set to
  /// <see langword="true"/>.
  /// </remarks>
  /// <threadsafety static="true" instance="true"/>
#elif NET9_0_OR_GREATER
  /// <summary>
  /// This class creates instances of <see cref="IModuleBuilder"/>.
  /// </summary>
  /// <remarks> The module will be created with <see cref="PersistedAssemblyBuilder"/> and the <c>emitSymbolInfo</c> flag set to
  /// <see langword="true"/>.
  /// </remarks>
  /// <threadsafety static="true" instance="true"/>
#else
  /// <summary>
  /// This class creates instances of <see cref="IModuleBuilder"/>.
  /// </summary>
  /// <remarks> The module will be created with <see cref="AssemblyBuilderAccess.Run"/>.
  /// </remarks>
  /// <threadsafety static="true" instance="true"/>
#endif
  public class ModuleBuilderFactory : IModuleBuilderFactory
  {
    private static readonly ConstructorInfo s_typePipeAssemblyAttributeCtor =
        MemberInfoFromExpressionUtility.GetConstructor (() => new TypePipeAssemblyAttribute ("participantConfigurationID"));

    private readonly string _participantConfigurationID;

    public ModuleBuilderFactory (string participantConfigurationID)
    {
      _participantConfigurationID = participantConfigurationID;
    }

    [CLSCompliant (false)]
    public IModuleBuilder CreateModuleBuilder (string assemblyName, string assemblyDirectoryOrNull, bool strongNamed, string keyFilePathOrNull)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("assemblyName", assemblyName);

      var assemName = new AssemblyName (assemblyName);
      if (strongNamed)
      {
#if FEATURE_STRONGNAMESIGNING
        assemName.KeyPair = GetKeyPair (keyFilePathOrNull);
#else
        throw new PlatformNotSupportedException();
#endif
      }

#if NETFRAMEWORK
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (assemName, AssemblyBuilderAccess.RunAndSave, assemblyDirectoryOrNull);
#elif NET9_0_OR_GREATER
      var assemblyBuilder = new PersistedAssemblyBuilder (new AssemblyName(assemblyName), typeof(object).Assembly);
#else
      var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly (assemName, AssemblyBuilderAccess.Run);
#endif

      var moduleName = assemblyName + ".dll";

#if FEATURE_PDBEMIT
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (moduleName, emitSymbolInfo: true);
#else
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (moduleName);
#endif

      var moduleBuilderAdapter = new ModuleBuilderAdapter (moduleBuilder);

      var typePipeAttribute = new CustomAttributeDeclaration (s_typePipeAssemblyAttributeCtor, new object[] { _participantConfigurationID });
      moduleBuilderAdapter.AssemblyBuilder.SetCustomAttribute (typePipeAttribute);

      return moduleBuilderAdapter;
    }

#if FEATURE_STRONGNAMESIGNING
    [NotNull]
    private StrongNameKeyPair GetKeyPair ([CanBeNull]string keyFilePathOrNull)
    {
      if (string.IsNullOrEmpty (keyFilePathOrNull))
        return FallbackKey.KeyPair;

      using (var fileStream = File.OpenRead (keyFilePathOrNull))
        return new StrongNameKeyPair (fileStream);
    }
#endif
  }
}