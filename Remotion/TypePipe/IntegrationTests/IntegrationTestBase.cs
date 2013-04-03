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
using System.Linq;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.Implementation;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.IntegrationTests
{
  public abstract class IntegrationTestBase
  {
    protected static IParticipant CreateParticipant (Action<MutableType> typeModification, ICacheKeyProvider cacheKeyProvider = null)
    {
      return CreateParticipant (ctx => typeModification (ctx.ProxyType), cacheKeyProvider);
    }

    protected static IParticipant CreateParticipant (Action<ITypeAssemblyContext> typeContextModification, ICacheKeyProvider cacheKeyProvider = null)
    {
      return new ParticipantStub (typeContextModification, cacheKeyProvider);
    }

    protected static IParticipant CreateNopParticipant ()
    {
      return CreateParticipant ((MutableType proxy) => { });
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
        Flush (_skipDeletion);
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

    protected IObjectFactory CreateObjectFactory (params IParticipant[] participants)
    {
      return CreateObjectFactory (GetNameOfRunningTest(), participants);
    }

    protected IObjectFactory CreateObjectFactory (string participantConfigurationID, params IParticipant[] participants)
    {
      var nonEmptyParticipants = GetNonEmptyParticipants (participants);
      var objectFactory = Pipeline.Create (participantConfigurationID, nonEmptyParticipants.AsOneTime());

      _codeManager = objectFactory.CodeManager;
      _codeManager.SetAssemblyDirectory (SetupFixture.GeneratedFileDirectory);
      _codeManager.SetAssemblyName (participantConfigurationID);

      return objectFactory;
    }

    protected string Flush (bool skipDeletion = false, bool skipPeVerification = false)
    {
      Assertion.IsNotNull (_codeManager, "Use IntegrationTestBase.CreateObjectFactory");

      var assemblyPath = _codeManager.FlushCodeToDisk();
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

    private IEnumerable<IParticipant> GetNonEmptyParticipants (IEnumerable<IParticipant> participants)
    {
      var participantList = participants.ToList();
      if (participantList.Count == 0)
        participantList.Add (CreateNopParticipant());

      return participantList;
    }
  }
}