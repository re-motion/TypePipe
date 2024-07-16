﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using Remotion.Development.UnitTesting.Reflection;
using Remotion.TypePipe.Dlr.Ast;
using Remotion.TypePipe.Serialization;
using Remotion.TypePipe.TypeAssembly.Implementation;

namespace Remotion.TypePipe.UnitTests.TypeAssembly.Implementation
{
  [TestFixture]
  public class NullTypeIdentifierProviderTest
  {
    private NullTypeIdentifierProvider _provider;

    [SetUp]
    public void SetUp ()
    {
      _provider = new NullTypeIdentifierProvider();
    }

    [Test]
    public void GetID ()
    {
      var requestedType = ReflectionObjectMother.GetSomeType();

      Assert.That (_provider.GetID (requestedType), Is.Null);
    }

    [Test]
    public void GetExpression ()
    {
      var result = _provider.GetExpression (id: null);

      Assert.That (result, Is.TypeOf<ConstantExpression>());
      var constantExpression = (ConstantExpression) result;
      Assert.That (constantExpression.Type, Is.SameAs (typeof (object)));
      Assert.That (constantExpression.Value, Is.Null);
    }

    [Test]
    public void GetFlattenedExpressionForSerialization ()
    {
      var result = _provider.GetFlatValueExpressionForSerialization (id: null);

      Assert.That (result, Is.InstanceOf<ConstantExpression>());
      var constantExpression = (ConstantExpression) result;
      Assert.That (constantExpression.Type, Is.SameAs (typeof (IFlatValue)));
      Assert.That (constantExpression.Value, Is.Null);
    }
  }
}