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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.CodeGeneration;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.BuilderAbstractions;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.TypeAssembly;
using Remotion.Utilities;
using Rhino.Mocks;

namespace TypePipe.IntegrationTests
{
  public abstract class TypeAssemblerIntegrationTestBase
  {
    private AssemblyBuilder _assemblyBuilder;
    private bool _shouldDeleteGeneratedFiles;
    private string _generatedFileName;

    [SetUp]
    public virtual void SetUp ()
    {
      _shouldDeleteGeneratedFiles = true;
      _generatedFileName = null;
    }

    [TearDown]
    public virtual void TearDown ()
    {
      if (_assemblyBuilder == null)
        return;

      Assertion.IsNotNull (_generatedFileName);
      var assemblyPath = Path.Combine (GeneratedFileDirectory, _generatedFileName);

      try
      {
        _assemblyBuilder.Save (_generatedFileName);

        PEVerifier.CreateDefault ().VerifyPEFile (assemblyPath);

        if (_shouldDeleteGeneratedFiles)
        {
          File.Delete (assemblyPath);
          File.Delete (Path.ChangeExtension (assemblyPath, "pdb"));
        }
      }
      catch
      {
        if (TestContext.CurrentContext.Result.Status != TestStatus.Failed)
          throw;
        
        // Else: Swallow exception if test already had failed state in order to avoid overwriting any exceptions.
      }
    }

    private string GeneratedFileDirectory
    {
      get { return SetupFixture.GeneratedFileDirectory; }
    }

    protected void SkipDeletion ()
    {
      _shouldDeleteGeneratedFiles = false;
    }

    protected Type AssembleType<T> (params Action<MutableType>[] participantActions)
    {
      var participants = participantActions.Select (CreateTypeAssemblyParticipant).ToArray();
      var originalType = typeof (T);

      return AssembleType (originalType, GetNameForThisTest (1), participants);
    }

    private ITypeAssemblyParticipant CreateTypeAssemblyParticipant (Action<MutableType> typeModification)
    {
      var participantStub = MockRepository.GenerateStub<ITypeAssemblyParticipant>();
      participantStub
          .Stub (stub => stub.ModifyType (Arg<MutableType>.Is.Anything))
          .Do (typeModification);

      return participantStub;
    }

    private Type AssembleType (Type originalType, string testName, params ITypeAssemblyParticipant[] participants)
    {
      var typeAssembler = new TypeAssembler (participants, CreateReflectionEmitTypeModifier (testName));
      var assembledType = typeAssembler.AssembleType (originalType);

      return assembledType;
    }

    private ITypeModifier CreateReflectionEmitTypeModifier (string testName)
    {
      var assemblyName = new AssemblyName (testName);
      _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (assemblyName, AssemblyBuilderAccess.RunAndSave, GeneratedFileDirectory);
      _generatedFileName = assemblyName.Name + ".dll";
      
      var moduleBuilder = _assemblyBuilder.DefineDynamicModule (_generatedFileName, true);
      var moduleBuilderAdapter = new ModuleBuilderAdapter (moduleBuilder);
      var guidBasedSubclassProxyNameProvider = new GuidBasedSubclassProxyNameProvider ();
      var expressionPreparer = new ExpandingExpressionPreparer();
      var debugInfoGenerator = DebugInfoGenerator.CreatePdbGenerator ();
      var handlerFactory = new SubclassProxyBuilderFactory (
          moduleBuilderAdapter, guidBasedSubclassProxyNameProvider, expressionPreparer, debugInfoGenerator);

      return new TypeModifier (handlerFactory);
    }

    private string GetNameForThisTest (int stackFramesToSkip)
    {
      var stackFrame = new StackFrame (stackFramesToSkip + 1, false);
      var method = stackFrame.GetMethod ();
      return string.Format ("{0}.{1}", method.DeclaringType.Name, method.Name);
    }
  }
}