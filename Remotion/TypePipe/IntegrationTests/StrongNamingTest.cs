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
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.StrongNaming;
using Rhino.Mocks;

namespace Remotion.TypePipe.IntegrationTests
{
  [TestFixture]
  [Ignore ("TODO 5287")]
  public class StrongNamingTest : ObjectFactoryIntegrationTestBase
  {
    private ITypePipeConfigurationProvider _typePipeConfigurationProviderMock;

    public override void SetUp ()
    {
      base.SetUp();

      _typePipeConfigurationProviderMock = MockRepository.GenerateStrictMock<ITypePipeConfigurationProvider>();
    }

    [Test]
    public void NoStrongName_Compatible ()
    {
      var compatibilities = new[] { StrongNameCompatibility.Compatible };
      CheckStrongNaming (compatibilities, forceStrongNaming: false, expectedIsStrongNamed: false);
    }

    [Test]
    public void ForceStrongName_Compatible ()
    {
      var compatibilities = new[] { StrongNameCompatibility.Compatible };
      CheckStrongNaming (compatibilities, forceStrongNaming: true, expectedIsStrongNamed: true);
    }

    [Test]
    public void ForceStrongName_Unknown_CompatibleModifications ()
    {
      var compatibilities = new[] { StrongNameCompatibility.Compatible, StrongNameCompatibility.Unknown };
      CheckStrongNaming (compatibilities, forceStrongNaming: true, expectedIsStrongNamed: true);
    }

    [Test]
    public void ForceStrongName_Unknown_CompatibleModifications_MutableType ()
    {
      var participant = CreateParticipant (
          mutableType =>
          {
            mutableType.AddField ("Field", mutableType);

            return StrongNameCompatibility.Unknown;
          });
      var objectFactory = CreateObjectFactory (participant);
      _typePipeConfigurationProviderMock.Expect (x => x.ForceStrongNaming).Return (true);

      objectFactory.GetAssembledType (typeof (DomainType));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "Strong-naming is enabled in the configuration, but participant '..' is strong-name incompatible.")]
    public void ForceStrongName_Incompatible ()
    {
      var objectFactory = CreateObjectFactoryForStrongNaming (StrongNameCompatibility.Compatible, StrongNameCompatibility.Incompatible);
      _typePipeConfigurationProviderMock.Expect (x => x.ForceStrongNaming).Return (true);

      objectFactory.GetAssembledType (typeof (DomainType));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "Strong-naming is enabled in the configuration, but participant '..' is strong-name incompatible.")]
    public void ForceStrongName_Unknown_IncompatibleModifications ()
    {
      var participant = CreateParticipant (
          mutableType =>
          {
            var unsignedType = CreateUnsignedType ("UnsignedType");
            mutableType.AddField ("Field", unsignedType);

            return StrongNameCompatibility.Unknown;
          });
      var objectFactory = CreateObjectFactory (participant);
      _typePipeConfigurationProviderMock.Expect (x => x.ForceStrongNaming).Return (true);

      objectFactory.GetAssembledType (typeof (DomainType));
    }

    private void CheckStrongNaming (StrongNameCompatibility[] compatibilities, bool forceStrongNaming, bool expectedIsStrongNamed)
    {
      var objectFactory = CreateObjectFactoryForStrongNaming (compatibilities);
      _typePipeConfigurationProviderMock.Expect (x => x.ForceStrongNaming).Return (forceStrongNaming);

      objectFactory.GetAssembledType (typeof (DomainType));
      var result = objectFactory.CodeGenerator.FlushCodeToDisk();

      var assembly = Assembly.LoadFrom (result);
      var isStrongNamed = assembly.GetName().GetPublicKeyToken().Any();
      Assert.That (isStrongNamed, Is.EqualTo (expectedIsStrongNamed));
    }

    private IObjectFactory CreateObjectFactoryForStrongNaming (params StrongNameCompatibility[] compatibilities)
    {
      var participants = compatibilities.Select (c => CreateParticipant (mutableType => c));
      using (new ServiceLocatorScope (typeof (ITypePipeConfigurationProvider), () => _typePipeConfigurationProviderMock))
      {
        return CreateObjectFactory (participants, stackFramesToSkip: 1);
      }
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