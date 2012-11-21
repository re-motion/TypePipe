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
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;
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

    private string _someDirectory;
    private string _currentDirectory;

    [SetUp]
    public void SetUp ()
    {
      _factory = new ModuleBuilderFactory();

      _someDirectory = Path.GetTempPath();
      _currentDirectory = Environment.CurrentDirectory;
    }

    [TearDown]
    public void TearDown ()
    {
      FileUtility.DeleteAndWaitForCompletion (Path.Combine (_someDirectory, c_assemblyFileName));
      FileUtility.DeleteAndWaitForCompletion (Path.Combine (_someDirectory, c_pdbFileName));
      FileUtility.DeleteAndWaitForCompletion (Path.Combine (_currentDirectory, c_assemblyFileName));
      FileUtility.DeleteAndWaitForCompletion (Path.Combine (_currentDirectory, c_pdbFileName));
    }

    [Test]
    public void CreateModuleBuilder ()
    {
      var result = _factory.CreateModuleBuilder (c_assemblyName, _someDirectory);

      CheckDecoratedAdapterAndSaveToDiskBehavior (result, _someDirectory);
    }

    [Test]
    public void CreateModuleBuilder_NullAssemblyDirectory ()
    {
      var result = _factory.CreateModuleBuilder (c_assemblyName, assemblyDirectoryOrNull: null);

      CheckDecoratedAdapterAndSaveToDiskBehavior (result, _currentDirectory);
    }

    private static void CheckDecoratedAdapterAndSaveToDiskBehavior (IModuleBuilder moduleBuilder, string assemblyDirectory)
    {
      Assert.That (moduleBuilder, Is.TypeOf<UniqueNamingModuleBuilderDecorator> ());
      var decorator = (UniqueNamingModuleBuilderDecorator) moduleBuilder;

      Assert.That (decorator.InnerModuleBuilder, Is.TypeOf<ModuleBuilderAdapter> ());
      var adapter = (ModuleBuilderAdapter) decorator.InnerModuleBuilder;
      Assert.That (adapter.AssemblyName, Is.EqualTo (c_assemblyName));
      Assert.That (adapter.ScopeName, Is.EqualTo (c_assemblyFileName));

      var assemblyPath = Path.Combine (assemblyDirectory, c_assemblyFileName);
      var pdbPath = Path.Combine (assemblyDirectory, c_pdbFileName);
      Assert.That (File.Exists (assemblyPath), Is.False);
      Assert.That (File.Exists (pdbPath), Is.False);

      var result = adapter.SaveToDisk ();

      Assert.That (File.Exists (assemblyPath), Is.True);
      Assert.That (File.Exists (pdbPath), Is.True);
      Assert.That (result, Is.EqualTo (assemblyPath));
    }
  }
}