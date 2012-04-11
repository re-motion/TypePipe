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
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class ParameterDeclarationTest
  {
    [Test]
    public void EmptyParameters ()
    {
      Assert.That (ParameterDeclaration.EmptyParameters, Is.Empty);
    }

    [Test]
    public void Initialization ()
    {
      var declaration = new ParameterDeclaration (typeof (string), "parameterName", ParameterAttributes.Out);

      Assert.That (declaration.Type, Is.EqualTo (typeof (string)));
      Assert.That (declaration.Name, Is.EqualTo ("parameterName"));
      Assert.That (declaration.Attributes, Is.EqualTo (ParameterAttributes.Out));
      Assert.That (declaration.Expression.Name, Is.EqualTo ("parameterName"));
      Assert.That (declaration.Expression.Type, Is.EqualTo (typeof (string)));
      Assert.That (declaration.Expression.IsByRef, Is.False);
    }

    [Test]
    public void Initialization_Defaults ()
    {
      var declaration = new ParameterDeclaration (typeof (object), "foo");

      Assert.That (declaration.Attributes, Is.EqualTo (ParameterAttributes.In));
    }

    [Test]
    public void Initialization_WithByRefType ()
    {
      var declaration = new ParameterDeclaration (typeof (string).MakeByRefType(), "p");

      Assert.That (declaration.Type, Is.EqualTo (typeof (string).MakeByRefType()));
      Assert.That (declaration.Expression.Type, Is.EqualTo (typeof (string)));
      Assert.That (declaration.Expression.IsByRef, Is.True);
    }
  }
}