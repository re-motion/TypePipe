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
    [Test]
    public void Initialization ()
    {
      var declaringType = ReflectionObjectMother.GetSomeType ();
      var methodAttributes = MethodAttributes.Public;
      var parameter1 = ParameterDeclarationObjectMother.Create ();
      var parameter2 = ParameterDeclarationObjectMother.Create ();

      var ctorInfo = new MutableConstructorInfo (declaringType, methodAttributes, new[] { parameter1, parameter2 });

      Assert.That (ctorInfo.DeclaringType, Is.SameAs (declaringType));
      Assert.That (ctorInfo.Attributes, Is.EqualTo (methodAttributes));
      var expectedParameterInfos =
          new[]
          {
              new { Member = (MemberInfo) ctorInfo, Position = 0, ParameterType = parameter1.Type, parameter1.Name, parameter1.Attributes },
              new { Member = (MemberInfo) ctorInfo, Position = 1, ParameterType = parameter2.Type, parameter2.Name, parameter2.Attributes },
          };
      var actualParameterInfos = ctorInfo.GetParameters ().Select (pi => new { pi.Member, pi.Position, pi.ParameterType, pi.Name, pi.Attributes });
      Assert.That (actualParameterInfos, Is.EqualTo (expectedParameterInfos));
    }

    [Test]
    public void GetParameters_DoesNotAllowModificationOfInternalList ()
    {
      var ctorInfo = MutableConstructorInfoObjectMother.Create (parameterDeclarations: new[] { ParameterDeclarationObjectMother.Create() });

      var parameters = ctorInfo.GetParameters();
      Assert.That (parameters[0], Is.Not.Null);
      parameters[0] = null;

      var parametersAgain = ctorInfo.GetParameters();
      Assert.That (parametersAgain[0], Is.Not.Null);
    }
  }
}