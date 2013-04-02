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
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests
{
  public abstract class IntegrationTestBase
  {
    private List<string> _assembliesToDelete;

    private bool _skipSavingAndVerification;
    private bool _skipDeletion;

    private ICodeGenerator _codeGenerator;

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

    protected static IParticipant CreateParticipant (Action<MutableType> typeModification, ICacheKeyProvider cacheKeyProvider = null)
    {
      return CreateParticipant (ctx => typeModification (ctx.ProxyType), cacheKeyProvider);
    }

    protected static IParticipant CreateParticipant (Action<ITypeAssemblyContext> typeContextModification, ICacheKeyProvider cacheKeyProvider = null)
    {
      return new ParticipantStub (typeContextModification, cacheKeyProvider);
    }

    protected IObjectFactory CreatePipeline (IEnumerable<IParticipant> participants)
    {
      var nameOfRunningTest = GetNameOfRunningTest();
      var nonEmptyParticipants = GetNonEmptyParticipants (participants);
      var objectFactory = Pipeline.Create (nameOfRunningTest, nonEmptyParticipants);

      _codeGenerator = objectFactory.CodeGenerator;
      _codeGenerator.SetAssemblyDirectory (SetupFixture.GeneratedFileDirectory);
      _codeGenerator.SetAssemblyName (nameOfRunningTest);

      return objectFactory;
    }

    protected string Flush (bool skipDeletion = false, bool skipPeVerification = false)
    {
      Assertion.IsNotNull (_codeGenerator, "Use IntegrationTestBase.CreatePipeline");

      var assemblyPath = _codeGenerator.FlushCodeToDisk();
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
      // The following might perform very poorly.
      var stackTrace = new StackTrace();

      for (int i = 0; i < stackTrace.FrameCount; i++)
      {
        var method = stackTrace.GetFrame (i).GetMethod();
        var isTestMethod = method.IsDefined (typeof (TestAttribute), inherit: true);
        var isSetupMethod = method.IsDefined (typeof (SetUpAttribute), inherit: true);

        if (isTestMethod || isSetupMethod)
        {
          Assertion.IsNotNull (method.DeclaringType);
          return string.Format ("{0}.{1}", method.DeclaringType.Name, method.Name);
        }
      }

      throw new Exception ("Should be called by test method.");
    }

    private List<IParticipant> GetNonEmptyParticipants (IEnumerable<IParticipant> participants)
    {
      var participantList = participants.ToList();
      if (participantList.Count == 0)
        participantList.Add (CreateParticipant ((MutableType proxy) => { }));

      return participantList;
    }
  }
}