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
using System.Linq;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class UnderlyingParameterInfoDescriptorTest
  {
    [Test]
    public void Create_ForNew ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var name = "parameterName";
      var attributes = ParameterAttributes.Optional | ParameterAttributes.In;
      var declaration = new ParameterDeclaration (type, name, attributes);

      var descriptor = UnderlyingParameterInfoDescriptor.Create (declaration);

      CheckDescriptor (descriptor, null, type, name, attributes, type, expectedIsByRef: false);
    }

    [Test]
    public void Create_ForExisting ()
    {
      string s;
      var originalParameter = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Method (out s)).GetParameters().Single();

      var descriptor = UnderlyingParameterInfoDescriptor.Create (originalParameter);

      var type = typeof (string);
      CheckDescriptor (descriptor, originalParameter, type.MakeByRefType(), "parameterName", ParameterAttributes.Out, type, expectedIsByRef: true);
    }

    [Test]
    public void Expression_ReturnsSameInstance ()
    {
      string s;
      var originalParameter = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Method (out s)).GetParameters ().Single ();

      var descriptor = UnderlyingParameterInfoDescriptor.Create (originalParameter);

      var result1 = descriptor.Expression;
      var result2 = descriptor.Expression;

      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    public void CreateFromDeclarations ()
    {
      var declaration = ParameterDeclarationObjectMother.Create ();

      var descriptor = UnderlyingParameterInfoDescriptor.CreateFromDeclarations (new[] { declaration }).Single ();

      var type = declaration.Type;
      Assert.That (type.IsByRef, Is.False);
      CheckDescriptor (descriptor, null, type, declaration.Name, declaration.Attributes, type, expectedIsByRef: false);
    }

    [Test]
    public void CreateFromMethodBase ()
    {
      string s;
      var method = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Method (out s));

      var descriptor = UnderlyingParameterInfoDescriptor.CreateFromMethodBase (method).Single();

      var originalParameter = method.GetParameters().Single();
      var type = typeof (string);
      CheckDescriptor (descriptor, originalParameter, type.MakeByRefType(), "parameterName", ParameterAttributes.Out, type, expectedIsByRef: true);
    }

    private static void CheckDescriptor (
        UnderlyingParameterInfoDescriptor descriptor,
        ParameterInfo expectedParameterInfo,
        Type expectedType,
        string expectedName,
        ParameterAttributes expectedAttributes,
        Type expectedExpressionType,
        bool expectedIsByRef)
    {
      Assert.That (descriptor.UnderlyingSystemParameterInfo, Is.SameAs (expectedParameterInfo));
      Assert.That (descriptor.Type, Is.SameAs (expectedType));
      Assert.That (descriptor.Name, Is.EqualTo (expectedName));
      Assert.That (descriptor.Attributes, Is.EqualTo (expectedAttributes));
      Assert.That (descriptor.Expression.Name, Is.EqualTo (expectedName));
      Assert.That (descriptor.Expression.Type, Is.SameAs (expectedExpressionType));
      Assert.That (descriptor.Expression.IsByRef, Is.EqualTo (expectedIsByRef));
    }

    private void Method (out string parameterName) { parameterName = ""; }
  }
}