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

namespace Remotion.TypePipe.UnitTests
{
  [TestFixture]
  public class DelegateFactoryTest
  {
    private DelegateFactory _factory;

    [SetUp]
    public void SetUp ()
    {
      _factory = new DelegateFactory();
    }

    [Test]
    public void CreateConstructorCall ()
    {
      var result = (Func<string, int, DomainType>) _factory.CreateConstructorCall (typeof (DomainType), new[] { typeof (string), typeof (int) }, false, typeof (Func<string, int, DomainType>));

      var instance = result ("abc", 7);
      Assert.That (instance.String, Is.EqualTo ("abc"));
      Assert.That (instance.Int, Is.EqualTo (7));
    }

    [Test]
    public void CreateConstructorCall_NonPublic ()
    {
      var result = (Func<DomainType>) _factory.CreateConstructorCall (typeof (DomainType), Type.EmptyTypes, true, typeof (Func<DomainType>));

      var instance = result();
      Assert.That (instance.String, Is.EqualTo ("non-public .ctor"));
    }

    [Ignore ("TODO 5172")]
    [Test]
    public void CreateConstructorCall_NonPublic_Throws ()
    {
      _factory.CreateConstructorCall (typeof (DomainType), Type.EmptyTypes, allowNonPublic: false, delegateType: typeof (Func<DomainType>));
    }

    class DomainType
    {
      public readonly string String;
      public readonly int Int;

      public DomainType (string s1, int i2) { String = s1; Int = i2; }
      protected DomainType () { String = "non-public .ctor"; }
    }
  }
}