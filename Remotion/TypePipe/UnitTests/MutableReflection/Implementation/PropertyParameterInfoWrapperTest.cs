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
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class PropertyParameterInfoWrapperTest
  {
    [Test]
    public void Initialization ()
    {
      var parameter = ReflectionObjectMother.GetSomeParameter();
      var member = ReflectionObjectMother.GetSomeProperty();

      var wrapper = new PropertyParameterInfoWrapper (member, parameter);

      Assert.That (wrapper.Member, Is.SameAs (member));
      Assert.That (wrapper.Position, Is.EqualTo (parameter.Position));
      Assert.That (wrapper.Name, Is.EqualTo (parameter.Name));
      Assert.That (wrapper.ParameterType, Is.SameAs (parameter.ParameterType));
      Assert.That (wrapper.Attributes, Is.EqualTo (parameter.Attributes));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var member = ReflectionObjectMother.GetSomeProperty();
      var customAttribute = CustomAttributeDeclarationObjectMother.Create();
      var parameter = CustomParameterInfoObjectMother.Create (customAttributes: new ICustomAttributeData[] { customAttribute });

      var wrapper = new PropertyParameterInfoWrapper (member, parameter);

      Assert.That (wrapper.GetCustomAttributeData(), Is.EqualTo (new[] { customAttribute }));
    }
  }
}