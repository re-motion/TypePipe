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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class TypePipeCustomAttributeDataTest
  {
    private ConstructorInfo _defaultCtor;
    private ConstructorInfo _ctorWithArgs;
    private PropertyInfo _property;

    [SetUp]
    public void Setup()
    {
      _defaultCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute());
      _ctorWithArgs = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new AbcAttribute (null));
      _property = NormalizingMemberInfoFromExpressionUtility.GetProperty ((AbcAttribute attr) => attr.Property);
    }

    [Test]
    public void CreateInstance_NoArguments ()
    {
      ICustomAttributeData declaration = new CustomAttributeDeclaration (_defaultCtor, new object[0]);

      var instance = TypePipeCustomAttributeData.CreateInstance (declaration);

      Assert.That (instance, Is.TypeOf<AbcAttribute>());
    }

    [Test]
    public void CreateInstance_CtorArgument ()
    {
      ICustomAttributeData declaration = new CustomAttributeDeclaration (_ctorWithArgs, new object[] { 7 });

      var instance = (AbcAttribute) declaration.CreateInstance ();

      Assert.That (instance.CtorArg, Is.EqualTo (7));
    }

    [Test]
    public void CreateInstance_NamedArgument ()
    {
      var declaration = new CustomAttributeDeclaration (_defaultCtor, new object[0], new NamedAttributeArgumentDeclaration (_property, 4711));

      var instance = (AbcAttribute) declaration.CreateInstance ();

      Assert.That (instance.Property, Is.EqualTo (4711));
    }

    [Test]
    public void CreateInstance_ComplexArguments ()
    {
      var ctorArg = new object[] { "x", 7, typeof (int), new[] { MyEnum.C, MyEnum.A } };
      var namedArg = new object[] { "z", 8, new[] { 1, 2, 3 }, new[] { typeof (double), typeof (string) }, MyEnum.B };
      var declaration = new CustomAttributeDeclaration (
          _ctorWithArgs, new object[] { ctorArg }, new NamedAttributeArgumentDeclaration (_property, namedArg));

      var instance = (AbcAttribute) declaration.CreateInstance ();

      Assert.That (instance.CtorArg, Is.EqualTo (ctorArg));
      Assert.That (instance.Property, Is.EqualTo (namedArg));
    }

    [Test]
    public void CreateInstance_NullArguments ()
    {
      var ctorArg = new object[] { "x", null };
      var declaration = new CustomAttributeDeclaration (
          _ctorWithArgs, new object[] { ctorArg }, new NamedAttributeArgumentDeclaration (_property, null));

      var instance = (AbcAttribute) declaration.CreateInstance ();

      Assert.That (instance.CtorArg, Is.EqualTo (ctorArg));
      Assert.That (instance.Property, Is.Null);
    }

    [Test]
    public void CreateInstance_DeepCopyForArrays ()
    {
      var arg = new[] { new[] { 1, 2 }, new[] { 3, 4 } };
      var declaration = new CustomAttributeDeclaration (_ctorWithArgs, new object[] { arg }, new NamedAttributeArgumentDeclaration (_property, arg));

      var instance = (AbcAttribute) declaration.CreateInstance ();

      Assert.That (instance.CtorArg, Is.Not.SameAs (arg).And.EqualTo (arg));
      var ctorArgValue = (int[][]) instance.CtorArg;
      Assert.That (ctorArgValue[0], Is.Not.SameAs (arg[0]).And.EqualTo (arg[0]));
      Assert.That (ctorArgValue[1], Is.Not.SameAs (arg[1]).And.EqualTo (arg[1]));

      Assert.That (instance.Property, Is.Not.SameAs (arg).And.EqualTo (arg));
      var namedArgValue = (int[][]) instance.CtorArg;
      Assert.That (namedArgValue[0], Is.Not.SameAs (arg[0]).And.EqualTo (arg[0]));
      Assert.That (namedArgValue[1], Is.Not.SameAs (arg[1]).And.EqualTo (arg[1]));
    }

    public class AbcAttribute : Attribute
    {
      public AbcAttribute () { }

      public AbcAttribute (object arg)
      {
        CtorArg = arg;
      }

      public object CtorArg { get; set; }
      public object Property { get; set; }
    }

    private enum MyEnum { A, B, C }
  }
}