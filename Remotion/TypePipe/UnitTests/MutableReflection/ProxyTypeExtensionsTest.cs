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
using Remotion.TypePipe.MutableReflection;
using System.Linq;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class ProxyTypeExtensionsTest
  {
    private ProxyType _proxyType;

    [SetUp]
    public void SetUp ()
    {
      _proxyType = ProxyTypeObjectMother.Create();
    }

    [Test]
    public void AddAbstractMethod ()
    {
      var name = "AbstractMethod";
      var attributes = MethodAttributes.Family;
      var returnType = ReflectionObjectMother.GetSomeType();
      var parameters = ParameterDeclarationObjectMother.CreateMultiple (2);

      var result = _proxyType.AddAbstractMethod (name, attributes, returnType, parameters);

      Assert.That (result.IsAbstract, Is.True);
      Assert.That (result.Name, Is.EqualTo (name));
      Assert.That (result.Attributes, Is.EqualTo (MethodAttributes.Family | MethodAttributes.Abstract | MethodAttributes.Virtual));
      Assert.That (result.ReturnType, Is.SameAs (returnType));
      Assert.That (result.GetParameters().Select (p => p.ParameterType), Is.EqualTo (parameters.Select (p => p.Type)));

      Assert.That (_proxyType.AddedMethods, Is.EqualTo (new[] { result }));
    }
  }
}