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
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.ServiceLocation;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.TypePipe.IntegrationTests
{
  public abstract class IntegrationTestBase
  {
    private List<string> _generatedAssemblyPaths;
    private bool _skipAll;
    private bool _skipPeVerify;
    private bool _skipDeletion;

    private ICodeGenerator _codeGenerator;

    [SetUp]
    public virtual void SetUp ()
    {
      _generatedAssemblyPaths = new List<string>();
      _skipAll = false;
      _skipPeVerify = false;
      _skipDeletion = false;

    }

    [TearDown]
    public virtual void TearDown ()
    {
      if (_skipAll)
        return;

      FlushAndTrackFilesForCleanup();

      try
      {
        foreach (var assemblyPath in _generatedAssemblyPaths)
        {
          if (!_skipPeVerify)
            PEVerifier.CreateDefault().VerifyPEFile (assemblyPath);

          if (!_skipDeletion)
          {
            File.Delete (assemblyPath);
            File.Delete (Path.ChangeExtension (assemblyPath, "pdb"));
          }
        }
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
      _skipAll = true;
    }

    protected void SkipDeletion ()
    {
      _skipDeletion = true;
    }

    protected void SkipPeVerification ()
    {
      _skipPeVerify = true;
    }

    protected IParticipant CreateParticipant (Action<MutableType> typeModification)
    {
      var participantStub = MockRepository.GenerateStub<IParticipant>();
      participantStub.Stub (stub => stub.ModifyType (Arg<MutableType>.Is.Anything)).Do (typeModification);

      return participantStub;
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    protected string GetNameForThisTest (int stackFramesToSkip)
    {
      var stackFrame = new StackFrame (stackFramesToSkip + 1, false);
      var method = stackFrame.GetMethod();
      Assertion.IsFalse (method.DeclaringType.Name.EndsWith ("TestBase"));

      return string.Format ("{0}.{1}", method.DeclaringType.Name, method.Name);
    }

    protected ITypeModifier CreateTypeModifier (string assemblyName)
    {
      var typeModifier = SafeServiceLocator.Current.GetInstance<ITypeModifier>();

      _codeGenerator = typeModifier.CodeGenerator;
      _codeGenerator.SetAssemblyDirectory (SetupFixture.GeneratedFileDirectory);
      _codeGenerator.SetAssemblyName (assemblyName);

      return typeModifier;
    }

    protected string FlushAndTrackFilesForCleanup ()
    {
      Assertion.IsNotNull (_codeGenerator, "Use IntegrationTestBase.CreateReflectionEmitTypeModifier");

      var assemblyPath = _codeGenerator.FlushCodeToDisk();
      if (assemblyPath == null)
        return null;

      _generatedAssemblyPaths.Add (assemblyPath);

      return assemblyPath;
    }
  }
}