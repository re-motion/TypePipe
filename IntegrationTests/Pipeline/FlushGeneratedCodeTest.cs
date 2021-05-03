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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.IO;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
#if !FEATURE_ASSEMBLYBUILDER_SAVE
  [Ignore ("CodeManager.FlushCodeToDisk() is not supported.")]
#endif
  public class FlushGeneratedCodeTest : IntegrationTestBase
  {
    [Test]
    public void Standard ()
    {
      var pipeline = CreatePipeline();
      var assembledType1 = RequestType (pipeline, typeof (RequestedType));
      var path1 = Flush();

      var assembledType2 = RequestType (pipeline, typeof (OtherRequestedType));
      var path2 = Flush();

      Assert.That (path1, Is.Not.EqualTo (path2));
      Assert.That (assembledType1.FullName, Is.Not.EqualTo (assembledType2.FullName));

      CheckSavedAssembly (pipeline.ParticipantConfigurationID, path1, assembledType1.FullName);
      CheckSavedAssembly (pipeline.ParticipantConfigurationID, path2, assembledType2.FullName);
    }

    [Test]
    public void NoNewTypes ()
    {
      var pipeline = CreatePipeline();
      var codeManager = pipeline.CodeManager;

      Assert.That (codeManager.FlushCodeToDisk(), Is.Empty);

      RequestTypeAndFlush (pipeline, typeof (RequestedType));
      RequestType (pipeline, typeof (RequestedType));

      Assert.That (codeManager.FlushCodeToDisk(), Is.Empty);
    }

    [Test]
    public void AssemblyAttributes ()
    {
      var pipeline = CreatePipeline();

      var attributeCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new ObsoleteAttribute ("message"));
      var assemblyAttribute = new CustomAttributeDeclaration (attributeCtor, new object[] { "abc" });

      var path = RequestTypeAndFlush (pipeline, assemblyAttributes: new[] { assemblyAttribute });

      var assembly = AssemblyLoader.LoadWithoutLocking (path);
      var attributes = assembly.GetCustomAttributes (inherit: true);
      Assert.That (attributes, Has.Length.GreaterThan (1));

      var typePipeAttribute = attributes.OfType<TypePipeAssemblyAttribute>().Single();
      var obsoleteAttribute = attributes.OfType<ObsoleteAttribute>().Single();
      Assert.That (typePipeAttribute.ParticipantConfigurationID, Is.EqualTo (pipeline.ParticipantConfigurationID));
      Assert.That (obsoleteAttribute.Message, Is.EqualTo ("abc"));
    }

    [Test]
    public void StandardNamePatternAndDirectory ()
    {
      // Get code generator directly to avoid having assembly name and directory set by the integration test setup.
      var settings = PipelineSettings.Defaults;
      Assert.That (settings.AssemblyDirectory, Is.Null); // Current directory.
      Assert.That (settings.AssemblyNamePattern, Is.EqualTo (@"TypePipe_GeneratedAssembly_{counter}"));

      var pipeline = CreatePipelineExactAssemblyLocation ("standard", settings, CreateParticipant());

      var path = RequestTypeAndFlush (pipeline, skipPeVerification: true);

      var counter = (int) PrivateInvoke.GetNonPublicStaticField (typeof (ReflectionEmitCodeGenerator), "s_counter");
      var filename = string.Format ("TypePipe_GeneratedAssembly_{0}.dll", counter);
      var expectedPath = Path.Combine (Environment.CurrentDirectory, filename);
      Assert.That (path, Is.EqualTo (expectedPath));
    }

    [Test]
    public void CustomNamePatternAndDirectory ()
    {
      var directory = Path.GetTempPath();
      var settings = PipelineSettings.New()
          .SetAssemblyDirectory (directory)
          .SetAssemblyNamePattern ("Abc")
          .Build();
      Assert.That (settings.AssemblyDirectory, Is.EqualTo (directory));
      Assert.That (settings.AssemblyNamePattern, Is.EqualTo ("Abc"));

      var pipeline = CreatePipelineExactAssemblyLocation ("standard", settings, CreateParticipant());

      // The assembly will be saved in a directory that lacks the needed references for peverify.
      var path = RequestTypeAndFlush (pipeline, skipPeVerification: true);

      Assert.That (path, Is.EqualTo (Path.Combine (directory, "Abc.dll")));
      Assert.That (File.Exists (path), Is.True);
    }

    [Test]
    public void CustomNamePatternWithoutCounter_OverwritesPreviousAssembly ()
    {
      var settings = PipelineSettings.New()
          .SetAssemblyNamePattern ("xxx")
          .Build();

      var pipeline = CreatePipelineExactAssemblyLocation ("standard", settings, CreateParticipant());

      var assemblyPath1 = RequestTypeAndFlush (pipeline, typeof (RequestedType));
      var assemblyPath2 = RequestTypeAndFlush (pipeline, typeof (OtherRequestedType));

      Assert.That (assemblyPath1, Is.EqualTo (assemblyPath2));
    }

    [Test]
    public void CustomNamePatternIncludingCounter_ProducesUniqueAssemblyNames ()
    {
      var settings = PipelineSettings.New()
          .SetAssemblyNamePattern ("xxx_{counter}")
          .Build();

      var pipeline = CreatePipelineExactAssemblyLocation ("standard", settings, CreateParticipant());

      var assemblyPath1 = RequestTypeAndFlush (pipeline, typeof (RequestedType));
      var assemblyPath2 = RequestTypeAndFlush (pipeline, typeof (OtherRequestedType));

      Assert.That (assemblyPath1, Is.Not.EqualTo (assemblyPath2));
    }

    private Type RequestType (IPipeline pipeline, Type requestedType = null)
    {
      requestedType = requestedType ?? typeof (RequestedType);
      return pipeline.ReflectionService.GetAssembledType (requestedType);
    }

    private string RequestTypeAndFlush (
        IPipeline pipeline, Type requestedType = null, CustomAttributeDeclaration[] assemblyAttributes = null, bool skipPeVerification = false)
    {
      RequestType (pipeline, requestedType);
      return Flush (assemblyAttributes, skipPeVerification);
    }

    private string Flush (CustomAttributeDeclaration[] assemblyAttributes = null, bool skipPeVerification = false)
    {
      var assemblyPaths = base.Flush (assemblyAttributes, skipPeVerification: skipPeVerification);
      Assert.That (assemblyPaths, Has.Length.EqualTo (1));

      var assemblyPath = assemblyPaths.Single();

      Assert.That (File.Exists (assemblyPath), Is.True);
      Assert.That (File.Exists (Path.ChangeExtension (assemblyPath, "pdb")), Is.True);

      return assemblyPath;
    }

    private void CheckSavedAssembly (string participantConfigurationID, string assemblyPath, string assembledTypeFullName)
    {
      Assert.That (File.Exists (assemblyPath), Is.True);

      var assembly = AssemblyLoader.LoadWithoutLocking (assemblyPath);
      var typePipeAssemblyAttribute = 
          (TypePipeAssemblyAttribute) assembly.GetCustomAttributes (typeof (TypePipeAssemblyAttribute), false).SingleOrDefault();
      Assert.That (typePipeAssemblyAttribute, Is.Not.Null);
      Assert.That (typePipeAssemblyAttribute.ParticipantConfigurationID, Is.EqualTo (participantConfigurationID));

      var typeNames = assembly.GetExportedTypes().Select (t => t.FullName);

      Assert.That (typeNames, Is.EqualTo (new[] { assembledTypeFullName }));
    }

    public class RequestedType { }
    public class OtherRequestedType { }
  }
}