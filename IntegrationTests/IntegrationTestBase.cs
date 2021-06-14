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
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.TypeAssembly;
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests
{
  public abstract class IntegrationTestBase
  {
    protected static IParticipant CreateParticipant (Action<MutableType> typeModification)
    {
      return CreateParticipant (ctx => typeModification (ctx.ProxyType));
    }

    protected static IParticipant CreateParticipant (Action<IProxyTypeAssemblyContext> participateAction)
    {
      return CreateParticipant ((id, ctx) => participateAction (ctx));
    }

    protected static IParticipant CreateParticipant (
        Action<object, IProxyTypeAssemblyContext> participateAction = null,
        ITypeIdentifierProvider typeIdentifierProvider = null,
        Func<Type, object> getAdditionalTypeIDFunc = null,
        Func<object, IAdditionalTypeAssemblyContext, Type> additionalTypeFunc = null,
        Action<Type> handleNonSubclassableTypeAction = null)
    {
      participateAction = participateAction ?? ((id, ctx) => { });
      getAdditionalTypeIDFunc = getAdditionalTypeIDFunc ?? (ctx => null);
      handleNonSubclassableTypeAction = handleNonSubclassableTypeAction ?? (ctx => { });
      additionalTypeFunc = additionalTypeFunc ?? ((id, ctx) => null);

      // Avoid no-modification optimization.
      participateAction = CreateModifyingAction (participateAction);

      return new ParticipantStub (typeIdentifierProvider, participateAction, getAdditionalTypeIDFunc, handleNonSubclassableTypeAction, additionalTypeFunc);
    }


    private List<string> _assembliesToDelete;

    private bool _skipSavingAndVerification;
    private bool _skipDeletion;

    private ICodeManager _codeManager;

    [OneTimeSetUp]
    public virtual void TestFixtureSetUp ()
    {
      _assembliesToDelete = new List<string>();
    }

    [OneTimeTearDown]
    public virtual void TestFixtureTearDown ()
    {
      foreach (var assembly in _assembliesToDelete)
      {
        File.Delete (assembly);
        File.Delete (Path.ChangeExtension (assembly, "pdb"));
      }
    }

    [SetUp]
    public virtual void SetUp ()
    {
      _skipSavingAndVerification = false;
      _skipDeletion = false;
    }

    [TearDown]
    public virtual void TearDown ()
    {
#if !FEATURE_ASSEMBLYBUILDER_SAVE
      SkipSavingAndPeVerification();
#endif

      if (_skipSavingAndVerification)
        return;

      try
      {
        Flush (skipDeletion: _skipDeletion);
      }
      catch
      {
        if (TestContext.CurrentContext.Result.Outcome != ResultState.Failure)
          throw;

        // Else: Swallow exception if test already had failed state in order to avoid overwriting any exceptions.
      }

      Assert.That (
          PipelineRegistry.HasInstanceProvider,
          Is.False,
          "The PipelineRegistry has not been reset using PipelineRegistryTestHelper.ResetPipelineRegistry().");
    }

    protected void EnableSavingAndPeVerification () 
    {
      _skipSavingAndVerification = false;
    }

    protected void SkipSavingAndPeVerification ()
    {
      _skipSavingAndVerification = true;
    }

    protected void SkipDeletion ()
    {
      _skipDeletion = true;
    }

    protected IPipeline CreatePipeline (params IParticipant[] participants)
    {
      return CreatePipeline (GetNameOfRunningTest(), participants);
    }

    protected IPipeline CreatePipeline (string participantConfigurationID, params IParticipant[] participants)
    {
      return CreatePipelineWithIntegrationTestAssemblyLocation (participantConfigurationID, PipelineSettings.Defaults, participants);
    }

    protected IPipeline CreatePipelineWithIntegrationTestAssemblyLocation (
        string participantConfigurationID, 
        PipelineSettings settings, 
        params IParticipant[] participants)
    {
      var customSettings = PipelineSettings.From (settings)
          .SetAssemblyDirectory (SetupFixture.GeneratedFileDirectory)
          .SetAssemblyNamePattern (participantConfigurationID + "_{counter}")
          .Build();

      return CreatePipelineExactAssemblyLocation (participantConfigurationID, customSettings, participants);
    }

    protected IPipeline CreatePipelineExactAssemblyLocation (string participantConfigurationID, PipelineSettings settings, params IParticipant[] participants)
    {
      // Avoid no-modification optimization.
      if (participants.Length == 0)
        participants = new[] { CreateParticipant (CreateModifyingAction ((id, ctx) => { })) };

      var pipeline = new DefaultPipelineFactory().Create (participantConfigurationID, settings, participants);

      _codeManager = pipeline.CodeManager;

      return pipeline;
    }

    protected string[] Flush (CustomAttributeDeclaration[] assemblyAttributes = null, bool skipDeletion = false, bool skipPeVerification = false)
    {
      Assertion.IsNotNull (_codeManager, "Use IntegrationTestBase.CreatePipeline");

      var assemblyPaths = _codeManager.FlushCodeToDisk (assemblyAttributes ?? new CustomAttributeDeclaration[0]);

      if (!skipDeletion)
      {
        _assembliesToDelete.AddRange (assemblyPaths);
      }
      else
      {
        foreach (var assemblyPath in assemblyPaths)
          Console.WriteLine ("Skipping deletion of: {0}", assemblyPath);
      }

      if (!skipPeVerification)
      {
        foreach (var assemblyPath in assemblyPaths)
          PeVerifyAssembly (assemblyPath);
      }

      return assemblyPaths;
    }

    private static void PeVerifyAssembly (string assemblyPath)
    {
      try
      {
        PEVerifier.CreateDefault().VerifyPEFile (assemblyPath);
      }
      catch (Exception exception)
      {
        Console.WriteLine (exception);
        throw;
      }
    }

    private static string GetNameOfRunningTest ()
    {
      // Test.FullName == Type.FullName + "." + MethodInfo.Name.
      var fullTestName = TestContext.CurrentContext.Test.FullName;
      var methodName = TestContext.CurrentContext.Test.Name;
      var fullTypeName = fullTestName.Substring (0, fullTestName.Length - (methodName.Length + 1));
      var typeName = fullTypeName.Substring (fullTypeName.LastIndexOf ('.') + 1);

      return typeName + '.' + methodName;
    }

    private static Action<object, IProxyTypeAssemblyContext> CreateModifyingAction (Action<object, IProxyTypeAssemblyContext> participateAction)
    {
      return (id, ctx) =>
      {
        participateAction (id, ctx);

        if (ctx.ProxyType.AddedCustomAttributes.Count == 0)
        {
          var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new TypeAssembledByIntegrationTestAttribute());
          var attribute = new CustomAttributeDeclaration (constructor, new object[0]);

          ctx.ProxyType.AddCustomAttribute (attribute);
        }
      };
    }

    [AttributeUsage (AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TypeAssembledByIntegrationTestAttribute : Attribute {}
  }
}