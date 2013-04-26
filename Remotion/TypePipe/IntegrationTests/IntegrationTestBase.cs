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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests
{
  public abstract class IntegrationTestBase
  {
    protected static IParticipant CreateParticipant (
        Action<MutableType> typeModification, ICacheKeyProvider cacheKeyProvider = null, Action<LoadedTypesContext> rebuildStateAction = null)
    {
      return CreateParticipant (ctx => typeModification (ctx.ProxyType), cacheKeyProvider, rebuildStateAction);
    }

    protected static IParticipant CreateParticipant (
        Action<ITypeAssemblyContext> participateAction = null,
        ICacheKeyProvider cacheKeyProvider = null,
        Action<LoadedTypesContext> rebuildStateAction = null,
        Action<Type> handleNonSubclassableTypeAction = null)
    {
      participateAction = participateAction ?? (ctx => { });
      rebuildStateAction = rebuildStateAction ?? (ctx => { });
      handleNonSubclassableTypeAction = handleNonSubclassableTypeAction ?? (ctx => { });

      // Avoid no-modification optimization.
      participateAction = CreateModifyingAction (participateAction);

      return new ParticipantStub (cacheKeyProvider, participateAction, rebuildStateAction, handleNonSubclassableTypeAction);
    }


    private List<string> _assembliesToDelete;

    private bool _skipSavingAndVerification;
    private bool _skipDeletion;

    private ICodeManager _codeManager;

    [TestFixtureSetUp]
    public virtual void TestFixtureSetUp ()
    {
      _assembliesToDelete = new List<string>();
    }

    [TestFixtureTearDown]
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
      if (_skipSavingAndVerification)
        return;

      try
      {
        Flush (skipDeletion: _skipDeletion);
      }
      catch
      {
        if (TestContext.CurrentContext.Result.Status != TestStatus.Failed)
          throw;

        // Else: Swallow exception if test already had failed state in order to avoid overwriting any exceptions.
      }
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
      return CreatePipeline (participantConfigurationID, participants, null);
    }

    protected IPipeline CreatePipeline (
        string participantConfigurationID, IEnumerable<IParticipant> participants, IConfigurationProvider configurationProvider = null)
    {
      // Avoid no-modification optimization.
      var participantList = participants.ToList();
      if (participantList.Count == 0)
        participantList.Add (CreateParticipant (CreateModifyingAction (ctx => { })));

      var objectFactory = PipelineFactory.Create (participantConfigurationID, participantList, configurationProvider);

      _codeManager = objectFactory.CodeManager;
      _codeManager.SetAssemblyDirectory (SetupFixture.GeneratedFileDirectory);
      _codeManager.SetAssemblyNamePattern (participantConfigurationID + "_{counter}");

      return objectFactory;
    }

    protected string Flush (IEnumerable<CustomAttributeDeclaration> assemblyAttributes = null, bool skipDeletion = false, bool skipPeVerification = false)
    {
      Assertion.IsNotNull (_codeManager, "Use IntegrationTestBase.CreatePipeline");

      var assemblyPath = _codeManager.FlushCodeToDisk (assemblyAttributes ?? new CustomAttributeDeclaration[0]);
      if (assemblyPath == null)
        return null;

      if (!skipDeletion)
        _assembliesToDelete.Add (assemblyPath);
      else
        Console.WriteLine ("Skipping deletion of: {0}", assemblyPath);

      if (!skipPeVerification)
        PeVerifyAssembly (assemblyPath);

      return assemblyPath;
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

    private static Action<ITypeAssemblyContext> CreateModifyingAction (Action<ITypeAssemblyContext> participateAction)
    {
      return ctx =>
      {
        participateAction (ctx);

        if (ctx.ProxyType.AddedCustomAttributes.Count == 0)
        {
          var constructor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new TypeAssembledByIntegrationTestAttribute());
          var attribute = new CustomAttributeDeclaration (constructor, new object[0]);

          ctx.ProxyType.AddCustomAttribute (attribute);
        }
      };
    }

    public class TypeAssembledByIntegrationTestAttribute : Attribute {}
  }
}