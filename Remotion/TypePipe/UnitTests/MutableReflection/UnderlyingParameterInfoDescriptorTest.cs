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
using Remotion.Development.UnitTesting.Reflection;
using System.Linq;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class UnderlyingParameterInfoDescriptorTest
  {
    [Test]
    public void Create_ForNew ()
    {
      var type = ReflectionObjectMother.GetSomeType();
      var name = "parameterName";
      var attributes = ParameterAttributes.Optional |  ParameterAttributes.In;

      var descriptor = UnderlyingParameterInfoDescriptor.Create (type, name, attributes);

      Assert.That (descriptor.UnderlyingSystemParameterInfo, Is.Null);
      Assert.That (descriptor.Type, Is.SameAs (type));
      Assert.That (descriptor.Name, Is.EqualTo (name));
      Assert.That (descriptor.Attributes, Is.EqualTo (attributes));
    }

    [Test]
    public void Create_ForExisting ()
    {
      string s;
      var originalParameter = NormalizingMemberInfoFromExpressionUtility.GetMethod (() => Method (out s)).GetParameters().Single();

      var descriptor = UnderlyingParameterInfoDescriptor.Create (originalParameter);

      Assert.That (descriptor.UnderlyingSystemParameterInfo, Is.SameAs (originalParameter));
      Assert.That (descriptor.Type, Is.SameAs (typeof(string).MakeByRefType()));
      Assert.That (descriptor.Name, Is.EqualTo ("parameterName"));
      Assert.That (descriptor.Attributes, Is.EqualTo (ParameterAttributes.Out));
    }

    void Method (out string parameterName) { parameterName = ""; }
  }
}