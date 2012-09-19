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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class UnderlyingConstructorInfoDescriptorTest
  {
    [Test]
    public void Create_ForNew ()
    {
      var attributes = MethodAttributes.Abstract;
      var parameterDescriptors = UnderlyingParameterInfoDescriptorObjectMother.CreateMultiple (2);
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));

      var descriptor = UnderlyingConstructorInfoDescriptor.Create (attributes, parameterDescriptors.AsOneTime(), body);

      Assert.That (descriptor.UnderlyingSystemMember, Is.Null);
      Assert.That (descriptor.Name, Is.EqualTo (".ctor"));
      Assert.That (descriptor.Attributes, Is.EqualTo (attributes));
      Assert.That (descriptor.ParameterDescriptors, Is.EqualTo (parameterDescriptors));
      Assert.That (descriptor.Body, Is.SameAs (body));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Constructor bodies must have void return type.\r\nParameter name: body")]
    public void Create_ForNew_WithNonVoidBody ()
    {
      var nonVoidBody = ExpressionTreeObjectMother.GetSomeExpression(typeof(object));

      UnderlyingConstructorInfoDescriptor.Create (0, new UnderlyingParameterInfoDescriptor[0], nonVoidBody);
    }

    [Test]
    public void Create_ForExisting ()
    {
      int v;
      var originalCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType ("string", out v, 1.0, null));

      var descriptor = UnderlyingConstructorInfoDescriptor.Create (originalCtor);

      Assert.That (descriptor.UnderlyingSystemMember, Is.SameAs (originalCtor));
      Assert.That (descriptor.Name, Is.EqualTo (".ctor"));
      Assert.That (descriptor.Attributes, Is.EqualTo (originalCtor.Attributes));

      var expectedParamterDecls =
          new[]
          {
              new { Type = typeof (string), Name = "s", Attributes = ParameterAttributes.None },
              new { Type = typeof (int).MakeByRefType(), Name = "i", Attributes = ParameterAttributes.Out },
              new { Type = typeof (double), Name = "d", Attributes = ParameterAttributes.In },
              new { Type = typeof (object), Name = "o", Attributes = ParameterAttributes.In | ParameterAttributes.Out },
          };
      var actualParameterDecls = descriptor.ParameterDescriptors.Select (pd => new { pd.Type, pd.Name, pd.Attributes });
      Assert.That (actualParameterDecls, Is.EqualTo (expectedParamterDecls));

      Assert.That (descriptor.Body, Is.TypeOf<OriginalBodyExpression> ());

      var originalBodyExpression = (OriginalBodyExpression) descriptor.Body;
      Assert.That (originalBodyExpression.Type, Is.SameAs (typeof (void)));
      Assert.That (originalBodyExpression.MethodBase, Is.SameAs (originalCtor));
      Assert.That (originalBodyExpression.Arguments, Is.EqualTo (descriptor.ParameterDescriptors.Select (pd => pd.Expression)));
    }

    [Test]
    public void Create_ForExisting_ChangesVisibilityProtectedOrInternalToProtected ()
    {
      var originalCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType ());
      Assert.That (originalCtor.IsFamilyOrAssembly, Is.True);

      var descriptor = UnderlyingConstructorInfoDescriptor.Create (originalCtor);

      var visibility = descriptor.Attributes & MethodAttributes.MemberAccessMask;
      Assert.That (visibility, Is.EqualTo (MethodAttributes.Family));
    }

    private class DomainType
    {
// ReSharper disable UnusedParameter.Local
      public DomainType (string s, out int i, [In] double d, [In, Out] object o)
// ReSharper restore UnusedParameter.Local
      {
        i = 5;
      }

      protected internal DomainType ()
      {
      }
    }
  }
}