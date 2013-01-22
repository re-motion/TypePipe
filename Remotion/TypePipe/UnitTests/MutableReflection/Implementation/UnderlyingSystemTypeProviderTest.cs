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
using NUnit.Framework;
using Remotion.Collections;
using Remotion.TypePipe.MutableReflection.Implementation;
using Remotion.TypePipe.MutableReflection;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation
{
  [Ignore("TODO 5365")]
  [TestFixture]
  public class UnderlyingSystemTypeProviderTest
  {
    private UnderlyingSystemTypeProvider _provider;

    [SetUp]
    public void SetUp ()
    {
      _provider = new UnderlyingSystemTypeProvider();
    }

    [Test]
    public void GetUnderlyingSystemType ()
    {
      var baseType = ReflectionObjectMother.GetSomeSubclassableType();
      var proxyType = CreateProxyType (baseType, typeof (IDisposable), typeof (IComparable));

      var result = _provider.GetUnderlyingSystemType (proxyType);

      Assert.That (result.IsRuntimeType(), Is.True);
      Assert.That (baseType.IsAssignableFrom (result), Is.True);
      Assert.That (typeof (IDisposable).IsAssignableFrom (result), Is.True);
      Assert.That (typeof (IComparable).IsAssignableFrom (result), Is.True);
      Assert.That (_provider.GetUnderlyingSystemType (proxyType), Is.SameAs (result), "Should be cached.");
    }

    //[Test]
    //public void GetUnderlyingSystemType_CacheKey ()
    //{
    //  var baseType = ReflectionObjectMother.GetSomeSubclassableType();
    //  var otherBaseType = ReflectionObjectMother.GetSomeDifferentType();
    //  var interfaceTypes = new[] { typeof (IDisposable), typeof (IComparable) };
    //  var otherInterfaceTypes = new[] { typeof (IDisposable), typeof (ICloneable) };
    //  var equivalentInterfaceTypes = new[] { typeof (IComparable), typeof (IDisposable) };

    //  var result1 = _provider.GetUnderlyingSystemType (CreateProxyType (baseType, interfaceTypes));
    //  var result2 = _provider.GetUnderlyingSystemType (CreateProxyType (otherBaseType, interfaceTypes));
    //  var result3 = _provider.GetUnderlyingSystemType (CreateProxyType (baseType, otherInterfaceTypes));
    //  var result4 = _provider.GetUnderlyingSystemType (CreateProxyType (baseType, equivalentInterfaceTypes));

    //  Assert.That (result1, Is.Not.SameAs (result2));
    //  Assert.That (result1, Is.Not.SameAs (result3));
    //  Assert.That (result1, Is.SameAs (result4));
    //}

    private ProxyType CreateProxyType (Type baseType, params Type[] addedInterfaces)
    {
      var proxyType = ProxyTypeObjectMother.Create (baseType);
      foreach (var addedInterface in addedInterfaces)
        proxyType.AddInterface (addedInterface);

      return proxyType;
    }
  }
}