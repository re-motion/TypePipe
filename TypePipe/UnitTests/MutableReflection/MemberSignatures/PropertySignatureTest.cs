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
using NUnit.Framework;
using Remotion.TypePipe.MutableReflection.MemberSignatures;

namespace Remotion.TypePipe.UnitTests.MutableReflection.MemberSignatures
{
  [TestFixture]
  public class PropertySignatureTest
  {
    [Test]
    public void Create ()
    {
      var property = typeof (Dictionary<int,string>).GetProperty ("Item");
      var signature = PropertySignature.Create (property);

      Assert.That (signature.PropertyType, Is.SameAs (typeof (string)));
      Assert.That (signature.IndexParameterTypes, Is.EqualTo (new[] { typeof (int) }));
    }

    [Test]
    public void ToString_WithIndexParameters ()
    {
      var signature = new PropertySignature (typeof (string), new[] { typeof (double), typeof (int) });

      Assert.That (signature.ToString(), Is.EqualTo ("System.String(System.Double,System.Int32)"));
    }

    [Test]
    public void ToString_WithoutIndexParameters ()
    {
      var signature = new PropertySignature (typeof (string), Type.EmptyTypes);

      Assert.That (signature.ToString (), Is.EqualTo ("System.String()"));
    }

    [Test]
    public void Equals_True ()
    {
      var signature1 = new PropertySignature (typeof (int), new[] { typeof (double), typeof (string) });
      var signature2 = new PropertySignature (typeof (int), new[] { typeof (double), typeof (string) });

      Assert.That (signature1.Equals (signature2), Is.True);
    }

    [Test]
    public void Equals_False ()
    {
      var signature = new PropertySignature (typeof (int), new[] { typeof (double), typeof (string) });
      Assert.That (signature.Equals (null), Is.False);

      var signatureWithDifferentPropertyType = new PropertySignature (typeof (string), new[] { typeof (double), typeof (string) });
      Assert.That (signature.Equals (signatureWithDifferentPropertyType), Is.False);

      var signatureWithDifferentIndexParameters = new PropertySignature (typeof (int), new[] { typeof (string), typeof (double) });
      Assert.That (signature.Equals (signatureWithDifferentIndexParameters), Is.False);
    }

    [Test]
    public void Equals_Object ()
    {
      var signature = new PropertySignature (typeof (int), new[] { typeof (double), typeof (string) });

      object otherSignatureAsObject = new PropertySignature (typeof (int), new[] { typeof (double), typeof (string) });
      Assert.That (signature.Equals (otherSignatureAsObject), Is.True);

      Assert.That (signature.Equals ((object) null), Is.False);

      object completelyUnrelatedObject = new object ();
      Assert.That (signature.Equals (completelyUnrelatedObject), Is.False);
    }

    [Test]
    public void GetHashCode_ForEqualObjects ()
    {
      var signature1 = new PropertySignature (typeof (int), new[] { typeof (double), typeof (string) });
      var signature2 = new PropertySignature (typeof (int), new[] { typeof (double), typeof (string) });

      Assert.That (signature1.GetHashCode (), Is.EqualTo (signature2.GetHashCode ()));
    }
  }
}