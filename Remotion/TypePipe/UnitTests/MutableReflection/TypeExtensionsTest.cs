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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection.Implementation;

namespace Remotion.TypePipe.UnitTests.MutableReflection
{
  [TestFixture]
  public class TypeExtensionsTest
  {
    [Test]
    public void IsRuntimeType ()
    {
      var runtimeType = typeof (int);
      var proxyType = ProxyTypeObjectMother.Create (baseType: typeof (object));

      Assert.That (runtimeType.IsRuntimeType(), Is.True);
      Assert.That (proxyType.IsRuntimeType(), Is.False);
    }

    [Test]
    public void IsAssignableFromFast_NoCustomTypes ()
    {
      Assert.That (typeof (string).IsAssignableFromFast (typeof (string)), Is.True);
      Assert.That (typeof (object).IsAssignableFromFast (typeof (string)), Is.True);
      Assert.That (typeof (string).IsAssignableFromFast (typeof (object)), Is.False);
    }

    [Test]
    public void IsAssignableFromFast_CustomType_OnLeftSide ()
    {
      var customType = CustomTypeObjectMother.Create();

      Assert.That (customType.IsAssignableFromFast (customType), Is.True);
      Assert.That (customType.IsAssignableFromFast (customType.BaseType), Is.False);
      Assert.That (customType.IsAssignableFromFast (typeof (object)), Is.False);
    }

    [Test]
    public void IsAssignableFromFast_CustomType_OnRightSide ()
    {
      var customType = CustomTypeObjectMother.Create (baseType: typeof (List<int>), interfaces: new[] { typeof (IDisposable) });

      Assert.That (customType.IsAssignableFromFast (customType), Is.True);

      Assert.That (typeof (List<int>).IsAssignableFromFast (customType), Is.True);
      Assert.That (typeof (object).IsAssignableFromFast (customType), Is.True);

      Assert.That (typeof (IDisposable).IsAssignableFromFast (customType), Is.True);

      var unrelatedType = ReflectionObjectMother.GetSomeType();
      Assert.That (unrelatedType.IsAssignableFromFast (customType), Is.False);
    }
  }
}