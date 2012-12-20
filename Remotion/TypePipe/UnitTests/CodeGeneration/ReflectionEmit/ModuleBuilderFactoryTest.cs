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
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
using Remotion.TypePipe.StrongNaming;
using Remotion.Utilities;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ModuleBuilderFactoryTest
  {
    private const string c_assemblyName = "MyAssembly";
    private const string c_assemblyFileName = "MyAssembly.dll";
    private const string c_pdbFileName = "MyAssembly.pdb";

    private ModuleBuilderFactory _factory;

    private string _currentDirectory;

    [SetUp]
    public void SetUp ()
    {
      _factory = new ModuleBuilderFactory();

      _currentDirectory = Environment.CurrentDirectory;
    }

    [Test]
    public void CreateModuleBuilder ()
    {
      var result = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: false, keyFilePathOrNull: null);

      CheckDecoratedAdapterAndSaveToDiskBehavior (result, _currentDirectory);
    }

    [Test]
    public void CreateModuleBuilder_CustomDirectory ()
    {
      var tempDirectory = Path.GetTempPath();
      var result = _factory.CreateModuleBuilder (c_assemblyName, tempDirectory, false, null);

      CheckDecoratedAdapterAndSaveToDiskBehavior (result, tempDirectory);
    }

    [Test]
    public void CreateModuleBuilder_StrongNamed_FallbackKey ()
    {
      var result1 = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: true, keyFilePathOrNull: null);
      var result2 = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: true, keyFilePathOrNull: string.Empty);

      var publicKey = FallbackKey.KeyPair.PublicKey;
      CheckDecoratedAdapterAndSaveToDiskBehavior (result1, _currentDirectory, expectedPublicKey: publicKey);
      CheckDecoratedAdapterAndSaveToDiskBehavior (result2, _currentDirectory, expectedPublicKey: publicKey);
    }

    [Test]
    public void CreateModuleBuilder_StrongNamed_ProvidedKey ()
    {
      var otherKeyPath = @"..\..\..\..\..\remotion.snk";
      var result = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null, strongNamed: true, keyFilePathOrNull: otherKeyPath);

      var publicKey = new StrongNameKeyPair (File.ReadAllBytes (otherKeyPath)).PublicKey;
      CheckDecoratedAdapterAndSaveToDiskBehavior (result, _currentDirectory, expectedPublicKey: publicKey);
    }

    private void CheckDecoratedAdapterAndSaveToDiskBehavior (IModuleBuilder moduleBuilder, string assemblyDirectory, byte[] expectedPublicKey = null)
    {
      Assert.That (moduleBuilder, Is.TypeOf<UniqueNamingModuleBuilderDecorator>());
      var decorator = (UniqueNamingModuleBuilderDecorator) moduleBuilder;

      Assert.That (decorator.InnerModuleBuilder, Is.TypeOf<ModuleBuilderAdapter>());
      var adapter = (ModuleBuilderAdapter) decorator.InnerModuleBuilder;
      Assert.That (adapter.AssemblyName, Is.EqualTo (c_assemblyName));
      Assert.That (adapter.ScopeName, Is.EqualTo (c_assemblyFileName));
      Assert.That (adapter.PublicKey, Is.EqualTo (expectedPublicKey ?? new byte[0]));

      var assemblyPath = Path.Combine (assemblyDirectory, c_assemblyFileName);
      var pdbPath = Path.Combine (assemblyDirectory, c_pdbFileName);
      Assert.That (File.Exists (assemblyPath), Is.False);
      Assert.That (File.Exists (pdbPath), Is.False);

      var result = adapter.SaveToDisk();

      Assert.That (File.Exists (assemblyPath), Is.True);
      Assert.That (File.Exists (pdbPath), Is.True);
      Assert.That (result, Is.EqualTo (assemblyPath));

      FileUtility.DeleteAndWaitForCompletion (Path.Combine (assemblyDirectory, c_assemblyFileName));
      FileUtility.DeleteAndWaitForCompletion (Path.Combine (assemblyDirectory, c_pdbFileName));
    }
  }
}