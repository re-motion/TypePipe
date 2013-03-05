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
using System.Reflection;
using NUnit.Framework;
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
        Type expectedBaseTypeConstraint,
        params Type[] expectedInterfaceConstraints)
    {
      Assert.That (genericParameter, Is.Not.Null);
      Assert.That (genericParameter.Name, Is.EqualTo (expectedName));
      Assert.That (genericParameter.Attributes, Is.EqualTo (expectedAttributes));
      Assert.That (genericParameter.BaseConstraintProvider (genericParameterContext), Is.SameAs (expectedBaseTypeConstraint));
      Assert.That (genericParameter.InterfaceConstraintsProvider (genericParameterContext), Is.EqualTo (expectedInterfaceConstraints));
    }

    [Test]
    public void None ()
    {
      Assert.That (GenericParameterDeclaration.None, Is.Empty);
    }

    [Test]
    public void Initialization ()
    {
      var name = "parameter";
      var attributes = (GenericParameterAttributes) 7;
      Func<GenericParameterContext, Type> baseConstraintProvider = ctx => null;
      Func<GenericParameterContext, IEnumerable<Type>> interfaceConstraintsProvider = ctx => null;

      var declaration = new GenericParameterDeclaration (name, attributes, baseConstraintProvider, interfaceConstraintsProvider);

      Assert.That (declaration.Name, Is.EqualTo (name));
      Assert.That (declaration.Attributes, Is.EqualTo (attributes));
      Assert.That (declaration.BaseConstraintProvider, Is.SameAs (baseConstraintProvider));
      Assert.That (declaration.InterfaceConstraintsProvider, Is.SameAs (interfaceConstraintsProvider));
    }

    [Test]
    public void Initialization_Defaults ()
    {
      var declaration = new GenericParameterDeclaration ("name");

      Assert.That (declaration.Attributes, Is.EqualTo (GenericParameterAttributes.None));
      Assert.That (declaration.BaseConstraintProvider (null), Is.SameAs (typeof (object)));
      Assert.That (declaration.InterfaceConstraintsProvider (null), Is.Empty);
    }
  }
}