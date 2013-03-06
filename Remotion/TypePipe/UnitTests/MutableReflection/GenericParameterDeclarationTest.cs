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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class GenericParameterDeclarationTest
  {
    public static void CheckGenericParameter (
        GenericParameterDeclaration genericParameter,
        GenericParameterContext genericParameterContext,
        string expectedName,
        GenericParameterAttributes expectedAttributes,
        params Type[] expectedConstraints)
    {
      Assert.That (genericParameter, Is.Not.Null);
      Assert.That (genericParameter.Name, Is.EqualTo (expectedName));
      Assert.That (genericParameter.Attributes, Is.EqualTo (expectedAttributes));
      Assert.That (genericParameter.ConstraintProvider (genericParameterContext), Is.EqualTo (expectedConstraints));
    }

    [Test]
    public void None ()
    {
      Assert.That (GenericParameterDeclaration.None, Is.Empty);
    }

    [Test]
    public void CreateEquivalent_GenericParameterOnMethod ()
    {
      var genericParameter = GetType().GetMethod ("Method").GetGenericArguments().First();

      var declaration = GenericParameterDeclaration.CreateEquivalent (genericParameter);

      var context = new GenericParameterContext (new[] { ReflectionObjectMother.GetSomeType(), ReflectionObjectMother.GetSomeOtherType() });
      CheckGenericParameter (
          declaration,
          context,
          "TFirst",
          GenericParameterAttributes.DefaultConstructorConstraint,
          context.GenericParameters[1],
          typeof (IList<>).MakeGenericType (context.GenericParameters[0]));
    }

    [Test]
    public void CreateEquivalent_GenericParameterOnType ()
    {
      var genericParameter = typeof (Class<>).GetGenericArguments().Single();

      var declaration = GenericParameterDeclaration.CreateEquivalent (genericParameter);

      var context = new GenericParameterContext (new[] { ReflectionObjectMother.GetSomeType() });
      CheckGenericParameter (declaration, context, "T", GenericParameterAttributes.None);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage =
        "The specified type must be a generic parameter (IsGenericParameter must be true).\r\nParameter name: genericParameter")]
    public void CreateEquivalent_NoGenericParameter ()
    {
      GenericParameterDeclaration.CreateEquivalent (ReflectionObjectMother.GetSomeType());
    }

    [Test]
    public void Initialization ()
    {
      var name = "parameter";
      var attributes = (GenericParameterAttributes) 7;
      Func<GenericParameterContext, IEnumerable<Type>> constraintProvider = ctx => null;

      var declaration = new GenericParameterDeclaration (name, attributes, constraintProvider);

      Assert.That (declaration.Name, Is.EqualTo (name));
      Assert.That (declaration.Attributes, Is.EqualTo (attributes));
      Assert.That (declaration.ConstraintProvider, Is.SameAs (constraintProvider));
    }

    [Test]
    public void Initialization_Defaults ()
    {
      var declaration = new GenericParameterDeclaration ("name");

      Assert.That (declaration.Attributes, Is.EqualTo (GenericParameterAttributes.None));
      Assert.That (declaration.ConstraintProvider (null), Is.Empty);
    }

    public void Method<TFirst, TLast> () where TFirst : TLast, IList<TFirst>, new() {}

    private class Class<T> {}
  }
}