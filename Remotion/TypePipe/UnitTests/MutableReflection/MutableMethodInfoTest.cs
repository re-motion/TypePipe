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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableMethodInfoTest
  {
    private Type _declaringType;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = ReflectionObjectMother.GetSomeType ();
    }

    [Test]
    public void Initialization ()
    {
      var name = "method name";
      var methodAttributes = MethodAttributes.Public;
      var returnType = ReflectionObjectMother.GetSomeType();
      var parameter1 = ParameterDeclarationObjectMother.Create();
      var parameter2 = ParameterDeclarationObjectMother.Create();
      var body = ExpressionTreeObjectMother.GetSomeExpression (returnType);

      var method = new MutableMethodInfo (_declaringType, name, methodAttributes, returnType, new[] { parameter1, parameter2}, body);

      Assert.That (method.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (method.Name, Is.EqualTo (name));
      Assert.That (method.Attributes, Is.EqualTo (methodAttributes));
      Assert.That (method.ReturnType, Is.SameAs (returnType));
      var expectedParameterInfos =
          new[]
          {
              new { Member = (MemberInfo) method, Position = 0, ParameterType = parameter1.Type, parameter1.Name, parameter1.Attributes },
              new { Member = (MemberInfo) method, Position = 1, ParameterType = parameter2.Type, parameter2.Name, parameter2.Attributes },
          };
      var actualParameterInfos = method.GetParameters ().Select (pi => new { pi.Member, pi.Position, pi.ParameterType, pi.Name, pi.Attributes });
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
      Assert.That (method.Body, Is.SameAs (body));
    }

    [Test]
    public void ParameterExpressions ()
    {
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      var methodInfo = CreateWithParameters (parameterDeclarations);

      Assert.That (methodInfo.ParameterExpressions, Is.EqualTo (parameterDeclarations.Select (pd => pd.Expression)));
    }

    [Test]
    public void GetParameters_DoesNotAllowModificationOfInternalList ()
    {
      var method = CreateWithParameters (ParameterDeclarationObjectMother.CreateMultiple (1));

      var parameters = method.GetParameters ();
      Assert.That (parameters[0], Is.Not.Null);
      parameters[0] = null;

      var parametersAgain = method.GetParameters ();
      Assert.That (parametersAgain[0], Is.Not.Null);
    }

    private MutableMethodInfo CreateWithParameters (ParameterDeclaration[] parameterDeclarations)
    {
      return new MutableMethodInfo (
          _declaringType, 
          "UnspecifiedMethod",
          MethodAttributes.Public,
          ReflectionObjectMother.GetSomeType(),
          parameterDeclarations,
          ExpressionTreeObjectMother.GetSomeExpression());
    }
  }
}