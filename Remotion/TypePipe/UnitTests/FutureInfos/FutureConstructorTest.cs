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
using Remotion.TypePipe.UnitTests.Utilities;

namespace Remotion.TypePipe.UnitTests.FutureInfos
{
  [TestFixture]
  public class FutureConstructorTest
  {
    [Test]
    public void Initialization ()
    {
      // Arrange
      var futureType = new FutureType();

      // Act
      var futureConstructor = new FutureConstructor (futureType);

      // Assert
      Assert.That (futureConstructor.DeclaringType, Is.SameAs (futureType));
    }

    [Test]
    public void FutureConstructorIsAConstructorInfo ()
    {
      Assert.That (NewFutureCtor(), Is.InstanceOf<ConstructorInfo>());
      Assert.That (NewFutureCtor(), Is.AssignableTo<ConstructorInfo>());
    }

    [Test]
    public void GetParameters ()
    {
      Assert.That (NewFutureCtor().GetParameters(), Is.EqualTo (new ParameterInfo[0]));
    }

    [Test]
    public void SetConstructorInfo_ThrowsIfCalledMoreThanOnce ()
    {
      // Arrange
      var futureConstructor = NewFutureCtor();
      var constructorInfo = new FakeAdapter<ConstructorInfo>();

      // Act
      TestDelegate action = () => futureConstructor.SetConstructorInfo (constructorInfo);

      // Assert
      Assert.That (action, Throws.Nothing);
      Assert.That (action, Throws.InvalidOperationException.With.Message.EqualTo ("ConstructorInfo already set"));
    }

    [Test]
    public void ConstructorInfo ()
    {
      // Arrange
      var futureConstructor = NewFutureCtor();
      var constructorInfo = new FakeAdapter<ConstructorInfo>();

      // Act
      futureConstructor.SetConstructorInfo (constructorInfo);

      // Assert
      Assert.That (futureConstructor.ConstructorInfo, Is.SameAs (constructorInfo));
    }

    [Test]
    public void ConstructorInfo_ThrowsIfNotSet ()
    {
      Assert.That (
          () => NewFutureCtor().ConstructorInfo,
          Throws.InvalidOperationException.With.Message.EqualTo ("ConstructorInfo not set"));
    }

    private FutureConstructor NewFutureCtor ()
    {
      return new FutureConstructor (null);
    }
  }
}