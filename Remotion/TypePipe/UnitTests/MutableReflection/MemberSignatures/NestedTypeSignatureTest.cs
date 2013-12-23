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
  public class NestedTypeSignatureTest
  {
    [Test]
    public void Create ()
    {
      var signature1 = NestedTypeSignature.Create (typeof (NestedType));
      var signature2 = NestedTypeSignature.Create (typeof (NestedType<>));

      Assert.That (signature1.GenericParameterCount, Is.EqualTo (0));
      Assert.That (signature2.GenericParameterCount, Is.EqualTo (1));
    }

    [Test]
    public new void ToString ()
    {
      var signature = NestedTypeSignature.Create (typeof (NestedType<>));

      Assert.That (signature.ToString(), Is.EqualTo ("`1"));
    }

    [Test]
    public void Equals_True ()
    {
      var signature1 = NestedTypeSignature.Create (typeof (NestedType<>));
      var signature2 = NestedTypeSignature.Create (typeof (NestedType<>));

      Assert.That (signature1.Equals (signature2), Is.True);
    }

    [Test]
    public void Equals_False ()
    {
      var signature = NestedTypeSignature.Create (typeof (NestedType));
      Assert.That (signature.Equals (null), Is.False);

      var signatureWithMoreGenericParameters = NestedTypeSignature.Create (typeof (NestedType<>));
      Assert.That (signature.Equals (signatureWithMoreGenericParameters), Is.False);
    }

    [Test]
    public void Equals_Object ()
    {
      var signature = NestedTypeSignature.Create (typeof (NestedType));

      object otherSignatureAsObject = NestedTypeSignature.Create (typeof (NestedType));
      Assert.That (signature.Equals (otherSignatureAsObject), Is.True);

      Assert.That (signature.Equals ((object) null), Is.False);

      object completelyUnrelatedObject = new object ();
      Assert.That (signature.Equals (completelyUnrelatedObject), Is.False);
    }

    [Test]
    public void GetHashCode_ForEqualObjects ()
    {
      var signature1 = NestedTypeSignature.Create (typeof (NestedType));
      var signature2 = NestedTypeSignature.Create (typeof (NestedType));

      Assert.That (signature1.GetHashCode (), Is.EqualTo (signature2.GetHashCode ()));
    }

    public class NestedType {}
    public class NestedType<T> {}
    
  }
}