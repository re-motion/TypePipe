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
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using Remotion.Development.UnitTesting.IO;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class LoadFlushedCodeTest : IntegrationTestBase
  {
    private const string c_participantConfigurationID = "LoadFlushedCodeTest";

    private Assembly _assembly1;
    private Assembly _assembly2;

    private ICodeManager _codeManager;
    private IReflectionService _reflectionService;

    public override void TestFixtureSetUp ()
    {
      base.TestFixtureSetUp();

      PreGenerateAssemblies();
    }

    public override void SetUp ()
    {
      base.SetUp ();

      var pipeline = CreatePipeline (c_participantConfigurationID);
      _codeManager = pipeline.CodeManager;
      _reflectionService = pipeline.ReflectionService;
    }

    [Test]
    public void LoadMultipleAssemblies ()
    {
      _codeManager.LoadFlushedCode (_assembly1);
      _codeManager.LoadFlushedCode (_assembly2);

      var assembledType1 = _reflectionService.GetAssembledType (typeof (DomainType1));
      var assembledType2 = _reflectionService.GetAssembledType (typeof (DomainType2));

      Assert.That (assembledType1.Assembly, Is.SameAs (_assembly1));
      Assert.That (assembledType2.Assembly, Is.SameAs (_assembly2));
      Assert.That (Flush(), Is.Null, "No new code should generated.");
    }

    [Test]
    [Ignore("TODO RM-5578")]
    public void LoadAssembly_ThenContinueGenerating ()
    {
      _codeManager.LoadFlushedCode (_assembly1);

      var assembledType1 = _reflectionService.GetAssembledType (typeof (DomainType1));

      Assert.That (assembledType1.Assembly, Is.SameAs (_assembly1));
      Assert.That (Flush(), Is.Null, "No new code should be generated.");

      var assembledType2 = _reflectionService.GetAssembledType (typeof (DomainType2));

      Assert.That (assembledType2.Assembly, Is.TypeOf<AssemblyBuilder>());
      Assert.That (Flush(), Is.Not.Null);
    }

    [Test]
    public void LoadAlreadyCachedType_DoesNothing ()
    {
      // Load and get type 1.
      _codeManager.LoadFlushedCode (_assembly1);
      var loadedType = _reflectionService.GetAssembledType (typeof (DomainType1));
      // Generate and get type 2.
      var generatedType = _reflectionService.GetAssembledType (typeof (DomainType2));

      _codeManager.LoadFlushedCode (_assembly1);
      _codeManager.LoadFlushedCode (_assembly2);

      Assert.That (_reflectionService.GetAssembledType (typeof (DomainType1)), Is.SameAs (loadedType));
      Assert.That (_reflectionService.GetAssembledType (typeof (DomainType2)), Is.SameAs (generatedType));
    }

    [Test]
    public void LoadTypes_RebuiltCacheKey_MustMatchRequestedCacheKey_ToReturnLoadedType ()
    {
      var loadedTypeWithMatchingKeys = _assembly1.GetTypes().Single();
      var loadedTypeWithNonMatchingKeys = _assembly2.GetTypes().Single();

      var cachKeyProviderStub = MockRepository.GenerateStub<ICacheKeyProvider>();
      cachKeyProviderStub.Stub (stub => stub.RebuildCacheKey (loadedTypeWithMatchingKeys.BaseType, loadedTypeWithMatchingKeys)).Return ("key");
      cachKeyProviderStub.Stub (stub => stub.RebuildCacheKey (loadedTypeWithMatchingKeys.BaseType, loadedTypeWithNonMatchingKeys)).Return ("key");
      cachKeyProviderStub.Stub (stub => stub.GetCacheKey (typeof (DomainType1))).Return ("key");
      cachKeyProviderStub.Stub (stub => stub.GetCacheKey (typeof (DomainType2))).Return ("runtime key differing from rebuilt key");
      var participant = CreateParticipant (cacheKeyProvider: cachKeyProviderStub);
      var pipeline = CreatePipeline (c_participantConfigurationID, participant);

      pipeline.CodeManager.LoadFlushedCode (_assembly1);
      pipeline.CodeManager.LoadFlushedCode (_assembly2);

      Assert.That (pipeline.ReflectionService.GetAssembledType (typeof (DomainType1)), Is.SameAs (loadedTypeWithMatchingKeys));
      Assert.That (pipeline.ReflectionService.GetAssembledType (typeof (DomainType2)), Is.Not.SameAs (loadedTypeWithNonMatchingKeys));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The specified assembly was generated with a different participant configuration: '" + c_participantConfigurationID
        + "'.\r\nParameter name: assembly")]
    public void LoadAssemblyGeneratedWithDifferentParticipantConfiguration ()
    {
      var objectFactory = CreatePipeline ("different config");
      objectFactory.CodeManager.LoadFlushedCode (_assembly1);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The specified assembly was not generated by the pipeline.\r\nParameter name: assembly")]
    public void LoadForeignAssembly ()
    {
      _codeManager.LoadFlushedCode (GetType ().Assembly);
    }

    private void PreGenerateAssemblies ()
    {
      var pipeline = CreatePipeline (c_participantConfigurationID);

      var assembledType1 = pipeline.ReflectionService.GetAssembledType (typeof (DomainType1));
      var assemblyPath1 = Flush();
      var assembledType2 = pipeline.ReflectionService.GetAssembledType (typeof (DomainType2));
      var assemblyPath2 = Flush();

      Assert.That (assembledType1.Assembly, Is.Not.SameAs (assembledType2.Assembly));
      Assert.That (assemblyPath1, Is.Not.Null.And.Not.EqualTo (assemblyPath2));

      _assembly1 = AssemblyLoader.LoadWithoutLocking (assemblyPath1);
      _assembly2 = AssemblyLoader.LoadWithoutLocking (assemblyPath2);
    }

    public class DomainType1 {}
    public class DomainType2 {}
  }
}