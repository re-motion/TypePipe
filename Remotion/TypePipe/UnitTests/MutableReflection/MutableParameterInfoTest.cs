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

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class MutableParameterInfoTest
  {
    [Test]
    public void CreateFromDescriptor ()
    {
      var member = ReflectionObjectMother.GetSomeMember();
      var position = 17;
      var declaration = new ParameterDeclaration (ReflectionObjectMother.GetSomeType (), "foo", ParameterAttributes.Out);
      var descriptor = UnderlyingParameterInfoDescriptor.Create (declaration);

      var result = MutableParameterInfo.CreateFromDescriptor (member, position, descriptor);

      Assert.That (result.Member, Is.SameAs (member));
      Assert.That (result.Position, Is.EqualTo (position));
      Assert.That (result.ParameterType, Is.SameAs (declaration.Type));
      Assert.That (result.Name, Is.EqualTo (declaration.Name));
      Assert.That (result.Attributes, Is.EqualTo (declaration.Attributes));
    }

    [Test]
    public void Initialization ()
    {
      var member = ReflectionObjectMother.GetSomeMember ();
      var position = 4711;
      var parameterType = ReflectionObjectMother.GetSomeType ();
      var name = "parameterName";
      var attributes = ParameterAttributes.Out;

      var futureParameterInfo = new MutableParameterInfo (member, position, parameterType, name, attributes);

      Assert.That (futureParameterInfo.Member, Is.SameAs (member));
      Assert.That (futureParameterInfo.Position, Is.EqualTo (position));
      Assert.That (futureParameterInfo.ParameterType, Is.SameAs (parameterType));
      Assert.That (futureParameterInfo.Name, Is.EqualTo (name));
      Assert.That (futureParameterInfo.Attributes, Is.EqualTo (attributes));
    }
  }
}