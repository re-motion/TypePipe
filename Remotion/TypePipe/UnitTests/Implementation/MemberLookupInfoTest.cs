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
using NUnit.Framework;
using Remotion.Collections;
using Remotion.Reflection;

namespace Remotion.UnitTests.Reflection
{
  [TestFixture]
  public class MemberLookupInfoTest
  {
    [Test]
    public void GetParameterTypes ()
    {
      var info = new MemberLookupInfo ("Foo");

      var parameterTypes = info.GetParameterTypes (typeof (Func<int, string, object>));

      var expected = new[] { typeof (int), typeof (string) };
      Assert.That (parameterTypes, Is.EqualTo (expected));
    }

    [Test]
    public void GetSignature ()
    {
      var info = new MemberLookupInfo ("Foo");

      var signature = info.GetSignature (typeof (Func<int, string, object>));

      var expected = Tuple.Create (new[] { typeof (int), typeof (string) }, typeof (object));
      Assert.That (signature.Item1, Is.EqualTo (expected.Item1));
      Assert.That (signature.Item2, Is.EqualTo (expected.Item2));
    }

  }
}