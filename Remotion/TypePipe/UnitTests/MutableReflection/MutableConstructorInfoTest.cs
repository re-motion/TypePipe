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
using Microsoft.Scripting.Ast;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using System.Linq;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableConstructorInfoTest
  {
    private MutableType _declaringType;
    
    private MutableConstructorInfo _mutableCtor;
    private UnderlyingConstructorInfoDescriptor _descriptor;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create();

      _descriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew();
      _mutableCtor = Create (_descriptor);
    }

    [Test]
    public void Initialization ()
    {
      var underlyingCtorInfoDescriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew ();

      var ctorInfo = new MutableConstructorInfo (_declaringType, underlyingCtorInfoDescriptor);

      Assert.That (ctorInfo.DeclaringType, Is.SameAs (_declaringType));
    }

    [Test]
    public void UnderlyingSystemConsructorInfo ()
    {
      var underlyingCtorInfoDescriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForExisting ();
      Assert.That (underlyingCtorInfoDescriptor.UnderlyingSystemConstructorInfo, Is.Not.Null);

      var ctorInfo = Create (underlyingCtorInfoDescriptor);

      Assert.That (ctorInfo.UnderlyingSystemConstructorInfo, Is.SameAs (underlyingCtorInfoDescriptor.UnderlyingSystemConstructorInfo));
    }

    [Test]
    public void UnderlyingSystemConsructorInfo_ForNull ()
    {
      var underlyingCtorInfoDescriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew ();
      Assert.That (underlyingCtorInfoDescriptor.UnderlyingSystemConstructorInfo, Is.Null);

      var ctorInfo = Create (underlyingCtorInfoDescriptor);

      Assert.That (ctorInfo.UnderlyingSystemConstructorInfo, Is.SameAs (ctorInfo));
    }

    [Test]
    public void IsNewConstructor_True ()
    {
      var underlyingCtorInfoDescriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew ();
      Assert.That (underlyingCtorInfoDescriptor.UnderlyingSystemConstructorInfo, Is.Null);

      var ctorInfo = Create (underlyingCtorInfoDescriptor);

      Assert.That (ctorInfo.IsNewConstructor, Is.True);
    }

    [Test]
    public void IsNewConstructor_False ()
    {
      var underlyingCtorInfoDescriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForExisting ();
      Assert.That (underlyingCtorInfoDescriptor.UnderlyingSystemConstructorInfo, Is.Not.Null);

      var ctorInfo = Create (underlyingCtorInfoDescriptor);

      Assert.That (ctorInfo.IsNewConstructor, Is.False);
    }

    [Test]
    public void IsModified_False ()
    {
      var ctorInfo = MutableConstructorInfoObjectMother.Create();
      Assert.That (ctorInfo.IsModified, Is.False);
    }

    [Test]
    public void IsModified_True ()
    {
      var ctorInfo = MutableConstructorInfoObjectMother.Create ();

      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));
      ctorInfo.SetBody (ctx => fakeBody);

      Assert.That (ctorInfo.IsModified, Is.True);
    }

    [Test]
    public void Attributes ()
    {
      Assert.That (_mutableCtor.Attributes, Is.EqualTo (_descriptor.Attributes));
    }

    [Test]
    public void CallingConvention ()
    {
      Assert.That (_mutableCtor.CallingConvention, Is.EqualTo (CallingConventions.HasThis));
    }

    [Test]
    public void Name ()
    {
      Assert.That (_mutableCtor.Name, Is.EqualTo (".ctor"));
    }

    [Test]
    public void ParameterExpressions ()
    {
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      var ctorInfo = CreateWithParameters (parameterDeclarations);

      Assert.That (ctorInfo.ParameterExpressions, Is.EqualTo (parameterDeclarations.Select (pd => pd.Expression)));
    }

    [Test]
    public void Body ()
    {
      Assert.That (_mutableCtor.Body, Is.SameAs (_descriptor.Body));
    }

    [Test]
    public void SetBody ()
    {
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (object));
      Func<ConstructorBodyModificationContext, Expression> bodyProvider = context =>
      {
        Assert.That (context.Parameters, Is.EqualTo (_mutableCtor.ParameterExpressions));
        Assert.That (context.This.Type, Is.SameAs (_declaringType));

        var previousBody = context.GetPreviousBody (context.Parameters.Cast<Expression>());
        Assert.That (previousBody, Is.SameAs (_mutableCtor.Body));

        return fakeBody;
      };

      _mutableCtor.SetBody (bodyProvider);

      var expectedBody = Expression.Block (typeof (void), fakeBody);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, _mutableCtor.Body);
    }

    [Test]
    public new void ToString ()
    {
      var ctorInfo = CreateWithParameters ();
      Assert.That (ctorInfo.ToString (), Is.EqualTo ("System.Void .ctor()"));
    }

    [Test]
    public void ToString_WithParameters ()
    {
      var ctorInfo = CreateWithParameters (
          ParameterDeclarationObjectMother.Create (typeof (int), "p1"),
          ParameterDeclarationObjectMother.Create (typeof (string).MakeByRefType(), "p2", ParameterAttributes.Out));

      Assert.That (ctorInfo.ToString (), Is.EqualTo ("System.Void .ctor(System.Int32, System.String&)"));
    }

    [Test]
    public void ToDebugString ()
    {
      var declaringType = MutableTypeObjectMother.CreateForExistingType (GetType());
      var ctorInfo = MutableConstructorInfoObjectMother.CreateForNewWithParameters (
          declaringType,
          ParameterDeclarationObjectMother.Create (typeof (int), "p1"));

      var expected = "MutableConstructor = \"System.Void .ctor(System.Int32)\", DeclaringType = \"MutableConstructorInfoTest\"";
      Assert.That (ctorInfo.ToDebugString (), Is.EqualTo (expected));
    }

    [Test]
    public void GetParameters ()
    {
      var paramDecl1 = ParameterDeclarationObjectMother.Create();
      var paramDecl2 = ParameterDeclarationObjectMother.Create();
      var ctorInfo = CreateWithParameters (paramDecl1, paramDecl2);

      var result = ctorInfo.GetParameters();

      var expectedParameterInfos =
          new[]
          {
              new { Member = (MemberInfo) ctorInfo, Position = 0, ParameterType = paramDecl1.Type, paramDecl1.Name, paramDecl1.Attributes },
              new { Member = (MemberInfo) ctorInfo, Position = 1, ParameterType = paramDecl2.Type, paramDecl2.Name, paramDecl2.Attributes },
          };
      var actualParameterInfos = result.Select (pi => new { pi.Member, pi.Position, pi.ParameterType, pi.Name, pi.Attributes }).ToArray();
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
    }

    [Test]
    public void GetParameters_ReturnsSameParameterInfoInstances()
    {
      var ctorInfo = CreateWithParameters (ParameterDeclarationObjectMother.Create());

      var result1 = ctorInfo.GetParameters().Single();
      var result2 = ctorInfo.GetParameters().Single();

      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    public void GetParameters_DoesNotAllowModificationOfInternalList ()
    {
      var ctorInfo = CreateWithParameters (ParameterDeclarationObjectMother.Create ());

      var parameters = ctorInfo.GetParameters ();
      Assert.That (parameters[0], Is.Not.Null);
      parameters[0] = null;

      var parametersAgain = ctorInfo.GetParameters ();
      Assert.That (parametersAgain[0], Is.Not.Null);
    }

    private MutableConstructorInfo Create (UnderlyingConstructorInfoDescriptor underlyingConstructorInfoDescriptor)
    {
      return new MutableConstructorInfo (_declaringType, underlyingConstructorInfoDescriptor);
    }

    private MutableConstructorInfo CreateWithParameters (params ParameterDeclaration[] parameterDeclarations)
    {
      return Create (UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew (parameterDeclarations: parameterDeclarations));
    }
  }
}