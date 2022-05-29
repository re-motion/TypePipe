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
using Remotion.Development.UnitTesting.Enumerables;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Development.UnitTesting.ObjectMothers.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.NUnit;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class MethodInstantiationInfoTest
  {
    private MethodInfo _genericMethodDefinition;

    private Type _customType;
    private Type _runtimeType;

    private MethodInstantiationInfo _infoWithCustomType;
    private MethodInstantiationInfo _infoWithRuntimeType;

    [SetUp]
    public void SetUp ()
    {
      _genericMethodDefinition = NormalizingMemberInfoFromExpressionUtility.GetGenericMethodDefinition (() => GenericMethod<Dev.T> (null));

      _customType = CustomTypeObjectMother.Create();
      _runtimeType = ReflectionObjectMother.GetSomeType();

      _infoWithCustomType = new MethodInstantiationInfo (_genericMethodDefinition, new[] { _customType }.AsOneTime());
      _infoWithRuntimeType = new MethodInstantiationInfo (_genericMethodDefinition, new[] { _runtimeType });
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_infoWithCustomType.GenericMethodDefinition, Is.SameAs (_genericMethodDefinition));
      Assert.That (_infoWithCustomType.TypeArguments, Is.EqualTo (new[] { _customType }));
    }

    [Test]
    public void Initialization_NoGenericMethodDefinition ()
    {
      var method = ReflectionObjectMother.GetSomeNonGenericMethod();
      Assert.That (
          () => Dev.Null = new MethodInstantiationInfo (method, Type.EmptyTypes),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo ("Specified method must be a generic method definition.", "genericMethodDefinition"));
    }

    [Test]
    public void Initialization_NonMatchingGenericArgumentCount ()
    {
      Assert.That (
          () => Dev.Null = new MethodInstantiationInfo (_genericMethodDefinition, Type.EmptyTypes),
          Throws.ArgumentException
              .With.ArgumentExceptionMessageEqualTo (
                  "Generic parameter count of the generic method definition does not match the number of supplied type arguments.", "typeArguments"));
    }

    [Test]
    public void Instantiate_CustomTypeArgument ()
    {
      var result = _infoWithCustomType.Instantiate();

      Assert.That (result, Is.TypeOf<MethodInstantiation>());
      Assert.That (result.GetGenericMethodDefinition(), Is.EqualTo (_infoWithCustomType.GenericMethodDefinition));
      Assert.That (result.GetGenericArguments (), Is.EqualTo (_infoWithCustomType.TypeArguments));
    }

    [Test]
    public void Instantiate_CustomGenericMethodDefinition ()
    {
      var typeParameter = ReflectionObjectMother.GetSomeGenericParameter();
      var customGenericMethodDefinition = CustomMethodInfoObjectMother.Create (typeArguments: new[] { typeParameter });
      var instantiationInfo = new MethodInstantiationInfo (customGenericMethodDefinition, new[] { _runtimeType });

      var result = instantiationInfo.Instantiate();

      Assert.That (result, Is.TypeOf<MethodInstantiation>());
      Assert.That (result.GetGenericMethodDefinition(), Is.EqualTo (instantiationInfo.GenericMethodDefinition));
      Assert.That (result.GetGenericArguments(), Is.EqualTo (instantiationInfo.TypeArguments));
    }

    [Test]
    public void Instantiate_RuntimeTypeArgument ()
    {
      var result = _infoWithRuntimeType.Instantiate();

      Assert.That (result.GetType().FullName, Is.EqualTo ("System.Reflection.RuntimeMethodInfo"));
      Assert.That (result.GetGenericMethodDefinition(), Is.EqualTo (_infoWithRuntimeType.GenericMethodDefinition));
      Assert.That (result.GetGenericArguments(), Is.EqualTo (_infoWithRuntimeType.TypeArguments));
    }

    void GenericMethod<T> (T t) {}
  }
}