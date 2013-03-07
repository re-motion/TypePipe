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
using Remotion.TypePipe.MutableReflection;
using Remotion.TypePipe.MutableReflection.Implementation.MemberFactory;
using Remotion.TypePipe.UnitTests.Expressions;

namespace Remotion.TypePipe.UnitTests.MutableReflection.Implementation.MemberFactory
{
  [TestFixture]
  public class InitializationFactoryTest
  {
    private InitializationFactory _factory;

    private ProxyType _proxyType;

    [SetUp]
    public void SetUp ()
    {
      _factory = new InitializationFactory();

      _proxyType = ProxyTypeObjectMother.Create();
    }

    [Test]
    public void CreateInitialization ()
    {
      var fakeExpression = ExpressionTreeObjectMother.GetSomeExpression();

      var result = _factory.CreateInitialization (
          _proxyType,
          ctx =>
          {
            Assert.That (ctx.DeclaringType, Is.SameAs (_proxyType));
            Assert.That (ctx.IsStatic, Is.False);

            return fakeExpression;
          });

      Assert.That (result, Is.SameAs (fakeExpression));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Provider must not return null.\r\nParameter name: initializationProvider")]
    public void CreateInitialization_NullBody ()
    {
      _factory.CreateInitialization (_proxyType, ctx => null);
    }
  }
}