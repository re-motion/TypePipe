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
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using Remotion.Development.UnitTesting;
using Remotion.TypePipe.Serialization;
using Remotion.TypePipe.UnitTests.Serialization.TestDomain;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Remotion.TypePipe.UnitTests.Serialization
{
  [TestFixture]
  public class SerializationEventRaiserTest
  {
    [Test]
    public void InvokeAttributedMethod_OnDeserialized ()
    {
      SerializationEventRaiser eventRaiser = new SerializationEventRaiser();

      ClassWithDeserializationEvents instance = new ClassWithDeserializationEvents ();
      Assert.That (instance.OnBaseDeserializingCalled, Is.False);
      Assert.That (instance.OnBaseDeserializedCalled, Is.False);
      Assert.That (instance.OnDeserializingCalled, Is.False);
      Assert.That (instance.OnDeserializedCalled, Is.False);
      Assert.That (instance.OnDeserializationCalled, Is.False);

      eventRaiser.InvokeAttributedMethod (instance, typeof (OnDeserializedAttribute), new StreamingContext ());

      Assert.That (instance.OnBaseDeserializingCalled, Is.False);
      Assert.That (instance.OnBaseDeserializedCalled, Is.True);
      Assert.That (instance.OnDeserializingCalled, Is.False);
      Assert.That (instance.OnDeserializedCalled, Is.True);
      Assert.That (instance.OnDeserializationCalled, Is.False);
    }

    [Test]
    public void InvokeAttributedMethod_OnDeserializing ()
    {
      SerializationEventRaiser eventRaiser = new SerializationEventRaiser ();

      ClassWithDeserializationEvents instance = new ClassWithDeserializationEvents ();
      Assert.That (instance.OnBaseDeserializingCalled, Is.False);
      Assert.That (instance.OnBaseDeserializedCalled, Is.False);
      Assert.That (instance.OnDeserializingCalled, Is.False);
      Assert.That (instance.OnDeserializedCalled, Is.False);
      Assert.That (instance.OnDeserializationCalled, Is.False);

      eventRaiser.InvokeAttributedMethod (instance, typeof (OnDeserializingAttribute), new StreamingContext ());

      Assert.That (instance.OnBaseDeserializingCalled, Is.True);
      Assert.That (instance.OnBaseDeserializedCalled, Is.False);
      Assert.That (instance.OnDeserializingCalled, Is.True);
      Assert.That (instance.OnDeserializedCalled, Is.False);
      Assert.That (instance.OnDeserializationCalled, Is.False);
    }

    [Test]
    public void InvokeAttributedMethod_UsesCache ()
    {
      ClassWithDeserializationEvents instance = new ClassWithDeserializationEvents ();
      StreamingContext context = new StreamingContext();

      MockRepository repository = new MockRepository();
      SerializationEventRaiser eventRaiserMock = repository.StrictMock<SerializationEventRaiser>();

      eventRaiserMock.InvokeAttributedMethod (instance, typeof (OnDeserializedAttribute), context);
      LastCall.CallOriginalMethod (OriginalCallOptions.CreateExpectation);

      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (eventRaiserMock, "FindDeserializationMethodsWithCache", typeof (ClassWithDeserializationEvents), typeof (OnDeserializedAttribute)))
          .Return (new List<MethodInfo>());

      repository.ReplayAll();

      eventRaiserMock.InvokeAttributedMethod (instance, typeof (OnDeserializedAttribute), context);

      repository.VerifyAll();
    }

    [Test]
    public void FindDeserializationMethodsWithCache_Caching ()
    {
      SerializationEventRaiser eventRaiser = new SerializationEventRaiser();
      List<MethodInfo> methods = (List<MethodInfo>) PrivateInvoke.InvokeNonPublicMethod (
          eventRaiser, "FindDeserializationMethodsWithCache", typeof (ClassWithDeserializationEvents), typeof (OnDeserializedAttribute));
      Assert.That (methods, Is.Not.Null);
      List<MethodInfo> methods2 = (List<MethodInfo>) PrivateInvoke.InvokeNonPublicMethod (
          eventRaiser, "FindDeserializationMethodsWithCache", typeof (ClassWithDeserializationEvents), typeof (OnDeserializedAttribute));
      Assert.That (methods2, Is.SameAs (methods));
    }

    [Test]
    public void RaiseOnDeserialization ()
    {
      SerializationEventRaiser eventRaiser = new SerializationEventRaiser ();

      ClassWithDeserializationEvents instance = new ClassWithDeserializationEvents ();
      Assert.That (instance.OnBaseDeserializingCalled, Is.False);
      Assert.That (instance.OnBaseDeserializedCalled, Is.False);
      Assert.That (instance.OnDeserializingCalled, Is.False);
      Assert.That (instance.OnDeserializedCalled, Is.False);
      Assert.That (instance.OnDeserializationCalled, Is.False);

      eventRaiser.RaiseDeserializationEvent (instance, null);

      Assert.That (instance.OnBaseDeserializingCalled, Is.False);
      Assert.That (instance.OnBaseDeserializedCalled, Is.False);
      Assert.That (instance.OnDeserializingCalled, Is.False);
      Assert.That (instance.OnDeserializedCalled, Is.False);
      Assert.That (instance.OnDeserializationCalled, Is.True);
    }
  }
}
