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
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.MutableReflection;
using Rhino.Mocks;

namespace Remotion.TypePipe.IntegrationTests
{
  [TestFixture]
  public class StrongNamingTest : ObjectFactoryIntegrationTestBase
  {
    [Test]
    public void NoStrongName ()
    {
      var participant = CreateParticipant (mt => mt.AddField ("Field", CreateUnsignedType()));

      CheckStrongNaming (false, participant);
    }

    [Test]
    public void ForceStrongName ()
    {
      var participant = CreateParticipant (mt => mt.AddField ("Field", typeof (int)));

      CheckStrongNaming (true, participant);
    }

    [Test]
    public void ForceStrongName_MutableTypeInSignature ()
    {
      var participant = CreateParticipant (mt => mt.AddField ("Field", mt));

      CheckStrongNaming (true, participant);
    }

    [Test]
    public void ForceStrongName_MutableTypeInExpression ()
    {
      var participant = CreateParticipant (
          mutableType =>
          {
            var expression = Expression.Default (mutableType);
            // TODO 4778
            var usableExpression = Expression.Convert (expression, typeof (DomainType));
            mutableType.AddMethod ("Method", 0, typeof (DomainType), ParameterDeclaration.EmptyParameters, ctx => usableExpression);
          });

      CheckStrongNaming (true, participant);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), MatchType = MessageMatch.Regex, ExpectedMessage =
        "Strong-naming is enabled but a participant used the type 'UnsignedType' which comes from the unsigned assembly 'testAssembly'.")]
    public void ForceStrongName_IncompatibleModifications ()
    {
      SkipSavingAndPeVerification();
      var participant = CreateParticipant (mt => mt.AddField ("Field", CreateUnsignedType()));
      var objectFactory = CreateObjectFactoryForStrongNaming (true, 0, participant);

      objectFactory.GetAssembledType (typeof (DomainType));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), MatchType = MessageMatch.Regex, ExpectedMessage =
        "Strong-naming is enabled but a participant used the type 'UnsignedType' which comes from the unsigned assembly 'testAssembly'.")]
    public void ForceStrongName_IncompatibleModifications_Expression ()
    {
      SkipSavingAndPeVerification();
      var participant = CreateParticipant (
          mt => mt.AddMethod ("Method", 0, typeof (object), ParameterDeclaration.EmptyParameters, ctx => Expression.New (CreateUnsignedType())));
      var objectFactory = CreateObjectFactoryForStrongNaming (true, 0, participant);

      objectFactory.GetAssembledType (typeof (DomainType));
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    private void CheckStrongNaming (bool forceStrongNaming, params IParticipant[] participants)
    {
      var objectFactory = CreateObjectFactoryForStrongNaming (forceStrongNaming, stackFramesToSkip: 1, participants: participants);

      objectFactory.GetAssembledType (typeof (DomainType));
      var assemblyPath = Flush();

      AppDomainRunner.Run (
          args =>
          {
            var path = (string) args[0];
            var expectedIsStrongNamed = (bool) args[1];
            var assembly = Assembly.LoadFrom (path);

            var isStrongNamed = assembly.GetName().GetPublicKeyToken().Length > 0;
            Assert.That (isStrongNamed, Is.EqualTo (expectedIsStrongNamed));
          },
          assemblyPath,
          forceStrongNaming);
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    private IObjectFactory CreateObjectFactoryForStrongNaming (bool forceStrongNaming, int stackFramesToSkip, params IParticipant[] participants)
    {
      var typePipeConfigurationProviderStub = MockRepository.GenerateStub<ITypePipeConfigurationProvider>();
      typePipeConfigurationProviderStub.Stub (stub => stub.ForceStrongNaming).Return (forceStrongNaming);

      using (new ServiceLocatorScope (typeof (ITypePipeConfigurationProvider), () => typePipeConfigurationProviderStub))
        return CreateObjectFactory (participants, stackFramesToSkip: stackFramesToSkip + 1);
    }

    private Type CreateUnsignedType ()
    {
      var assemblyName = "testAssembly";
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName (assemblyName), AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyName + ".dll");
      var typeBuilder = moduleBuilder.DefineType ("UnsignedType");
      var type = typeBuilder.CreateType();

      return type;
    }

    public class DomainType {}
  }
}