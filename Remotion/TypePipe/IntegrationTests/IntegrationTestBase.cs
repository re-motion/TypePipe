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
using Remotion.TypePipe.Caching;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.Serialization.Implementation;
using Remotion.Utilities;

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

    [MethodImpl (MethodImplOptions.NoInlining)]
    protected ITypeAssembler CreateTypeAssembler (IEnumerable<IParticipant> participants, int stackFramesToSkip)
    {
      var mutableTypeFactory = new MutableTypeFactory();
      var codeGenerator = new ReflectionEmitCodeGenerator (new ModuleBuilderFactory(), new TypePipeConfigurationProvider());
      var mutableTypeCodeGeneratorFactory = new MutableTypeCodeGeneratorFactory (
          new MemberEmitterFactory(), codeGenerator, new InitializationBuilder(), new ProxySerializationEnabler (new SerializableFieldFinder()));
      var typeAssemblyContextCodeGenerator = new TypeAssemblyContextCodeGenerator (new DependentTypeSorter(), mutableTypeCodeGeneratorFactory);

      var nameOfRunningTest = GetNameOfRunningTest (stackFramesToSkip + 1);
      codeGenerator.SetAssemblyDirectory (SetupFixture.GeneratedFileDirectory);
      codeGenerator.SetAssemblyName (nameOfRunningTest);
      _codeGenerator = codeGenerator;

      return new TypeAssembler (nameOfRunningTest, participants, mutableTypeFactory, typeAssemblyContextCodeGenerator);
    }

    protected string Flush (bool skipDeletion = false, bool skipPeVerification = false)
    {
      Assertion.IsNotNull (_codeGenerator, "Use IntegrationTestBase.CreateTypeAssembler");

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

    [MethodImpl (MethodImplOptions.NoInlining)]
    private static string GetNameOfRunningTest (int stackFramesToSkip)
    {
      var stackFrame = new StackFrame (stackFramesToSkip + 1, false);
      var method = stackFrame.GetMethod();
      Assertion.IsNotNull (method.DeclaringType);

      return string.Format ("{0}.{1}", method.DeclaringType.Name, method.Name);
    }
  }
}