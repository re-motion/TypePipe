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
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.StrongNaming;
using Rhino.Mocks;

namespace Remotion.TypePipe.IntegrationTests
{
  [TestFixture]
  public class StrongNamingTest : ObjectFactoryIntegrationTestBase
  {
    [Test]
    public void NoStrongName_Compatible ()
    {
      CheckStrongNaming (false, StrongNameCompatibility.Compatible);
    }

    [Test]
    public void ForceStrongName_Compatible ()
    {
      CheckStrongNaming (true, StrongNameCompatibility.Compatible);
    }

    [Test]
    public void ForceStrongName_Unknown_CompatibleModifications ()
    {
      CheckStrongNaming (true, StrongNameCompatibility.Compatible, StrongNameCompatibility.Unknown);
    }

    [Test]
    public void ForceStrongName_Unknown_CompatibleModifications_MutableTypeInSignature ()
    {
      var participant = CreateParticipant (
          mutableType =>
          {
            mutableType.AddField ("Field", mutableType);

            return StrongNameCompatibility.Unknown;
          });

      CheckStrongNaming (true, 0, participant);
    }

    [Test]
    public void ForceStrongName_Unknown_CompatibleModifications_MutableTypeInExpression ()
    {
      var participant = CreateParticipant (
          mutableType =>
          {
            var expression = Expression.Default (mutableType);
            // TODO 4778
            var usableExpression = Expression.Convert (expression, typeof (DomainType));
            mutableType.AddMethod ("Method", 0, typeof (DomainType), ParameterDeclaration.EmptyParameters, ctx => usableExpression);

            return StrongNameCompatibility.Unknown;
          });

      CheckStrongNaming (true, 0, participant);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), MatchType = MessageMatch.Regex, ExpectedMessage =
        "Strong-naming is enabled but at least one of the following participants requested incompatible type modifications: 'IParticipantProxy.*'.")]
    public void ForceStrongName_Unknown_IncompatibleModifications_Expression ()
    {
      var participant = CreateParticipant (
          mutableType =>
          {
            var unsignedType = CreateUnsignedType ("UnsignedType");
            mutableType.AddMethod ("Method", 0, typeof (object), ParameterDeclaration.EmptyParameters, ctx => Expression.Default (unsignedType));

            return StrongNameCompatibility.Unknown;
          });
      var objectFactory = CreateObjectFactoryForStrongNaming (true, 1, participant);

      objectFactory.GetAssembledType (typeof (DomainType));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), MatchType = MessageMatch.Regex, ExpectedMessage =
        "Strong-naming is enabled but the following participants requested incompatible type modifications: 'IParticipantProxy.*'.")]
    public void ForceStrongName_Incompatible ()
    {
      var participant1 = CreateParticipant (mt => StrongNameCompatibility.Compatible);
      var participant2 = CreateParticipant (mt => StrongNameCompatibility.Incompatible);
      var objectFactory = CreateObjectFactoryForStrongNaming (true, 1, participant1, participant2);

      objectFactory.GetAssembledType (typeof (DomainType));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), MatchType = MessageMatch.Regex, ExpectedMessage =
        "Strong-naming is enabled but at least one of the following participants requested incompatible type modifications: 'IParticipantProxy.*'.")]
    public void ForceStrongName_Unknown_IncompatibleModifications ()
    {
      var participant = CreateParticipant (
          mutableType =>
          {
            var unsignedType = CreateUnsignedType ("UnsignedType");
            mutableType.AddField ("Field", unsignedType);

            return StrongNameCompatibility.Unknown;
          });
      var objectFactory = CreateObjectFactoryForStrongNaming (true, 1, participant);

      objectFactory.GetAssembledType (typeof (DomainType));
    }

    private void CheckStrongNaming (bool forceStrongNaming, params StrongNameCompatibility[] compatibilities)
    {
      var participants = compatibilities.Select (c => CreateParticipant (mutableType => c));
      CheckStrongNaming (forceStrongNaming, 1, participants.ToArray());
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    private void CheckStrongNaming (bool forceStrongNaming, int stackFramesToSkip, params IParticipant[] participants)
    {
      var objectFactory = CreateObjectFactoryForStrongNaming (forceStrongNaming, stackFramesToSkip + 1, participants);

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

    private Type CreateUnsignedType (string typeName)
    {
      var assemblyName = "test";
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName (assemblyName), AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyName + ".dll");
      var typeBuilder = moduleBuilder.DefineType (typeName);
      var type = typeBuilder.CreateType();

      return type;
    }

    public class DomainType {}
  }
}