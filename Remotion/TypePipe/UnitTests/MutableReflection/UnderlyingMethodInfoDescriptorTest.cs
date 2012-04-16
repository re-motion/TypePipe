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
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class UnderlyingMethodInfoDescriptorTest
  {
    [Test]
    public void Create_ForNew ()
    {
      var name = "Method";
      var attributes = MethodAttributes.Abstract;
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      var returnType = ReflectionObjectMother.GetSomeType();
      var body = ExpressionTreeObjectMother.GetSomeExpression (returnType);

      var descriptor = UnderlyingMethodInfoDescriptor.Create (name, attributes, returnType, parameterDeclarations, body);

      Assert.That (descriptor.UnderlyingSystemMethodInfo, Is.Null);
      Assert.That (descriptor.Name, Is.EqualTo (name));
      Assert.That (descriptor.Attributes, Is.EqualTo (attributes));
      Assert.That (descriptor.ReturnType, Is.SameAs (returnType));
      Assert.That (descriptor.ParameterDeclarations, Is.EqualTo (parameterDeclarations));
      Assert.That (descriptor.Body, Is.SameAs (body));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage =
        "The body's return type must be assignable to the method return type.\r\nParameter name: body")]
    public void Create_ForNew_ThrowsForInvalidBodyReturnType ()
    {
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (string));
      UnderlyingMethodInfoDescriptor.Create ("Method", MethodAttributes.Abstract, typeof (int), ParameterDeclaration.EmptyParameters, body);
    }
  }
}