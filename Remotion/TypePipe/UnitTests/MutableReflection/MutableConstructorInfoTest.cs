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
using Remotion.TypePipe.MutableReflection.Descriptors;
using Remotion.TypePipe.UnitTests.Expressions;
using Remotion.TypePipe.UnitTests.MutableReflection.Descriptors;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableConstructorInfoTest
  {
    private MutableType _declaringType;
    private ConstructorDescriptor _descriptor;

    private MutableConstructorInfo _constructor;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create();
      var parameters = ParameterDescriptorObjectMother.CreateMultiple (2);
      _descriptor = ConstructorDescriptorObjectMother.Create (parameterDescriptors: parameters);

      _constructor = Create (_descriptor);
    }

    [Test]
    public void Initialization ()
    {
      var ctor = new MutableConstructorInfo (_declaringType, _descriptor);

      Assert.That (ctor.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (ctor.Name, Is.EqualTo (".ctor"));
      Assert.That (_constructor.Body, Is.SameAs (_descriptor.Body));
    }

    [Test]
    public void UnderlyingSystemConstructorInfo ()
    {
      var underlyingConstructor = ReflectionObjectMother.GetSomeConstructor();
      var constructor = MutableConstructorInfoObjectMother.CreateForExisting (underlyingConstructor);

      Assert.That (constructor.UnderlyingSystemConstructorInfo, Is.SameAs (underlyingConstructor));
    }

    [Test]
    public void UnderlyingSystemConstructorInfo_ForNull ()
    {
      var constructor = MutableConstructorInfoObjectMother.CreateForNew();

      Assert.That (constructor.UnderlyingSystemConstructorInfo, Is.SameAs (constructor));
    }

    [Test]
    public void IsNew ()
    {
      var ctor1 = MutableConstructorInfoObjectMother.CreateForExisting();
      var ctor2 = MutableConstructorInfoObjectMother.CreateForNew();

      Assert.That (ctor1.IsNew, Is.False);
      Assert.That (ctor2.IsNew, Is.True);
    }

    [Test]
    public void IsModified_CustomAttributes ()
    {
      Assert.That (_constructor.IsModified, Is.False);
      _constructor.AddCustomAttribute (CustomAttributeDeclarationObjectMother.Create());

      Assert.That (_constructor.IsModified, Is.True);
    }

    [Test]
    public void IsModified_Body ()
    {
      Assert.That (_constructor.IsModified, Is.False);
      _constructor.SetBody (ctx => Expression.Empty());

      Assert.That (_constructor.IsModified, Is.True);
    }

    [Test]
    public void Name ()
    {
      Assert.That (_constructor.Name, Is.EqualTo (_descriptor.Name));
    }

    [Test]
    public void Attributes ()
    {
      Assert.That (_constructor.Attributes, Is.EqualTo (_descriptor.Attributes));
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
    public void CustomAttributeMethods ()
    {
      var declaration = CustomAttributeDeclarationObjectMother.Create (typeof (ObsoleteAttribute));
      Assert.That (_constructor.CanAddCustomAttributes, Is.True);
      _constructor.AddCustomAttribute (declaration);

      Assert.That (_constructor.AddedCustomAttributeDeclarations, Is.EqualTo (new[] { declaration }));

      Assert.That (_constructor.GetCustomAttributeData().Select (a => a.Type), Is.EquivalentTo (new[] { typeof (ObsoleteAttribute) }));
      // TODO Review

      Assert.That (_constructor.GetCustomAttributes (false).Single(), Is.TypeOf<ObsoleteAttribute>());
      Assert.That (_constructor.GetCustomAttributes (typeof (NonSerializedAttribute), false), Is.Empty);

      Assert.That (_constructor.IsDefined (typeof (ObsoleteAttribute), false), Is.True);
      Assert.That (_constructor.IsDefined (typeof (NonSerializedAttribute), false), Is.False);
    }

    [Test]
    public void ParameterExpressions ()
    {
      var parameterDeclarations = ParameterDescriptorObjectMother.CreateMultiple (2);
      var ctorInfo = CreateWithParameters (parameterDeclarations);

      Assert.That (ctorInfo.ParameterExpressions, Is.EqualTo (parameterDeclarations.Select (pd => pd.Expression)));
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
      var parameters = ParameterDescriptorObjectMother.CreateMultiple (2);
      var ctorInfo = CreateWithParameters (parameters);

      var result = ctorInfo.GetParameters();

      var actualParameterInfos = result.Select (pi => new { pi.Member, pi.Position, pi.ParameterType, pi.Name, pi.Attributes }).ToArray ();
      var expectedParameterInfos =
          new[]
          {
              new { Member = (MemberInfo) ctorInfo, Position = 0, ParameterType = parameters[0].Type, parameters[0].Name, parameters[0].Attributes },
              new { Member = (MemberInfo) ctorInfo, Position = 1, ParameterType = parameters[1].Type, parameters[1].Name, parameters[1].Attributes }
          };
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
    }

    [Test]
    public void GetParameters_ReturnsSameParameterInfoInstances()
    {
      var ctorInfo = CreateWithParameters (ParameterDescriptorObjectMother.CreateForNew());

      var result1 = ctorInfo.GetParameters().Single();
      var result2 = ctorInfo.GetParameters().Single();

      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    public void GetParameters_DoesNotAllowModificationOfInternalList ()
    {
      var ctorInfo = CreateWithParameters (ParameterDescriptorObjectMother.CreateForNew ());

      var parameters = ctorInfo.GetParameters ();
      Assert.That (parameters[0], Is.Not.Null);
      parameters[0] = null;

      var parametersAgain = ctorInfo.GetParameters ();
      Assert.That (parametersAgain[0], Is.Not.Null);
    }

    [Test]
    public void ToString_WithParameters ()
    {
      var ctorInfo = CreateWithParameters (
          ParameterDescriptorObjectMother.CreateForNew (typeof (int), "p1"),
          ParameterDescriptorObjectMother.CreateForNew (typeof (string).MakeByRefType (), "p2", attributes: ParameterAttributes.Out));

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

    private MutableConstructorInfo Create (ConstructorDescriptor constructorDescriptor)
    {
      return new MutableConstructorInfo (_declaringType, constructorDescriptor);
    }

    private MutableConstructorInfo CreateWithParameters (params ParameterDescriptor[] parameterDescriptors)
    {
      return Create (ConstructorDescriptorObjectMother.CreateForNew (parameterDescriptors: parameterDescriptors));
    }

    class DomainType
    {
      public DomainType () { }
      internal DomainType (int i) { Dev.Null = i; }
    }
  }
}