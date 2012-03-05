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
using Remotion.TypePipe.FutureInfos;

namespace Remotion.TypePipe.UnitTests.FutureInfos
{
  [TestFixture]
  public class FutureConstructorTest
  {
    private FutureConstructor _futureConstructor;

    [SetUp]
    public void SetUp ()
    {
      _futureConstructor = new FutureConstructor (typeof (string));
    }

    [Test]
    public void Initialization ()
    {
      var declaringType = typeof (string);

      var futureConstructor = new FutureConstructor (declaringType);
      
      Assert.That (futureConstructor.DeclaringType, Is.SameAs (declaringType));
    }

    [Test]
    public void FutureConstructor_IsAConstructorInfo ()
    {
      Assert.That (_futureConstructor, Is.InstanceOf<ConstructorInfo>());
      Assert.That (_futureConstructor, Is.AssignableTo<ConstructorInfo>());
    }

    [Test]
    public void GetParameters_Empty ()
    {
      Assert.That (_futureConstructor.GetParameters(), Is.EqualTo (new ParameterInfo[0]));
    }
  }
}