// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using NUnit.Framework;
using Remotion.Reflection.MemberSignatures;

namespace Remotion.UnitTests.Reflection.MemberSignatures
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