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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class ParameterDeclarationTest
  {
    public static void CheckParameter (
        ParameterDeclaration parameter, Type expectedType, string expectedName, ParameterAttributes expectedAttributes)
    {
      Assert.That (parameter, Is.Not.Null);
      Assert.That (parameter.Type, Is.SameAs (expectedType));
      Assert.That (parameter.Name, Is.EqualTo (expectedName));
      Assert.That (parameter.Attributes, Is.EqualTo (expectedAttributes));
    }

    [Test]
    public void None ()
    {
      Assert.That (ParameterDeclaration.None, Is.Empty);
    }

    [Test]
    public void Initialization ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var declaration = new ParameterDeclaration (type, "parameterName", ParameterAttributes.Out);

      Assert.That (declaration.Type, Is.SameAs (type));
      Assert.That (declaration.Name, Is.EqualTo ("parameterName"));
      Assert.That (declaration.Attributes, Is.EqualTo (ParameterAttributes.Out));
      Assert.That (declaration.Expression.Type, Is.SameAs (type));
      Assert.That (declaration.Expression.Name, Is.EqualTo ("parameterName"));
    }

    [Test]
    public void Initialization_Defaults ()
    {
      var declaration = new ParameterDeclaration (typeof (object));

      Assert.That (declaration.Attributes, Is.EqualTo (ParameterAttributes.None));
      Assert.That (declaration.Name, Is.Null);
      Assert.That (declaration.Expression.Name, Is.Null);
    }

    [Test]
    public void Initialization_NullName ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var declaration = new ParameterDeclaration (type, name: null);

      Assert.That (declaration.Name, Is.Null);
      Assert.That (declaration.Expression.Name, Is.Null);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Parameter cannot be of type void.\r\nParameter name: type")]
    public void Initialization_VoidType ()
    {
      Dev.Null = new ParameterDeclaration (typeof (void), "foo");
    }
  }
}