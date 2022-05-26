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
using JetBrains.Annotations;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.StrongNaming;

namespace Remotion.TypePipe.IntegrationTests.StrongNaming
{
  [TestFixture]
#if !FEATURE_STRONGNAMESIGNING
  [Ignore ("Strong name signing is not supported.")]
#endif
  public class StrongNamingTest : IntegrationTestBase
  {
    private Type _signedType;
    private Type _unsignedType;
    private Type _signedInterfaceType;
    private Type _unsignedInterfaceType;
    private Type _signedDelegateType;
    private Type _unsignedDelegateType;
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
      _unsignedType = CreateUnsignedType(TypeAttributes.Class, typeof (object));

      _signedInterfaceType = typeof (IMarkerInterface);
      _unsignedInterfaceType = CreateUnsignedType (TypeAttributes.Interface | TypeAttributes.Abstract, baseType: null);

      _signedDelegateType = typeof (Action);
      _unsignedDelegateType = typeof (Action<>).MakeGenericType (_unsignedType);

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
      Action<MutableType> action = p => p.AddField ("f", FieldAttributes.Private, _signedType);

      CheckStrongNaming (action, forceStrongNaming: false);
    }

    [Test]
    public void NoStrongName_UnsignedType ()
    {
      Action<MutableType> action = p => p.AddField ("f", FieldAttributes.Private, _unsignedType);

      CheckStrongNaming (action, forceStrongNaming: false);
    }

    [Test]
    public void ForceStrongName ()
    {
      Action<MutableType> action = p => p.AddField ("f", FieldAttributes.Private, _signedType);

      CheckStrongNaming (action, forceStrongNaming: true, expectedKey: FallbackKey.KeyPair);
    }

    [Test]
    public void ForceStrongName_CustomKey ()
    {
      Action<MutableType> action = p => p.AddField ("f", FieldAttributes.Private, _signedType);

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
      CheckStrongNaming (p => p.AddMethod ("m", 0, _signedType, new[] { new ParameterDeclaration (_signedType) }, ctx => Expression.Default (_signedType)));
      CheckStrongNaming (p => p.AddProperty ("p", _signedType, new[] { new ParameterDeclaration (_signedType) }, 0, ctx => Expression.Default (_signedType), ctx => Expression.Empty()));
      CheckStrongNaming (p => p.AddEvent ("e", _signedDelegateType, 0, ctx => Expression.Empty(), ctx => Expression.Empty()));
      // TODO 4791: nested types

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
      CheckStrongNaming (p => p.AddProperty (
          "p", _signedType, new[] { new ParameterDeclaration (_signedType) }, 0, null, ctx => Expression.Empty()).AddCustomAttribute (_signedAttribute));
      CheckStrongNaming (p => p.AddEvent (
          "e", _signedDelegateType, 0, ctx => Expression.Empty (), ctx => Expression.Empty ()).AddCustomAttribute (_signedAttribute));
      // TODO 4791: Attributes on nested types?
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
      CheckStrongNamingException (p => p.AddProperty ("p", _unsignedType, new[] { new ParameterDeclaration (_signedType) }, 0, ctx => Expression.Default (_unsignedType), ctx => Expression.Empty()));
      CheckStrongNamingException (p => p.AddProperty ("p", _signedType, new[] { new ParameterDeclaration (_unsignedType) }, 0, ctx => Expression.Default (_signedType), ctx => Expression.Empty()));
      CheckStrongNamingException (p => p.AddEvent ("e", _unsignedDelegateType, 0, ctx => Expression.Empty(), ctx => Expression.Empty()), unsignedType: _unsignedDelegateType);
      // TODO 4791: nested types

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
      CheckStrongNamingException (
          p => p.AddProperty ("p", _signedType, new[] { new ParameterDeclaration (_signedType) }, 0, null, ctx => Expression.Empty ())
            .AddCustomAttribute (_unsignedAttribute));
      CheckStrongNamingException (
          p => p.AddEvent ("e", _signedDelegateType, 0, ctx => Expression.Empty (), ctx => Expression.Empty ())
            .AddCustomAttribute (_unsignedAttribute));
      // TODO 4791: Attributes on nested types?
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
      Action<MutableType> action = p => p.AddMethod ("Method", 0, p, new[] { new ParameterDeclaration (p, "p1") }, ctx => Expression.New (p));

      CheckStrongNaming (action, forceStrongNaming: true);
    }

    private void CheckStrongNaming (
        Action<MutableType> participantAction,
        bool forceStrongNaming = true,
        string keyFilePath = null,
        StrongNameKeyPair expectedKey = null,
        int stackFramesToSkip = 0)
    {
      var participant = CreateParticipant (participantAction);
      var objectFactory = CreateObjectFactoryForStrongNaming (participant, forceStrongNaming, keyFilePath);

      var type = objectFactory.ReflectionService.GetAssembledType (typeof (DomainType));
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

    private void CheckStrongNamingExpression (Expression methodBody)
    {
      CheckStrongNaming (p => p.AddMethod ("m", 0, typeof (void), ParameterDeclaration.None, ctx => methodBody), forceStrongNaming: true);
    }

    private void CheckStrongNamingException (Action<MutableType> participantAction, Type requestedType = null, Type unsignedType = null)
    {
      requestedType = requestedType ?? typeof (DomainType);
      unsignedType = unsignedType ?? _unsignedType;
      var participant = CreateParticipant (participantAction);
      var objectFactory = CreateObjectFactoryForStrongNaming (participant, forceStrongNaming: true);

      var message =
          "An error occurred during code generation for '" + requestedType.Name + "':\r\n"
          + "Strong-naming is enabled but a participant used the type '" + unsignedType.FullName + "' which comes from the unsigned assembly "
          + "'" + unsignedType.Assembly.GetName().Name + "'.\r\n"
          + @"The following participants are currently configured and may have caused the error: 'ParticipantStub'.";
      Assert.That (() => objectFactory.Create (requestedType), Throws.InvalidOperationException.With.Message.EqualTo (message));
    }

    private void CheckStrongNamingExpressionException (Expression methodBody)
    {
      CheckStrongNamingException (p => p.AddMethod ("m", MethodAttributes.Public, typeof (void), ParameterDeclaration.None, ctx => methodBody));
    }

    private IPipeline CreateObjectFactoryForStrongNaming (IParticipant participant, bool forceStrongNaming, string keyFilePath = null)
    {
      var settings = PipelineSettings
          .New()
          .SetForceStrongNaming (forceStrongNaming)
          .SetKeyFilePath (keyFilePath)
          .Build();
      return CreatePipelineWithIntegrationTestAssemblyLocation ("StrongNamingTest", settings, participant);
    }

    private Type CreateUnsignedType (TypeAttributes attributes, Type baseType)
    {
      var assemblyName = "testAssembly";
      var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly (new AssemblyName (assemblyName), AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyName + ".dll");
      var typeBuilder = moduleBuilder.DefineType ("UnsignedType", attributes | TypeAttributes.Public, baseType);

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
      [UsedImplicitly] public static int Field;
      public static void Method () { }
      public static void VarArgsMethod (__arglist) { }
    }
    public interface IMarkerInterface { }
    public class AbcAttribute : Attribute{ public AbcAttribute (Type type) { Dev.Null = type; } }
  }
}