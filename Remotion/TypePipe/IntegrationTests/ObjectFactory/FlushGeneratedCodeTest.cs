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
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.ObjectFactory
{
  [TestFixture]
  public class FlushGeneratedCodeTest : IntegrationTestBase
  {
    private IObjectFactory _objectFactory;
    private ICodeGenerator _codeGenerator;

    public override void SetUp ()
    {
      base.SetUp();

      _objectFactory = CreateObjectFactory();
      _codeGenerator = _objectFactory.CodeGenerator;
    }

    [Test]
    public void FlushGeneratedCode ()
    {
      var assembledType1 = RequestType (typeof (RequestedType));
      var path1 = Flush();

      var assembledType2 = RequestType (typeof (object));
      var path2 = Flush();

      Assert.That (path1, Is.Not.EqualTo (path2));
      Assert.That (assembledType1.FullName, Is.Not.EqualTo (assembledType2.FullName));

      CheckSavedAssembly (path1, assembledType1.FullName);
      CheckSavedAssembly (path2, assembledType2.FullName);
    }

    [Test]
    public void FlushGeneratedCode_NoNewTypes ()
    {
      Assert.That (_codeGenerator.FlushCodeToDisk(), Is.Null);

      RequestTypeAndFlush (typeof (RequestedType));
      RequestType (typeof (RequestedType));

      Assert.That (_codeGenerator.FlushCodeToDisk(), Is.Null);
    }

    [Test]
    public void StandardNameAndDirectory_Initial ()
    {
      // Get code generator directly to avoid having assembly name and directory set by the integration test setup.
      var objectFactory = Pipeline.Create ("standard", CreateNopParticipant());
      var codeGenerator = objectFactory.CodeGenerator;

      var assemblyName = codeGenerator.AssemblyName;
      Assert.That (codeGenerator.AssemblyDirectory, Is.Null); // Current directory.
      Assert.That (assemblyName, Is.StringMatching (@"TypePipe_GeneratedAssembly_\d+"));

      objectFactory.GetAssembledType (typeof (RequestedType));
      var assemblyPath = objectFactory.CodeGenerator.FlushCodeToDisk();

      var expectedAssemblyPath = Path.Combine (Environment.CurrentDirectory, assemblyName + ".dll");
      Assert.That (assemblyPath, Is.EqualTo (expectedAssemblyPath));
    }

    [Test]
    public void StandardName_IsUnique ()
    {
      var oldAssemblyName = _codeGenerator.AssemblyName;

      RequestTypeAndFlush();

      Assert.That (_codeGenerator.AssemblyName, Is.Not.EqualTo (oldAssemblyName));
    }

    [Test]
    public void CustomNameAndDirectory ()
    {
      var directory = Path.GetTempPath();
      _codeGenerator.SetAssemblyDirectory (directory);
      _codeGenerator.SetAssemblyName ("Abc");

      Assert.That (_codeGenerator.AssemblyDirectory, Is.EqualTo (directory));
      Assert.That (_codeGenerator.AssemblyName, Is.EqualTo ("Abc"));

      // The assembly will be saved in a directory that lacks the needed references for peverify.
      var path = RequestTypeAndFlush (skipPeVerification: true);

      Assert.That (path, Is.EqualTo (Path.Combine (directory, "Abc.dll")));
      Assert.That (File.Exists (path), Is.True);
    }

    [Test]
    public void SetNameAndDirectory_AfterFlush ()
    {
      RequestType();

      var message1 = "Cannot set assembly directory after a type has been defined (use FlushCodeToDisk() to start a new assembly).";
      var message2 = "Cannot set assembly name after a type has been defined (use FlushCodeToDisk() to start a new assembly).";
      Assert.That (() => _codeGenerator.SetAssemblyDirectory ("Uio"), Throws.InvalidOperationException.With.Message.EqualTo (message1));
      Assert.That (() => _codeGenerator.SetAssemblyName ("Xyz"), Throws.InvalidOperationException.With.Message.EqualTo (message2));

      Flush();

      _codeGenerator.SetAssemblyDirectory ("Uio");
      _codeGenerator.SetAssemblyName ("Xyz");

      Assert.That (_codeGenerator.AssemblyDirectory, Is.EqualTo ("Uio"));
      Assert.That (_codeGenerator.AssemblyName, Is.EqualTo ("Xyz"));
    }

    private Type RequestType (Type requestedType = null)
    {
      requestedType = requestedType ?? typeof (RequestedType);
      return _objectFactory.GetAssembledType (requestedType);
    }

    private string RequestTypeAndFlush (Type requestedType = null, bool skipPeVerification = false)
    {
      RequestType (requestedType);
      return Flush (skipPeVerification);
    }

    private string Flush (bool skipPeVerification = false)
    {
      var assemblyPath = base.Flush (skipPeVerification: skipPeVerification);
      Assert.That (assemblyPath, Is.Not.Null);

      Assert.That (File.Exists (assemblyPath), Is.True);
      Assert.That (File.Exists (Path.ChangeExtension (assemblyPath, "pdb")), Is.True);

      return assemblyPath;
    }

    private void CheckSavedAssembly (string assemblyPath, string assembledTypeFullName)
    {
      Assert.That (File.Exists (assemblyPath), Is.True);

      // Load via raw bytes so that the file is not locked and can be deleted.
      var assembly = Assembly.Load (File.ReadAllBytes (assemblyPath));
      var typeNames = assembly.GetExportedTypes().Select (t => t.FullName);

      Assert.That (typeNames, Is.EqualTo (new[] { assembledTypeFullName }));
    }

    public class RequestedType { }
  }
}