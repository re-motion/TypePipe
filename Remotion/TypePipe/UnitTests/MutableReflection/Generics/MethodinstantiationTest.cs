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
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.MutableReflection.Generics;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class MethodInstantiationTest
  {
    [Test]
    public void Initialization ()
    {
      var genericMethod = ReflectionObjectMother.GetSomeGenericMethod();
      var typeArgument = ReflectionObjectMother.GetSomeType();

      var instantiation = new MethodInstantiation (genericMethod, new[] { typeArgument });

      Assert.That (instantiation.DeclaringType, Is.SameAs (genericMethod.DeclaringType));
      Assert.That (instantiation.Name, Is.EqualTo (genericMethod.Name));
      Assert.That (instantiation.Attributes, Is.EqualTo (genericMethod.Attributes));
      Assert.That (instantiation.GetGenericArguments(), Is.EqualTo (new[] { typeArgument }));
    }

    [Test]
    public void GetCustomAttributeData ()
    {
      var customAttributes = new[] { CustomAttributeDeclarationObjectMother.Create () };
      var method = CustomMethodInfoObjectMother.Create (customAttributes: customAttributes);
      var methodInstantiation = new MethodInstantiation (method, Type.EmptyTypes);

      Assert.That (methodInstantiation.GetCustomAttributeData (), Is.EqualTo (customAttributes));
    }
  }
}