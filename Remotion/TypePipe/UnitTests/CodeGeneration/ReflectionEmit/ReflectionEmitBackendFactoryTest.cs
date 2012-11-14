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

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class ReflectionEmitBackendFactoryTest
  {
    [Test]
    public void CreateModuleBuilder ()
    {
      var directory = Path.GetTempPath();
      var assemblyName = "TestAssembly";
      var assemblyFileName = assemblyName + ".dll";
      var tuple = ReflectionEmitBackendFactory.CreateModuleBuilder (assemblyName, directory);

      var moduleBuilder = tuple.Item1;
      var assemblyBuilder = tuple.Item2;

      Assert.That (assemblyBuilder.GetName().Name, Is.EqualTo (assemblyName));

      Assert.That (moduleBuilder, Is.TypeOf<UniqueNamingModuleBuilderDecorator>());
      var uniqueNamingModuleBuilderDecorator = (UniqueNamingModuleBuilderDecorator) moduleBuilder;
      Assert.That (uniqueNamingModuleBuilderDecorator.InnerModuleBuilder, Is.TypeOf<ModuleBuilderAdapter>());
      var moduleBuilderAdapter = (ModuleBuilderAdapter) uniqueNamingModuleBuilderDecorator.InnerModuleBuilder;
      Assert.That (moduleBuilderAdapter.ScopeName, Is.EqualTo (assemblyFileName));

      assemblyBuilder.Save (assemblyFileName);
      Assert.That (File.Exists (Path.Combine (directory, assemblyFileName)), Is.True);
      Assert.That (File.Exists (Path.Combine (directory, assemblyName + ".pdb")), Is.True);
    }
  }
}