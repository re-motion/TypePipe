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
using Remotion.TypePipe.MutableReflection.MemberSignatures;

namespace Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures
{
  [TestFixture]
  public class EventSignatureTest
  {
    [Test]
    public void Create ()
    {
      var eventInfo = typeof (AppDomain).GetEvent ("AssemblyResolve");
      var signature = EventSignature.Create (eventInfo);

      Assert.That (signature.EventHandlerType, Is.SameAs (typeof (ResolveEventHandler)));
    }

    [Test]
    public new void ToString ()
    {
      var signature = new EventSignature (typeof (EventHandler));

      Assert.That (signature.ToString(), Is.EqualTo ("System.EventHandler"));
    }

    [Test]
    public void Equals_True ()
    {
      var signature1 = new EventSignature (typeof (Action));
      var signature2 = new EventSignature (typeof (Action));

      Assert.That (signature1.Equals (signature2), Is.True);
    }

    [Test]
    public void Equals_False ()
    {
      var signature = new EventSignature (typeof (Action));
      Assert.That (signature.Equals (null), Is.False);

      var signatureWithDifferentEventHandlerType = new EventSignature (typeof (EventHandler));
      Assert.That (signature.Equals (signatureWithDifferentEventHandlerType), Is.False);
    }

    [Test]
    public void Equals_Object ()
    {
      var signature = new EventSignature (typeof (Action));

      object otherSignatureAsObject = new EventSignature (typeof (Action));
      Assert.That (signature.Equals (otherSignatureAsObject), Is.True);

      Assert.That (signature.Equals ((object) null), Is.False);

      object completelyUnrelatedObject = new object ();
      Assert.That (signature.Equals (completelyUnrelatedObject), Is.False);
    }

    [Test]
    public void GetHashCode_ForEqualObjects ()
    {
      var signature1 = new EventSignature (typeof (Action));
      var signature2 = new EventSignature (typeof (Action));

      Assert.That (signature1.GetHashCode(), Is.EqualTo (signature2.GetHashCode()));
    }
  }
}