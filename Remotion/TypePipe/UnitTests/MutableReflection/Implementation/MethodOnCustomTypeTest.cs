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
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [TestFixture]
  public class MethodOnCustomTypeTest
  {
    private CustomType _declaringType;
    private string _name;
    private MethodAttributes _attributes;
    private IEnumerable<Type> _typeArguments;
    private Type _returnType;
    private IEnumerable<ParameterOnCustomMember> _parameters;

    private MethodOnCustomType _method;

    [SetUp]
    public void SetUp ()
    {
      _declaringType = CustomTypeObjectMother.Create ();
      _name = "Method";
      _attributes = (MethodAttributes) 7;
      _typeArguments = new[] { ReflectionObjectMother.GetSomeType() };
      _returnType = ReflectionObjectMother.GetSomeType();
      _parameters = new[] { ParameterOnCustomMemberObjectMother.Create() };

      _method = new MethodOnCustomType (_declaringType, _name, _attributes, _typeArguments, _returnType, _parameters);
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_method.DeclaringType, Is.SameAs (_declaringType));
      Assert.That (_method.Name, Is.EqualTo (_name));
      Assert.That (_method.Attributes, Is.EqualTo (_attributes));
      Assert.That (_method.GetGenericArguments(), Is.EqualTo (_typeArguments));
      CustomParameterInfoTest.CheckParameter (_method.ReturnParameter, _method, -1, null, _returnType, ParameterAttributes.None);
      Assert.That (_method.GetParameters(), Is.EqualTo (_parameters));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      Assert.That (_method.GetCustomAttributeData (), Is.Empty);
    }
  }
}