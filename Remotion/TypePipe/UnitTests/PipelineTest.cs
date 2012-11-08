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
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests
{
  [TestFixture]
  public class PipelineTest
  {
    private Type _requestedType;
    private Type _generatedType;
    private ITypeCache _typeCacheMock;

    private Pipeline _pipeline;

    [SetUp]
    public void SetUp ()
    {
      _requestedType = typeof (RequestedType);
      _generatedType = typeof (GeneratedType);
      _typeCacheMock = MockRepository.GenerateStrictMock<ITypeCache>();
      _typeCacheMock.Expect (mock => mock.GetOrGenerate (_requestedType)).Return (_generatedType).Repeat.Any();

      _pipeline = new Pipeline (_typeCacheMock);
    }

    [Test]
    public void CreateInstance_NoConstructorArguments ()
    {
      var instance1 = (GeneratedType) _pipeline.CreateInstance (_requestedType);
      var instance2 = (GeneratedType) _pipeline.CreateInstance<RequestedType>();

      Assert.That (instance1.String, Is.EqualTo ("default .ctor"));
      Assert.That (instance2.String, Is.EqualTo ("default .ctor"));
    }

    [Test]
    public void CreateInstance_ConstructorArguments ()
    {
      var instance1 = (GeneratedType) _pipeline.CreateInstance (_requestedType, ParamList.Create ("abc"));
      var instance2 = (GeneratedType) _pipeline.CreateInstance<RequestedType> (ParamList.Create ("def"));

      Assert.That (instance1.String, Is.EqualTo ("abc"));
      Assert.That (instance2.String, Is.EqualTo ("def"));
    }

    [Ignore("TODO 5163")]
    [Test]
    public void CreateInstance_NonPublicConstructor ()
    {
    }

    private class RequestedType { }
    private class GeneratedType : RequestedType
    {
      public readonly string String;
      public GeneratedType () { String = "default .ctor"; }
      public GeneratedType (string s) { String = s; }
    }
  }
}