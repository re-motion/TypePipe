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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Implementation;
using Remotion.Development.UnitTesting.Enumerables;

namespace Remotion.TypePipe.UnitTests.Implementation
{
  [TestFixture]
  public class LoadedTypesContextTest
  {
    [Test]
    public void Initialization ()
    {
      var proxyType = ReflectionObjectMother.GetSomeType();
      var additionalType = ReflectionObjectMother.GetSomeOtherType();
      var state = new Dictionary<string, object>();

      var context = new LoadedTypesContext (new[] { proxyType }.AsOneTime(), new[] { additionalType }.AsOneTime(), state);

      Assert.That (context.ProxyTypes, Has.Count.EqualTo (1));
      var loadedProxy = context.ProxyTypes[0];
      Assert.That (loadedProxy.RequestedType, Is.SameAs (proxyType.BaseType));
      Assert.That (loadedProxy.GeneratedType, Is.SameAs (proxyType));
      Assert.That (context.AdditionalTypes, Is.EqualTo (new[] { additionalType }));
      Assert.That (context.State, Is.SameAs (state));
    }
  }
}