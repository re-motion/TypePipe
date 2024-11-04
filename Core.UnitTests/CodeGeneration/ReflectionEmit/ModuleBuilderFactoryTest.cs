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

    [TearDown]
    public void TearDown ()
    {
      var assemblyPath = Path.Combine (_currentDirectory, c_assemblyFileName);
      if (File.Exists (assemblyPath))
        File.Delete (assemblyPath);
    }

    [Test]
    public void CreateModuleBuilder ()
    {
      var result = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: false, keyFilePathOrNull: null);

      CheckAdapterBehavior (result);
#if NETFRAMEWORK || NET9_0_OR_GREATER
      CheckSaveToDiskBehavior (result, _currentDirectory);
#endif
    }

    [Test]
    public void CreateModuleBuilder_AppliesTypePipeAssemblyAttribute ()
    {
      var assemblyName = c_assemblyName + Guid.NewGuid();
      var moduleBuilder = _factory.CreateModuleBuilder (assemblyName, assemblyDirectoryOrNull: null, strongNamed: false, keyFilePathOrNull: null);

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
#if NETFRAMEWORK || NET9_0_OR_GREATER
      CheckSaveToDiskBehavior (result, tempDirectory);
#endif
    }

    [Test]
#if !FEATURE_STRONGNAMESIGNING
    [Ignore("Platform does not support strong named assembly signing.")]
#endif
    public void CreateModuleBuilder_StrongNamed_FallbackKey ()
    {
      Dev.Null = FallbackKey.KeyPair.PublicKey;

      var result1 = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: true, keyFilePathOrNull: null);
      var result2 = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: true, keyFilePathOrNull: string.Empty);

      var publicKey = FallbackKey.KeyPair.PublicKey;

      CheckAdapterBehavior (result1, expectedPublicKey: publicKey);
      CheckSaveToDiskBehavior (result1, _currentDirectory);

      CheckAdapterBehavior (result2, expectedPublicKey: publicKey);
      CheckSaveToDiskBehavior (result2, _currentDirectory);
    }

    [Test]
#if !FEATURE_STRONGNAMESIGNING
    [Ignore("Platform does not support strong named assembly signing.")]
#endif
    public void CreateModuleBuilder_StrongNamed_ProvidedKey ()
    {
      Dev.Null = FallbackKey.KeyPair.PublicKey;

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
#if NETFRAMEWORK || NET9_0_OR_GREATER
      Assert.That (moduleBuilderAdapter.ScopeName, Is.EqualTo (c_assemblyFileName));
#else
      // .NET5 as a hardcoded module name since it does not support AssemblyBuilder.Save().
      Assert.That (moduleBuilderAdapter.ScopeName, Is.EqualTo ("RefEmit_InMemoryManifestModule"));
#endif
      Assert.That (moduleBuilderAdapter.AssemblyBuilder, Is.TypeOf<AssemblyBuilderAdapter>());
      var assemblyBuilderAdapter = (AssemblyBuilderAdapter) moduleBuilder.AssemblyBuilder;

      Assert.That (assemblyBuilderAdapter.AssemblyName, Is.EqualTo (c_assemblyName));
#if NET9_0_OR_GREATER
      Assert.That (assemblyBuilderAdapter.PublicKey, Is.EqualTo (expectedPublicKey));
#else
      Assert.That (assemblyBuilderAdapter.PublicKey, Is.EqualTo (expectedPublicKey ?? new byte[0]));
#endif
    }

    private void CheckSaveToDiskBehavior (IModuleBuilder moduleBuilder, string assemblyDirectory)
    {
      var assemblyPath = Path.Combine (assemblyDirectory, c_assemblyFileName);
      var pdbPath = Path.Combine (assemblyDirectory, c_pdbFileName);
      Assert.That (File.Exists (assemblyPath), Is.False, assemblyPath);
      Assert.That (File.Exists (pdbPath), Is.False, pdbPath);

      var result = moduleBuilder.AssemblyBuilder.SaveToDisk();

      Assert.That (File.Exists (assemblyPath), Is.True, assemblyPath);
#if FEATURE_PDBEMIT
      Assert.That (File.Exists (pdbPath), Is.True, pdbPath);
#endif
      Assert.That (result, Is.EqualTo (assemblyPath));

      FileUtility.DeleteAndWaitForCompletion (Path.Combine (assemblyDirectory, c_assemblyFileName));
      FileUtility.DeleteAndWaitForCompletion (Path.Combine (assemblyDirectory, c_pdbFileName));
    }
  }
}
