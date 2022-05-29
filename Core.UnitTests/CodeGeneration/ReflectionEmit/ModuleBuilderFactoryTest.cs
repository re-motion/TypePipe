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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.StrongNaming;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ModuleBuilderFactoryTest
  {
    private const string c_participantID = "MyParticipantID";
    private const string c_assemblyName = "MyAssembly";
    private const string c_assemblyFileName = "MyAssembly.dll";
    private const string c_pdbFileName = "MyAssembly.pdb";

    private ModuleBuilderFactory _factory;

    private string _currentDirectory;

    [SetUp]
    public void SetUp ()
    {
      _factory = new ModuleBuilderFactory (c_participantID);

      _currentDirectory = Environment.CurrentDirectory;
    }
    [Test]
    public void CreateModuleBuilder ()
    {
      var result = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: false, keyFilePathOrNull: null);

      CheckAdapterBehavior (result);
#if FEATURE_ASSEMBLYBUILDER_SAVE
      CheckSaveToDiskBehavior (result, _currentDirectory);
#endif
    }

    [Test]
    public void CreateModuleBuilder_AppliesTypePipeAssemblyAttribute ()
    {
      var assemblyName = c_assemblyName + Guid.NewGuid();
      _factory.CreateModuleBuilder (assemblyName, assemblyDirectoryOrNull: null, strongNamed: false, keyFilePathOrNull: null);

      var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == assemblyName);
      Assert.That (assembly, Is.Not.Null);
      var typePipeAssemblyAttribute =
          (TypePipeAssemblyAttribute) assembly.GetCustomAttributes (typeof (TypePipeAssemblyAttribute), false).SingleOrDefault();
      Assert.That (typePipeAssemblyAttribute, Is.Not.Null);
      Assert.That (typePipeAssemblyAttribute.ParticipantConfigurationID, Is.EqualTo (c_participantID));
    }

    [Test]
    public void CreateModuleBuilder_CustomDirectory ()
    {
      var tempDirectory = Path.GetTempPath();
      var result = _factory.CreateModuleBuilder (c_assemblyName, tempDirectory, false, null);

      CheckAdapterBehavior (result);
#if FEATURE_ASSEMBLYBUILDER_SAVE
      CheckSaveToDiskBehavior (result, tempDirectory);
#endif
    }

    [Test]
    public void CreateModuleBuilder_StrongNamed_FallbackKey ()
    {
      try
      {
        Dev.Null = FallbackKey.KeyPair.PublicKey;
      }
      catch (PlatformNotSupportedException)
      {
#if FEATURE_ASSEMBLYBUILDER_SAVE
        throw;
#else
        Assert.Ignore (".NET does not support assembly persistence.");
#endif
      }

      var result1 = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: true, keyFilePathOrNull: null);
      var result2 = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: true, keyFilePathOrNull: string.Empty);

      var publicKey = FallbackKey.KeyPair.PublicKey;

      CheckAdapterBehavior (result1, expectedPublicKey: publicKey);
      CheckSaveToDiskBehavior (result1, _currentDirectory);

      CheckAdapterBehavior (result2, expectedPublicKey: publicKey);
      CheckSaveToDiskBehavior (result2, _currentDirectory);
    }

    [Test]
    public void CreateModuleBuilder_StrongNamed_ProvidedKey ()
    {
      try
      {
        Dev.Null = FallbackKey.KeyPair.PublicKey;
      }
      catch (PlatformNotSupportedException)
      {
#if FEATURE_ASSEMBLYBUILDER_SAVE
        throw;
#else
        Assert.Ignore (".NET does not support assembly persistence.");
#endif
      }

      var otherKeyPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, @"CodeGeneration\ReflectionEmit\OtherKey.snk");
      var result = _factory.CreateModuleBuilder (
          c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: true, keyFilePathOrNull: otherKeyPath);

      var publicKey = new StrongNameKeyPair (File.ReadAllBytes (otherKeyPath)).PublicKey;

      CheckAdapterBehavior (result, expectedPublicKey: publicKey);
      CheckSaveToDiskBehavior (result, _currentDirectory);
    }

    private void CheckAdapterBehavior (IModuleBuilder moduleBuilder, byte[] expectedPublicKey = null)
    {
      Assert.That (moduleBuilder, Is.TypeOf<ModuleBuilderAdapter>());
      var moduleBuilderAdapter = (ModuleBuilderAdapter) moduleBuilder;
#if FEATURE_ASSEMBLYBUILDER_SAVE
      Assert.That (moduleBuilderAdapter.ScopeName, Is.EqualTo (c_assemblyFileName));
#else
      // .NET5 as a hardcoded module name since it does not support AssemblyBuilder.Save().
      Assert.That (moduleBuilderAdapter.ScopeName, Is.EqualTo ("RefEmit_InMemoryManifestModule"));
#endif
      Assert.That (moduleBuilderAdapter.AssemblyBuilder, Is.TypeOf<AssemblyBuilderAdapter>());
      var assemblyBuilderAdapter = (AssemblyBuilderAdapter) moduleBuilder.AssemblyBuilder;

      Assert.That (assemblyBuilderAdapter.AssemblyName, Is.EqualTo (c_assemblyName));
      Assert.That (assemblyBuilderAdapter.PublicKey, Is.EqualTo (expectedPublicKey ?? new byte[0]));
    }

    private void CheckSaveToDiskBehavior (IModuleBuilder moduleBuilder, string assemblyDirectory)
    {
      var assemblyPath = Path.Combine (assemblyDirectory, c_assemblyFileName);
      var pdbPath = Path.Combine (assemblyDirectory, c_pdbFileName);
      Assert.That (File.Exists (assemblyPath), Is.False);
      Assert.That (File.Exists (pdbPath), Is.False);

      var result = moduleBuilder.AssemblyBuilder.SaveToDisk();

      Assert.That (File.Exists (assemblyPath), Is.True);
      Assert.That (File.Exists (pdbPath), Is.True);
      Assert.That (result, Is.EqualTo (assemblyPath));

      FileUtility.DeleteAndWaitForCompletion (Path.Combine (assemblyDirectory, c_assemblyFileName));
      FileUtility.DeleteAndWaitForCompletion (Path.Combine (assemblyDirectory, c_pdbFileName));
    }
  }
}
