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

    private FieldInfo _signedField;
    private FieldInfo _unsignedField;
    private ConstructorInfo _signedCtor;
    private ConstructorInfo _unsignedCtor;
    private MethodInfo _signedMethod;
    private MethodInfo _unsignedMethod;
    private MethodInfo _signedVarArgsMethod;
    private MethodInfo _unsignedVarArgsMethod;

    public override void SetUp ()
    {
      base.SetUp ();

      _signedType = typeof (int);
      _unsignedType = CreateUnsignedType();

      _signedInterfaceType = typeof (IMarkerInterface);
      _unsignedInterfaceType = CreateUnsignedType (TypeAttributes.Interface | TypeAttributes.Abstract);

      var attributeCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (null));
      _signedAttribute = new CustomAttributeDeclaration (attributeCtor, new object[] { _signedType });
      _unsignedAttribute = new CustomAttributeDeclaration (attributeCtor, new object[] { _unsignedType });

      _signedField = NormalizingMemberInfoFromExpressionUtility.GetField (() => DomainType.Field);
      _unsignedField = _unsignedType.GetField ("field");

      _signedCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType());
      _unsignedCtor = _unsignedType.GetConstructors().Single();

      _signedMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => DomainType.Method());
      _unsignedMethod = _unsignedType.GetMethod ("method");

      _signedVarArgsMethod = typeof (DomainType).GetMethod ("VarArgsMethod");
      _unsignedVarArgsMethod = _unsignedType.GetMethod ("varargs");
    }

    [Test]
    public void NoStrongName_Default ()
    {
      // Could be strong-named, but isn't - the default is to output assemblies without strong name.
      Action<ProxyType> action = p => p.AddField ("f", FieldAttributes.Private, _signedType);

      CheckStrongNaming (action, forceStrongNaming: false);
    }

    [Test]
    public void NoStrongName_UnsignedType ()
    {
      Action<ProxyType> action = p => p.AddField ("f", FieldAttributes.Private, _unsignedType);

      CheckStrongNaming (action, forceStrongNaming: false);
    }

    [Test]
    public void ForceStrongName ()
    {
      Action<ProxyType> action = p => p.AddField ("f", FieldAttributes.Private, _signedType);

      CheckStrongNaming (action, forceStrongNaming: true, expectedKey: FallbackKey.KeyPair);
    }

    [Test]
    public void ForceStrongName_CustomKey ()
    {
      Action<ProxyType> action = p => p.AddField ("f", FieldAttributes.Private, _signedType);

      var keyPath = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, @"StrongNaming\OtherKey.snk");
      var customKey = new StrongNameKeyPair (File.ReadAllBytes (keyPath));
      CheckStrongNaming (action, forceStrongNaming: true, keyFilePath: keyPath, expectedKey: customKey);
    }

    [Test]
    public void ForceStrongName_Signature ()
    {
      // Base type is DomainType which is signed.
      CheckStrongNaming (p => p.AddInterface (_signedInterfaceType));
      CheckStrongNaming (p => p.AddField ("f", FieldAttributes.Private, _signedType));
      CheckStrongNaming (p => p.AddConstructor (0, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => ctx.CallThisConstructor()));
      CheckStrongNaming (p => p.AddMethod ("m", 0, _signedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_signedType)));
      // Properties and Events: event type, property type, property index parameter
      // TODO 4675
      // TODO 4676

      // Attributes
      CheckStrongNaming (p => p.AddCustomAttribute (_signedAttribute));
      CheckStrongNaming (p => p.AddField ("f", FieldAttributes.Private, _signedType).AddCustomAttribute (_signedAttribute));
      CheckStrongNaming (p => p.AddConstructor (
              0, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => ctx.CallThisConstructor ()).AddCustomAttribute (_signedAttribute));
      CheckStrongNaming (
          p =>
          {
            var method = p.AddMethod ("m", 0, _signedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_signedType));
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

      CheckStrongNamingException (p => { }, requestedType: _unsignedType); // Requested type will be parent.
      CheckStrongNamingException (p => p.AddInterface (_unsignedInterfaceType));
      CheckStrongNamingException (p => p.AddField ("f", FieldAttributes.Private, _unsignedType));
      CheckStrongNamingException (p => p.AddConstructor (0, new[] { new ParameterDeclaration (_unsignedType, "p") }, ctx => ctx.CallThisConstructor()));
      CheckStrongNamingException (p => p.AddMethod ("m", 0, _unsignedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_unsignedType)));
      CheckStrongNamingException (p => p.AddMethod ("m", 0, _signedType, new[] { new ParameterDeclaration (_unsignedType, "p") }, ctx => Expression.Default (_signedType)));
      // Properties and Events: event type, property type, property index parameter
      // TODO 4675
      // TODO 4676

      // Attributes
      CheckStrongNamingException (p => p.AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (p => p.AddField ("f", FieldAttributes.Private, _signedType).AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (p => p.AddConstructor (
              0, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => ctx.CallThisConstructor ()).AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (
          p =>
          p.AddMethod ("m", 0, _signedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_signedType))
            .AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (
          p =>
          p.AddMethod ("m", 0, _signedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_signedType))
            .MutableReturnParameter.AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (
          p =>
          p.AddMethod ("m", 0, _signedType, new[] { new ParameterDeclaration (_signedType, "p") }, ctx => Expression.Default (_signedType))
            .MutableParameters.Single().AddCustomAttribute (_unsignedAttribute));
      // Attributes on properties and events.
      // TODO 4675
      // TODO 4676
    }

    [Test]
    public void ForceStrongName_Expression ()
    {
      // UnemittableExpressionVisitor.VisitConstant
      CheckStrongNamingExpression (Expression.Constant (_signedType));

      // ILGeneratorDecorator.Emit (Opcode, Type)
      CheckStrongNamingExpression (Expression.Convert (Expression.Constant (null), _signedType));
      // Emit (Opcode, FieldInfo)
      CheckStrongNamingExpression (Expression.Field (null, _signedField));
      // Emit (Opcode, ConstructorInfo)
      CheckStrongNamingExpression (Expression.New (_signedCtor));
      // Emit (Opcode, MethodInfo)
      CheckStrongNamingExpression (Expression.Call (_signedMethod));
      // EmitCall (OpCode, MethodInfo)
      CheckStrongNamingExpression (Expression.Call (_signedVarArgsMethod));
      // DeclareLocal (Type)
      CheckStrongNamingExpression (Expression.Block (new[] { Expression.Variable (_signedType) }, Expression.Empty()));
      // BeginCatchBlock (Type)
      CheckStrongNamingExpression (Expression.TryCatch (Expression.Empty(), Expression.Catch (_signedType, Expression.Empty())));
    }

    [Test]
    public void ForceStrongName_Expression_IncompatibleType ()
    {
      SkipSavingAndPeVerification();

      // UnemittableExpressionVisitor.VisitConstant
      CheckStrongNamingExpressionException (Expression.Constant (_unsignedType));

      // ILGeneratorDecorator.Emit (Opcode, Type)
      CheckStrongNamingExpressionException (Expression.Convert (Expression.Constant (null), _unsignedType));
      // Emit (Opcode, FieldInfo)
      CheckStrongNamingExpressionException (Expression.Field (null, _unsignedField));
      // Emit (Opcode, ConstructorInfo)
      CheckStrongNamingExpressionException (Expression.New (_unsignedCtor));
      // Emit (Opcode, MethodInfo)
      CheckStrongNamingExpressionException (Expression.Call (_unsignedMethod));
      // EmitCall (OpCode, MethodInfo)
      CheckStrongNamingExpressionException (Expression.Call (_unsignedVarArgsMethod));
      // DeclareLocal (Type)
      CheckStrongNamingExpressionException (Expression.Block (new[] { Expression.Variable (_unsignedType) }, Expression.Empty()));
      // BeginCatchBlock (Type)
      CheckStrongNamingExpressionException (Expression.TryCatch (Expression.Empty(), Expression.Catch (_unsignedType, Expression.Empty())));
    }

    [Test]
    public void ForceStrongName_SignatureAndExpression_ProxyType ()
    {
      Action<ProxyType> action = p => p.AddMethod ("Method", 0, p, new[] { new ParameterDeclaration (p, "p1") }, ctx => Expression.New (p));

      CheckStrongNaming (action, forceStrongNaming: true);
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    private void CheckStrongNaming (
        Action<ProxyType> participantAction,
        bool forceStrongNaming = true,
        string keyFilePath = null,
        StrongNameKeyPair expectedKey = null,
        int stackFramesToSkip = 0)
    {
      var participant = CreateParticipant (participantAction);
      var objectFactory = CreateObjectFactoryForStrongNaming (participant, stackFramesToSkip + 1, forceStrongNaming, keyFilePath);

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
    private void CheckStrongNamingExpression (Expression methodBody)
    {
      CheckStrongNaming (p => p.AddMethod ("m", 0, typeof (void), ParameterDeclaration.EmptyParameters, ctx => methodBody), forceStrongNaming: true);
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    private void CheckStrongNamingException (Action<ProxyType> participantAction, Type requestedType = null, int stackFramesToSkip = 0)
    {
      requestedType = requestedType ?? typeof (DomainType);
      var participant = CreateParticipant (participantAction);
      var objectFactory = CreateObjectFactoryForStrongNaming (participant, stackFramesToSkip + 1, forceStrongNaming: true);

      var messageRegex =
          "An error occurred during code generation for '" + requestedType.Name + "_Proxy1':\r\n"
          + "Strong-naming is enabled but a participant used the type 'UnsignedType' which comes from the unsigned assembly 'testAssembly'.\r\n"
          + @"The following participants are currently configured and may have caused the error: 'IParticipantProxy.*'\.";
      Assert.That (() => objectFactory.GetAssembledType (requestedType), Throws.InvalidOperationException.With.Message.Matches (messageRegex));
    }

    [MethodImpl (MethodImplOptions.NoInlining)]
    private void CheckStrongNamingExpressionException (Expression methodBody)
    {
      CheckStrongNamingException (p => p.AddMethod ("m", bodyProvider: ctx => methodBody));
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

    private Type CreateUnsignedType (TypeAttributes attributes = TypeAttributes.Class)
    {
      var assemblyName = "testAssembly";
      var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName (assemblyName), AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyName + ".dll");
      var typeBuilder = moduleBuilder.DefineType ("UnsignedType", attributes | TypeAttributes.Public);

      if (attributes == TypeAttributes.Class)
      {
        typeBuilder.DefineField ("field", typeof (int), FieldAttributes.Public | FieldAttributes.Static);
        typeBuilder.DefineMethod ("method", MethodAttributes.Public | MethodAttributes.Static).GetILGenerator().Emit (OpCodes.Ret);
        typeBuilder.DefineMethod ("varargs", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.VarArgs).GetILGenerator().Emit (OpCodes.Ret);
      }

      return typeBuilder.CreateType();
    }

    public class DomainType
    {
      public static int Field;
      public static void Method () { }
      public static void VarArgsMethod (__arglist) { }
    }
    public interface IMarkerInterface { }
    public class AbcAttribute : Attribute{ public AbcAttribute (Type type) { Dev.Null = type; } }
  }
}