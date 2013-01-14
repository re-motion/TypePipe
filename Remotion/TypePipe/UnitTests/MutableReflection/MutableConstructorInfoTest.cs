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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using System.Linq;
using Remotion.TypePipe.MutableReflection.BodyBuilding;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableConstructorInfoTest
  {
    private MutableType _declaringType;

    private MutableConstructorInfo _constructor;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create();

      _constructor = MutableConstructorInfoObjectMother.Create();
    }

    [Test]
    public void Initialization ()
    {
      var declaringType = MutableTypeObjectMother.Create();
      var attributes = (MethodAttributes) 7;
      var parameters = ParameterDeclarationObjectMother.CreateMultiple (2);
      var body = ExpressionTreeObjectMother.GetSomeExpression (typeof (void));

      var ctor = new MutableConstructorInfo (_declaringType, attributes, parameters, body);

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
    public void CallingConvention ()
    {
      var constructor = Create (ConstructorDescriptorObjectMother.CreateForNew (attributes: 0));
      var typeInitializer = Create (ConstructorDescriptorObjectMother.CreateForNew (MethodAttributes.Static));

      Assert.That (constructor.CallingConvention, Is.EqualTo (CallingConventions.HasThis));
      Assert.That (typeInitializer.CallingConvention, Is.EqualTo (CallingConventions.Standard));
    }

    [Test]
    public void CanAddCustomAttributes ()
    {
      var ctor1 = MutableConstructorInfoObjectMother.CreateForExisting();
      var ctor2 = MutableConstructorInfoObjectMother.CreateForNew();

      Assert.That (ctor1.CanAddCustomAttributes, Is.True);
      Assert.That (ctor2.CanAddCustomAttributes, Is.True);
    }

    [Test]
    public void MutableParameters ()
    {
      var decl1 = ParameterDeclarationObjectMother.Create (typeof (int), "p1");
      var decl2 = ParameterDeclarationObjectMother.Create (typeof (string).MakeByRefType(), "p2", attributes: ParameterAttributes.Out);
      var method = CreateWithParameters (decl1, decl2);

      var result = method.MutableParameters;

      var actualParameter = result.Select (pi => new { pi.Member, pi.Position, pi.ParameterType, pi.Name, pi.Attributes }).ToArray();
      var expectedParameter =
          new[]
          {
              new { Member = (MemberInfo) method, Position = 0, ParameterType = decl1.Type, decl1.Name, decl1.Attributes },
              new { Member = (MemberInfo) method, Position = 1, ParameterType = decl2.Type, decl2.Name, decl2.Attributes }
          };
      Assert.That (actualParameter, Is.EqualTo (expectedParameter));
    }

    [Test]
    public void ParameterExpressions ()
    {
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      var descriptor = ConstructorDescriptorObjectMother.Create (parameterDeclarations: parameterDeclarations);
      var ctor = Create (descriptor);

      Assert.That (ctor.ParameterExpressions, Is.EqualTo (descriptor.Parameters.Select (pd => pd.Expression)));
    }

    [Test]
    public void CanSetBody ()
    {
      var newInaccessibleCtor = Create (ConstructorDescriptorObjectMother.CreateForNew (attributes: MethodAttributes.Assembly));
      var newAccessibleCtor = Create (ConstructorDescriptorObjectMother.CreateForNew (attributes: MethodAttributes.Family));

      var existingInaccesibleCtor = Create (ConstructorDescriptorObjectMother.CreateForExisting (
          NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType (7))));
      var existingAccessibleCtor = Create (ConstructorDescriptorObjectMother.CreateForExisting (
          NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType ())));
      Assert.That (existingInaccesibleCtor.IsPublic, Is.False);

      Assert.That (newInaccessibleCtor.CanSetBody, Is.True);
      Assert.That (newAccessibleCtor.CanSetBody, Is.True);
      Assert.That (existingInaccesibleCtor.CanSetBody, Is.False);
      Assert.That (existingAccessibleCtor.CanSetBody, Is.True);
    }

    [Test]
    public void SetBody ()
    {
      var fakeBody = ExpressionTreeObjectMother.GetSomeExpression (typeof (object));
      Func<ConstructorBodyModificationContext, Expression> bodyProvider = context =>
      {
        Assert.That (_constructor.ParameterExpressions, Is.Not.Empty);
        Assert.That (context.Parameters, Is.EqualTo (_constructor.ParameterExpressions));
        Assert.That (context.DeclaringType, Is.SameAs (_declaringType));
        Assert.That (context.IsStatic, Is.False);
        Assert.That (context.PreviousBody, Is.SameAs (_constructor.Body));

        return fakeBody;
      };

      _constructor.SetBody (bodyProvider);

      var expectedBody = Expression.Block (typeof (void), fakeBody);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedBody, _constructor.Body);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The body of the existing inaccessible constructor 'Void .ctor(Int32)' cannot be replaced.")]
    public void SetBody_NonSettableCtor ()
    {
      var inaccessibleCtor = NormalizingMemberInfoFromExpressionUtility.GetConstructor (() => new DomainType (7));
      var descriptor = ConstructorDescriptorObjectMother.CreateForExisting (inaccessibleCtor);
      var mutableCtor = Create (descriptor);

      Func<ConstructorBodyModificationContext, Expression> bodyProvider = context =>
      {
        Assert.Fail ("Should not be called.");
        throw new NotImplementedException ();
      };

      mutableCtor.SetBody (bodyProvider);
    }

    [Test]
    public void GetParameters ()
    {
      var ctor = CreateWithParameters (ParameterDeclarationObjectMother.CreateMultiple (2));

      var result = ctor.GetParameters();

      Assert.That (result, Is.EqualTo (ctor.MutableParameters));
      Assert.That (ctor.GetParameters()[0], Is.SameAs (result[0]), "should return same instances");
    }

    [Test]
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      Assert.That (_constructor.CanAddCustomAttributes, Is.True);
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
      var ctorInfo = CreateWithParameters (
          ParameterDeclarationObjectMother.Create (typeof (int), "p1"),
          ParameterDeclarationObjectMother.Create (typeof (string).MakeByRefType (), "p2", attributes: ParameterAttributes.Out));

      Assert.That (ctorInfo.ToString (), Is.EqualTo ("Void .ctor(Int32, String&)"));
    }

    [Test]
    public void ToDebugString ()
    {
      var declaringType = MutableTypeObjectMother.CreateForExisting (GetType ());
      var ctorInfo = MutableConstructorInfoObjectMother.CreateForNewWithParameters (declaringType, new ParameterDeclaration (typeof (int), "p1"));

      var expected = "MutableConstructor = \"Void .ctor(Int32)\", DeclaringType = \"MutableConstructorInfoTest\"";
      Assert.That (ctorInfo.ToDebugString (), Is.EqualTo (expected));
    }

    [Test]
    public void UnsupportedMembers ()
    {
      UnsupportedMemberTestHelper.CheckProperty (() => _constructor.MethodHandle, "MethodHandle");
      UnsupportedMemberTestHelper.CheckProperty (() => _constructor.ReflectedType, "ReflectedType");

      UnsupportedMemberTestHelper.CheckMethod (() => _constructor.Invoke (null, 0, null, null, null), "Invoke");
      UnsupportedMemberTestHelper.CheckMethod (() => _constructor.Invoke (0, null, null, null), "Invoke");
    }

    class DomainType
    {
      public DomainType () { }
      internal DomainType (int i) { Dev.Null = i; }
    }
  }
}