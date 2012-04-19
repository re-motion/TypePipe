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
using Remotion.TypePipe.MutableReflection;
using System.Linq;

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
    public void CreateEquivalent ()
    {
      var parameterInfo = ReflectionObjectMother.GetSomeParameter();

      var result = ParameterDeclaration.CreateEquivalent (parameterInfo);

      Assert.That (result.Type, Is.SameAs (parameterInfo.ParameterType));
      Assert.That (result.Name, Is.EqualTo (parameterInfo.Name));
      Assert.That (result.Attributes, Is.EqualTo (parameterInfo.Attributes));
    }

    [Test]
    public void CreateForEquivalentSignature ()
    {
      string v;
      var method = MemberInfoFromExpressionUtility.GetMethod ((DomainType obj) => obj.Method (42, out v));

      var result = ParameterDeclaration.CreateForEquivalentSignature (method);

      var expected = new[] 
      { 
        new { Type = typeof (int), Name = "i", Attributes = ParameterAttributes.None },
        new { Type = typeof (string).MakeByRefType(), Name = "s", Attributes = ParameterAttributes.Out }
      };
      var actual = result.Select (pd => new { pd.Type, pd.Name, pd.Attributes });
      Assert.That (actual, Is.EqualTo (expected));
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

    private abstract class DomainType
    {
      public void Method (int i, out string s)
      {
        Dev.Null = i;
        s = "no";
      }
    }
  }
}