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
using Remotion.TypePipe.MutableReflection.Generics;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Generics
{
  [TestFixture]
  public class InstantiationInfoTest
  {
    private InstantiationInfo.EqualityComparer _comparer;

    private InstantiationInfo _info1;
    private InstantiationInfo _info2;
    private InstantiationInfo _info3;
    private InstantiationInfo _info4;
    private InstantiationInfo _info5;

    [SetUp]
    public void SetUp ()
    {
      _comparer = new InstantiationInfo.EqualityComparer();

      var genericTypeDef1 = typeof (List<>);
      var genericTypeDef2 = typeof (Func<>);

      var typeArg1 = typeof (int);
      var typeArg2 = typeof (string);

      _info1 = new InstantiationInfo (genericTypeDef1, new[] { typeArg1 });
      _info2 = new InstantiationInfo (genericTypeDef2, new[] { typeArg1 });
      _info3 = new InstantiationInfo (genericTypeDef1, new[] { typeArg2 });
      _info4 = new InstantiationInfo (genericTypeDef1, new[] { typeArg1, typeArg2 });
      _info5 = new InstantiationInfo (genericTypeDef1, new[] { typeArg1 });
    }

    [Test]
    public void EqualityComparer_Equals ()
    {
      Assert.That (_comparer.Equals (_info1, _info2), Is.False);
      Assert.That (_comparer.Equals (_info1, _info3), Is.False);
      Assert.That (_comparer.Equals (_info1, _info4), Is.False);
      Assert.That (_comparer.Equals (_info1, _info5), Is.True);
    }

    [Test]
    public void EqualityComparer_GetHashCode ()
    {
      Assert.That (_comparer.GetHashCode (_info1), Is.EqualTo (_comparer.GetHashCode (_info5)));
    }
  }
}