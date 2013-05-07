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
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.CodeGeneration.ReflectionEmit.LambdaCompilation;
using Remotion.TypePipe.UnitTests.MutableReflection;
using Remotion.Development.UnitTesting;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit.LambdaCompilation
{
  [TestFixture]
  public class DelegateTypePlaceholderTest
  {
    private Type _parameterType;
    private Type _returnType;

    private DelegateTypePlaceholder _type;

    [SetUp]
    public void SetUp ()
    {
      _returnType = ReflectionObjectMother.GetSomeType();
      _parameterType = ReflectionObjectMother.GetSomeOtherType();

      _type = new DelegateTypePlaceholder (_returnType, new[] { _parameterType }.AsOneTime());
    }

    [Test]
    public void Initialization ()
    {
      Assert.That (_type.BaseType, Is.SameAs (typeof (MulticastDelegate)));
      Assert.That (_type.Name, Is.EqualTo ("DelegateTypePlaceholder"));
      Assert.That (_type.Namespace, Is.Null);
      Assert.That (_type.Attributes, Is.EqualTo (TypeAttributes.Public | TypeAttributes.Sealed));
      Assert.That (_type.IsGenericType, Is.False);
      Assert.That (_type.GetGenericArguments(), Is.Empty);

      Assert.That (_type.ReturnType, Is.SameAs (_returnType));
      Assert.That (_type.ParameterTypes, Is.EqualTo (new[] { _parameterType }));
    }

    [Test]
    public void GetAllMethods ()
    {
      var result = _type.Invoke<IEnumerable<MethodInfo>> ("GetAllMethods").ToList();

      Assert.That (result, Has.Count.EqualTo (1));
      var invokeMethod = result.Single();
      Assert.That (invokeMethod.Name, Is.EqualTo ("Invoke"));
      Assert.That (invokeMethod.Attributes, Is.EqualTo (MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig));
      Assert.That (invokeMethod.ReturnType, Is.SameAs (_returnType));
      Assert.That (invokeMethod.GetParameters().Single().ParameterType, Is.SameAs (_parameterType));
    }

    [Test]
    public void UnsupportedMembers ()
    {
      UnsupportedMemberTestHelper.CheckMethod (() => _type.GetCustomAttributeData(), "GetCustomAttributeData");
      UnsupportedMemberTestHelper.CheckMethod (() => _type.Invoke ("GetAllInterfaces"), "GetAllInterfaces");
      UnsupportedMemberTestHelper.CheckMethod (() => _type.Invoke ("GetAllFields"), "GetAllFields");
      UnsupportedMemberTestHelper.CheckMethod (() => _type.Invoke ("GetAllConstructors"), "GetAllConstructors");
      UnsupportedMemberTestHelper.CheckMethod (() => _type.Invoke ("GetAllProperties"), "GetAllProperties");
      UnsupportedMemberTestHelper.CheckMethod (() => _type.Invoke ("GetAllEvents"), "GetAllEvents");
    }
  }
}