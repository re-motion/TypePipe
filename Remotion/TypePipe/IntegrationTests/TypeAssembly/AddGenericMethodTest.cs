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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class AddGenericMethodTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
    public void GenericParameterInSignature_AndLocalVariable ()
    {
      // public T GenericMethod<T> (T arg) {
      //   T localVar = arg;
      //   return localVar;
      // }

      var type = AssembleType<DomainType> (
          p => p.AddMethod (
              "GenericMethod",
              MethodAttributes.Public,
              new[] { new GenericParameterDeclaration ("T") },
              returnTypeProvider: ctx => ctx.GenericParameters[0],
              parameterProvider: ctx => new[] { new ParameterDeclaration (ctx.GenericParameters[0], "arg") },
              bodyProvider: ctx =>
              {
                var local = Expression.Variable (ctx.GenericParameters[0], "localVar");
                return Expression.Block (
                    new[] { local },
                    Expression.Assign (local, ctx.Parameters[0]),
                    local);
              }));

      var genericMethod = type.GetMethod ("GenericMethod");
      Assert.That (genericMethod.IsGenericMethodDefinition, Is.True);
      var genericParameter = genericMethod.GetGenericArguments().Single();
      Assert.That (genericParameter.Name, Is.EqualTo ("T"));

      var method = genericMethod.MakeGenericMethod (typeof (long));
      var instance = Activator.CreateInstance (type);
      var result = method.Invoke (instance, new object[] { 5L });
      Assert.That (result, Is.EqualTo (5L));
    }

    [Test]
    public void AbstractMethod_GenericParameter_GenericParameterAttributes ()
    {
      // public abstract void GenericMethod<T> () where T : struct;

      var type = AssembleType<DomainType> (
          p => p.AddMethod (
              "GenericMethod",
              MethodAttributes.Public | MethodAttributes.Abstract | MethodAttributes.Virtual,
              new[] { new GenericParameterDeclaration ("T", GenericParameterAttributes.NotNullableValueTypeConstraint) },
              returnTypeProvider: ctx => typeof (void),
              parameterProvider: ctx => ParameterDeclaration.None,
              bodyProvider: null));

      var genericMethod = type.GetMethod ("GenericMethod");
      var genericParameter = genericMethod.GetGenericArguments().Single();
      Assert.That (genericParameter.GenericParameterAttributes, Is.EqualTo (GenericParameterAttributes.NotNullableValueTypeConstraint));
    }

    [Test]
    public void GenericMethodParametersUsedInsideParameters_AndInvokingLambda ()
    {
      // public static TResult GenericMethod<TArg, TResult> (Func<TArg, TResult> conv, TArg arg)
      // { return conv (arg); }

      var type = AssembleType<DomainType> (
          p => p.AddMethod (
              "GenericMethod",
              MethodAttributes.Public | MethodAttributes.Static,
              new[] { new GenericParameterDeclaration ("TArg"), new GenericParameterDeclaration ("TReturn") },
              ctx => ctx.GenericParameters[1],
              ctx =>
              new[]
              {
                  new ParameterDeclaration (typeof (Func<,>).MakeTypePipeGenericType (ctx.GenericParameters[0], ctx.GenericParameters[1]), "conv"),
                  new ParameterDeclaration (ctx.GenericParameters[0], "arg")
              },
              ctx => Expression.Invoke (ctx.Parameters[0], ctx.Parameters[1])));

      var method = type.GetMethod ("GenericMethod").MakeGenericMethod (typeof (int), typeof (string));
      Assert.That (method.IsStatic, Is.True);
      Func<int, string> converter = i => "Integer " + i;
      var result = method.Invoke (null, new object[] { converter, 7 });
      Assert.That (result, Is.EqualTo ("Integer 7"));
    }

    [Test]
    public void Constraints_Interface_CallInterfaceMethod_InstantiatedWithReferenceType_AndWithValueType ()
    {
      // public string GenericMethod<T> (T t)
      //     where T : IDomainInterface
      // { return t.GetTypeName(); }

      var ifcMethod = NormalizingMemberInfoFromExpressionUtility.GetMethod ((IDomainInterface o) => o.GetTypeName ());
      var type = AssembleType<DomainType> (
          p => p.AddMethod (
              "GenericMethod",
              MethodAttributes.Public,
              new[] { new GenericParameterDeclaration ("T", constraintProvider: ctx => new[] { typeof (IDomainInterface) }) },
              ctx => typeof (string),
              ctx => new[] { new ParameterDeclaration (ctx.GenericParameters[0], "t") },
              ctx =>
              {
                var genericParameter = ctx.GenericParameters[0];
                Assert.That (genericParameter, Is.SameAs (ctx.Parameters[0].Type));
                Assert.That (genericParameter.IsGenericParameter, Is.True);
                Assert.That (genericParameter.Name, Is.EqualTo ("T"));
                Assert.That (genericParameter.GenericParameterAttributes, Is.EqualTo (GenericParameterAttributes.None));
                Assert.That (genericParameter.GetInterfaces(), Is.EqualTo (new[] { typeof (IDomainInterface) }));
                Assert.That (genericParameter.GetGenericParameterConstraints(), Is.EqualTo (new[] { typeof (IDomainInterface) }));

                // TODO 5480: Remove conversion and inline.
                var castedInstance = Expression.Convert (ctx.Parameters[0], typeof (IDomainInterface));
                return Expression.Call (castedInstance, ifcMethod); }));

      var genericMethod = type.GetMethod ("GenericMethod");
      var instance = Activator.CreateInstance (type);

      var instantiation1 = genericMethod.MakeGenericMethod (typeof (DomainType));
      var instantiation2 = genericMethod.MakeGenericMethod (typeof (DomainValueType));

      Assert.That (instantiation1.Invoke (instance, new object[] { new DomainType () }), Is.EqualTo ("DomainType"));
      Assert.That (instantiation2.Invoke (instance, new object[] { new DomainValueType () }), Is.EqualTo ("value type"));
    }

    [Test]
    public void Constraints_DefaultCtor_AndReferenceType_AndBaseTypeAndInterfaces_AndBaseMethodCall ()
    {
      // public string GenericMethod<T> (T arg)
      //     where T : class, BaseType, IDomainInterface, new()
      // { return new T ().BaseMethod (); }

      var type = AssembleType<DomainType> (
          p => p.AddMethod (
              "GenericMethod",
              MethodAttributes.Public,
              new[]
              {
                  new GenericParameterDeclaration (
                      "T",
                      GenericParameterAttributes.DefaultConstructorConstraint | GenericParameterAttributes.ReferenceTypeConstraint,
                      ctx => new[] { typeof (BaseType), typeof (IDomainInterface) })
              },
              ctx => typeof (string),
              ctx => new[] { new ParameterDeclaration (ctx.GenericParameters[0], "arg") },
              ctx =>
              {
                Assert.That (ctx.GenericParameters[0].BaseType, Is.SameAs (typeof (BaseType)));
                Assert.That (ctx.GenericParameters[0].GetInterfaces (), Is.EqualTo (new[] { typeof (IDomainInterface) }));
                Assert.That (
                    ctx.GenericParameters[0].GetGenericParameterConstraints(),
                    Is.EquivalentTo (new[] { typeof (BaseType), typeof (IDomainInterface) }));
                Assert.That (
                    ctx.GenericParameters[0].GenericParameterAttributes,
                    Is.EqualTo (GenericParameterAttributes.DefaultConstructorConstraint | GenericParameterAttributes.ReferenceTypeConstraint));
                Assert.That (
                    ctx.GenericParameters[0].GetConstructors(),
                    Has.Length.EqualTo (1).And.All.Matches<ConstructorInfo> (c => c.GetParameters().Length == 0));

                // TODO 5480: Remove conversion and inline.
                var newInstance = Expression.New (ctx.GenericParameters[0]);
                var castedInstance = Expression.Convert (newInstance, typeof (BaseType));
                return Expression.Call (castedInstance, "BaseMethod", Type.EmptyTypes); 
              }));

      var genericMethod = type.GetMethod ("GenericMethod");
      var genericParameter = genericMethod.GetGenericArguments().Single();
      Assert.That (
          genericParameter.GenericParameterAttributes,
          Is.EqualTo (GenericParameterAttributes.DefaultConstructorConstraint | GenericParameterAttributes.ReferenceTypeConstraint));
      Assert.That (genericParameter.GetGenericParameterConstraints(), Is.EquivalentTo (new[] { typeof (BaseType), typeof (IDomainInterface) }));
      Assert.That (genericParameter.BaseType, Is.SameAs (typeof (BaseType)));
      Assert.That (genericParameter.GetInterfaces(), Is.EqualTo (new[] { typeof (IDomainInterface) }));

      var method = genericMethod.MakeGenericMethod (typeof (DomainType));
      var instance = Activator.CreateInstance (type);
      var result = method.Invoke (instance, new object[] { null });
      Assert.That (result, Is.EqualTo ("base method"));
    }

    [Test]
    public void Constraints_RecursiveConstraint ()
    {
      // public bool GenericEquals<T> (T a, T b)
      //    where T : IComparable<T>
      // {  return a.CompareTo(b) == 0; }

      var type = AssembleType<DomainType> (
          p => p.AddMethod (
              "GenericEquals",
              MethodAttributes.Public,
              new[]
              {
                  new GenericParameterDeclaration (
                      "T",
                      constraintProvider: ctx => new[] { typeof (IComparable<>).MakeTypePipeGenericType (ctx.GenericParameters[0]) })
              },
              ctx => typeof (bool),
              ctx => new[] { new ParameterDeclaration (ctx.GenericParameters[0], "a"), new ParameterDeclaration (ctx.GenericParameters[0], "b") },
              ctx =>
              {
                var genericParam = ctx.GenericParameters[0];
                Assert.That (
                    genericParam.GetGenericParameterConstraints(),
                    Is.EquivalentTo (new[] {typeof (IComparable<>).MakeTypePipeGenericType (genericParam) }));
                var paramTypes = ctx.Parameters.Select (para => para.Type).ToList();
                Assert.That (paramTypes[0], Is.SameAs (paramTypes[1]).And.SameAs (genericParam));

                var compareMethod = typeof (IComparable<>).MakeTypePipeGenericType (ctx.GenericParameters[0]).GetMethod ("CompareTo");
                // TODO 5480: Remove conversion and inline.
                var constraintType = typeof (IComparable<>).MakeTypePipeGenericType (ctx.GenericParameters[0]);
                var castedInstance = Expression.Convert (ctx.Parameters[0], constraintType);
                return Expression.Equal (Expression.Call (castedInstance, compareMethod, ctx.Parameters[1]), Expression.Constant (0));
              }));

      var genericMethod = type.GetMethod ("GenericEquals");
      var genericParameter = genericMethod.GetGenericArguments().Single();
      Assert.That (genericParameter.IsGenericParameter, Is.True);
      Assert.That (
          genericParameter.GetGenericParameterConstraints(),
          Is.EquivalentTo (new[] { typeof (object), typeof (IComparable<>).MakeGenericType (genericParameter) }));
      var parameterTypes = genericMethod.GetParameters().Select (p => p.ParameterType).ToList();
      Assert.That (parameterTypes[0], Is.SameAs (parameterTypes[1]).And.SameAs (genericParameter));

      var method = genericMethod.MakeGenericMethod (typeof (int));
      var instance = Activator.CreateInstance (type);

      Assert.That (method.Invoke (instance, new object[] { 7, 7 }), Is.True);
      Assert.That (method.Invoke (instance, new object[] { 7, 8 }), Is.False);
    }

    [Test]
    public void AddMethod_MethodDeclaration ()
    {
      var method =
          NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition ((DomainType obj) => obj.MethodDeclaration<MemoryStream> (null));
      var declaration = MethodDeclaration.CreateEquivalent (method);
      var type = AssembleType<DomainType> (p => p.AddMethod ("GenericMethod", MethodAttributes.Public, declaration, ctx => Expression.Empty()));

      var genericMethod = type.GetMethod ("GenericMethod");
      var genericParameter = genericMethod.GetGenericArguments().Single();
      Assert.That (
          genericParameter.GenericParameterAttributes,
          Is.EqualTo (GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint));
      Assert.That (genericParameter.GetGenericParameterConstraints(), Is.EquivalentTo (new[] { typeof (object), typeof (IDisposable) }));
    }

    public interface IDomainInterface
    {
      string GetTypeName ();
    }
    public class BaseType
    {
      public string BaseMethod () { return "base method"; }
    }
    public class DomainType : BaseType, IDomainInterface
    {
      public string GetTypeName () { return GetType().Name; }
      public void MethodDeclaration<T> (T arg) where T : class, IDisposable, new() {}
    }
    public class DomainValueType : IDomainInterface
    {
      public string GetTypeName () { return "value type"; }
    }
  }
}