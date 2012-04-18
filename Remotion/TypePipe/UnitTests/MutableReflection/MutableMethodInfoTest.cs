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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableMethodInfoTest
  {
    private MutableType _declaringType;

    private UnderlyingMethodInfoDescriptor _descriptor;
    private MutableMethodInfo _mutableMethod;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create();

      var attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;
      var parameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      _descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (attributes: attributes, parameterDeclarations: parameters);
      _mutableMethod = Create(_descriptor);
    }

    [Test]
    public void Initialization ()
    {
      var mutableMethodInfo = new MutableMethodInfo (_declaringType, _descriptor);

      Assert.That (mutableMethodInfo.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (((IMutableMethodBase) mutableMethodInfo).DeclaringType, Is.SameAs (_declaringType));
      Assert.That (mutableMethodInfo.Body, Is.SameAs (_descriptor.Body));
    }

    [Test]
    public void UnderlyingSystemMethodInfo ()
    {
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForExisting ();
      Assert.That (descriptor.UnderlyingSystemMethodBase, Is.Not.Null);

      var methodInfo = Create (descriptor);

      Assert.That (methodInfo.UnderlyingSystemMethodInfo, Is.SameAs (descriptor.UnderlyingSystemMethodBase));
    }

    [Test]
    public void UnderlyingSystemMethodInfo_ForNull ()
    {
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew ();
      Assert.That (descriptor.UnderlyingSystemMethodBase, Is.Null);

      var methodInfo = Create (descriptor);

      Assert.That (methodInfo.UnderlyingSystemMethodInfo, Is.SameAs (methodInfo));
    }

    [Test]
    public void IsNewMethod_True ()
    {
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew ();
      Assert.That (descriptor.UnderlyingSystemMethodBase, Is.Null);

      var methodInfo = Create (descriptor);

      Assert.That (methodInfo.IsNew, Is.True);
    }

    [Test]
    public void IsNewMethod_False ()
    {
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForExisting ();
      Assert.That (descriptor.UnderlyingSystemMethodBase, Is.Not.Null);

      var methodInfo = Create (descriptor);

      Assert.That (methodInfo.IsNew, Is.False);
    }

    [Test]
    public void IsModified_False ()
    {
      Assert.That (_mutableMethod.IsModified, Is.False);
    }

    [Test]
    public void IsModified_True ()
    {
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      _mutableMethod.SetBody (ctx => fakeBody);

      Assert.That (_mutableMethod.IsModified, Is.True);
    }

    [Test]
    public void Name ()
    {
      Assert.That (_mutableMethod.Name, Is.EqualTo (_descriptor.Name));
      }

    [Test]
    public void Attributes ()
    {
      Assert.That (_mutableMethod.Attributes, Is.EqualTo (_descriptor.Attributes));
}

    [Test]
    public void ReturnType ()
    {
      Assert.That (_mutableMethod.ReturnType, Is.SameAs (_descriptor.ReturnType));
    }

    [Test]
    public void CallingConvention ()
    {
      var instanceDescriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (attributes: 0);
      var staticDescriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (attributes: MethodAttributes.Static);

      var instanceMethod = new MutableMethodInfo (_declaringType, instanceDescriptor);
      var staticMethod = new MutableMethodInfo (_declaringType, staticDescriptor);

      Assert.That (instanceMethod.CallingConvention, Is.EqualTo (CallingConventions.HasThis));
      Assert.That (staticMethod.CallingConvention, Is.EqualTo (CallingConventions.Standard));
    }

    [Test]
    public void ParameterExpressions ()
    {
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      var methodInfo = CreateWithParameters (parameterDeclarations);

      Assert.That (methodInfo.ParameterExpressions, Is.EqualTo (parameterDeclarations.Select (pd => pd.Expression)));
    }

    [Test]
    public void SetBody ()
    {
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (object));
      Func<MethodBodyModificationContext, Expression> bodyProvider = context =>
      {
        Assert.That (_mutableMethod.ParameterExpressions, Is.Not.Empty);
        Assert.That (context.Parameters, Is.EqualTo (_mutableMethod.ParameterExpressions));
        Assert.That (context.DeclaringType, Is.SameAs (_declaringType));
        Assert.That (context.IsStatic, Is.False);

        var previousBody = context.GetPreviousBody ();
        Assert.That (previousBody, Is.SameAs (_mutableMethod.Body));

        return fakeBody;
      };

      _mutableMethod.SetBody (bodyProvider);

      var expectedBody = Expression.Block (typeof (void), fakeBody);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, _mutableMethod.Body);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The body of the non-virtual method 'UnspecifiedMethod' cannot be replaced.")]
    public void SetBody_NonVirtualMethod ()
    {
      MethodAttributes attributesOfNonVirtualMethod = 0;
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (attributes: attributesOfNonVirtualMethod);
      var mutableMethod = Create (descriptor);

      Func<MethodBodyModificationContext, Expression> bodyProvider = context =>
      {
        Assert.Fail ("Should not be called.");
        return ExpressionTreeObjectMother.GetSomeExpression();
      };

      mutableMethod.SetBody (bodyProvider);
    }

    [Test]
    public void ToString_WithParameters ()
    {
      var parameters = new[]
                       {
                           ParameterDeclarationObjectMother.Create (typeof (int), "p1"),
                           ParameterDeclarationObjectMother.Create (typeof (string).MakeByRefType(), "p2", ParameterAttributes.Out)
                       };
      var methodInfo = MutableMethodInfoObjectMother.Create (returnType: typeof (string), name: "Xxx", parameterDeclarations: parameters);

      Assert.That (methodInfo.ToString(), Is.EqualTo ("System.String Xxx(System.Int32, System.String&)"));
    }

    [Test]
    public void ToDebugString ()
    {
      var methodInfo = MutableMethodInfoObjectMother.Create (
          declaringType: MutableTypeObjectMother.CreateForExistingType (GetType()),
          returnType: typeof (void),
          name: "Xxx",
          parameterDeclarations: new[] { ParameterDeclarationObjectMother.Create (typeof (int), "p1") });

      var expected = "MutableMethod = \"System.Void Xxx(System.Int32)\", DeclaringType = \"MutableMethodInfoTest\"";
      Assert.That (methodInfo.ToDebugString(), Is.EqualTo (expected));
    }

    [Test]
    public void GetParameters ()
    {
      var parameter1 = ParameterDeclarationObjectMother.Create();
      var parameter2 = ParameterDeclarationObjectMother.Create();
      var methodInfo = CreateWithParameters (parameter1, parameter2);

      var result = methodInfo.GetParameters();

      var actualParameterInfos = result.Select (pi => new { pi.Member, pi.Position, pi.ParameterType, pi.Name, pi.Attributes });
      var expectedParameterInfos =
          new[]
          {
              new { Member = (MemberInfo) methodInfo, Position = 0, ParameterType = parameter1.Type, parameter1.Name, parameter1.Attributes },
              new { Member = (MemberInfo) methodInfo, Position = 1, ParameterType = parameter2.Type, parameter2.Name, parameter2.Attributes },
          };
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
    }

    [Test]
    public void GetParameters_ReturnsSameParameterInfoInstances ()
    {
      var methodInfo = CreateWithParameters (ParameterDeclarationObjectMother.Create ());

      var result1 = methodInfo.GetParameters ().Single ();
      var result2 = methodInfo.GetParameters ().Single ();

      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    public void GetParameters_DoesNotAllowModificationOfInternalList ()
    {
      var methodInfo = CreateWithParameters (ParameterDeclarationObjectMother.CreateMultiple (1));

      var parameters = methodInfo.GetParameters ();
      Assert.That (parameters[0], Is.Not.Null);
      parameters[0] = null;

      var parametersAgain = methodInfo.GetParameters ();
      Assert.That (parametersAgain[0], Is.Not.Null);
    }

    private MutableMethodInfo Create (UnderlyingMethodInfoDescriptor descriptor)
    {
      return new MutableMethodInfo (_declaringType, descriptor);
    }

    private MutableMethodInfo CreateWithParameters (params ParameterDeclaration[] parameterDeclarations)
    {
      var descriptor = UnderlyingMethodInfoDescriptorObjectMother.CreateForNew (parameterDeclarations: parameterDeclarations);
      return new MutableMethodInfo (_declaringType, descriptor);
    }
  }
}