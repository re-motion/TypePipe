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
using System.Reflection;
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection;
using System.Linq;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableConstructorInfoTest
  {
    private MutableType _declaringTypeStub;
    private IUnderlyingConstructorInfoStrategy _ctorInfoStrategyStub;
    private MutableConstructorInfo _ctorInfo;

    [SetUp]
    public void SetUp ()
    {
      _declaringTypeStub = MutableTypeObjectMother.Create();
      _ctorInfoStrategyStub = MockRepository.GenerateStub<IUnderlyingConstructorInfoStrategy>();
      _ctorInfoStrategyStub.Stub (stub => stub.GetParameterDeclarations()).Return (Enumerable.Empty<ParameterDeclaration>()).Repeat.Once();
      _ctorInfo = new MutableConstructorInfo(_declaringTypeStub, _ctorInfoStrategyStub);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_ctorInfo.DeclaringType, Is.SameAs (_declaringTypeStub));
    }

    [Test]
    public void UnderlyingSystemConsructorInfo ()
    {
      var ctorInfo = ReflectionObjectMother.GetSomeDefaultConstructor();
      _ctorInfoStrategyStub.Stub (stub => stub.GetUnderlyingSystemConstructorInfo()).Return (ctorInfo);

      Assert.That (_ctorInfo.UnderlyingSystemConstructorInfo, Is.SameAs (ctorInfo));
    }

    [Test]
    public void UnderlyingSystemConsructorInfo_ForNull ()
    {
      _ctorInfoStrategyStub.Stub (stub => stub.GetUnderlyingSystemConstructorInfo()).Return(null);

      Assert.That (_ctorInfo.UnderlyingSystemConstructorInfo, Is.SameAs (_ctorInfo));
    }

    [Test]
    public void Attributes ()
    {
      var attributes = MethodAttributes.Abstract;
      _ctorInfoStrategyStub.Stub (stub => stub.GetAttributes()).Return (attributes);

      Assert.That (_ctorInfo.Attributes, Is.EqualTo (attributes));
    }

    [Test]
    public void GetParameters ()
    {
      var paramDecl1 = ParameterDeclarationObjectMother.Create();
      var paramDecl2 = ParameterDeclarationObjectMother.Create();
      var ctorInfo = MutableConstructorInfoObjectMother.CreateWithParameters (paramDecl1, paramDecl2);

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
      var ctorInfo = MutableConstructorInfoObjectMother.CreateWithParameters (ParameterDeclarationObjectMother.Create());

      var result1 = ctorInfo.GetParameters().Single();
      var result2 = ctorInfo.GetParameters().Single();

      Assert.That (result1, Is.SameAs (result2));
    }

    [Test]
    public void GetParameters_DoesNotAllowModificationOfInternalList ()
    {
      var ctorInfo = MutableConstructorInfoObjectMother.CreateWithParameters (ParameterDeclarationObjectMother.Create ());

      var parameters = ctorInfo.GetParameters ();
      Assert.That (parameters[0], Is.Not.Null);
      parameters[0] = null;

      var parametersAgain = ctorInfo.GetParameters ();
      Assert.That (parametersAgain[0], Is.Not.Null);
    }
  }
}