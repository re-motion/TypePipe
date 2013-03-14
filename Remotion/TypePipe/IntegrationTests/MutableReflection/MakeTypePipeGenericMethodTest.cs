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
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.TypePipe.IntegrationTests.MutableReflection
{
  [TestFixture]
  public class MakeTypePipeGenericMethodTest
  {
    private MethodInfo _genericMethodDefinition;
    private MutableType _typeArg1;
    private MutableType _typeArg2;

    private MethodInfo _instantiation;

    [SetUp]
    public void SetUp ()
    {
      _genericMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => GenericMethod<Dev.T, Dev.T> (null));
      _typeArg1 = MutableTypeObjectMother.Create (typeof (object));
      _typeArg2 = MutableTypeObjectMother.Create (typeof (Exception));

      _instantiation = _genericMethodDefinition.MakeTypePipeGenericMethod (_typeArg1, _typeArg2);
    }

    [Test]
    public void MethodInstantiation ()
    {
      Assert.That (_instantiation, Is.TypeOf<MethodInstantiation>());
      Assert.That (_instantiation.GetGenericMethodDefinition(), Is.SameAs (_genericMethodDefinition));
      Assert.That (_instantiation.GetGenericArguments(), Is.EqualTo (new[] { _typeArg1, _typeArg2 }));
    }

    [Test]
    public void Names ()
    {
      Assert.That (_instantiation.Name, Is.EqualTo ("GenericMethod"));
      Assert.That (_instantiation.ToString(), Is.EqualTo ("Object_Proxy1 GenericMethod[Object_Proxy1,Exception_Proxy1](Exception_Proxy1)"));
    }

    [Test]
    public void ReturnParameter ()
    {
      var returnParameter = _instantiation.ReturnParameter;
      Assertion.IsNotNull (returnParameter);

      Assert.That (returnParameter, Is.TypeOf<MemberParameterOnInstantiation>());
      Assert.That (
          returnParameter.As<MemberParameterOnInstantiation>().MemberParameterOnGenericDefinition,
          Is.EqualTo (_genericMethodDefinition.ReturnParameter));
      Assert.That (returnParameter.ParameterType, Is.SameAs (_typeArg1));
    }

    [Test]
    public void Parameters ()
    {
      var parameter = _instantiation.GetParameters().Single();

      Assert.That (parameter, Is.TypeOf<MemberParameterOnInstantiation>());
      Assert.That (
          parameter.As<MemberParameterOnInstantiation>().MemberParameterOnGenericDefinition,
          Is.EqualTo (_genericMethodDefinition.GetParameters().Single()));
      Assert.That (parameter.ParameterType, Is.SameAs (_typeArg2));
    }

    T1 GenericMethod<T1, T2> (T2 t) { return default (T1); }
  }
}