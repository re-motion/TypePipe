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
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Configuration;
using Remotion.TypePipe.Configuration;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.StrongNaming;

namespace Remotion.TypePipe.IntegrationTests.StrongNaming
{
  [TestFixture]
  public class StrongNamingTest : ObjectFactoryIntegrationTestBase
  {
    private Type _signedType;
    private Type _unsignedType;

    public override void SetUp ()
    {
      base.SetUp ();

      _signedType = typeof (int);
      _unsignedType = CreateUnsignedType ();
    }

    [Test]
    public void NoStrongName_Default ()
    {
      // Could be strong-named, but isn't - the default is to output assemblies without strong name.
      var participant = CreateParticipant (mt => mt.AddField ("Field", _signedType));

      CheckStrongNaming (participant, forceStrongNaming: false);
    }

    [Test]
    public void NoStrongName_UnsignedType ()
    {
      var participant = CreateParticipant (mt => mt.AddField ("Field", _unsignedType));

      CheckStrongNaming (participant, forceStrongNaming: false);
    }

    [Test]
    public void ForceStrongName ()
    {
      var participant = CreateParticipant (mt => mt.AddField ("Field", _signedType));

      CheckStrongNaming (participant, forceStrongNaming: true, expectedKey: FallbackKey.KeyPair);
    }

    [Test]
    public void ForceStrongName_CustomKey ()
    {
      var participant = CreateParticipant (mt => mt.AddField ("Field", _signedType));

      var keyPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, @"StrongNaming\OtherKey.snk");
      var customKey = new StrongNameKeyPair (File.ReadAllBytes (keyPath));
      CheckStrongNaming (participant, forceStrongNaming: true, keyFilePath: keyPath, expectedKey: customKey);
    }

    [Test]
    public void ForceStrongName_MutableTypeInFieldSignature ()
    {
      var participant = CreateParticipant (mt => mt.AddField ("Field", mt));

      CheckStrongNaming (participant, forceStrongNaming: true);
    }

    [Test]
    public void ForceStrongName_MutableTypeInExpression ()
    {
      var participant = CreateParticipant (
          mutableType =>
          {
            var expression = Expression.New (mutableType);
            // TODO 4778
            var usableExpression = Expression.Convert (expression, typeof (DomainType));
            mutableType.AddMethod ("Method", 0, typeof (DomainType), ParameterDeclaration.EmptyParameters, ctx => usableExpression);
          });

      CheckStrongNaming (participant, forceStrongNaming: true);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), MatchType = MessageMatch.Regex, ExpectedMessage =
        "Strong-naming is enabled but a participant used the type 'UnsignedType' which comes from the unsigned assembly 'testAssembly'.")]
    public void ForceStrongName_IncompatibleType_InFieldSignature ()
    {
      SkipSavingAndPeVerification();
      var participant = CreateParticipant (mt => mt.AddField ("Field", _unsignedType));
      var objectFactory = CreateObjectFactoryForStrongNaming (participant, stackFramesToSkip: 0, forceStrongNaming: true);

      objectFactory.GetAssembledType (typeof (DomainType));
    }

    // TODO Review: Refactor above test to be one-liner, add tests for (positive and negative case):
    // base type, interface types
    // method parameter, method return type
    // constructor parameter
    // attributes (on constructors, methods, type, fields, parameters, return parameter, later: events, properties - mark TODO 4675 and 4676)
    // later: event type, property type, property index parameter  - mark TODO 4675 and 4676

    [Test]
    [ExpectedException (typeof (InvalidOperationException), MatchType = MessageMatch.Regex, ExpectedMessage =
        "Strong-naming is enabled but a participant used the type 'UnsignedType' which comes from the unsigned assembly 'testAssembly'.")]
    public void ForceStrongName_IncompatibleType_InExpression ()
    {
      SkipSavingAndPeVerification();
      var participant = CreateParticipant (
          mt => mt.AddMethod ("Method", 0, typeof (object), ParameterDeclaration.EmptyParameters, ctx => Expression.New (_unsignedType)));
      var objectFactory = CreateObjectFactoryForStrongNaming (participant, stackFramesToSkip: 0, forceStrongNaming: true);

      objectFactory.GetAssembledType (typeof (DomainType));
    }

    // TODO Review: Refactor above test to be one-liner, add tests for each opcode type, catch blocks, local variables (positive and negative case)

    [MethodImpl (MethodImplOptions.NoInlining)]
    private void CheckStrongNaming (IParticipant participant, bool forceStrongNaming, string keyFilePath = null, StrongNameKeyPair expectedKey = null)
    {
      var objectFactory = CreateObjectFactoryForStrongNaming (participant, 1, forceStrongNaming, keyFilePath);

      var type = objectFactory.GetAssembledType (typeof (DomainType));
      var assemblyName = type.Assembly.GetName();

      var isStrongNamed = assemblyName.GetPublicKeyToken().Length > 0;
      Assert.That (isStrongNamed, Is.EqualTo (forceStrongNaming));

      if (forceStrongNaming)
      {
        expectedKey = expectedKey ?? FallbackKey.KeyPair;
        var publicKey = assemblyName.GetPublicKey();
        Assert.That (publicKey, Is.EqualTo (expectedKey.PublicKey));
      }
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    private IObjectFactory CreateObjectFactoryForStrongNaming (
        IParticipant participant, int stackFramesToSkip, bool forceStrongNaming, string keyFilePath = null)
    {
      var configurationProvider = new TypePipeConfigurationProvider();
      var configSection = new TypePipeConfigurationSection();
      var config = forceStrongNaming
                       ? string.Format ("<typePipe><forceStrongNaming keyFilePath=\"{0}\" /></typePipe>", keyFilePath)
                       : "<typePipe/>";
      ConfigurationHelper.DeserializeSection (configSection, config);
      PrivateInvoke.SetNonPublicField (configurationProvider, "_section", configSection);

      using (new ServiceLocatorScope (typeof (ITypePipeConfigurationProvider), () => configurationProvider))
        return CreateObjectFactory (new[] { participant }, stackFramesToSkip: stackFramesToSkip + 1);
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