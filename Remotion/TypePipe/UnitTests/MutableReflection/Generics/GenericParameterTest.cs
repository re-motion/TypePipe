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
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.MutableReflection.Implementation;
using Rhino.Mocks;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class GenericParameterTest
  {
    private GenericParameter _parameter;

    private string _name;
    private GenericParameterAttributes _genericParameterAttributes;
    private Type _baseTypeConstraint;
    private Type _interfaceConstraint;
    private IMemberSelector _memberSelectorMock;

    [SetUp]
    public void SetUp ()
    {
      _memberSelectorMock = MockRepository.GenerateStrictMock<IMemberSelector>();
      _name = "parameter";
      _genericParameterAttributes = (GenericParameterAttributes) 7;
      _baseTypeConstraint = ReflectionObjectMother.GetSomeType();
      _interfaceConstraint = ReflectionObjectMother.GetSomeInterfaceType();

      _parameter = new GenericParameter (
          _memberSelectorMock, _name, _genericParameterAttributes, _baseTypeConstraint, new[] { _interfaceConstraint }.AsOneTime());
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_parameter.Name, Is.EqualTo (_name));
      Assert.That (_parameter.GenericParameterAttributes, Is.EqualTo (_genericParameterAttributes));
      Assert.That (_parameter.BaseType, Is.SameAs (_baseTypeConstraint));
      Assert.That (_parameter.GetInterfaces(), Is.EqualTo (new[] { _interfaceConstraint }));
    }

    [Test]
    public void IsGenericParameter ()
    {
      Assert.That (_parameter.IsGenericParameter, Is.True);
    }

    [Test]
    public void GetGenericParameterConstraints ()
    {
      var result = _parameter.GetGenericParameterConstraints();

      Assert.That (result, Is.EqualTo (new[] { _baseTypeConstraint, _interfaceConstraint }));
    }

    [Test]
    public void GetGenericParameterConstraints_NoBaseTypeConstraint ()
    {
      var baseTypeConstraint = typeof (object);
      var parameter = new GenericParameter (
          _memberSelectorMock, _name, _genericParameterAttributes, baseTypeConstraint, new[] { _interfaceConstraint });

      var result = parameter.GetGenericParameterConstraints();

      Assert.That (result, Is.EqualTo (new[] { _interfaceConstraint }));
    }
  }
}