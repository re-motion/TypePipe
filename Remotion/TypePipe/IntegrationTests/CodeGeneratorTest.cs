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
using Remotion.TypePipe;
using Remotion.Utilities;

namespace TypePipe.IntegrationTests
{
  [Ignore ("5204")]
  [TestFixture]
  public class FlushGeneratedCodeTest : ObjectFactoryIntegrationTestBase
  {
    private IObjectFactory _factory;
    private string _assemblyPath1;
    private string _assemblyPath2;

    public override void SetUp ()
    {
      _factory = CreateObjectFactory();

      _assemblyPath1 = null;
      _assemblyPath2 = null;
    }

    public override void TearDown ()
    {
      if (_assemblyPath1 != null) FileUtility.DeleteAndWaitForCompletion (_assemblyPath1);
      if (_assemblyPath2 != null) FileUtility.DeleteAndWaitForCompletion (_assemblyPath2);
    }

    [Test]
    public void FlushToDisk ()
    {
      _factory.CodeGenerator.SetAssemblyName ("TestAssembly");
      var assembledType = _factory.GetAssembledType (typeof (RequestedType));
      _assemblyPath1 = _factory.CodeGenerator.FlushToDisk();
      
      Assert.That (File.Exists (_assemblyPath1), Is.True);
      var assembly = Assembly.LoadFrom (_assemblyPath1);
      Assert.That (() => assembly.GetType (assembledType.FullName, true), Throws.Nothing);
    }

    [Test]
    public void SetName_AfterFlush ()
    {
      _factory.CodeGenerator.SetAssemblyName ("TestAssembly");
      _factory.GetAssembledType (typeof (RequestedType));
      _assemblyPath1 = _factory.CodeGenerator.FlushToDisk ();

      Assert.That (() => _factory.CodeGenerator.SetAssemblyName ("TestAssembly2"), Throws.Nothing);
      Assert.That (() => _assemblyPath2 = _factory.CodeGenerator.FlushToDisk (), Throws.Nothing);
    }

    [Test]
    public void FlushToDisk_UniqueNaming ()
    {
      var assemblyName = _factory.CodeGenerator.AssemblyName;
      Assert.That (assemblyName, Is.StringMatching (@"TypePipe_GeneratedAssembly_\d+\.dll"));

      _factory.GetAssembledType (typeof (RequestedType));
      _assemblyPath1 = _factory.CodeGenerator.FlushToDisk ();

      Assert.That (_factory.CodeGenerator.AssemblyName, Is.Not.EqualTo (assemblyName));
    }

    class RequestedType { }
  }
}