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
using Remotion.Reflection;

namespace Remotion.TypePipe.IntegrationTests.Pipeline
{
  [TestFixture]
  public class NoModificationOptimizationTest
  {
    private Type _requestedType;

    private IPipeline _pipeline;

    [SetUp]
    public void SetUp ()
    {
      _requestedType = typeof (RequestedType);

      _pipeline = PipelineFactory.Create ("NoModificationOptimizationTest");
    }

    [Test]
    public void InstantiatesRequestedType ()
    {
      var instance = _pipeline.Create (_requestedType, ParamList.Create ("string"));

      Assert.That (instance.GetType(), Is.SameAs (_requestedType));
    }

    [Test]
    public void ReturnsRequestedType ()
    {
      var assembledType = _pipeline.ReflectionService.GetAssembledType (_requestedType);

      Assert.That (assembledType, Is.SameAs (_requestedType));
    }

    public class RequestedType
    {
      public RequestedType (string s) {}
    }
  }
}