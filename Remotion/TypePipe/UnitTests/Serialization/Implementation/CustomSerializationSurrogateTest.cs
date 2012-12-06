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
using System.Runtime.Serialization;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.Reflection;
using Remotion.TypePipe.Serialization.Implementation;
using Rhino.Mocks;

namespace Remotion.TypePipe.UnitTests.Serialization.Implementation
{
  [TestFixture]
  public class CustomSerializationSurrogateTest
  {
    [Test]
    public void CreateRealObject ()
    {
      var underlyingType = ReflectionObjectMother.GetSomeType();
      var serializationInfo = new SerializationInfo (ReflectionObjectMother.GetSomeDifferentType(), new FormatterConverter());
      var streamingContext = new StreamingContext (StreamingContextStates.Persistence);
      var fakeObject = new object();
      var objectFactoryMock = MockRepository.GenerateStrictMock<IObjectFactory>();
      var surrogate = new CustomSerializationSurrogate (serializationInfo, new StreamingContext(StreamingContextStates.File));

      objectFactoryMock
          .Expect (x => x.CreateObject (Arg.Is (underlyingType), Arg<ParamList>.Is.Anything, Arg.Is (true)))
          .WhenCalled (
              mi =>
              Assert.That (((ParamList) mi.Arguments[1]).GetParameterValues(), Is.EqualTo (new object[] { serializationInfo, streamingContext })))
          .Return (fakeObject);


      var result = PrivateInvoke.InvokeNonPublicMethod (surrogate, "CreateRealObject", objectFactoryMock, underlyingType, streamingContext);

      objectFactoryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeObject));
    }
  }
}