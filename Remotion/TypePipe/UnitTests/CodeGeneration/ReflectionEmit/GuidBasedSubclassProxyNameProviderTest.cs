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
using Remotion.TypePipe.CodeGeneration.ReflectionEmit;
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.UnitTests.MutableReflection;

namespace Remotion.TypePipe.UnitTests.CodeGeneration.ReflectionEmit
{
  [TestFixture]
  public class GuidBasedSubclassProxyNameProviderTest
  {
    private GuidBasedSubclassProxyNameProvider _provider;
    private MutableType _mutableType;

    [SetUp]
    public void SetUp ()
    {
      _provider = new GuidBasedSubclassProxyNameProvider ();
      _mutableType = MutableTypeObjectMother.Create (underlyingTypeDescriptor: UnderlyingTypeDescriptorObjectMother.Create (typeof (object)));
    }

    [Test]
    public void GetSubclassProxyName ()
    {
      var result = _provider.GetSubclassProxyName (_mutableType);

      Assert.That (result, Is.StringMatching (@"System\.Object_Proxy_.{32}"));
    }

    [Test]
    public void GetSubclassProxyName_NameIsUnique ()
    {
      var result1 = _provider.GetSubclassProxyName (_mutableType);
      var result2 = _provider.GetSubclassProxyName (_mutableType);

      Assert.That (result1, Is.Not.EqualTo (result2));
    }


  }
}