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
using NUnit.Framework;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.Abstractions;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class DefaultSubclassProxyBuilderFactoryTest
  {
    [Test]
    public void Initialization ()
    {
      var factory = new DefaultSubclassProxyBuilderFactory();

      var moduleNamePattern = @"TypePipe_GeneratedAssembly_\d+\.dll"; // e.g. TypePipe_GeneratedAssembly_7.dll
      var moduleName = GetModuleName (factory);
      Assert.That (moduleName, Is.StringMatching (moduleNamePattern));

      Assert.That (factory.DebugInfoGenerator.GetType().FullName, Is.EqualTo ("System.Runtime.CompilerServices.SymbolDocumentGenerator"));
    }

    [Test]
    public void Initialization_DifferentModuleName ()
    {
      var factory1 = new DefaultSubclassProxyBuilderFactory();
      var factory2 = new DefaultSubclassProxyBuilderFactory();

      Assert.That (GetModuleName (factory1), Is.Not.EqualTo (GetModuleName (factory2)));
    }

    private string GetModuleName (DefaultSubclassProxyBuilderFactory factory)
    {
      var moduleBuilderDecorator = (UniqueNamingModuleBuilderDecorator) factory.ModuleBuilder;
      var moduleBuilderAdapter = (ModuleBuilderAdapter) moduleBuilderDecorator.InnerModuleBuilder;

      return moduleBuilderAdapter.ScopeName;
    }
  }
}