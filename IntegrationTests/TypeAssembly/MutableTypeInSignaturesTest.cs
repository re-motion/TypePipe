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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.MutableReflection;
using Remotion.Utilities;

namespace Remotion.TypePipe.IntegrationTests.TypeAssembly
{
  [TestFixture]
  public class MutableTypeInSignaturesTest : TypeAssemblerIntegrationTestBase
  {
    [Test]
#if NET6_0
    [Ignore("This test fails in .NET 6 due to a bug in the .NET Runtime (https://github.com/dotnet/runtime/issues/67802). It was fixed in .NET 7 and may receive a backup.")]
#endif
    public void CustomAttributes ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            proxyType.AddCustomAttribute (CreateAttribute (proxyType));
            proxyType.AddField ("Field", FieldAttributes.Public, typeof (int)).AddCustomAttribute (CreateAttribute (proxyType));
            var ctor = proxyType.AddConstructor (
                MethodAttributes.Public, new[] { new ParameterDeclaration (typeof (int), "p") }, ctx => ctx.CallThisConstructor());
            ctor.AddCustomAttribute (CreateAttribute (proxyType));
            ctor.MutableParameters.Single().AddCustomAttribute (CreateAttribute (proxyType));
            proxyType.AddMethod ("Method", MethodAttributes.Public, typeof (void), ParameterDeclaration.None, ctx => Expression.Empty())
                     .AddCustomAttribute (CreateAttribute (proxyType));
            proxyType.AddProperty ("Property", typeof (int), ParameterDeclaration.None, MethodAttributes.Public, ctx => Expression.Constant (7), null)
                     .AddCustomAttribute (CreateAttribute (proxyType));
            proxyType.AddEvent ("Event", typeof (Action), MethodAttributes.Public, ctx => Expression.Empty(), ctx => Expression.Empty())
                     .AddCustomAttribute (CreateAttribute (proxyType));
          });

      var constructor = type.GetConstructor (new[] { typeof (int) });
      Assertion.IsNotNull (constructor);

      CheckCustomAttribute (type, type);
      CheckCustomAttribute (type.GetField ("Field"), type);
      CheckCustomAttribute (constructor, type);
      CheckCustomAttribute (constructor.GetParameters().Single(), type);
      CheckCustomAttribute (type.GetMethod ("Method"), type);
      CheckCustomAttribute (type.GetProperty ("Property"), type);
      CheckCustomAttribute (type.GetEvent ("Event"), type);
    }

    [Test]
    public void LocalVariable ()
    {
      var type = AssembleType<DomainType> (
          p => p.AddMethod (
              "Method",
              MethodAttributes.Public,
              typeof (void),
              ParameterDeclaration.None,
              ctx =>
              {
                var localVariable = Expression.Parameter (p);
                return Expression.Block (new[] { localVariable }, Expression.Empty());
              }));

      var methodBody = type.GetMethod ("Method").GetMethodBody();
      Assertion.IsNotNull (methodBody);
      var localVariableType = methodBody.LocalVariables.Single().LocalType;

      Assert.That (localVariableType, Is.SameAs (type));
    }

    [Test]
    public void Field ()
    {
      var type = AssembleType<DomainType> (p => p.AddField ("Field", FieldAttributes.Public, p));

      var field = type.GetField ("Field");
      Assert.That (field.FieldType, Is.SameAs (type));
    }

    [Test]
    public void Constructor ()
    {
      var type = AssembleType<DomainType> (
          p => p.AddConstructor (MethodAttributes.Public, new[] { new ParameterDeclaration (p, "param") }, ctx => ctx.CallBaseConstructor()));

      var constructor = type.GetConstructor (new[] { type });
      Assertion.IsNotNull (constructor);
      Assert.That (constructor.GetParameters().Single().ParameterType, Is.SameAs (type));
    }

    [Test]
    public void Method ()
    {
      var type = AssembleType<DomainType> (
          p => p.AddMethod ("Method", MethodAttributes.Public, p, new[] { new ParameterDeclaration (p, "p") }, ctx => Expression.Default (p)));

      var method = type.GetMethod ("Method");
      Assert.That (method.ReturnType, Is.SameAs (type));
      Assert.That (method.GetParameters().Single().ParameterType, Is.SameAs (type));
    }

    [Test]
    public void Method_RefAndOutParameters ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var byRefType = proxyType.MakeByRefType();
            proxyType.AddMethod (
                "Method",
                MethodAttributes.Public,
                typeof (void),
                new[] { new ParameterDeclaration (byRefType, "ref"), new ParameterDeclaration (byRefType, "out", ParameterAttributes.Out) },
                ctx => Expression.Empty());
          });

      var parameters = type.GetMethod ("Method").GetParameters();
      var referenceType = type.MakeByRefType();

      Assert.That (parameters[0].ParameterType, Is.SameAs (referenceType));
      Assert.That (parameters[0].Attributes, Is.EqualTo (ParameterAttributes.None));
      Assert.That (parameters[1].ParameterType, Is.SameAs (referenceType));
      Assert.That (parameters[1].Attributes, Is.EqualTo (ParameterAttributes.Out));
    }

    [Test]
    public void Signature_WithProxyTypeAsGenericArgument ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var instantiation = typeof (List<>).MakeTypePipeGenericType (proxyType);
            proxyType.AddMethod (
                "Method", MethodAttributes.Public, instantiation, ParameterDeclaration.None, ctx => Expression.Default (instantiation));
          });

      var method = type.GetMethod ("Method");
      var expectedType = typeof (List<>).MakeGenericType (type);
      Assert.That (method.ReturnType, Is.SameAs (expectedType));
    }

    [Test]
    public void Signature_WithProxyTypeAsNestedGenericArgument ()
    {
      var type = AssembleType<DomainType> (
          proxyType =>
          {
            var funcType = typeof (Func<>).MakeTypePipeGenericType (proxyType);
            var enumerableType = typeof (IEnumerable<>).MakeTypePipeGenericType (funcType);
            proxyType.AddMethod (
                "Method", MethodAttributes.Public, enumerableType, ParameterDeclaration.None, ctx => Expression.Default (enumerableType));
          });

      var method = type.GetMethod ("Method");
      var expectedType = typeof (IEnumerable<>).MakeGenericType (typeof (Func<>).MakeGenericType (type));
      Assert.That (method.ReturnType, Is.SameAs (expectedType));
    }

    private void CheckCustomAttribute (ICustomAttributeProvider customAttributeProvider, Type expectedType)
    {
      // Retrieving custom attribute data triggers type loading and assembly resolving.
      // The assembly cannot be resolved because it is not saved to disk.
      ResolveEventHandler resolver = (sender, args) => expectedType.Assembly;
      AppDomain.CurrentDomain.AssemblyResolve += resolver;
      try
      {
        var attribute = (AbcAttribute) customAttributeProvider.GetCustomAttributes (typeof (AbcAttribute), false).Single();
        Assert.That (attribute.Type, Is.SameAs (expectedType));
      }
      finally
      {
        AppDomain.CurrentDomain.AssemblyResolve -= resolver;
      }
    }

    private CustomAttributeDeclaration CreateAttribute (MutableType mutableType)
    {
      var attributeCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (null));
      return new CustomAttributeDeclaration (attributeCtor, new object[] { mutableType });
    }

    public class DomainType { }

    public class AbcAttribute : Attribute
    {
      public readonly Type Type;
      public AbcAttribute (Type type) { Type = type; }
    }
  }
}