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
using System.Runtime.InteropServices;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Expressions;
using Remotion.TypePipe.MutableReflection.Descriptors;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Descriptors
{
  [TestFixture]
  public class ConstructorDescriptorTest
  {
    [Test]
    public void Create_ForNew ()
    {
      var attributes = MethodAttributes.Abstract;
      var parameterDescriptors = ParameterDescriptorObjectMother.CreateMultiple (2);
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));

      var descriptor = ConstructorDescriptor.Create (attributes, parameterDescriptors.AsOneTime(), body);

      Assert.That (descriptor.Name, Is.EqualTo (".ctor"));
      Assert.That (descriptor.Attributes, Is.EqualTo (attributes));
      Assert.That (descriptor.Parameters, Is.EqualTo (parameterDescriptors));
      Assert.That (descriptor.CustomAttributeDataProvider.Invoke(), Is.Empty);
      Assert.That (descriptor.Body, Is.SameAs (body));
    }

    [Test]
    public void Create_ForNew_Static ()
    {
      var attributes = MethodAttributes.Static;
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));

      var descriptor = ConstructorDescriptor.Create (attributes, ParameterDescriptorObjectMother.Empty, body);

      Assert.That (descriptor.Name, Is.EqualTo (".cctor"));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Constructor bodies must have void return type.\r\nParameter name: body")]
    public void Create_ForNew_WithNonVoidBody ()
    {
      var nonVoidBody = ExpressionTreeObjectMother.GetSomeExpression(typeof(object));

      ConstructorDescriptor.Create (0, new ParameterDescriptor[0], nonVoidBody);
    }

    [Test]
    public void Create_ForExisting ()
    {
      int v;
      var underlyingCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType ("string", out v, 1.0, null));

      var descriptor = ConstructorDescriptor.Create (underlyingCtor);

      Assert.That (descriptor.Name, Is.EqualTo (".ctor"));
      Assert.That (descriptor.Attributes, Is.EqualTo (underlyingCtor.Attributes));

      var expectedParamterDecls =
          new[]
          {
              new { Type = typeof (string), Name = "s", Attributes = ParameterAttributes.None },
              new { Type = typeof (int).MakeByRefType(), Name = "i", Attributes = ParameterAttributes.Out },
              new { Type = typeof (double), Name = "d", Attributes = ParameterAttributes.In },
              new { Type = typeof (object), Name = "o", Attributes = ParameterAttributes.In | ParameterAttributes.Out },
          };
      var actualParameters = descriptor.Parameters.Select (pd => new { pd.Type, pd.Name, pd.Attributes });
      Assert.That (actualParameters, Is.EqualTo (expectedParamterDecls));

      Assert.That (
          descriptor.CustomAttributeDataProvider.Invoke ().Select (ad => ad.Type),
          Is.EquivalentTo (new[] { typeof (AbcAttribute), typeof (DefAttribute) }));

      Assert.That (descriptor.Body, Is.TypeOf<OriginalBodyExpression> ());
      var originalBodyExpression = (OriginalBodyExpression) descriptor.Body;
      Assert.That (originalBodyExpression.Type, Is.SameAs (typeof (void)));
      Assert.That (originalBodyExpression.MethodBase, Is.SameAs (underlyingCtor));
      Assert.That (originalBodyExpression.Arguments, Is.EqualTo (descriptor.Parameters.Select (pd => pd.Expression)));
    }

    [Test]
    public void Create_ForExisting_ChangesVisibilityProtectedOrInternalToProtected ()
    {
      var underlyingCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType ());
      Assert.That (underlyingCtor.IsFamilyOrAssembly, Is.True);

      var descriptor = ConstructorDescriptor.Create (underlyingCtor);

      var visibility = descriptor.Attributes & MethodAttributes.MemberAccessMask;
      Assert.That (visibility, Is.EqualTo (MethodAttributes.Family));
    }

    private class DomainType
    {
// ReSharper disable UnusedParameter.Local
      [Abc, Def]
      public DomainType (string s, out int i, [In] double d, [In, Out] object o)
// ReSharper restore UnusedParameter.Local
      {
        i = 5;
      }

      protected internal DomainType ()
      {
      }
    }

    public class AbcAttribute : Attribute { }
    public class DefAttribute : Attribute { }
  }
}