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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.IO;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class LoadFlushedCodeTest : IntegrationTestBase
  {
    private const string c_participantConfigurationID = "LoadFlushedCodeTest";
    private const string c_additionalTypeID = "AdditionalTypeID";

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

     var pipeline = CreatePipelineWithAdditionalTypeParticipant();
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
      Assert.That (Flush(), Is.Empty, "No new code should generated.");
    }

    [Test]
    public void LoadAssembly_ThenContinueGenerating ()
    {
      _codeManager.LoadFlushedCode (_assembly1);

      var assembledType1 = _reflectionService.GetAssembledType (typeof (DomainType1));

      Assert.That (assembledType1.Assembly, Is.SameAs (_assembly1));
      Assert.That (Flush(), Is.Empty, "No new code should be generated.");

      var assembledType2 = _reflectionService.GetAssembledType (typeof (DomainType2));

      Assert.That (assembledType2.Assembly.IsDynamic, Is.True);
      Assert.That (Flush(), Is.Not.Empty);
    }

    [Test]
    public void LoadAlreadyCachedType_DoesNothing ()
    {
      // Load and get type 1.
      _codeManager.LoadFlushedCode (_assembly1);
      var loadedType = _reflectionService.GetAssembledType (typeof (DomainType1));
      // Generate and get type 2.
      var generatedType = _reflectionService.GetAssembledType (typeof (DomainType2));
      var additionalType = _reflectionService.GetAdditionalType (c_additionalTypeID);

      _codeManager.LoadFlushedCode (_assembly1);
      _codeManager.LoadFlushedCode (_assembly2);

      Assert.That (_reflectionService.GetAssembledType (typeof (DomainType1)), Is.SameAs (loadedType));
      Assert.That (_reflectionService.GetAssembledType (typeof (DomainType2)), Is.SameAs (generatedType));
      Assert.That (_reflectionService.GetAdditionalType (c_additionalTypeID), Is.SameAs (additionalType));
    }

    [Test]
    public void LoadTypes_IdentifierSavedInType_MustMatchComputedIdentifier_ToReturnLoadedType ()
    {
      var typeIdentifierProviderStub = MockRepository.GenerateStub<ITypeIdentifierProvider>();
      typeIdentifierProviderStub.Stub (_ => _.GetID (typeof (DomainType1))).Return ("key1");
      typeIdentifierProviderStub.Stub (_ => _.GetID (typeof (DomainType2))).Return ("key2").Repeat.Once();
      typeIdentifierProviderStub.Stub (_ => _.GetExpression ("key1")).Return (Expression.Constant ("key1"));
      typeIdentifierProviderStub.Stub (_ => _.GetExpression ("key2")).Return (Expression.Constant ("key2"));
      var participant = CreateParticipant (typeIdentifierProvider: typeIdentifierProviderStub);

      var savingPipeline = CreatePipeline (c_participantConfigurationID, participant);
      var assembly1 = GenerateTypeFlushAndLoadAssembly (savingPipeline, typeof (DomainType1));
      var assembly2 = GenerateTypeFlushAndLoadAssembly (savingPipeline, typeof (DomainType2));

      // Change returned identifier.
      typeIdentifierProviderStub.Stub (_ => _.GetID (typeof (DomainType2))).Return ("other key");

      var loadingPipeline = CreatePipeline (c_participantConfigurationID, participant);
      loadingPipeline.CodeManager.LoadFlushedCode (assembly1);
      loadingPipeline.CodeManager.LoadFlushedCode (assembly2);

      var loadedTypeWithMatchingIdentifier = assembly1.GetTypes().Single();
      var loadedTypeWithNonMatchingIdentifier = assembly2.GetTypes().Single();
      Assert.That (loadingPipeline.ReflectionService.GetAssembledType (typeof (DomainType1)), Is.SameAs (loadedTypeWithMatchingIdentifier));
      Assert.That (loadingPipeline.ReflectionService.GetAssembledType (typeof (DomainType2)), Is.Not.SameAs (loadedTypeWithNonMatchingIdentifier));
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

    private Assembly GenerateTypeFlushAndLoadAssembly (IPipeline pipeline, Type requestedType)
    {
      pipeline.ReflectionService.GetAssembledType (requestedType);
      var assemblyPath = Flush().Single();

      return AssemblyLoader.LoadWithoutLocking (assemblyPath);
    }

    private void PreGenerateAssemblies ()
    {
      var pipeline = CreatePipelineWithAdditionalTypeParticipant();

      var assembledType1 = pipeline.ReflectionService.GetAssembledType (typeof (DomainType1));
      pipeline.ReflectionService.GetAdditionalType (c_additionalTypeID);
      var assemblyPath1 = Flush().Single();
      var assembledType2 = pipeline.ReflectionService.GetAssembledType (typeof (DomainType2));
      var assemblyPath2 = Flush().Single();

      Assert.That (assembledType1.Assembly, Is.Not.SameAs (assembledType2.Assembly));
      Assert.That (assemblyPath1, Is.Not.Null.And.Not.EqualTo (assemblyPath2));

      _assembly1 = AssemblyLoader.LoadWithoutLocking (assemblyPath1);
      _assembly2 = AssemblyLoader.LoadWithoutLocking (assemblyPath2);
    }

    private IPipeline CreatePipelineWithAdditionalTypeParticipant ()
    {
      var defaultPipeline = CreatePipeline();
      var additionalTypeParticipant =
          CreateParticipant (
              additionalTypeFunc: (id, ctx) =>
              {
                if ((string) id == c_additionalTypeID)
                  return ctx.CreateAdditionalType (c_additionalTypeID, "AdditionalType", "MyNs", TypeAttributes.Class, typeof (object));
                return null;
              });

      return CreatePipeline (
          c_participantConfigurationID,
          defaultPipeline.Participants.Concat (new[] { additionalTypeParticipant }).ToArray());
    }

    public class DomainType1 {}
    public class DomainType2 {}
  }
}