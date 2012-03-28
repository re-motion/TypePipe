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
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableConstructorInfoTest
  {
    private MutableType _declaringType;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = MutableTypeObjectMother.Create();
    }

    [Test]
    public void Initialization ()
    {
      var underlyingCtorInfoDescriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew ();

      var ctorInfo = Create (underlyingCtorInfoDescriptor);

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
    public void Attributes ()
    {
      var underlyingCtorInfoDescriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew (attributes: MethodAttributes.Abstract);

      var ctorInfo = Create (underlyingCtorInfoDescriptor);

      Assert.That (ctorInfo.Attributes, Is.EqualTo (MethodAttributes.Abstract));
    }

    [Test]
    public void Name ()
    {
      var underlyingCtorInfoDescriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew();

      var ctorInfo = Create (underlyingCtorInfoDescriptor);

      Assert.That (ctorInfo.Name, Is.EqualTo (".ctor"));
    }

    [Test]
    public void ParameterExpressions ()
    {
      var parameterDeclarations = ParameterDeclarationObjectMother.CreateMultiple (2);
      var underlyingCtorInfoDescriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew (parameterDeclarations: parameterDeclarations);

      var ctorInfo = Create (underlyingCtorInfoDescriptor);

      Assert.That (ctorInfo.ParameterExpressions, Is.EqualTo (parameterDeclarations.Select (pd => pd.Expression)));
    }

    [Test]
    public void Body ()
    {
      var underlyingCtorInfoDescriptor = UnderlyingConstructorInfoDescriptorObjectMother.CreateForNew ();

      var ctorInfo = Create (underlyingCtorInfoDescriptor);

      Assert.That (ctorInfo.Body, Is.SameAs (underlyingCtorInfoDescriptor.Body));
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