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
using Remotion.TypePipe.Implementation;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class NoModificationOptimizationTest
  {
    private IPipeline _pipeline;

    [SetUp]
    public void SetUp ()
    {
      _pipeline = new DefaultPipelineFactory().Create ("NoModificationOptimizationTest");
    }

    [Test]
    public void InstantiatesRequestedType ()
    {
      var requestedType = typeof (RequestedType);
      var instance = _pipeline.Create (requestedType, ParamList.Create ("string"));

      Assert.That (instance.GetType(), Is.SameAs (requestedType));
    }

    [Test]
    public void ReturnsRequestedType ()
    {
      var requestedType = typeof (RequestedType);
      var assembledType = _pipeline.ReflectionService.GetAssembledType (requestedType);

      Assert.That (assembledType, Is.SameAs (requestedType));
    }

    [Test]
    [Ignore ("TODO 5735")]
    public void AbstractType_WithoutAbstractMethods_IsAssembledAsNonAbstract ()
    {
      var assembledType = _pipeline.ReflectionService.GetAssembledType (typeof (RequestedAbstractType));

      Assert.That (assembledType, Is.Not.SameAs (typeof (RequestedAbstractType)));
      Assert.That (assembledType.IsAbstract, Is.False);
    }

    public class RequestedType
    {
      public RequestedType (string s) {}
    }

    public abstract class RequestedAbstractType
    {
      public RequestedAbstractType (string s) { }
    }
  }
}