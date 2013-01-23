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
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableConstructorInfoTest
  {
    private MutableConstructorInfo _constructor;

    [SetUp]
    public void SetUp ()
    {
      _constructor = MutableConstructorInfoObjectMother.Create (parameters: ParameterDeclarationObjectMother.CreateMultiple (2));
    }

    [Test]
    public void Initialization ()
    {
      var declaringType = ProxyTypeObjectMother.Create();
      var attributes = (MethodAttributes) 7;
      var parameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));

      var ctor = new MutableConstructorInfo (declaringType, attributes, parameters.AsOneTime(), body);

      Assert.That (ctor.DeclaringType, Is.SameAs (declaringType));
      Assert.That (ctor.Name, Is.EqualTo (".ctor"));

      var actualParameters = ctor.GetParameters();
      Assert.That (actualParameters, Has.Length.EqualTo (2));
      MutableParameterInfoTest.CheckParameter (actualParameters[0], ctor, 0, parameters[0].Name, parameters[0].Type, parameters[0].Attributes);
      MutableParameterInfoTest.CheckParameter (actualParameters[1], ctor, 1, parameters[1].Name, parameters[1].Type, parameters[1].Attributes);
      Assert.That (ctor.MutableParameters, Is.EqualTo (actualParameters));

      var paramExpressions = ctor.ParameterExpressions;
      Assert.That (paramExpressions, Has.Count.EqualTo (2));
      Assert.That (paramExpressions[0], Has.Property ("Name").EqualTo (parameters[0].Name).And.Property ("Type").SameAs (parameters[0].Type));
      Assert.That (paramExpressions[1], Has.Property ("Name").EqualTo (parameters[1].Name).And.Property ("Type").SameAs (parameters[1].Type));

      Assert.That (ctor.Body, Is.SameAs (body));
    }

    [Test]
    public void Name ()
    {
      var constructor = MutableConstructorInfoObjectMother.Create (attributes: 0);
      var typeInitializer = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);

      Assert.That (constructor.Name, Is.EqualTo (".ctor"));
      Assert.That (typeInitializer.Name, Is.EqualTo (".cctor"));
    }

    [Test]
    public void CallingConvention ()
    {
      var constructor = MutableConstructorInfoObjectMother.Create (attributes: 0);
      var typeInitializer = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);

      Assert.That (constructor.CallingConvention, Is.EqualTo (CallingConventions.HasThis));
      Assert.That (typeInitializer.CallingConvention, Is.EqualTo (CallingConventions.Standard));
    }

    [Test]
    public void SetBody ()
    {
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (object));
      Func<ConstructorBodyModificationContext, Expression> bodyProvider = context =>
      {
        Assert.That (_constructor.ParameterExpressions, Is.Not.Empty);
        Assert.That (context.Parameters, Is.EqualTo (_constructor.ParameterExpressions));
        Assert.That (context.DeclaringType, Is.SameAs (_constructor.DeclaringType));
        Assert.That (context.IsStatic, Is.False);
        Assert.That (context.PreviousBody, Is.SameAs (_constructor.Body));

        return fakeBody;
      };

      _constructor.SetBody (bodyProvider);

      var expectedBody = Expression.Block (typeof (void), fakeBody);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, _constructor.Body);
    }

    [Test]
    public void SetBody_Static ()
    {
      var typeInitializer = MutableConstructorInfoObjectMother.Create (attributes: MethodAttributes.Static);
      Func<ConstructorBodyModificationContext, Expression> bodyProvider = ctx =>
      {
        Assert.That (ctx.IsStatic, Is.True);
        return Expression.Empty();
      };

      typeInitializer.SetBody (bodyProvider);
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      _constructor.AddCustomAttribute (declaration);

      Assert.That (_constructor.AddedCustomAttributes, Is.EqualTo (new[] { declaration }));

      Assert.That (_constructor.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));

      Assert.That (_constructor.GetCustomAttributes (false).Single(), Is.TypeOf<ObsoleteAttribute>());
      Assert.That (_constructor.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (_constructor.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (_constructor.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public new void ToString ()
    {
      var ctor = MutableConstructorInfoObjectMother.Create (
          parameters:
              new[]
              {
                  new ParameterDeclaration (typeof (int), "p1"),
                  new ParameterDeclaration (typeof (string).MakeByRefType(), "p2", attributes: ParameterAttributes.Out)
              });

      Assert.That (ctor.ToString(), Is.EqualTo ("Void .ctor(Int32, String&)"));
    }

    [Test]
    public void ToDebugString ()
    {
      var declaringType = ProxyTypeObjectMother.Create (name: "Abc");
      var ctor = MutableConstructorInfoObjectMother.Create (declaringType, parameters: new[] { new ParameterDeclaration (typeof (int), "p1") });

      var expected = "MutableConstructor = \"Void .ctor(Int32)\", DeclaringType = \"Abc\"";
      Assert.That (ctor.ToDebugString(), Is.EqualTo (expected));
    }

    [Test]
    public void UnsupportedMembers ()
    {
      UnsupportedMemberTestHelper.CheckProperty (() => _constructor.MethodHandle, "MethodHandle");
      UnsupportedMemberTestHelper.CheckProperty (() => _constructor.ReflectedType, "ReflectedType");

      UnsupportedMemberTestHelper.CheckMethod (() => _constructor.Invoke (null, 0, null, null, null), "Invoke");
      UnsupportedMemberTestHelper.CheckMethod (() => _constructor.Invoke (0, null, null, null), "Invoke");
    }
  }
}