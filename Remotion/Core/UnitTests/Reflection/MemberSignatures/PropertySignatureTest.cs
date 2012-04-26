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
  public class PropertySignatureTest
  {
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