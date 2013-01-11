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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Configuration;
using Remotion.Development.UnitTesting.Reflection;
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
    private Type _signedInterfaceType;
    private Type _unsignedInterfaceType;
    private CustomAttributeDeclaration _signedAttribute;
    private CustomAttributeDeclaration _unsignedAttribute;

    public override void SetUp ()
    {
      base.SetUp ();

      _signedType = typeof (int);
      _unsignedType = CreateUnsignedType (TypeAttributes.Class);

      _signedInterfaceType = typeof (IMarkerInterface);
      _unsignedInterfaceType = CreateUnsignedType (TypeAttributes.Interface | TypeAttributes.Abstract);

      var attributeCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (null));
      _signedAttribute = new CustomAttributeDeclaration (attributeCtor, new object[] { _signedType });
      _unsignedAttribute = new CustomAttributeDeclaration (attributeCtor, new object[] { _unsignedType });
    }

    [Test]
    public void NoStrongName_Default ()
    {
      // Could be strong-named, but isn't - the default is to output assemblies without strong name.
      Action<MutableType> action = mt => mt.AddField ("Field", _signedType);

      CheckStrongNaming (action, forceStrongNaming: false);
    }

    [Test]
    public void NoStrongName_UnsignedType ()
    {
      Action<MutableType> action = mt => mt.AddField ("Field", _unsignedType);

      CheckStrongNaming (action, forceStrongNaming: false);
    }

    [Test]
    public void ForceStrongName ()
    {
      Action<MutableType> action = mt => mt.AddField ("Field", _signedType);

      CheckStrongNaming (action, forceStrongNaming: true, expectedKey: FallbackKey.KeyPair);
    }

    [Test]
    public void ForceStrongName_CustomKey ()
    {
      Action<MutableType> action = mt => mt.AddField ("Field", _signedType);

      var keyPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, @"StrongNaming\OtherKey.snk");
      var customKey = new StrongNameKeyPair (File.ReadAllBytes (keyPath));
      CheckStrongNaming (action, forceStrongNaming: true, keyFilePath: keyPath, expectedKey: customKey);
    }

    [Test]
    public void ForceStrongName_Signature ()
    {
      // Base type is DomainType which is signed.
      CheckStrongNaming (mt => mt.AddInterface (_signedInterfaceType));
      CheckStrongNaming (mt => mt.AddField ("field", _signedType));
      CheckStrongNaming (mt => mt.AddConstructor (0, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => ctx.GetConstructorCall()));
      CheckStrongNaming (mt => mt.AddMethod (
              "method", 0, _signedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_signedType)));
      // Properties and Events: event type, property type, property index parameter
      // TODO 4675
      // TODO 4676

      // Attributes
      CheckStrongNaming (mt => mt.AddCustomAttribute (_signedAttribute));
      CheckStrongNaming (mt => mt.AddField ("f", _signedType).AddCustomAttribute (_signedAttribute));
      CheckStrongNaming (mt => mt.AddConstructor (
              0, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => ctx.GetConstructorCall ()).AddCustomAttribute (_signedAttribute));
      CheckStrongNaming (
          mt =>
          {
            var method = mt.AddMethod (
                "method", 0, _signedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_signedType));
            method.AddCustomAttribute (_signedAttribute);
            method.MutableReturnParameter.AddCustomAttribute (_signedAttribute);
            method.MutableParameters.Single().AddCustomAttribute (_signedAttribute);
          });
      // Attributes on properties and events.
      // TODO 4675
      // TODO 4676
    }

    [Test]
    public void ForceStrongName_Signature_IncompatibleType ()
    {
      SkipSavingAndPeVerification();

      CheckStrongNamingException (mt => { }, requestedType: _unsignedType);
      CheckStrongNamingException (mt => mt.AddInterface (_unsignedInterfaceType));
      CheckStrongNamingException (mt => mt.AddField ("field", _unsignedType));
      CheckStrongNamingException (mt => mt.AddConstructor (0, new[] { new ParameterDeclaration (_unsignedType, "p") }, ctx => ctx.GetConstructorCall()));
      CheckStrongNamingException (mt => mt.AddMethod (
              "method", 0, _unsignedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_unsignedType)));
      CheckStrongNamingException (mt => mt.AddMethod (
              "method", 0, _signedType, new[] { new ParameterDeclaration (_unsignedType, "p") }, ctx => Expression.Default (_signedType)));
      // Properties and Events: event type, property type, property index parameter
      // TODO 4675
      // TODO 4676

      // Attributes
      CheckStrongNamingException (mt => mt.AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (mt => mt.AddField ("f", _signedType).AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (mt => mt.AddConstructor (
              0, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => ctx.GetConstructorCall ()).AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (
          mt =>
          mt.AddMethod ("method", 0, _signedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_signedType))
            .AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (
          mt =>
          mt.AddMethod ("method", 0, _signedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_signedType))
            .MutableReturnParameter.AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (
          mt =>
          mt.AddMethod ("method", 0, _signedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_signedType))
            .MutableParameters.Single().AddCustomAttribute (_unsignedAttribute));
      // Attributes on properties and events.
      // TODO 4675
      // TODO 4676
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

    [Test]
    public void ForceStrongName_Signature_MutableType ()
    {
      Action<MutableType> action = mt => mt.AddField ("Field", mt);

      CheckStrongNaming (action, forceStrongNaming: true);
    }

    [Test]
    public void ForceStrongName_Expression_MutableType ()
    {
      Action<MutableType> action =
          mutableType =>
          {
            var expression = Expression.New (mutableType);
            // TODO 4778
            var usableExpression = Expression.Convert (expression, typeof (DomainType));
            mutableType.AddMethod ("Method", 0, typeof (DomainType), ParameterDeclaration.EmptyParameters, ctx => usableExpression);
          };

      CheckStrongNaming (action, forceStrongNaming: true);
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    private void CheckStrongNaming (
        Action<MutableType> participantAction, bool forceStrongNaming = true, string keyFilePath = null, StrongNameKeyPair expectedKey = null)
    {
      var participant = CreateParticipant (participantAction);
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
    private void CheckStrongNamingException (Action<MutableType> participantAction, Type requestedType = null)
    {
      requestedType = requestedType ?? typeof (DomainType);
      var participant = CreateParticipant (participantAction);
      var objectFactory = CreateObjectFactoryForStrongNaming (participant, stackFramesToSkip: 1, forceStrongNaming: true);

      var messageRegex = "An error occurred during code generation for '.*Type': Strong-naming is enabled but a participant used the type "
                         + "'UnsignedType' which comes from the unsigned assembly 'testAssembly'. The following participants are currently "
                         + @"configured and may have caused the error: 'IParticipantProxy.*'\.";
      Assert.That (() => objectFactory.GetAssembledType (requestedType), Throws.InvalidOperationException.With.Message.Matches (messageRegex));
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

    private Type CreateUnsignedType (TypeAttributes attributes)
    {
      var assemblyName = "testAssembly";
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName (assemblyName), AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyName + ".dll");
      var typeBuilder = moduleBuilder.DefineType ("UnsignedType", attributes);
      return typeBuilder.CreateType();
    }

    public class DomainType {}
    public interface IMarkerInterface { }
    public class AbcAttribute : Attribute{ public AbcAttribute (Type type) { Dev.Null = type; } }
  }
}